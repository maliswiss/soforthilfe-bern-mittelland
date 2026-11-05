using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Soforthilfe.Dienste.Konten;

namespace Soforthilfe.Api.Anmeldung;

/// <summary>
/// Ausgestelltes Token mit Ablaufzeitpunkt.
/// </summary>
public sealed record TokenAntwort(string Token, DateTimeOffset GueltigBis);

/// <summary>
/// Stellt signierte Tokens (JWT, HS256) aus.
/// </summary>
public sealed class TokenDienst(JwtEinstellungen einstellungen)
{
    private readonly JsonWebTokenHandler _handler = new();

    public TokenAntwort Erstellen(KontoInfo konto)
    {
        var ablauf = DateTimeOffset.UtcNow.AddMinutes(einstellungen.LaufzeitMinuten);

        // Datensparsam: Das Token enthält nur Kennung, Rolle und Region, keine Namen oder Adressen.
        var angaben = new Dictionary<string, object>
        {
            ["sub"] = konto.Id.ToString(),
            ["role"] = konto.Rolle,
        };

        if (konto.Region is not null)
        {
            angaben["region"] = konto.Region;
        }

        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = einstellungen.Herausgeber,
            Audience = einstellungen.Zielgruppe,
            Expires = ablauf.UtcDateTime,
            Claims = angaben,
            SigningCredentials = new SigningCredentials(einstellungen.Schluessel, SecurityAlgorithms.HmacSha256),
        });

        return new TokenAntwort(token, ablauf);
    }
}
