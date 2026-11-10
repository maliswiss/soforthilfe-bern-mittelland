namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Stammdaten, die mit den Migrationen in jede Datenbank kommen: Region, Kategorien und Fähigkeiten.
/// Die Kennungen sind fest, damit Migrationen und Tests stabil bleiben.
/// </summary>
public static class Stammdaten
{
    // Pilotregion der Plattform; die Kennung der Region bleibt seit der ersten Migration gleich. Weitere Regionen entstehen im laufenden Betrieb als Daten, ohne Migration.
    public static readonly Guid RegionBernMittelland = Guid.Parse("9abd0c6c-da62-4469-99e0-cf9406a99ac6");

    public static readonly Guid KategorieSchluesseldienst = Guid.Parse("495056ab-d82d-4e33-be3b-dc3e96e6b75a");
    public static readonly Guid KategorieKleineReparatur = Guid.Parse("3e09dd9e-7619-4b11-9aee-b0dc7150aacf");
    public static readonly Guid KategorieSanitaerElektro = Guid.Parse("8d71da98-bf81-45df-9633-d5b3d2d0b6a7");
    public static readonly Guid KategoriePannenhilfe = Guid.Parse("725c18b0-04e0-4f12-8c08-2ac5c2ec9d64");
    public static readonly Guid KategorieTransporthilfe = Guid.Parse("e3b9eb6e-e4b7-47d2-997b-63d4ad05e262");
    public static readonly Guid KategorieItSoforthilfe = Guid.Parse("7013d888-2f47-4ea8-9736-b91825f849d3");
    public static readonly Guid KategorieReinigung = Guid.Parse("aba279c9-09f5-4426-b30a-2b04b6baa08a");
    public static readonly Guid KategorieLieferdienst = Guid.Parse("3000be64-c436-4751-ac80-e3d4b20b50bd");
    public static readonly Guid KategorieNachbarschaftshilfe = Guid.Parse("c87f69fb-01f1-4a32-9891-a7f31d9ffa20");
    public static readonly Guid KategorieBegleitung = Guid.Parse("01af187a-c8fa-47fa-a425-4b3b63c260a3");

    // Gemeinden des Verwaltungskreises Bern-Mittelland (74 Gemeinden, BFS-Nummern), abgeglichen mit dem
    // amtlichen Ortschaftenverzeichnis vom 01.09.2026.
    public static readonly int[] GemeindenBernMittelland =
    [
        307, 351, 352, 353, 354, 355, 356, 357, 358, 359, 360, 361, 362, 363, 403,
        535, 538, 540, 541, 543, 544, 546, 551, 553, 557, 602, 603, 605, 606, 607,
        608, 609, 610, 611, 612, 613, 614, 615, 616, 617, 619, 620, 622, 623, 626,
        627, 628, 629, 630, 632, 662, 663, 665, 666, 667, 668, 669, 670, 671, 852,
        853, 855, 861, 866, 868, 869, 870, 872, 877, 879, 880, 884, 888, 889,
    ];

    public static Region[] Regionen =>
    [
        new() { Id = RegionBernMittelland, Kennung = "bern-mittelland", Name = "Soforthilfe Bern/Mittelland", Aktiv = true, Gemeinden = GemeindenBernMittelland },
    ];

    // Reihenfolge wie auf der Startseite. Nachbarschaftshilfe sowie Begleitung und Orientierung bleiben kostenlos;
    // gesundheitliche Anliegen sind auf Orientierung, Transport und Begleitung begrenzt.
    public static Kategorie[] Kategorien =>
    [
        new() { Id = KategorieSchluesseldienst, Kennung = "schluesseldienst", Bezeichnung = "Schlüsseldienst", Kostenpflichtig = true, Reihenfolge = 1 },
        new() { Id = KategorieKleineReparatur, Kennung = "kleine-reparatur", Bezeichnung = "Kleine Reparatur", Kostenpflichtig = true, Reihenfolge = 2 },
        new() { Id = KategorieSanitaerElektro, Kennung = "sanitaer-elektro", Bezeichnung = "Sanitär, Heizung, Elektro", Kostenpflichtig = true, Reihenfolge = 3 },
        new() { Id = KategoriePannenhilfe, Kennung = "pannenhilfe", Bezeichnung = "Panne und Abschleppdienst", Kostenpflichtig = true, Reihenfolge = 4 },
        new() { Id = KategorieTransporthilfe, Kennung = "transporthilfe", Bezeichnung = "Transport- und Umzugshilfe", Kostenpflichtig = true, Reihenfolge = 5 },
        new() { Id = KategorieItSoforthilfe, Kennung = "it-soforthilfe", Bezeichnung = "IT- und Handy-Soforthilfe", Kostenpflichtig = true, Reihenfolge = 6 },
        new() { Id = KategorieReinigung, Kennung = "reinigung", Bezeichnung = "Reinigung nach Schäden", Kostenpflichtig = true, Reihenfolge = 7 },
        new() { Id = KategorieLieferdienst, Kennung = "lieferdienst", Bezeichnung = "Lieferung Apotheke und Einkauf", Kostenpflichtig = true, Reihenfolge = 8 },
        new() { Id = KategorieNachbarschaftshilfe, Kennung = "nachbarschaftshilfe", Bezeichnung = "Nachbarschaftshilfe", Kostenpflichtig = false, Reihenfolge = 9 },
        new() { Id = KategorieBegleitung, Kennung = "begleitung", Bezeichnung = "Begleitung und Orientierung", Kostenpflichtig = false, NurOrientierung = true, Reihenfolge = 10 },
    ];

    // Je Kategorie eine Fähigkeit mit derselben Kennung; Helfende wählen ihre Fähigkeiten aus dieser Liste.
    public static Skill[] Skills =>
    [
        new() { Id = Guid.Parse("f944be5a-3e77-471b-a0e0-97d82287547c"), KategorieId = KategorieSchluesseldienst, Kennung = "schluesseldienst", Bezeichnung = "Schlüsseldienst" },
        new() { Id = Guid.Parse("a68e8a08-980e-4fc6-86b1-0baf5da9172e"), KategorieId = KategorieKleineReparatur, Kennung = "kleine-reparatur", Bezeichnung = "Kleine Reparatur" },
        new() { Id = Guid.Parse("c9a7b813-8f26-4dc1-8d0f-28d4a3cbad74"), KategorieId = KategorieSanitaerElektro, Kennung = "sanitaer-elektro", Bezeichnung = "Sanitär, Heizung, Elektro" },
        new() { Id = Guid.Parse("f38abfd7-19c8-45b1-9343-f5cf66a40b4b"), KategorieId = KategoriePannenhilfe, Kennung = "pannenhilfe", Bezeichnung = "Pannen- und Abschleppdienst" },
        new() { Id = Guid.Parse("45cc60eb-cce3-41c6-afa0-92b265177884"), KategorieId = KategorieTransporthilfe, Kennung = "transporthilfe", Bezeichnung = "Transport- und Umzugshilfe" },
        new() { Id = Guid.Parse("7d0fc8b8-f5e2-44f3-84e3-0c83452a7b33"), KategorieId = KategorieItSoforthilfe, Kennung = "it-soforthilfe", Bezeichnung = "IT- und Handy-Soforthilfe" },
        new() { Id = Guid.Parse("086f4f0d-06ad-4856-9d05-0c5136cb3f93"), KategorieId = KategorieReinigung, Kennung = "reinigung", Bezeichnung = "Reinigung nach Schäden" },
        new() { Id = Guid.Parse("4cc55036-09e9-454e-a2fb-9d4a7ea4bd43"), KategorieId = KategorieLieferdienst, Kennung = "lieferdienst", Bezeichnung = "Lieferdienst Apotheke und Einkauf" },
        new() { Id = Guid.Parse("11c55505-ba55-464c-86ea-f34088f5ab36"), KategorieId = KategorieNachbarschaftshilfe, Kennung = "nachbarschaftshilfe", Bezeichnung = "Nachbarschaftshilfe" },
        new() { Id = Guid.Parse("137f3e2b-d5fe-4c8e-814d-1941b47121d4"), KategorieId = KategorieBegleitung, Kennung = "begleitung", Bezeichnung = "Begleitung und Orientierung" },
    ];
}
