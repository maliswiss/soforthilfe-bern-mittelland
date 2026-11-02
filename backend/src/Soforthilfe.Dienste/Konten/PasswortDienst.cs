using Microsoft.AspNetCore.Identity;
using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Dienste.Konten;

/// <summary>
/// Passwörter werden nur als Hash gespeichert (PBKDF2 mit HMAC-SHA512, Salz je Passwort,
/// Verfahren von ASP.NET Core Identity).
/// </summary>
public sealed class PasswortDienst
{
    private readonly PasswordHasher<Konto> _hasher = new();

    // Hash eines Platzhalters: wird geprüft, wenn es kein Konto gibt, damit die Antwortzeit
    // nichts darüber verrät, ob eine E-Mail-Adresse registriert ist.
    public string Blindwert { get; }

    public PasswortDienst()
    {
        Blindwert = Hash("blindwert-ohne-konto");
    }

    public string Hash(string passwort) => _hasher.HashPassword(null!, passwort);

    public bool Stimmt(string hash, string passwort) =>
        _hasher.VerifyHashedPassword(null!, hash, passwort) != PasswordVerificationResult.Failed;
}
