using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Dienste.Info;

namespace Soforthilfe.Dienste;

/// <summary>
/// Registriert die Fachdienste und die darunterliegende Datenzugriffsschicht.
/// </summary>
public static class DiensteErweiterungen
{
    public static IServiceCollection AddDienste(this IServiceCollection dienste, IConfiguration konfiguration)
    {
        // Die Dienstschicht bindet den Datenzugriff selbst ein. So bleibt die Reihenfolge
        // der Schichten Controller → Dienste → Datenzugriff fest.
        dienste.AddDatenzugriff(konfiguration);

        // Zustandslos, darum eine einzige Instanz für die ganze Laufzeit.
        dienste.AddSingleton<IInfoDienst, InfoDienst>();

        return dienste;
    }
}
