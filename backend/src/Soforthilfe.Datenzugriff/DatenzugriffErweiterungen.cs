using Microsoft.Extensions.DependencyInjection;

namespace Soforthilfe.Datenzugriff;

/// <summary>
/// Registriert die Bestandteile der Schicht Datenzugriff.
/// </summary>
public static class DatenzugriffErweiterungen
{
    public static IServiceCollection AddDatenzugriff(this IServiceCollection dienste)
    {
        // Einstiegspunkt der Schicht: Hier wird der Datenbankkontext registriert,
        // sobald PostgreSQL angebunden ist. Nur die Dienstschicht ruft diese Methode auf.
        return dienste;
    }
}
