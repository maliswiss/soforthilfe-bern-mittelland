namespace Soforthilfe.Dienste.Preise;

/// <summary>
/// Preisangaben einer helfenden Person oder eines Betriebs (Anforderung A9). Beträge in Franken, Zuschläge in Prozent
/// auf den Stundenansatz.
/// </summary>
public sealed record Preisangaben(
    decimal? AnfahrtChf,
    decimal? StundenansatzChf,
    int? NachtzuschlagProzent,
    int? WochenendzuschlagProzent,
    bool MwstPflichtig)
{
    // Für bezahlpflichtige Leistungen sind Anfahrt und Stundenansatz nötig.
    public bool Vollstaendig => AnfahrtChf is not null && StundenansatzChf is not null;
}

/// <summary>
/// Preis zu einem Zeitpunkt, wie ihn Anfragende vor der Zusage sehen. Der Richtpreis gilt für die erste Stunde
/// inklusive Anfahrt, Zuschlag und, falls zutreffend, MWST; null, wenn Angaben fehlen.
/// <c>AngewandterZuschlag</c> ist «nacht», «wochenende» oder leer.
/// </summary>
public sealed record PreisInfo(
    decimal? AnfahrtChf,
    decimal? StundenansatzChf,
    int? NachtzuschlagProzent,
    int? WochenendzuschlagProzent,
    bool MwstPflichtig,
    decimal MwstSatzProzent,
    decimal? RichtpreisErsteStunde,
    string? AngewandterZuschlag,
    int AngewandterZuschlagProzent);

/// <summary>
/// Einstellungen der Preisberechnung, aus der Konfiguration (Abschnitt «Preise»).
/// </summary>
public sealed record PreisEinstellungen(decimal MwstSatzProzent = 8.1m, int NachtVonStunde = 20, int NachtBisStunde = 7);
