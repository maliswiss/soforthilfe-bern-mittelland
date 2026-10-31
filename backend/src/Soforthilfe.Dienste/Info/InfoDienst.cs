using System.Reflection;

namespace Soforthilfe.Dienste.Info;

/// <summary>
/// Liest Name und Version aus den Angaben der Assembly.
/// </summary>
public sealed class InfoDienst : IInfoDienst
{
    private const string Name = "Soforthilfe Bern/Mittelland";

    public AnwendungsInfo LiesInfo()
    {
        // Die Version stammt aus Directory.Build.props und wird beim Build in die Assembly geschrieben.
        var version = typeof(InfoDienst).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "unbekannt";

        // Ein angehängter Commit-Bezug («+abc123») wird für die Anzeige entfernt.
        var plus = version.IndexOf('+');
        if (plus >= 0)
        {
            version = version[..plus];
        }

        return new AnwendungsInfo(Name, version);
    }
}
