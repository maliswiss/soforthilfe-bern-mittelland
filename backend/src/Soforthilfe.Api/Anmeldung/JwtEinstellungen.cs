using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Soforthilfe.Api.Anmeldung;

/// <summary>
/// Einstellungen für die Tokens, gelesen aus JWT_KEY, JWT_ISSUER und JWT_AUDIENCE.
/// </summary>
public sealed class JwtEinstellungen
{
    public required string Herausgeber { get; init; }

    public required string Zielgruppe { get; init; }

    public required SymmetricSecurityKey Schluessel { get; init; }

    public int LaufzeitMinuten { get; init; } = 60;

    public static JwtEinstellungen Lesen(IConfiguration konfiguration)
    {
        var schluessel = konfiguration["JWT_KEY"];

        // Ohne ausreichend langen Schlüssel startet die Anwendung nicht: HS256 braucht mindestens 256 Bit.
        // Platzhalter aus der Vorlage («<…>») gelten nicht als Schlüssel.
        if (string.IsNullOrWhiteSpace(schluessel)
            || schluessel.StartsWith('<')
            || Encoding.UTF8.GetByteCount(schluessel) < 32)
        {
            throw new InvalidOperationException(
                "JWT_KEY fehlt oder ist kürzer als 32 Zeichen. Den Wert in der Umgebung beziehungsweise in der lokalen .env setzen.");
        }

        return new JwtEinstellungen
        {
            Herausgeber = konfiguration["JWT_ISSUER"] ?? "soforthilfe",
            Zielgruppe = konfiguration["JWT_AUDIENCE"] ?? "soforthilfe",
            Schluessel = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(schluessel)),
        };
    }
}
