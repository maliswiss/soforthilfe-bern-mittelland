namespace Soforthilfe.Api.Konfiguration;

/// <summary>
/// Liest für die lokale Entwicklung eine .env-Datei ein und setzt ihre Werte als Umgebungsvariablen.
/// In Containern und in der CI kommen die Werte direkt aus der Umgebung.
/// </summary>
public static class UmgebungsDatei
{
    public static void Laden()
    {
        var pfad = Suchen(Directory.GetCurrentDirectory());
        if (pfad is null)
        {
            return;
        }

        foreach (var zeile in File.ReadAllLines(pfad))
        {
            var text = zeile.Trim();

            // Leere Zeilen und Kommentare überspringen.
            if (text.Length == 0 || text.StartsWith('#'))
            {
                continue;
            }

            var trenner = text.IndexOf('=');
            if (trenner <= 0)
            {
                continue;
            }

            var name = text[..trenner].Trim();
            var wert = text[(trenner + 1)..].Trim().Trim('"');

            // Bereits gesetzte Umgebungsvariablen werden nicht überschrieben.
            if (Environment.GetEnvironmentVariable(name) is null)
            {
                Environment.SetEnvironmentVariable(name, wert);
            }
        }
    }

    // Sucht die .env im aktuellen Verzeichnis und darüber, höchstens bis zur Wurzel des Repositorys.
    private static string? Suchen(string start)
    {
        for (var verzeichnis = new DirectoryInfo(start); verzeichnis is not null; verzeichnis = verzeichnis.Parent)
        {
            var kandidat = Path.Combine(verzeichnis.FullName, ".env");
            if (File.Exists(kandidat))
            {
                return kandidat;
            }

            if (Directory.Exists(Path.Combine(verzeichnis.FullName, ".git")))
            {
                return null;
            }
        }

        return null;
    }
}
