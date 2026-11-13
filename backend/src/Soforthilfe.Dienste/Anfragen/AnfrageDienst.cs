using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Mandanten;
using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Notfall;
using Soforthilfe.Dienste.Orte;
using Soforthilfe.Dienste.Protokoll;
using Soforthilfe.Dienste.Verstaendnis;

namespace Soforthilfe.Dienste.Anfragen;

/// <summary>
/// Erfasst Anfragen in der Region der laufenden Anfrage und liest sie wieder.
/// </summary>
public sealed partial class AnfrageDienst(
    SoforthilfeDbContext datenbank,
    IMandantenKontext mandant,
    INotfallPruefung notfallPruefung,
    IProtokollDienst protokoll,
    IOrtDienst orte,
    IEinordnung einordnung) : IAnfrageDienst
{
    // Schweizer Postleitzahlen sind vierstellig und beginnen nicht mit 0.
    [GeneratedRegex("^[1-9][0-9]{3}$")]
    private static partial Regex PostleitzahlMuster();

    public async Task<AnfrageErgebnis> ErfassenAsync(AnfrageDaten daten, CancellationToken abbruch)
    {
        if (mandant.RegionId is not Guid region)
        {
            return new AnfrageErgebnis(null, AnfrageFehler.RegionFehlt);
        }

        // Mindestens eine Kategorie oder ein Freitext; beides zusammen ist erlaubt.
        var freitext = Leer(daten.Freitext);
        if (daten.KategorieId is null && freitext is null)
        {
            return new AnfrageErgebnis(null, AnfrageFehler.InhaltFehlt);
        }

        // Gesundheitlicher Notfall: nicht speichern und nicht vermitteln, sondern auf 144 verweisen.
        var notfall = notfallPruefung.Pruefen(freitext);
        if (notfall.IstNotfall)
        {
            // Protokolliert wird nur der Vorgang mit dem Begriff aus der festen Liste, ohne Konto und ohne Freitext.
            await protokoll.SchreibenAsync(ProtokollAktionen.NotfallStopp, ProtokollAktionen.ObjektAnfrage, null,
                NotfallTexte.Grund(notfall.Begriff), abbruch);
            return new AnfrageErgebnis(null, AnfrageFehler.Notfall, NotfallTexte.Grund(notfall.Schluesselwort));
        }

        var plz = daten.Plz.Trim();
        if (!PostleitzahlMuster().IsMatch(plz))
        {
            return new AnfrageErgebnis(null, AnfrageFehler.PostleitzahlUngueltig);
        }

        var ort = Leer(daten.Ort);
        if (ort is null)
        {
            return new AnfrageErgebnis(null, AnfrageFehler.OrtFehlt);
        }

        // Die Postleitzahl muss im Ortschaftenverzeichnis stehen und eine Gemeinde der Region berühren.
        switch (await orte.PruefeFuerRegionAsync(plz, region, abbruch))
        {
            case PlzPruefung.Unbekannt:
                return new AnfrageErgebnis(null, AnfrageFehler.PostleitzahlUnbekannt);
            case PlzPruefung.AndereRegion:
                return new AnfrageErgebnis(null, AnfrageFehler.PostleitzahlAusserhalbRegion);
        }

        if (!Enum.IsDefined(typeof(Prioritaet), daten.Prioritaet))
        {
            return new AnfrageErgebnis(null, AnfrageFehler.PrioritaetUngueltig);
        }

        Kategorie? kategorie = null;
        if (daten.KategorieId is Guid kategorieId)
        {
            kategorie = await datenbank.Kategorien.AsNoTracking().FirstOrDefaultAsync(k => k.Id == kategorieId, abbruch);
            if (kategorie is null)
            {
                return new AnfrageErgebnis(null, AnfrageFehler.KategorieUnbekannt);
            }
        }
        else
        {
            // Ohne gewählte Kategorie ordnet die Verständniskette den Freitext ein; ohne sicheres Ergebnis bleibt sie leer.
            var eingeordnet = await einordnung.EinordnenAsync(null, freitext, abbruch);
            if (eingeordnet.KategorieId is Guid erkannt)
            {
                kategorie = await datenbank.Kategorien.AsNoTracking().FirstAsync(k => k.Id == erkannt, abbruch);
            }
        }

        var anfrage = new Anfrage
        {
            Id = Guid.NewGuid(),
            RegionId = region,
            KontoId = daten.KontoId,
            KategorieId = kategorie?.Id,
            Freitext = freitext,
            Postleitzahl = plz,
            Ort = ort,
            Adresse = Leer(daten.Adresse),
            Zeitfenster = Leer(daten.Zeitfenster),
            Prioritaet = (Prioritaet)daten.Prioritaet,
            Status = AnfrageStatus.Offen,
            ErstelltAm = DateTimeOffset.UtcNow,
        };

        datenbank.Anfragen.Add(anfrage);
        await datenbank.SaveChangesAsync(abbruch);

        anfrage.Kategorie = kategorie;
        return new AnfrageErgebnis(Info(anfrage), null);
    }

    public async Task<IReadOnlyList<AnfrageInfo>> ListeAsync(Guid? nurVon, CancellationToken abbruch)
    {
        // Der Regionsfilter des Datenzugriffs gilt immer; nurVon schränkt zusätzlich auf ein Konto ein.
        var liste = await Abfrage(nurVon)
            .OrderByDescending(a => a.ErstelltAm)
            .ToListAsync(abbruch);

        return liste.Select(Info).ToList();
    }

    public async Task<AnfrageInfo?> LiesAsync(Guid id, Guid? nurVon, CancellationToken abbruch)
    {
        var anfrage = await Abfrage(nurVon).FirstOrDefaultAsync(a => a.Id == id, abbruch);
        return anfrage is null ? null : Info(anfrage);
    }

    private IQueryable<Anfrage> Abfrage(Guid? nurVon)
    {
        var abfrage = datenbank.Anfragen.Include(a => a.Kategorie).AsNoTracking();
        return nurVon is Guid konto ? abfrage.Where(a => a.KontoId == konto) : abfrage;
    }

    private static string? Leer(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private static AnfrageInfo Info(Anfrage anfrage) => new(
        anfrage.Id,
        anfrage.KategorieId,
        anfrage.Kategorie?.Bezeichnung,
        anfrage.Freitext,
        anfrage.Postleitzahl,
        anfrage.Ort,
        anfrage.Adresse,
        anfrage.Zeitfenster,
        (int)anfrage.Prioritaet,
        (int)anfrage.Status,
        anfrage.ErstelltAm,
        anfrage.Kategorie?.Kostenpflichtig ?? false);
}
