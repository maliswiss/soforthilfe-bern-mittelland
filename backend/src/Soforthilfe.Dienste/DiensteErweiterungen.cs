using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Dienste.Anfragen;
using Soforthilfe.Dienste.Helfende;
using Soforthilfe.Dienste.Info;
using Soforthilfe.Dienste.Kategorien;
using Soforthilfe.Dienste.Konten;
using Soforthilfe.Dienste.Notfall;
using Soforthilfe.Dienste.Orte;
using Soforthilfe.Dienste.Preise;
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

        // Preisberechnung (Anforderung A9): MWST-Satz und Nachtzeit aus dem Abschnitt «Preise», sonst Standardwerte.
        dienste.AddSingleton(new PreisEinstellungen(
            konfiguration.GetValue("Preise:MwstSatzProzent", 8.1m),
            konfiguration.GetValue("Preise:NachtVonStunde", 20),
            konfiguration.GetValue("Preise:NachtBisStunde", 7)));
        dienste.AddSingleton<IPreisrechner, Preisrechner>();

        // Das Protokoll schreibt in die Region der laufenden Anfrage.
        dienste.AddScoped<IProtokollDienst, ProtokollDienst>();

        // Kategorien und Anfragen werden je Anfrage aus der Datenbank gelesen.
        dienste.AddScoped<IKategorienDienst, KategorienDienst>();
        dienste.AddScoped<IOrtDienst, OrtDienst>();
        dienste.AddScoped<IAnfrageDienst, AnfrageDienst>();
        dienste.AddScoped<IHelferprofilDienst, HelferprofilDienst>();
        dienste.AddScoped<IVerstaendnisDienst, VerstaendnisDienst>();

        return dienste;
    }
}
