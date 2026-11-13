using System.ComponentModel.DataAnnotations;
using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Verstaendnis;

namespace Soforthilfe.Api.Modelle;

/// <summary>
/// Eingabe für eine neue Anfrage: Kategorie, Freitext oder beides, dazu Postleitzahl und Ort des Einsatzes.
/// Prioritaet: 0 tief, 1 mittel (Standard), 2 hoch.
/// </summary>
public sealed record AnfrageErstellen(
    Guid? KategorieId,
    [StringLength(2000)] string? Freitext,
    [Required, StringLength(10)] string Plz,
    [Required, StringLength(120)] string Ort,
    [StringLength(250)] string? Adresse = null,
    [StringLength(120)] string? Zeitfenster = null,
    int Prioritaet = 1);

/// <summary>
/// Eingabe für den Verständnis-Schritt vor dem Erfassen: Kategorie, Freitext oder beides.
/// </summary>
public sealed record VerstaendnisAnfrage(Guid? KategorieId, [StringLength(2000)] string? Freitext);

/// <summary>
/// Antwort des Verständnis-Schritts. Bei «Notfall» ist die Vermittlung gestoppt und die Einordnung bleibt leer.
/// Sonst nennt sie Kategorie mit Code, Weg (0 Kategorieauswahl, 1 Regeln, 2 maschinelles Lernen, 3 Sprachmodell),
/// Sicherheit und Begründung; ohne sicheres Ergebnis bietet sie Alternativen zur Auswahl an.
/// </summary>
public sealed record VerstaendnisAntwort(
    bool Notfall,
    string? NotfallGrund,
    string? NotfallHinweis,
    Guid? KategorieId = null,
    string? Kategorie = null,
    string? Code = null,
    bool Bezahlpflichtig = false,
    Erfassungsweg? Erfassungsweg = null,
    double? Konfidenz = null,
    string? Begruendung = null,
    IReadOnlyList<KategorieVorschlagAntwort>? Alternativen = null)
{
    public static VerstaendnisAntwort Aus(VerstaendnisInfo v) => v.Einordnung is not { } e
        ? new(v.Notfall, v.NotfallGrund, v.NotfallHinweis, Alternativen: [])
        : new(v.Notfall, v.NotfallGrund, v.NotfallHinweis, e.KategorieId, e.Bezeichnung, e.Kennung, e.Kostenpflichtig,
            e.Weg, e.Konfidenz, e.Begruendung,
            e.Alternativen.Select(a => new KategorieVorschlagAntwort(a.KategorieId, a.Bezeichnung, a.Kennung, a.Aehnlichkeit)).ToList());
}

/// <summary>
/// Kategorie zur Auswahl, wenn die Einordnung unsicher ist, mit der Ähnlichkeit zum Freitext.
/// </summary>
public sealed record KategorieVorschlagAntwort(Guid KategorieId, string Kategorie, string Code, double Aehnlichkeit);
