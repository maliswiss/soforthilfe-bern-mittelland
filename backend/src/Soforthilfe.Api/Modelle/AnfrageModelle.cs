using System.ComponentModel.DataAnnotations;

namespace Soforthilfe.Api.Modelle;

/// <summary>
/// Eingabe für eine neue Anfrage: Kategorie, Freitext oder beides, dazu Postleitzahl und Ort des Einsatzes.
/// Prioritaet: 0 tief, 1 mittel (Standard), 2 hoch.
/// </summary>
public sealed record AnfrageErstellen(
    Guid? KategorieId,
    [StringLength(2000)] string? Freitext,
    [Required, StringLength(10)] string Plz,
    [Required, StringLength(120)] string Ort,
    [StringLength(250)] string? Adresse = null,
    [StringLength(120)] string? Zeitfenster = null,
    int Prioritaet = 1);

/// <summary>
/// Eingabe für den Verständnis-Schritt vor dem Erfassen: Kategorie, Freitext oder beides.
/// </summary>
public sealed record VerstaendnisAnfrage(Guid? KategorieId, [StringLength(2000)] string? Freitext);

/// <summary>
/// Antwort des Verständnis-Schritts. Bei «Notfall» ist die Vermittlung gestoppt.
/// </summary>
public sealed record VerstaendnisAntwort(bool Notfall, string? NotfallGrund, string? NotfallHinweis);
