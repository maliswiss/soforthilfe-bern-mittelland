using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Dienste.Matching;

/// <summary>
/// Suchradius einer Anfrage nach Dringlichkeit: Je dringender, desto weiter wird gesucht, damit schnell jemand
/// gefunden wird. Bei tiefer Dringlichkeit bleibt die Hilfe in der Nähe.
/// </summary>
public static class Suchradius
{
    public const int HochKm = 15;
    public const int MittelKm = 10;
    public const int NiedrigKm = 5;

    public static int FuerPrioritaet(Prioritaet prioritaet) => prioritaet switch
    {
        Prioritaet.Hoch => HochKm,
        Prioritaet.Mittel => MittelKm,
        _ => NiedrigKm,
    };

    /// <summary>
    /// Liegt eine helfende Person im Einsatzgebiet? Die Distanz muss im Suchradius der Anfrage und im
    /// Einsatzradius des Profils liegen; beide Grenzen gelten einschliesslich.
    /// </summary>
    public static bool ImEinsatzgebiet(double distanzMeter, int suchradiusKm, int einsatzradiusKm) =>
        distanzMeter >= 0 && distanzMeter <= suchradiusKm * 1000.0 && distanzMeter <= einsatzradiusKm * 1000.0;
}
