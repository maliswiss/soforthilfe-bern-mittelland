using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Mandanten;
using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Dienste.Anfragen;

public sealed record AnfrageInfo(Guid Id, string? Kategorie, bool? Kostenpflichtig, string? Freitext, string Postleitzahl, string Status, DateTimeOffset ErstelltAm);
public sealed record AnfrageDaten(Guid KontoId, Guid? KategorieId, string? Freitext, string Postleitzahl);
public enum AnfrageFehler { RegionFehlt, AuswahlUngueltig, KategorieNichtGefunden }
public sealed record AnfrageErgebnis(AnfrageInfo? Anfrage, AnfrageFehler? Fehler);

public sealed class AnfrageDienst(SoforthilfeDbContext datenbank, IMandantenKontext mandant)
{
    public async Task<AnfrageErgebnis> ErfassenAsync(AnfrageDaten daten, CancellationToken abbruch)
    {
        if (mandant.RegionId is not Guid region) return new AnfrageErgebnis(null, AnfrageFehler.RegionFehlt);
        var freitext = daten.Freitext?.Trim();
        var hatKategorie = daten.KategorieId.HasValue;
        if (hatKategorie == !string.IsNullOrWhiteSpace(freitext) ||
            !System.Text.RegularExpressions.Regex.IsMatch(daten.Postleitzahl, "^[0-9]{4}$"))
            return new AnfrageErgebnis(null, AnfrageFehler.AuswahlUngueltig);

        Kategorie? kategorie = null;
        if (daten.KategorieId is Guid kategorieId)
        {
            kategorie = await datenbank.Kategorien.AsNoTracking().FirstOrDefaultAsync(k => k.Id == kategorieId, abbruch);
            if (kategorie is null) return new AnfrageErgebnis(null, AnfrageFehler.KategorieNichtGefunden);
        }

        var anfrage = new Anfrage { Id = Guid.NewGuid(), RegionId = region, KontoId = daten.KontoId, KategorieId = kategorie?.Id,
            Freitext = freitext, Postleitzahl = daten.Postleitzahl, Status = AnfrageStatus.Offen, ErstelltAm = DateTimeOffset.UtcNow };
        datenbank.Anfragen.Add(anfrage);
        await datenbank.SaveChangesAsync(abbruch);
        return new AnfrageErgebnis(new AnfrageInfo(anfrage.Id, kategorie?.Bezeichnung, kategorie?.Kostenpflichtig,
            anfrage.Freitext, anfrage.Postleitzahl, anfrage.Status.ToString(), anfrage.ErstelltAm), null);
    }
}
