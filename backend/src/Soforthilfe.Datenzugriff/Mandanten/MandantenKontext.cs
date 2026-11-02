namespace Soforthilfe.Datenzugriff.Mandanten;

/// <summary>
/// Hält die Region einer einzelnen Anfrage. Lebt genau so lange wie die Anfrage.
/// </summary>
public sealed class MandantenKontext : IMandantenKontext
{
    public Guid? RegionId { get; private set; }

    public bool Uebergreifend { get; private set; }

    public void SetzeRegion(Guid regionId)
    {
        // Eine einmal festgelegte Region darf innerhalb derselben Anfrage nicht wechseln.
        if (Uebergreifend || (RegionId is not null && RegionId != regionId))
        {
            throw new MandantenVerletzung("Die Region der Anfrage ist bereits festgelegt.");
        }

        RegionId = regionId;
    }

    public void SetzeUebergreifend()
    {
        if (RegionId is not null)
        {
            throw new MandantenVerletzung("Die Region der Anfrage ist bereits festgelegt.");
        }

        Uebergreifend = true;
    }
}
