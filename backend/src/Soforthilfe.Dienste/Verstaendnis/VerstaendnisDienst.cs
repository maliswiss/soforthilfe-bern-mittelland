using Soforthilfe.Dienste.Notfall;
using Soforthilfe.Dienste.Protokoll;

namespace Soforthilfe.Dienste.Verstaendnis;

/// <summary>
/// Führt den Verständnis-Schritt aus. Bei einem gesundheitlichen Notfall wird nicht eingeordnet und nicht vermittelt.
/// </summary>
public sealed class VerstaendnisDienst(INotfallPruefung notfallPruefung, IProtokollDienst protokoll) : IVerstaendnisDienst
{
    public async Task<VerstaendnisInfo> VerstehenAsync(Guid? kategorieId, string? freitext, CancellationToken abbruch)
    {
        // Schritt 0: Notfallprüfung, unabhängig davon, ob eine Kategorie gewählt wurde.
        var notfall = notfallPruefung.Pruefen(freitext);
        if (!notfall.IstNotfall)
        {
            return new VerstaendnisInfo(false, null, null);
        }

        // Jeder Notfall-Stopp wird protokolliert, nur mit dem Begriff aus der festen Liste.
        await protokoll.SchreibenAsync(ProtokollAktionen.NotfallStopp, ProtokollAktionen.ObjektVerstaendnis, null,
            NotfallTexte.Grund(notfall.Begriff), abbruch);

        return new VerstaendnisInfo(true, NotfallTexte.Grund(notfall.Schluesselwort), NotfallTexte.Hinweis);
    }
}
