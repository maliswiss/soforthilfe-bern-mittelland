using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Dienste.Verstaendnis;

/// <summary>
/// Kategorie, die für eine unsichere Einordnung zur Auswahl angeboten wird.
/// </summary>
public sealed record KategorieVorschlag(Guid KategorieId, string Kennung, string Bezeichnung, double Aehnlichkeit);

/// <summary>
/// Ergebnis der Einordnung. Ohne Kategorie nennt die Begründung, warum keine sicher bestimmt werden konnte.
/// </summary>
public sealed record EinordnungInfo(
    Guid? KategorieId,
    string? Kennung,
    string? Bezeichnung,
    bool Kostenpflichtig,
    Erfassungsweg? Weg,
    double? Konfidenz,
    string Begruendung,
    IReadOnlyList<KategorieVorschlag> Alternativen);

/// <summary>
/// Ordnet eine Anfrage einer Kategorie zu: zuerst die Wahl der anfragenden Person, dann eindeutige Schlüsselwörter.
/// Die Notfallprüfung läuft vorher und gehört nicht dazu.
/// </summary>
public interface IEinordnung
{
    Task<EinordnungInfo> EinordnenAsync(Guid? kategorieId, string? freitext, CancellationToken abbruch);
}

/// <summary>
/// Feste Begründungen der Einordnung. Die Oberfläche zeigt sie unverändert an.
/// </summary>
public static class EinordnungTexte
{
    public const string NichtsUebermittelt = "Weder eine Kategorie noch ein Freitext wurde übermittelt.";

    public const string Unsicher =
        "Es konnte keine Kategorie sicher bestimmt werden. Bitte wählen Sie eine Kategorie aus der Liste.";

    public static string Gewaehlt(string kategorie) => $"Kategorie durch die anfragende Person gewählt: {kategorie}.";

    public static string Regel(string wort) => $"Erkannt über das Schlüsselwort «{wort}».";
}

public sealed class Einordnung(SoforthilfeDbContext datenbank) : IEinordnung
{
    public async Task<EinordnungInfo> EinordnenAsync(Guid? kategorieId, string? freitext, CancellationToken abbruch)
    {
        var kategorien = await datenbank.Kategorien.AsNoTracking().ToListAsync(abbruch);

        // Stufe 1: die gewählte Kategorie gilt immer, auch wenn der Freitext etwas anderes nahelegt.
        if (kategorien.FirstOrDefault(k => k.Id == kategorieId) is { } gewaehlt)
        {
            return Ergebnis(gewaehlt, Erfassungsweg.Kategorieauswahl, 1.0, EinordnungTexte.Gewaehlt(gewaehlt.Bezeichnung));
        }

        if (string.IsNullOrWhiteSpace(freitext))
        {
            return Ohne(EinordnungTexte.NichtsUebermittelt);
        }

        // Stufe 2: eindeutige Schlüsselwörter. Die Begründung nennt das Wort so, wie es geschrieben wurde.
        var regel = Schluesselwortregeln.Anwenden(freitext);
        if (regel is not null && kategorien.FirstOrDefault(k => k.Kennung == regel.Kennung) is { } ueberRegel)
        {
            return Ergebnis(ueberRegel, Erfassungsweg.Regeln, regel.Konfidenz,
                EinordnungTexte.Regel(Textnormalisierung.Fundstelle(freitext, regel.Begriff)));
        }

        return Ohne(EinordnungTexte.Unsicher);
    }

    private static EinordnungInfo Ergebnis(Kategorie kategorie, Erfassungsweg weg, double konfidenz, string begruendung) =>
        new(kategorie.Id, kategorie.Kennung, kategorie.Bezeichnung, kategorie.Kostenpflichtig, weg, konfidenz,
            begruendung, []);

    private static EinordnungInfo Ohne(string begruendung) =>
        new(null, null, null, false, null, null, begruendung, []);
}
