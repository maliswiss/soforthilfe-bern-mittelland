namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Stammdaten, die mit den Migrationen in jede Datenbank kommen: Regionen, Kategorien und Fähigkeiten.
/// Die Kennungen sind fest, damit Migrationen und Tests stabil bleiben.
/// </summary>
public static class Stammdaten
{
    public static readonly Guid RegionBern = Guid.Parse("9abd0c6c-da62-4469-99e0-cf9406a99ac6");
    public static readonly Guid RegionMittelland = Guid.Parse("a61871bc-f698-4e48-9d36-90fb837b56ae");

    public static readonly Guid KategorieHandwerk = Guid.Parse("426d6e5b-34b4-425c-b408-efa9a96d06bd");
    public static readonly Guid KategorieHaushalt = Guid.Parse("ff0714f3-a6bc-4578-8e1c-006043768441");
    public static readonly Guid KategorieDigital = Guid.Parse("77a421f5-aa9f-4701-87b0-233dc23d7098");
    public static readonly Guid KategorieNachbarschaft = Guid.Parse("c87f69fb-01f1-4a32-9891-a7f31d9ffa20");
    public static readonly Guid KategorieBegleitung = Guid.Parse("01af187a-c8fa-47fa-a425-4b3b63c260a3");
    public static readonly Guid KategorieOrientierung = Guid.Parse("79adfee0-8bc3-440f-abfd-3c0981d08d78");

    public static Region[] Regionen =>
    [
        new() { Id = RegionBern, Kennung = "bern", Name = "Bern", Aktiv = true },
        new() { Id = RegionMittelland, Kennung = "mittelland", Name = "Mittelland", Aktiv = true },
    ];

    // Nachbarschaftshilfe, Begleitung und Orientierung bleiben bewusst kostenlos.
    public static Kategorie[] Kategorien =>
    [
        new() { Id = KategorieHandwerk, Kennung = "handwerk", Bezeichnung = "Handwerk und Reparaturen", Kostenpflichtig = true },
        new() { Id = KategorieHaushalt, Kennung = "haushalt", Bezeichnung = "Haushalt", Kostenpflichtig = true },
        new() { Id = KategorieDigital, Kennung = "digital", Bezeichnung = "Computer und Smartphone", Kostenpflichtig = true },
        new() { Id = KategorieNachbarschaft, Kennung = "nachbarschaft", Bezeichnung = "Nachbarschaftshilfe", Kostenpflichtig = false },
        new() { Id = KategorieBegleitung, Kennung = "begleitung", Bezeichnung = "Begleitung", Kostenpflichtig = false },
        new() { Id = KategorieOrientierung, Kennung = "orientierung", Bezeichnung = "Orientierung und Formulare", Kostenpflichtig = false },
    ];

    public static Skill[] Skills =>
    [
        new() { Id = Guid.Parse("403c1897-d38b-46a2-aaa5-118e15190285"), KategorieId = KategorieHandwerk, Kennung = "sanitaer", Bezeichnung = "Sanitär" },
        new() { Id = Guid.Parse("a0e17f6f-a52d-4624-ad1c-050cad1c8e0c"), KategorieId = KategorieHandwerk, Kennung = "elektro", Bezeichnung = "Elektro" },
        new() { Id = Guid.Parse("dfdf01df-dd1c-4dfe-8138-810d32f1751f"), KategorieId = KategorieHandwerk, Kennung = "schluesseldienst", Bezeichnung = "Schlüsseldienst" },
        new() { Id = Guid.Parse("47a61bb0-fae5-4202-9c46-8e6c12fb83c3"), KategorieId = KategorieHaushalt, Kennung = "reinigung", Bezeichnung = "Reinigung" },
        new() { Id = Guid.Parse("dab5a587-49b7-4a13-ba57-575e79aba354"), KategorieId = KategorieHaushalt, Kennung = "transport", Bezeichnung = "Transport und Umzug" },
        new() { Id = Guid.Parse("8853ae85-82fe-4bf8-a46d-356e74bf282e"), KategorieId = KategorieDigital, Kennung = "computer", Bezeichnung = "Computer und Smartphone einrichten" },
        new() { Id = Guid.Parse("90385480-007c-47b5-9ae3-9e5e906b1fda"), KategorieId = KategorieNachbarschaft, Kennung = "einkauf", Bezeichnung = "Einkauf" },
        new() { Id = Guid.Parse("6ae96d9a-d647-477c-8f50-7b91876b4c13"), KategorieId = KategorieNachbarschaft, Kennung = "haustiere", Bezeichnung = "Haustiere betreuen" },
        new() { Id = Guid.Parse("f0828c46-e830-4fec-81e5-9006b4e00ddd"), KategorieId = KategorieBegleitung, Kennung = "termin", Bezeichnung = "Begleitung zu Terminen" },
        new() { Id = Guid.Parse("f40abd8a-fd5a-473c-8064-362823f1b214"), KategorieId = KategorieBegleitung, Kennung = "spaziergang", Bezeichnung = "Spaziergang" },
        new() { Id = Guid.Parse("82f5ec98-bdb2-4fc2-9228-2542d64a7fb1"), KategorieId = KategorieOrientierung, Kennung = "formulare", Bezeichnung = "Hilfe bei Formularen" },
    ];
}
