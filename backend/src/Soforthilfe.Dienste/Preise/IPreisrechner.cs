namespace Soforthilfe.Dienste.Preise;

/// <summary>
/// Richtpreis für die erste Stunde aus den Preisangaben (Anforderung A9).
/// </summary>
public interface IPreisrechner
{
    decimal MwstSatzProzent { get; }

    /// <summary>Preis zum Zeitpunkt; null bei kostenlosen Leistungen.</summary>
    PreisInfo? Berechnen(Preisangaben angaben, bool bezahlpflichtig, DateTimeOffset zeitpunkt);
}
