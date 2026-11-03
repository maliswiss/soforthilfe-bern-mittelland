namespace Soforthilfe.Dienste.Konten;

/// <summary>Prüft Schweizer Mehrwertnummern unabhängig von der Schreibweise.</summary>
public static class Mehrwertnummer
{
    public static bool Pruefen(string nummer)
    {
        // Trennzeichen entfernen und internationale Schweizer Vorwahlen vereinheitlichen.
        var ziffern = new string(nummer.Where(char.IsDigit).ToArray());
        if (ziffern.StartsWith("0041", StringComparison.Ordinal))
        {
            ziffern = "0" + ziffern[4..];
        }
        else if (ziffern.StartsWith("41", StringComparison.Ordinal))
        {
            ziffern = "0" + ziffern[2..];
        }

        return ziffern.StartsWith("0900", StringComparison.Ordinal)
            || ziffern.StartsWith("0901", StringComparison.Ordinal)
            || ziffern.StartsWith("0906", StringComparison.Ordinal);
    }
}
