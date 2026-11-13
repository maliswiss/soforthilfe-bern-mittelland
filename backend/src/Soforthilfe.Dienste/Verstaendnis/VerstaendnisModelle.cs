namespace Soforthilfe.Dienste.Verstaendnis;

/// <summary>
/// Ergebnis des Verständnis-Schritts. Bei einem gesundheitlichen Notfall nennt «NotfallGrund» das erkannte Wort
/// und «NotfallHinweis» den Verweis auf 144; die Vermittlung ist dann gestoppt und es gibt keine Einordnung.
/// </summary>
public sealed record VerstaendnisInfo(bool Notfall, string? NotfallGrund, string? NotfallHinweis, EinordnungInfo? Einordnung);

/// <summary>
/// Feste Texte des Notfall-Stopps. Die Oberfläche zeigt sie unverändert an.
/// </summary>
public static class NotfallTexte
{
    public const string Hinweis = "Bitte sofort 144 anrufen. Diese Plattform vermittelt keine medizinischen Leistungen.";

    public static string Grund(string? schluesselwort) => $"Erkanntes Schlüsselwort: {schluesselwort}";
}
