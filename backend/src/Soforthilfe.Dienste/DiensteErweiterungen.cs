using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Dienste.Anfragen;
using Soforthilfe.Dienste.Helfende;
using Soforthilfe.Dienste.Info;
using Soforthilfe.Dienste.Kategorien;
using Soforthilfe.Dienste.Konten;
using Soforthilfe.Dienste.Matching;
using Soforthilfe.Dienste.Nachweise;
using Soforthilfe.Dienste.Notfall;
using Soforthilfe.Dienste.Orte;
using Soforthilfe.Dienste.Preise;
using Soforthilfe.Dienste.Protokoll;
using Soforthilfe.Dienste.Regionen;
using Soforthilfe.Dienste.Sprachen;
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
        dienste.AddScoped<IVerifizierungDienst, VerifizierungDienst>();

        // Matching: Kandidatensuche nach Postleitzahl, Fähigkeit und Suchradius.
        dienste.AddScoped<IKandidatenSuche, KandidatenSuche>();

        // Der Klassifikator wird beim Start einmal trainiert und danach nur noch gelesen.
        dienste.AddSingleton<ITextKlassifikator, TfIdfKlassifikator>();

        // Lokales Sprachmodell (Ollama) als letzte Stufe, nur über die konfigurierte Adresse; ohne Adresse aus.
        var sprachmodell = new SprachmodellEinstellungen(
            konfiguration["OLLAMA_BASE_URL"],
            konfiguration["OLLAMA_MODELL"] is { Length: > 0 } modell ? modell : "llama3.2:3b",
            TimeSpan.FromSeconds(konfiguration.GetValue("OLLAMA_ZEITLIMIT_SEKUNDEN", 10)),
            "30m");
        dienste.AddSingleton(sprachmodell);
        dienste.TryAddSingleton(TimeProvider.System);
        dienste.AddHttpClient(OllamaSprachmodell.KlientName, klient =>
        {
            if (sprachmodell.Eingeschaltet)
            {
                klient.BaseAddress = new Uri(sprachmodell.Adresse!.TrimEnd('/') + "/");
            }
        });
        dienste.AddSingleton<ISprachmodell, OllamaSprachmodell>();
        dienste.AddHostedService<SprachmodellAufwaermer>();
        dienste.AddScoped<IEinordnung, Einordnung>();
        dienste.AddScoped<ISprachDienst, SprachDienst>();
        dienste.AddScoped<IVerstaendnisDienst, VerstaendnisDienst>();

        return dienste;
    }
}
