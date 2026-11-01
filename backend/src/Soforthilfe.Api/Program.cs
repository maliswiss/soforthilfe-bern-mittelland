using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Soforthilfe.Api.Fehlerbehandlung;
using Soforthilfe.Api.Konfiguration;
using Soforthilfe.Dienste;

var builder = WebApplication.CreateBuilder(args);

// Lokale Entwicklung: Werte aus der .env im Repository übernehmen.
// Bereits gesetzte Umgebungsvariablen haben Vorrang, darum wird die Quelle danach neu eingelesen.
if (builder.Environment.IsDevelopment())
{
    UmgebungsDatei.Laden();
    builder.Configuration.AddEnvironmentVariables();
}

// Kein Serverkopf mit Produktname und Version in den Antworten.
builder.WebHost.ConfigureKestrel(optionen => optionen.AddServerHeader = false);

// Ausserhalb der Entwicklung als JSON protokollieren, damit die Protokolle maschinell auswertbar sind.
if (!builder.Environment.IsDevelopment())
{
    builder.Logging.ClearProviders();
    builder.Logging.AddJsonConsole();
}

builder.Services.AddControllers();

// Zentrale Fehlerbehandlung: Fehler werden einheitlich als «application/problem+json» beantwortet.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ZentraleFehlerbehandlung>();

// Zustandsprüfungen; weitere Prüfungen (zum Beispiel die Datenbank) tragen den Tag «bereit».
builder.Services.AddHealthChecks();

// Fachdienste inklusive Datenzugriff. Die Schnittstelle kennt nur die Dienstschicht.
builder.Services.AddDienste(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapControllers();

// Lebt der Prozess? Ohne weitere Prüfungen.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

// Ist die Anwendung bereit? Führt alle Prüfungen mit dem Tag «bereit» aus.
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = eintrag => eintrag.Tags.Contains("bereit") });

// Testdaten mit fiktiven Personen nur auf ausdrücklichen Wunsch laden (Testdaten__Laden=true).
if (app.Configuration.GetValue<bool>("Testdaten:Laden"))
{
    await app.Services.LadeTestdatenAsync();
}

await app.RunAsync();
