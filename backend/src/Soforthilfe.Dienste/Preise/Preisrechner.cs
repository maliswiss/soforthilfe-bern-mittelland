namespace Soforthilfe.Dienste.Preise;

/// <summary>
/// Richtpreis für die erste Stunde: Anfahrt plus Stundenansatz, bei Nacht (Standard 20 bis 7 Uhr) oder am Wochenende
/// (Samstag, Sonntag) mit Zuschlag auf den Stundenansatz. Gelten beide, zählt der höhere Zuschlag; Zuschläge werden
/// nicht addiert. MWST kommt hinzu, wenn die helfende Seite mehrwertsteuerpflichtig ist. Massgebend ist die
/// Schweizer Ortszeit; gerundet wird auf 5 Rappen.
/// </summary>
public sealed class Preisrechner(PreisEinstellungen einstellungen) : IPreisrechner
{
    private static readonly TimeZoneInfo Zeitzone = ZeitzoneSchweiz();

    public decimal MwstSatzProzent => einstellungen.MwstSatzProzent;

    public PreisInfo? Berechnen(Preisangaben angaben, bool bezahlpflichtig, DateTimeOffset zeitpunkt)
    {
        if (!bezahlpflichtig)
        {
            return null;
        }

        var ortszeit = TimeZoneInfo.ConvertTime(zeitpunkt, Zeitzone);
        var nacht = ortszeit.Hour >= einstellungen.NachtVonStunde || ortszeit.Hour < einstellungen.NachtBisStunde;
        var wochenende = ortszeit.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

        var nachtzuschlag = nacht ? angaben.NachtzuschlagProzent ?? 0 : 0;
        var wochenendzuschlag = wochenende ? angaben.WochenendzuschlagProzent ?? 0 : 0;

        // Es gilt der höhere Zuschlag.
        var zuschlag = Math.Max(nachtzuschlag, wochenendzuschlag);
        string? art = zuschlag == 0 ? null : nachtzuschlag >= wochenendzuschlag ? "nacht" : "wochenende";

        decimal? richtpreis = null;
        if (angaben.Vollstaendig)
        {
            var netto = angaben.AnfahrtChf!.Value + angaben.StundenansatzChf!.Value * (1 + zuschlag / 100m);
            var brutto = angaben.MwstPflichtig ? netto * (1 + MwstSatzProzent / 100m) : netto;
            richtpreis = Runden(brutto);
        }

        return new PreisInfo(angaben.AnfahrtChf, angaben.StundenansatzChf, angaben.NachtzuschlagProzent,
            angaben.WochenendzuschlagProzent, angaben.MwstPflichtig, MwstSatzProzent, richtpreis, art, zuschlag);
    }

    /// <summary>Auf 5 Rappen gerundet, wie in der Schweiz üblich.</summary>
    public static decimal Runden(decimal betrag) =>
        Math.Round(betrag * 20m, MidpointRounding.AwayFromZero) / 20m;

    private static TimeZoneInfo ZeitzoneSchweiz()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Zurich");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        }
    }
}
