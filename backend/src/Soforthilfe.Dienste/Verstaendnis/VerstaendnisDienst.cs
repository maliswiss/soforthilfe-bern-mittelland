using Soforthilfe.Dienste.Notfall;

namespace Soforthilfe.Dienste.Verstaendnis;

/// <summary>
/// Führt den Verständnis-Schritt aus. Bei einem gesundheitlichen Notfall wird nicht eingeordnet und nicht vermittelt.
/// </summary>
public sealed class VerstaendnisDienst(INotfallPruefung notfallPruefung) : IVerstaendnisDienst
{
    public Task<VerstaendnisInfo> VerstehenAsync(Guid? kategorieId, string? freitext, CancellationToken abbruch)
    {
        // Schritt 0: Notfallprüfung, unabhängig davon, ob eine Kategorie gewählt wurde.
        var notfall = notfallPruefung.Pruefen(freitext);

        return Task.FromResult(notfall.IstNotfall
            ? new VerstaendnisInfo(true, NotfallTexte.Grund(notfall.Schluesselwort), NotfallTexte.Hinweis)
            : new VerstaendnisInfo(false, null, null));
    }
}
