using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Konten;
using Soforthilfe.Dienste.Protokoll;

namespace Soforthilfe.Dienste.Nachweise;

/// <summary>
/// Nachweise zum Helferprofil. Angenommen werden nur überprüfbare Angaben: UID-Nummer, Eintrag im Handelsregister
/// (über die UID), Telefonnummer, E-Mail-Adresse des Kontos und eine Referenz. Ausweisdokumente oder Dateien nimmt
/// die Plattform nicht entgegen. Bestätigt werden Telefonnummer und E-Mail-Adresse durch die Koordination,
/// zum Beispiel mit einem Rückruf oder einer Antwort auf ihre Nachricht.
/// </summary>
public sealed class VerifizierungDienst(SoforthilfeDbContext datenbank, IProtokollDienst protokoll) : IVerifizierungDienst
{
    public async Task<VerifizierungErgebnis> EinreichenAsync(Guid kontoId, Nachweisart art, string? nachweiswert,
        CancellationToken abbruch)
    {
        var profil = await datenbank.Helferprofile
            .Include(p => p.Konto)
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

        if (!Enum.IsDefined(art))
        {
            return Fehler(VerifizierungFehler.ArtNichtUnterstuetzt);
        }

        var (wert, fehler) = WertPruefen(art, nachweiswert, profil.Konto!);
        if (fehler is not null)
        {
            return Fehler(fehler.Value);
        }

        // Ein noch offener Nachweis derselben Art wird ersetzt statt verdoppelt.
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
        nachweis.EingereichtAm = DateTimeOffset.UtcNow;

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

    // Die Region ergibt sich aus dem Filter des Datenbankkontexts: die Koordination sieht nur ihre Region.
    public async Task<IReadOnlyList<PruefungInfo>> OffeneAsync(CancellationToken abbruch) =>
        await datenbank.Verifizierungen
            .AsNoTracking()
            .Where(v => v.Status == Verifizierungsstatus.InPruefung)
            .OrderBy(v => v.EingereichtAm)
            .Select(v => new PruefungInfo(
                new VerifizierungInfo(v.Id, v.HelferprofilId, v.Helferprofil!.Konto!.Anzeigename, v.Art,
                    v.Nachweiswert, v.Status, v.Bemerkung, v.EingereichtAm, v.GeprueftAm),
                v.Helferprofil!.Typ,
                v.Helferprofil!.Betriebsname,
                v.Helferprofil!.Postleitzahl,
                v.Helferprofil!.Ort,
                v.Helferprofil!.Verifizierung))
            .ToListAsync(abbruch);

    public async Task<EntscheidErgebnis> EntscheidenAsync(Guid nachweisId, Guid pruefendeId, bool freigeben,
        string? bemerkung, CancellationToken abbruch)
    {
        // Ein Nachweis einer anderen Region wird vom Filter ausgeblendet und gilt als nicht vorhanden.
        var nachweis = await datenbank.Verifizierungen
            .Include(v => v.Helferprofil!)
            .ThenInclude(p => p.Verifizierungen)
            .FirstOrDefaultAsync(v => v.Id == nachweisId, abbruch);
        if (nachweis is null)
        {
            return new EntscheidErgebnis(null, EntscheidFehler.NichtGefunden);
        }

        if (nachweis.Status != Verifizierungsstatus.InPruefung)
        {
            return new EntscheidErgebnis(null, EntscheidFehler.BereitsEntschieden);
        }

        nachweis.Status = freigeben ? Verifizierungsstatus.Verifiziert : Verifizierungsstatus.Abgelehnt;
        nachweis.Bemerkung = string.IsNullOrWhiteSpace(bemerkung) ? null : bemerkung.Trim();
        nachweis.GeprueftVonId = pruefendeId;
        nachweis.GeprueftAm = DateTimeOffset.UtcNow;

        var profil = nachweis.Helferprofil!;
        profil.Verifizierung = Profilstatus(profil, nachweis);
        await datenbank.SaveChangesAsync(abbruch);

        // Im Protokoll stehen Entscheid und Art, nicht der Wert und nicht die Bemerkung.
        await protokoll.SchreibenAsync(freigeben ? "Verifizierung.Freigegeben" : "Verifizierung.Abgelehnt",
            "Verifizierung", nachweis.Id.ToString(), nachweis.Art.ToString(), abbruch);

        var info = await datenbank.Verifizierungen
            .AsNoTracking()
            .Where(v => v.Id == nachweis.Id)
            .Select(v => new VerifizierungInfo(v.Id, v.HelferprofilId, v.Helferprofil!.Konto!.Anzeigename, v.Art,
                v.Nachweiswert, v.Status, v.Bemerkung, v.EingereichtAm, v.GeprueftAm))
            .FirstAsync(abbruch);
        return new EntscheidErgebnis(info, null);
    }

    /// <summary>
    /// Stand des Profils nach einem Entscheid. Eine Sperre bleibt bestehen. Mit einer Freigabe ist das Profil
    /// verifiziert. Nach einer Ablehnung bleibt es in Prüfung, solange weitere Nachweise offen sind, und bleibt
    /// verifiziert, wenn schon ein anderer Nachweis freigegeben ist; sonst ist es abgelehnt.
    /// </summary>
    public static Verifizierungsstatus Profilstatus(Helferprofil profil, Verifizierung entschieden)
    {
        if (profil.Verifizierung == Verifizierungsstatus.Gesperrt)
        {
            return Verifizierungsstatus.Gesperrt;
        }

        if (entschieden.Status == Verifizierungsstatus.Verifiziert)
        {
            return Verifizierungsstatus.Verifiziert;
        }

        var andere = profil.Verifizierungen.Where(v => v.Id != entschieden.Id).ToList();
        if (andere.Any(v => v.Status == Verifizierungsstatus.InPruefung))
        {
            return Verifizierungsstatus.InPruefung;
        }

        return andere.Any(v => v.Status == Verifizierungsstatus.Verifiziert)
            ? Verifizierungsstatus.Verifiziert
            : Verifizierungsstatus.Abgelehnt;
    }

    /// <summary>Prüft die Angabe je nach Art und liefert sie in der gespeicherten Form.</summary>
    private static (string? Wert, VerifizierungFehler? Fehler) WertPruefen(Nachweisart art, string? eingabe, Konto konto)
    {
        switch (art)
        {
            case Nachweisart.UidNummer:
            case Nachweisart.Handelsregister:
                // Gültiges Format und Prüfziffer, gespeichert in der Form «CHE-123.456.788».
                if (string.IsNullOrWhiteSpace(eingabe))
                {
                    return (null, VerifizierungFehler.NachweiswertFehlt);
                }

                var uid = Uid.Normalisieren(eingabe);
                return uid is null ? (null, VerifizierungFehler.UidUngueltig) : (uid, null);

            case Nachweisart.TelefonBestaetigt:
                // Ohne Angabe gilt die Telefonnummer des Kontos. Mehrwertnummern nie (Anforderung A10).
                var nummer = string.IsNullOrWhiteSpace(eingabe) ? konto.Telefon : eingabe;
                if (string.IsNullOrWhiteSpace(nummer))
                {
                    return (null, VerifizierungFehler.NachweiswertFehlt);
                }

                if (Telefonnummer.IstKostenpflichtig(nummer))
                {
                    return (null, VerifizierungFehler.Mehrwertnummer);
                }

                var telefon = Nachweiswerte.Telefon(nummer);
                return telefon is null ? (null, VerifizierungFehler.TelefonUngueltig) : (telefon, null);

            case Nachweisart.EMailBestaetigt:
                // Bestätigt wird immer die Adresse des Kontos, keine zweite Adresse.
                if (!string.IsNullOrWhiteSpace(eingabe)
                    && !string.Equals(eingabe.Trim(), konto.EMail, StringComparison.OrdinalIgnoreCase))
                {
                    return (null, VerifizierungFehler.EMailNichtKonto);
                }

                return (konto.EMail, null);

            default:
                // Referenz einer Organisation oder Gemeinde mit Kontaktperson.
                if (string.IsNullOrWhiteSpace(eingabe))
                {
                    return (null, VerifizierungFehler.NachweiswertFehlt);
                }

                var referenz = Nachweiswerte.Referenz(eingabe);
                return referenz is null ? (null, VerifizierungFehler.ReferenzUngueltig) : (referenz, null);
        }
    }

    private static VerifizierungErgebnis Fehler(VerifizierungFehler fehler) => new(null, fehler);
}
