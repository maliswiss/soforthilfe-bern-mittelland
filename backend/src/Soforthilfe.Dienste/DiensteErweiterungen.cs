using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Dienste.Anfragen;
using Soforthilfe.Dienste.Info;
using Soforthilfe.Dienste.Kategorien;
using Soforthilfe.Dienste.Konten;
using Soforthilfe.Dienste.Notfall;
using Soforthilfe.Dienste.Protokoll;
using Soforthilfe.Dienste.Regionen;
using Soforthilfe.Dienste.Verstaendnis;

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

        // Je Anfrage, weil der Dienst die Region der laufenden Anfrage hält.
        dienste.AddScoped<IRegionDienst, RegionDienst>();

        // Passwort-Hashing ist zustandslos; Konten werden je Anfrage gelesen.
        dienste.AddSingleton<PasswortDienst>();
        dienste.AddScoped<IKontoDienst, KontoDienst>();

        // Die Notfallprüfung arbeitet nur mit festen Regeln und ist zustandslos.
        dienste.AddSingleton<INotfallPruefung, NotfallPruefung>();

        // Das Protokoll schreibt in die Region der laufenden Anfrage.
        dienste.AddScoped<IProtokollDienst, ProtokollDienst>();

        // Kategorien und Anfragen werden je Anfrage aus der Datenbank gelesen.
        dienste.AddScoped<IKategorienDienst, KategorienDienst>();
        dienste.AddScoped<IAnfrageDienst, AnfrageDienst>();
        dienste.AddScoped<IVerstaendnisDienst, VerstaendnisDienst>();

        return dienste;
    }
}
