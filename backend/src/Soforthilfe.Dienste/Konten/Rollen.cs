namespace Soforthilfe.Dienste.Konten;

/// <summary>
/// Namen der Rollen, wie sie im Token und in den Berechtigungsprüfungen stehen.
/// </summary>
public static class Rollen
{
    public const string Hilfesuchende = "Hilfesuchende";
    public const string Helfende = "Helfende";
    public const string Koordination = "Koordination";
    public const string Mandantenadministration = "Mandantenadministration";
    public const string Plattformbetrieb = "Plattformbetrieb";

    // Rollengruppen für die Berechtigungsprüfung an den Schnittstellen (durch Komma getrennt).
    public const string KontenEinsehen = Koordination + "," + Mandantenadministration + "," + Plattformbetrieb;
    public const string RollenVergeben = Mandantenadministration + "," + Plattformbetrieb;
    public const string ProtokollEinsehen = KontenEinsehen;

    // Über die Schnittstelle vergebbar. Der Plattformbetrieb ist an keine Region gebunden und wird nicht vergeben.
    public static readonly IReadOnlySet<string> Vergebbar =
        new HashSet<string> { Hilfesuchende, Helfende, Koordination, Mandantenadministration };
}
