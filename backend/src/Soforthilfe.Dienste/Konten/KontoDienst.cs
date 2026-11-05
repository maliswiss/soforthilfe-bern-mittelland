using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Mandanten;
using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Regionen;

namespace Soforthilfe.Dienste.Konten;

/// <summary>
/// Registrierung und Anmeldung. Selbst registrieren können sich nur Hilfesuchende und Helfende.
/// </summary>
public sealed class KontoDienst(
    SoforthilfeDbContext datenbank,
    IMandantenKontext mandant,
    IRegionDienst regionen,
    PasswortDienst passwortDienst) : IKontoDienst
{
    public async Task<KontoErgebnis> RegistrierenAsync(RegistrierungsDaten daten, CancellationToken abbruch)
    {
        // Ein Konto gehört immer zur Region der Anfrage.
        if (mandant.RegionId is not Guid region)
        {
            return new KontoErgebnis(null, KontoFehler.RegionFehlt);
        }

        // Telefonnummer ist freiwillig. Kostenpflichtige Mehrwertnummern werden in jeder Schreibweise abgelehnt
        // (Anforderung A10), alle anderen Angaben müssen eine Schweizer Nummer sein.
        string? telefon = null;
        if (!string.IsNullOrWhiteSpace(daten.Telefon))
        {
            if (Telefonnummer.IstKostenpflichtig(daten.Telefon))
            {
                return new KontoErgebnis(null, KontoFehler.KostenpflichtigeNummer);
            }

            telefon = Telefonnummer.Normieren(daten.Telefon);
            if (telefon is null)
            {
                return new KontoErgebnis(null, KontoFehler.TelefonUngueltig);
            }
        }

        if (daten.Rolle is not (Rollen.Hilfesuchende or Rollen.Helfende))
        {
            return new KontoErgebnis(null, KontoFehler.RolleNichtErlaubt);
        }

        var eMail = Normiere(daten.EMail);

        // Die E-Mail-Adresse ist plattformweit eindeutig, darum wird hier ohne Regionsfilter gesucht.
        if (await datenbank.Konten.IgnoreQueryFilters().AnyAsync(k => k.EMail == eMail, abbruch))
        {
            return new KontoErgebnis(null, KontoFehler.EMailVergeben);
        }

        var konto = new Konto
        {
            Id = Guid.NewGuid(),
            RegionId = region,
            EMail = eMail,
            Anzeigename = daten.Anzeigename.Trim(),
            Telefon = telefon,
            Rolle = Enum.Parse<Rolle>(daten.Rolle),
            PasswortHash = passwortDienst.Hash(daten.Passwort),
            ErstelltAm = DateTimeOffset.UtcNow,
        };

        datenbank.Konten.Add(konto);
        await datenbank.SaveChangesAsync(abbruch);

        var aktuelle = regionen.Aktuelle;
        return new KontoErgebnis(
            new KontoInfo(konto.Id, konto.EMail, konto.Anzeigename, konto.Telefon, konto.Rolle.ToString(),
                aktuelle?.Kennung, aktuelle?.Name, false),
            null);
    }

    public async Task<KontoInfo?> AnmeldenAsync(string eMail, string passwort, string? region, CancellationToken abbruch)
    {
        var normiert = Normiere(eMail);

        // Die Anmeldung kennt die Region noch nicht; die Adresse ist plattformweit eindeutig.
        var konto = await datenbank.Konten
            .IgnoreQueryFilters()
            .Include(k => k.Region)
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.EMail == normiert, abbruch);

        // Immer einen Hash prüfen, auch ohne Konto, damit die Antwortzeit gleich bleibt.
        var stimmt = passwortDienst.Stimmt(konto?.PasswortHash ?? passwortDienst.Blindwert, passwort);

        if (konto is null || konto.PasswortHash is null || !stimmt || konto.Region is { Aktiv: false })
        {
            return null;
        }

        // Anfragen gelten nur in der Region des Kontos (Anforderung A18). Wer eine fremde Region wählt,
        // erhält dieselbe Antwort wie bei einem falschen Passwort. Der Plattformbetrieb hat keine Region.
        if (konto.Region is not null && region is not null && !string.Equals(konto.Region.Kennung, Normiere(region), StringComparison.Ordinal))
        {
            return null;
        }

        var hatProfil = await datenbank.Helferprofile.IgnoreQueryFilters().AnyAsync(h => h.KontoId == konto.Id, abbruch);
        return Info(konto, hatProfil);
    }

    public async Task<KontoInfo?> LiesAsync(Guid id, CancellationToken abbruch)
    {
        // Mit Regionsfilter: Ein Konto einer fremden Region wird nicht gefunden.
        var konto = await datenbank.Konten
            .Include(k => k.Region)
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == id, abbruch);

        if (konto is null)
        {
            return null;
        }

        var hatProfil = await datenbank.Helferprofile.AnyAsync(h => h.KontoId == konto.Id, abbruch);
        return Info(konto, hatProfil);
    }

    public async Task<IReadOnlyList<KontoInfo>> ListeAsync(CancellationToken abbruch)
    {
        // Der Regionsfilter sorgt dafür, dass nur Konten der eigenen Region erscheinen.
        var liste = await datenbank.Konten
            .Include(k => k.Region)
            .AsNoTracking()
            .OrderBy(k => k.Anzeigename)
            .ToListAsync(abbruch);

        var mitProfil = (await datenbank.Helferprofile.Select(h => h.KontoId).ToListAsync(abbruch)).ToHashSet();
        return liste.Select(k => Info(k, mitProfil.Contains(k.Id))).ToList();
    }

    public async Task<RollenErgebnis> RolleSetzenAsync(Guid id, string rolle, CancellationToken abbruch)
    {
        if (!Rollen.Vergebbar.Contains(rolle))
        {
            return RollenErgebnis.NichtErlaubt;
        }

        // Mit Regionsfilter: Konten einer fremden Region gelten als nicht vorhanden.
        var konto = await datenbank.Konten.FirstOrDefaultAsync(k => k.Id == id, abbruch);
        if (konto is null)
        {
            return RollenErgebnis.NichtGefunden;
        }

        // Konten ohne Region (Plattformbetrieb) werden über die Schnittstelle nicht verändert.
        if (konto.RegionId is null)
        {
            return RollenErgebnis.NichtErlaubt;
        }

        konto.Rolle = Enum.Parse<Rolle>(rolle);
        await datenbank.SaveChangesAsync(abbruch);
        return RollenErgebnis.Gesetzt;
    }

    private static KontoInfo Info(Konto konto, bool hatProfil) => new(
        konto.Id,
        konto.EMail,
        konto.Anzeigename,
        konto.Telefon,
        konto.Rolle.ToString(),
        konto.Region?.Kennung,
        konto.Region?.Name,
        hatProfil);

    private static string Normiere(string text) => text.Trim().ToLowerInvariant();
}
