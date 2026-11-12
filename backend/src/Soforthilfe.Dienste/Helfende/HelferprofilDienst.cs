using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Mandanten;
using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Kategorien;
using Soforthilfe.Dienste.Orte;
using Soforthilfe.Dienste.Preise;
using Soforthilfe.Dienste.Protokoll;

namespace Soforthilfe.Dienste.Helfende;

/// <summary>
/// Helferprofil in der Region des Kontos. Der Ort muss im Ortschaftenverzeichnis stehen und zur Region gehören;
/// als Standort gilt der Punkt der Postleitzahl, nie ein genauer Standort der Person.
/// </summary>
public sealed partial class HelferprofilDienst(
    SoforthilfeDbContext datenbank,
    IMandantenKontext mandant,
    IOrtDienst orte,
    IProtokollDienst protokoll) : IHelferprofilDienst
{
    [GeneratedRegex("^[1-9][0-9]{3}$")]
    private static partial Regex PostleitzahlMuster();

    public async Task<HelferprofilInfo?> LadenAsync(Guid kontoId, CancellationToken abbruch)
    {
        var profil = await MitSkills()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.KontoId == kontoId, abbruch);

        return profil is null ? null : Info(profil);
    }

    public async Task<HelferprofilErgebnis> SpeichernAsync(Guid kontoId, HelferprofilDaten daten, CancellationToken abbruch)
    {
        if (mandant.RegionId is not Guid region || !await datenbank.Konten.AnyAsync(k => k.Id == kontoId, abbruch))
        {
            return Fehler(HelferprofilFehler.KontoFehlt);
        }

        // Fähigkeiten: nur bekannte, mindestens eine.
        var skillIds = daten.SkillIds.Distinct().ToList();
        var skills = await datenbank.Skills
            .Include(s => s.Kategorie)
            .Where(s => skillIds.Contains(s.Id))
            .ToListAsync(abbruch);
        if (skills.Count == 0 || skills.Count != skillIds.Count)
        {
            return Fehler(HelferprofilFehler.SkillFehlt);
        }

        // Anforderung A9: Wer bezahlpflichtige Leistungen anbietet, nennt Anfahrt und Stundenansatz,
        // damit Anfragende den Preis vor der Zusage kennen.
        if (skills.Any(s => s.Kategorie?.Kostenpflichtig == true) && !daten.Preise.Vollstaendig)
        {
            return Fehler(HelferprofilFehler.PreiseFehlen);
        }

        var betriebsname = Leer(daten.Betriebsname);
        if (daten.Typ == Helfertyp.Betrieb && betriebsname is null)
        {
            return Fehler(HelferprofilFehler.BetriebsnameFehlt);
        }

        var plz = daten.Plz.Trim();
        if (!PostleitzahlMuster().IsMatch(plz))
        {
            return Fehler(HelferprofilFehler.PostleitzahlUngueltig);
        }

        var ort = Leer(daten.Ort);
        if (ort is null)
        {
            return Fehler(HelferprofilFehler.OrtFehlt);
        }

        switch (await orte.PruefeFuerRegionAsync(plz, region, abbruch))
        {
            case PlzPruefung.Unbekannt:
                return Fehler(HelferprofilFehler.PostleitzahlUnbekannt);
            case PlzPruefung.AndereRegion:
                return Fehler(HelferprofilFehler.PostleitzahlAusserhalbRegion);
        }

        var profil = await datenbank.Helferprofile
            .Include(p => p.Skills)
            .FirstOrDefaultAsync(p => p.KontoId == kontoId, abbruch);
        var neu = profil is null;
        if (profil is null)
        {
            profil = new Helferprofil { Id = Guid.NewGuid(), KontoId = kontoId, RegionId = region, Postleitzahl = plz, Ort = ort };
            datenbank.Helferprofile.Add(profil);
        }

        var ortGeaendert = neu || profil.Postleitzahl != plz || profil.Ort != ort;

        // Prüfrelevant sind Angaben zur Person oder zum Betrieb, zum Ort und zu den Fähigkeiten.
        // Preise, Beschreibung und Radius lassen sich ohne neue Prüfung anpassen.
        var neuerBetriebsname = daten.Typ == Helfertyp.Betrieb ? betriebsname : null;
        var pruefrelevantGeaendert = !neu && (ortGeaendert
            || profil.Typ != daten.Typ
            || profil.Betriebsname != neuerBetriebsname
            || profil.Adresse != Leer(daten.Adresse)
            || !profil.Skills.Select(s => s.Id).Order().SequenceEqual(skills.Select(s => s.Id).Order()));

        profil.Typ = daten.Typ;
        profil.Betriebsname = neuerBetriebsname;
        profil.Beschreibung = Leer(daten.Beschreibung);
        profil.Adresse = Leer(daten.Adresse);
        profil.Postleitzahl = plz;
        profil.Ort = ort;
        profil.EinsatzradiusKm = daten.RadiusKm;
        profil.AnfahrtChf = daten.Preise.AnfahrtChf;
        profil.StundenansatzChf = daten.Preise.StundenansatzChf;
        profil.NachtzuschlagProzent = daten.Preise.NachtzuschlagProzent;
        profil.WochenendzuschlagProzent = daten.Preise.WochenendzuschlagProzent;
        profil.MwstPflichtig = daten.Preise.MwstPflichtig;
        profil.Skills.Clear();
        profil.Skills.AddRange(skills);

        // Ein freigegebenes Profil muss nach prüfrelevanten Änderungen erneut geprüft werden.
        if (pruefrelevantGeaendert && profil.Verifizierung == Verifizierungsstatus.Verifiziert)
        {
            profil.Verifizierung = Verifizierungsstatus.InPruefung;
        }

        // Für Entfernungen gilt der Punkt der Postleitzahl; ein genauer Standort wird nie gespeichert.
        if (ortGeaendert)
        {
            profil.Standort = await orte.PunktZuPlzAsync(plz, ort, abbruch);
        }

        await datenbank.SaveChangesAsync(abbruch);
        await protokoll.SchreibenAsync(neu ? "Helferprofil.Erstellt" : "Helferprofil.Geaendert", "Helferprofil",
            profil.Id.ToString(), null, abbruch);

        return new HelferprofilErgebnis(await LadenAsync(kontoId, abbruch), null);
    }

    public async Task<bool?> VerfuegbarkeitSetzenAsync(Guid kontoId, bool jetztVerfuegbar, CancellationToken abbruch)
    {
        var profil = await datenbank.Helferprofile.FirstOrDefaultAsync(p => p.KontoId == kontoId, abbruch);
        if (profil is null)
        {
            return null;
        }

        profil.Einsatzbereit = jetztVerfuegbar;
        await datenbank.SaveChangesAsync(abbruch);
        return profil.Einsatzbereit;
    }

    private IQueryable<Helferprofil> MitSkills() =>
        datenbank.Helferprofile.Include(p => p.Skills).ThenInclude(s => s.Kategorie);

    private static HelferprofilErgebnis Fehler(HelferprofilFehler fehler) => new(null, fehler);

    private static string? Leer(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    /// <summary>Preisangaben eines gespeicherten Profils, zum Beispiel für den Richtpreis in Vorschlägen.</summary>
    public static Preisangaben Preise(Helferprofil p) =>
        new(p.AnfahrtChf, p.StundenansatzChf, p.NachtzuschlagProzent, p.WochenendzuschlagProzent, p.MwstPflichtig);

    private static HelferprofilInfo Info(Helferprofil p) => new(
        p.Id,
        p.Typ,
        p.Betriebsname,
        p.Beschreibung,
        p.Postleitzahl,
        p.Ort,
        p.EinsatzradiusKm,
        p.Einsatzbereit,
        p.Verifizierung,
        p.Partner,
        p.Bewertungsschnitt,
        p.AnzahlBewertungen,
        p.AnzahlEinsaetze,
        p.Skills
            .OrderBy(s => s.Kategorie?.Reihenfolge ?? 0)
            .ThenBy(s => s.Bezeichnung, StringComparer.Ordinal)
            .Select(s => new SkillInfo(s.Id, s.Kennung, s.Bezeichnung, s.Kategorie?.Kostenpflichtig ?? false))
            .ToList(),
        p.Adresse,
        Preise(p),
        p.Skills.Any(s => s.Kategorie?.Kostenpflichtig == true));
}
