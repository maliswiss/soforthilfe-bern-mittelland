using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Soforthilfe.Datenzugriff.Zustand;

namespace Soforthilfe.Datenzugriff;

/// <summary>
/// Registriert die Bestandteile der Schicht Datenzugriff.
/// </summary>
public static class DatenzugriffErweiterungen
{
    public static IServiceCollection AddDatenzugriff(this IServiceCollection dienste, IConfiguration konfiguration)
    {
        // Npgsql mit NetTopologySuite, damit PostGIS-Typen (Punkte, Flächen) abgebildet werden können.
        dienste.AddDbContext<SoforthilfeDbContext>(optionen =>
            optionen.UseNpgsql(
                DatenbankVerbindung.Erstellen(konfiguration),
                npgsql => npgsql.UseNetTopologySuite()));

        // Die Anwendung ist erst bereit, wenn die Datenbank erreichbar ist.
        dienste.AddHealthChecks().AddCheck<DatenbankZustand>("datenbank", tags: ["bereit"]);

        return dienste;
    }
}
