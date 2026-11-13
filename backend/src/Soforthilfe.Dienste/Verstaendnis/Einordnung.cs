using System.Globalization;
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
/// Ordnet eine Anfrage einer Kategorie zu: zuerst die Wahl der anfragenden Person, dann eindeutige Schlüsselwörter,
/// danach der TF-IDF-Klassifikator. Die Notfallprüfung läuft vorher und gehört nicht dazu.
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

    public static string MaschinellesLernen(double aehnlichkeit, string kategorie, double abstand) =>
        string.Format(Kultur, "Klassifikation über maschinelles Lernen: Ähnlichkeit {0:0.00} zur Kategorie «{1}», "
            + "Abstand zur zweitbesten Kategorie {2:0.00}.", aehnlichkeit, kategorie, abstand);

    private static readonly CultureInfo Kultur = CultureInfo.GetCultureInfo("de-CH");
}

public sealed class Einordnung(SoforthilfeDbContext datenbank, ITextKlassifikator klassifikator) : IEinordnung
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

        // Stufe 3: maschinelles Lernen, nur mit genügend Ähnlichkeit und Abstand zur zweitbesten Kategorie.
        var ml = klassifikator.Klassifizieren(freitext);
        if (ml.Kennung is not null && kategorien.FirstOrDefault(k => k.Kennung == ml.Kennung) is { } ueberMl)
        {
            return Ergebnis(ueberMl, Erfassungsweg.MaschinellesLernen, ml.Aehnlichkeit,
                EinordnungTexte.MaschinellesLernen(ml.Aehnlichkeit, ueberMl.Bezeichnung, ml.Abstand));
        }

        // Unsicher: die bis zu drei ähnlichsten Kategorien zur Auswahl anbieten.
        var alternativen = ml.Rangfolge
            .Take(3)
            .Select(a => kategorien.FirstOrDefault(x => x.Kennung == a.Kennung) is { } k
                ? new KategorieVorschlag(k.Id, k.Kennung, k.Bezeichnung, a.Aehnlichkeit)
                : null)
            .OfType<KategorieVorschlag>()
            .ToList();
        return Ohne(EinordnungTexte.Unsicher) with { Alternativen = alternativen };
    }

    private static EinordnungInfo Ergebnis(Kategorie kategorie, Erfassungsweg weg, double konfidenz, string begruendung) =>
        new(kategorie.Id, kategorie.Kennung, kategorie.Bezeichnung, kategorie.Kostenpflichtig, weg, konfidenz,
            begruendung, []);

    private static EinordnungInfo Ohne(string begruendung) =>
        new(null, null, null, false, null, null, begruendung, []);
}
