using System.ComponentModel.DataAnnotations;
using Soforthilfe.Dienste.Preise;

namespace Soforthilfe.Api.Modelle;

/// <summary>
/// Preisangaben zur Berechnung eines Richtpreises, zum Beispiel für die Vorschau im Helferprofil.
/// Ohne Zeitpunkte gilt der aktuelle; berechnet werden höchstens zehn Zeitpunkte.
/// </summary>
public sealed record RichtpreisAnfrage(
    [Range(0, 500)] decimal? AnfahrtChf,
    [Range(0, 500)] decimal? StundenansatzChf,
    [Range(0, 200)] int? NachtzuschlagProzent,
    [Range(0, 200)] int? WochenendzuschlagProzent,
    bool MwstPflichtig = false,
    IReadOnlyList<DateTimeOffset>? Zeitpunkte = null);

/// <summary>
/// Preis zu einem Zeitpunkt (Anforderung A9). Der Richtpreis gilt für die erste Stunde inklusive Anfahrt und, falls
/// zutreffend, Zuschlag und MWST. Nacht- und Wochenendzuschlag werden nicht addiert; es gilt der höhere.
/// </summary>
public sealed record PreisAntwort(
    decimal? AnfahrtChf,
    decimal? StundenansatzChf,
    int? NachtzuschlagProzent,
    int? WochenendzuschlagProzent,
    bool MwstPflichtig,
    decimal MwstSatzProzent,
    decimal? RichtpreisErsteStunde,
    string? AngewandterZuschlag,
    int AngewandterZuschlagProzent)
{
    public static PreisAntwort Aus(PreisInfo p) => new(p.AnfahrtChf, p.StundenansatzChf, p.NachtzuschlagProzent,
        p.WochenendzuschlagProzent, p.MwstPflichtig, p.MwstSatzProzent, p.RichtpreisErsteStunde,
        p.AngewandterZuschlag, p.AngewandterZuschlagProzent);
}
