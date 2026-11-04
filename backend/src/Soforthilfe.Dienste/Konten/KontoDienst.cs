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

        // Telefonnummer ist freiwillig; wenn angegeben, muss sie eine Schweizer Nummer und darf keine Mehrwertnummer sein.
        string? telefon = null;
        if (!string.IsNullOrWhiteSpace(daten.Telefon))
        {
            telefon = Telefonnummer.Normieren(daten.Telefon);
            if (telefon is null)
            {
                return new KontoErgebnis(null, KontoFehler.TelefonUngueltig);
            }

            if (Telefonnummer.IstMehrwertnummer(telefon))
            {
                return new KontoErgebnis(null, KontoFehler.KostenpflichtigeNummer);
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

        return new KontoErgebnis(
            new KontoInfo(konto.Id, konto.EMail, konto.Anzeigename, konto.Rolle.ToString(), regionen.Aktuelle?.Kennung),
            null);
    }

    public async Task<KontoInfo?> AnmeldenAsync(string eMail, string passwort, CancellationToken abbruch)
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

        return new KontoInfo(konto.Id, konto.EMail, konto.Anzeigename, konto.Rolle.ToString(), konto.Region?.Kennung);
    }

    public async Task<KontoInfo?> LiesAsync(Guid id, CancellationToken abbruch)
    {
        // Mit Regionsfilter: Ein Konto einer fremden Region wird nicht gefunden.
        var konto = await datenbank.Konten
            .Include(k => k.Region)
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == id, abbruch);

        return konto is null
            ? null
            : new KontoInfo(konto.Id, konto.EMail, konto.Anzeigename, konto.Rolle.ToString(), konto.Region?.Kennung);
    }

    public async Task<IReadOnlyList<KontoInfo>> ListeAsync(CancellationToken abbruch)
    {
        // Der Regionsfilter sorgt dafür, dass nur Konten der eigenen Region erscheinen.
        var liste = await datenbank.Konten
            .Include(k => k.Region)
            .AsNoTracking()
            .OrderBy(k => k.Anzeigename)
            .ToListAsync(abbruch);

        return liste
            .Select(k => new KontoInfo(k.Id, k.EMail, k.Anzeigename, k.Rolle.ToString(), k.Region?.Kennung))
            .ToList();
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

    private static string Normiere(string eMail) => eMail.Trim().ToLowerInvariant();
}
