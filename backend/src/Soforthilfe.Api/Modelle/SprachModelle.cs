using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Soforthilfe.Dienste.Sprachen;

namespace Soforthilfe.Api.Modelle;

/// <summary>
/// Import einer Sprache: eigener Name, ob sie im Umschalter erscheint, und die Texte als JSON-Objekt mit derselben
/// Gliederung wie die Sprachdateien der Oberfläche.
/// </summary>
public sealed record SprachImport([Required] string Name, bool Aktiv, JsonElement Texte);

/// <summary>
/// Sprache ein- oder ausschalten.
/// </summary>
public sealed record SprachSchalter(bool Aktiv);

/// <summary>
/// Importierte Sprache mit Stand, ohne die Texte.
/// </summary>
public sealed record SprachAntwort(string Code, string Name, bool Aktiv, int AnzahlTexte, DateTimeOffset GeaendertAm)
{
    public static SprachAntwort Aus(SprachInfo s) => new(s.Code, s.Name, s.Aktiv, s.AnzahlTexte, s.GeaendertAm);
}
