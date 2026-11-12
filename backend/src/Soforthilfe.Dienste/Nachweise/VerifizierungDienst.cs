using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Protokoll;

namespace Soforthilfe.Dienste.Nachweise;

/// <summary>
/// Nachweise zum Helferprofil. Angenommen werden nur überprüfbare Angaben: die UID-Nummer und der Eintrag im
/// Handelsregister, nachgewiesen über die UID. Ausweisdokumente oder Dateien nimmt die Plattform nicht entgegen.
/// </summary>
public sealed class VerifizierungDienst(SoforthilfeDbContext datenbank, IProtokollDienst protokoll) : IVerifizierungDienst
{
    // Nachweisarten, die dieser Dienst prüft. Weitere Arten kommen mit eigenen Prüfungen hinzu.
    public static readonly IReadOnlySet<Nachweisart> Unterstuetzt =
        new HashSet<Nachweisart> { Nachweisart.UidNummer, Nachweisart.Handelsregister };

    public async Task<VerifizierungErgebnis> EinreichenAsync(Guid kontoId, Nachweisart art, string? nachweiswert,
        CancellationToken abbruch)
    {
        var profil = await datenbank.Helferprofile
            .Include(p => p.Verifizierungen)
            .FirstOrDefaultAsync(p => p.KontoId == kontoId, abbruch);
        if (profil is null)
        {
            return Fehler(VerifizierungFehler.KeinHelferprofil);
        }

        // Ein gesperrtes Profil hebt die Sperre nicht selbst auf.
        if (profil.Verifizierung == Verifizierungsstatus.Gesperrt)
        {
            return Fehler(VerifizierungFehler.Gesperrt);
        }

        if (!Unterstuetzt.Contains(art))
        {
            return Fehler(VerifizierungFehler.ArtNichtUnterstuetzt);
        }

        if (string.IsNullOrWhiteSpace(nachweiswert))
        {
            return Fehler(VerifizierungFehler.NachweiswertFehlt);
        }

        // UID und Handelsregister: gültiges Format und Prüfziffer, gespeichert in der Form «CHE-123.456.788».
        var wert = Uid.Normalisieren(nachweiswert);
        if (wert is null)
        {
            return Fehler(VerifizierungFehler.UidUngueltig);
        }

        // Ein noch offener Nachweis derselben Art wird ersetzt statt verdoppelt.
        var jetzt = DateTimeOffset.UtcNow;
        var nachweis = profil.Verifizierungen.FirstOrDefault(v => v.Art == art && v.Status == Verifizierungsstatus.InPruefung);
        if (nachweis is null)
        {
            nachweis = new Verifizierung
            {
                Id = Guid.NewGuid(),
                RegionId = profil.RegionId,
                HelferprofilId = profil.Id,
                Art = art,
            };
            datenbank.Verifizierungen.Add(nachweis);
        }

        nachweis.Nachweiswert = wert;
        nachweis.Status = Verifizierungsstatus.InPruefung;
        nachweis.EingereichtAm = jetzt;

        profil.Verifizierung = Verifizierungsstatus.InPruefung;
        await datenbank.SaveChangesAsync(abbruch);

        // Im Protokoll stehen nur die Art und die Kennung, nicht der Wert.
        await protokoll.SchreibenAsync("Verifizierung.Eingereicht", "Verifizierung", nachweis.Id.ToString(),
            art.ToString(), abbruch);

        var info = (await MeineAsync(kontoId, abbruch)).First(v => v.Id == nachweis.Id);
        return new VerifizierungErgebnis(info, null);
    }

    public async Task<IReadOnlyList<VerifizierungInfo>> MeineAsync(Guid kontoId, CancellationToken abbruch) =>
        await datenbank.Verifizierungen
            .AsNoTracking()
            .Where(v => v.Helferprofil!.KontoId == kontoId)
            .OrderByDescending(v => v.EingereichtAm)
            .Select(v => new VerifizierungInfo(v.Id, v.HelferprofilId, v.Helferprofil!.Konto!.Anzeigename, v.Art,
                v.Nachweiswert, v.Status, v.Bemerkung, v.EingereichtAm, v.GeprueftAm))
            .ToListAsync(abbruch);

    private static VerifizierungErgebnis Fehler(VerifizierungFehler fehler) => new(null, fehler);
}
