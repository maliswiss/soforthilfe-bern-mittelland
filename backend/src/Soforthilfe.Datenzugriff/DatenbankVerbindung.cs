using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Soforthilfe.Datenzugriff;

/// <summary>
/// Baut die Verbindungsangabe aus den Umgebungswerten POSTGRES_HOST, POSTGRES_PORT,
/// POSTGRES_DB, POSTGRES_USER und POSTGRES_PASSWORD.
/// </summary>
public static class DatenbankVerbindung
{
    public static string Erstellen(IConfiguration konfiguration)
    {
        var verbindung = new NpgsqlConnectionStringBuilder
        {
            Host = konfiguration["POSTGRES_HOST"] ?? "localhost",
            Port = int.TryParse(konfiguration["POSTGRES_PORT"], out var port) ? port : 5432,
            Database = konfiguration["POSTGRES_DB"] ?? "soforthilfe",
            Username = konfiguration["POSTGRES_USER"] ?? "soforthilfe",
        };

        // Das Kennwort steht nur in der Umgebung, nie in einer Datei des Repositorys.
        var kennwort = konfiguration["POSTGRES_PASSWORD"];
        if (!string.IsNullOrEmpty(kennwort))
        {
            verbindung.Password = kennwort;
        }

        return verbindung.ConnectionString;
    }
}
