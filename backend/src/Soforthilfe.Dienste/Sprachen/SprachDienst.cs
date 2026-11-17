using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Protokoll;

namespace Soforthilfe.Dienste.Sprachen;

/// <summary>
/// Importierte Sprache mit Stand, ohne die Texte.
/// </summary>
public sealed record SprachInfo(string Code, string Name, bool Aktiv, int AnzahlTexte, DateTimeOffset GeaendertAm);

/// <summary>
/// Ergebnis eines Imports: die gespeicherte Sprache oder ein Fehler mit dem Pfad des betroffenen Texts.
/// </summary>
public sealed record SprachErgebnis(SprachInfo? Sprache, SprachdateiFehler? Fehler, string? Pfad);

/// <summary>
/// Sprachen der Oberfläche importieren und verwalten. Neue Sprachen und geänderte Texte gelten sofort, ohne
/// Neustart und ohne neue Version der Oberfläche, weil die Oberfläche sie zur Laufzeit lädt.
/// </summary>
public interface ISprachDienst
{
    Task<IReadOnlyList<SprachInfo>> ListeAsync(bool nurAktive, CancellationToken abbruch);

    /// <summary>Texte einer aktiven Sprache als JSON, sonst null.</summary>
    Task<string?> TexteAsync(string code, CancellationToken abbruch);

    Task<SprachErgebnis> ImportierenAsync(string code, string name, bool aktiv, JsonElement texte, Guid kontoId,
        CancellationToken abbruch);

    Task<SprachInfo?> AktivSetzenAsync(string code, bool aktiv, CancellationToken abbruch);

    Task<bool> LoeschenAsync(string code, CancellationToken abbruch);
}

public sealed class SprachDienst(SoforthilfeDbContext datenbank, IProtokollDienst protokoll) : ISprachDienst
{
    public async Task<IReadOnlyList<SprachInfo>> ListeAsync(bool nurAktive, CancellationToken abbruch) =>
        await datenbank.Sprachen
            .AsNoTracking()
            .Where(s => !nurAktive || s.Aktiv)
            .OrderBy(s => s.Code)
            .Select(s => new SprachInfo(s.Code, s.Name, s.Aktiv, s.AnzahlTexte, s.GeaendertAm))
            .ToListAsync(abbruch);

    public async Task<string?> TexteAsync(string code, CancellationToken abbruch) =>
        await datenbank.Sprachen
            .AsNoTracking()
            .Where(s => s.Code == code && s.Aktiv)
            .Select(s => s.Texte)
            .FirstOrDefaultAsync(abbruch);

    public async Task<SprachErgebnis> ImportierenAsync(string code, string name, bool aktiv, JsonElement texte,
        Guid kontoId, CancellationToken abbruch)
    {
        if (!Sprachdatei.CodeGueltig(code))
        {
            return new SprachErgebnis(null, SprachdateiFehler.CodeUngueltig, null);
        }

        if (!Sprachdatei.NameGueltig(name))
        {
            return new SprachErgebnis(null, SprachdateiFehler.NameUngueltig, null);
        }

        var pruefung = Sprachdatei.Pruefen(texte);
        if (pruefung.Fehler is not null)
        {
            return new SprachErgebnis(null, pruefung.Fehler, pruefung.Pfad);
        }

        // Ein erneuter Import ersetzt die Texte vollständig.
        var sprache = await datenbank.Sprachen.FirstOrDefaultAsync(s => s.Code == code, abbruch);
        if (sprache is null)
        {
            sprache = new Sprache { Code = code, Name = name.Trim(), Texte = "{}" };
            datenbank.Sprachen.Add(sprache);
        }

        sprache.Name = name.Trim();
        sprache.Aktiv = aktiv;
        sprache.Texte = texte.GetRawText();
        sprache.AnzahlTexte = pruefung.AnzahlTexte;
        sprache.GeaendertAm = DateTimeOffset.UtcNow;
        sprache.GeaendertVonId = kontoId;
        await datenbank.SaveChangesAsync(abbruch);

        await protokoll.SchreibenAsync("Sprache.Importiert", "Sprache", code,
            $"{pruefung.AnzahlTexte} Texte, {(aktiv ? "aktiv" : "inaktiv")}", abbruch);

        return new SprachErgebnis(Info(sprache), null, null);
    }

    public async Task<SprachInfo?> AktivSetzenAsync(string code, bool aktiv, CancellationToken abbruch)
    {
        var sprache = await datenbank.Sprachen.FirstOrDefaultAsync(s => s.Code == code, abbruch);
        if (sprache is null)
        {
            return null;
        }

        sprache.Aktiv = aktiv;
        sprache.GeaendertAm = DateTimeOffset.UtcNow;
        await datenbank.SaveChangesAsync(abbruch);
        await protokoll.SchreibenAsync(aktiv ? "Sprache.Aktiviert" : "Sprache.Deaktiviert", "Sprache", code, null, abbruch);
        return Info(sprache);
    }

    public async Task<bool> LoeschenAsync(string code, CancellationToken abbruch)
    {
        var sprache = await datenbank.Sprachen.FirstOrDefaultAsync(s => s.Code == code, abbruch);
        if (sprache is null)
        {
            return false;
        }

        datenbank.Sprachen.Remove(sprache);
        await datenbank.SaveChangesAsync(abbruch);
        await protokoll.SchreibenAsync("Sprache.Geloescht", "Sprache", code, null, abbruch);
        return true;
    }

    private static SprachInfo Info(Sprache s) => new(s.Code, s.Name, s.Aktiv, s.AnzahlTexte, s.GeaendertAm);
}
