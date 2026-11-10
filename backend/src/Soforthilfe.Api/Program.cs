using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Soforthilfe.Api.Anmeldung;
using Soforthilfe.Api.Fehlerbehandlung;
using Soforthilfe.Api.Konfiguration;
using Soforthilfe.Api.Mandanten;
using Soforthilfe.Dienste;
using Soforthilfe.Dienste.Orte;

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

// Anmeldung mit JWT: Schlüssel, Herausgeber und Zielgruppe stammen aus der Umgebung.
var jwt = JwtEinstellungen.Lesen(builder.Configuration);
builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<TokenDienst>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(optionen =>
    {
        // Kurze Namen («sub», «role», «region») unverändert übernehmen.
        optionen.MapInboundClaims = false;
        optionen.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Herausgeber,
            ValidateAudience = true,
            ValidAudience = jwt.Zielgruppe,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwt.Schluessel,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "sub",
            RoleClaimType = "role",
        };
    });
builder.Services.AddAuthorization();

// Zentrale Fehlerbehandlung: Fehler werden einheitlich als «application/problem+json» beantwortet.
// Jedes Problem-Dokument enthält «fehler» (Text für die Oberfläche) und bei Eingabefehlern «feld».
builder.Services.AddProblemDetails(optionen => optionen.CustomizeProblemDetails = FehlerAntwort.Ergaenzen);
builder.Services.AddExceptionHandler<ZentraleFehlerbehandlung>();

// Zustandsprüfungen; weitere Prüfungen (zum Beispiel die Datenbank) tragen den Tag «bereit».
builder.Services.AddHealthChecks();

// Fachdienste inklusive Datenzugriff. Die Schnittstelle kennt nur die Dienstschicht.
builder.Services.AddDienste(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// Reihenfolge: zuerst das Token prüfen, dann die Region festlegen, dann die Berechtigungen.
app.UseAuthentication();
app.UseMiddleware<MandantenMiddleware>();
app.UseAuthorization();

app.MapControllers();

// Lebt der Prozess? Ohne weitere Prüfungen.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

// Ist die Anwendung bereit? Führt alle Prüfungen mit dem Tag «bereit» aus.
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = eintrag => eintrag.Tags.Contains("bereit") });

// Im Container entsteht das Schema beim Start aus den Migrationen (Datenbank__MigrationBeimStart=true).
if (app.Configuration.GetValue<bool>("Datenbank:MigrationBeimStart"))
{
    await app.Services.MigriereDatenbankAsync();
}

// Amtliches Ortschaftenverzeichnis (swisstopo): Referenzdaten, in jeder Umgebung einmal eingelesen.
await app.Services.LeseOrtschaftenEinAsync(app.Configuration);

// Testdaten mit fiktiven Personen nur auf ausdrücklichen Wunsch laden (Testdaten__Laden=true).
if (app.Configuration.GetValue<bool>("Testdaten:Laden"))
{
    await app.Services.LadeTestdatenAsync(app.Configuration["TESTDATEN_PASSWORT"]);
}

await app.RunAsync();
