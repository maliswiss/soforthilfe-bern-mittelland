namespace Soforthilfe.Dienste.Protokoll;

/// <summary>
/// Feste Namen der protokollierten Vorgänge und der betroffenen Bereiche.
/// </summary>
public static class ProtokollAktionen
{
    public const string NotfallStopp = "Notfall.Stopp";

    // Weg der Einordnung einer neuen Anfrage, ohne Freitext und ohne Personendaten.
    public const string AnfrageEingeordnet = "Anfrage.Eingeordnet";

    // Vorschläge des Smart-Matchings, nur mit Anzahl.
    public const string AnfrageVorgeschlagen = "Anfrage.Vorgeschlagen";

    public const string ObjektVerstaendnis = "Verstaendnis";
    public const string ObjektAnfrage = "Anfrage";
}
