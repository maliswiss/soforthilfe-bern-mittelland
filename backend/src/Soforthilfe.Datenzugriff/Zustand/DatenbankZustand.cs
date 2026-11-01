using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Soforthilfe.Datenzugriff.Zustand;

/// <summary>
/// Zustandsprüfung: Ist die Datenbank erreichbar?
/// </summary>
internal sealed class DatenbankZustand(SoforthilfeDbContext datenbank) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext kontext, CancellationToken abbruch = default)
    {
        // Nur die Verbindung wird geprüft. Fehlertexte der Datenbank gelangen nicht in die Antwort.
        return await datenbank.Database.CanConnectAsync(abbruch)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Datenbank nicht erreichbar");
    }
}
