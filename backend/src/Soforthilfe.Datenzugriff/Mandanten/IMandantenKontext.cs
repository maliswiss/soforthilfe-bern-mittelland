namespace Soforthilfe.Datenzugriff.Mandanten;

/// <summary>
/// Region (Mandant) der laufenden Anfrage. Der Datenzugriff liefert und schreibt nur Daten dieser Region.
/// </summary>
public interface IMandantenKontext
{
    Guid? RegionId { get; }

    // Nur der Plattformbetrieb arbeitet regionsübergreifend.
    bool Uebergreifend { get; }

    void SetzeRegion(Guid regionId);

    void SetzeUebergreifend();
}
