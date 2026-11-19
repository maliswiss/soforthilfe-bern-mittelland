using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Mandanten;
using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Helfende;
using Soforthilfe.Dienste.Orte;
using Soforthilfe.Dienste.Preise;
using Soforthilfe.Dienste.Protokoll;
using Soforthilfe.Dienste.Verstaendnis;

namespace Soforthilfe.Dienste.Matching;

/// <summary>
/// Smart-Matching nach Distanz, Fähigkeit, Bewertung, Verfügbarkeit und Erfahrung mit einstellbaren Gewichten.
/// Gespeichert werden Rang, Punktzahl, Teilpunkte, die verwendeten Gewichte und die Begründung.
/// </summary>
public sealed class MatchingDienst(
    SoforthilfeDbContext datenbank,
    IKandidatenSuche kandidatenSuche,
    MatchingGewichte gewichte,
    IProtokollDienst protokoll,
    IOrtDienst orte,
    IPreisrechner preise,
    IMandantenKontext mandant) : IMatchingDienst
{
    public async Task<int> VorschlaegeErzeugenAsync(Guid anfrageId, CancellationToken abbruch)
    {
        var anfrage = await datenbank.Anfragen.Include(a => a.Kategorie).FirstOrDefaultAsync(a => a.Id == anfrageId, abbruch);

        // Nur offene Anfragen mit Kategorie erhalten Vorschläge.
        if (anfrage?.Kategorie is null || anfrage.Status is not (AnfrageStatus.Offen or AnfrageStatus.Vorgeschlagen))
        {
            return 0;
        }

        var ergebnis = await kandidatenSuche.FuerAnfrageAsync(anfrageId, abbruch);
        if (ergebnis is null || ergebnis.Kandidaten.Count == 0)
        {
            return 0;
        }

        var vorhanden = await datenbank.Vorschlaege
            .Where(v => v.AnfrageId == anfrageId)
            .Select(v => new { v.HelferprofilId, v.Rang })
            .ToListAsync(abbruch);
        var bekannt = vorhanden.Select(v => v.HelferprofilId).ToHashSet();
        var rang = vorhanden.Count == 0 ? 1 : vorhanden.Max(v => v.Rang) + 1;

        var beste = Punktzahl.Ordnen(ergebnis.Kandidaten
                .Where(k => !bekannt.Contains(k.Profil.Id))
                .Select(k => Punktzahl.Bewerten(k, ergebnis.SuchradiusKm, gewichte)))
            .Take(Punktzahl.AnzahlVorschlaege)
            .ToList();
        if (beste.Count == 0)
        {
            return 0;
        }

        var jetzt = DateTimeOffset.UtcNow;
        foreach (var eintrag in beste)
        {
            datenbank.Vorschlaege.Add(Neu(anfrage, eintrag, rang++, jetzt));
        }

        anfrage.Status = AnfrageStatus.Vorgeschlagen;
        await datenbank.SaveChangesAsync(abbruch);

        // Im Protokoll steht nur die Anzahl, keine Personen.
        await protokoll.SchreibenAsync(ProtokollAktionen.AnfrageVorgeschlagen, ProtokollAktionen.ObjektAnfrage,
            anfrage.Id.ToString(), beste.Count == 1 ? "1 Vorschlag" : $"{beste.Count} Vorschläge", abbruch);

        return beste.Count;
    }

    public async Task<IReadOnlyList<VorschlagInfo>?> VorschlaegeAsync(Guid anfrageId, Guid? nurVon, CancellationToken abbruch)
    {
        // Der Regionsfilter blendet fremde Regionen aus; nurVon beschränkt auf die eigenen Anfragen.
        var anfrage = await datenbank.Anfragen
            .AsNoTracking()
            .Include(a => a.Kategorie)
            .FirstOrDefaultAsync(a => a.Id == anfrageId && (nurVon == null || a.KontoId == nurVon), abbruch);
        if (anfrage is null)
        {
            return null;
        }

        var vorschlaege = await datenbank.Vorschlaege
            .AsNoTracking()
            .Include(v => v.Helferprofil!)
            .ThenInclude(p => p.Konto)
            .Where(v => v.AnfrageId == anfrageId)
            .OrderBy(v => v.Rang)
            .ToListAsync(abbruch);

        // Preisangaben wie jetzt, damit der Preis vor der Zusage dem aktuellen Profil entspricht (Anforderung A9).
        var bezahlpflichtig = anfrage.Kategorie?.Kostenpflichtig ?? false;
        var jetzt = DateTimeOffset.UtcNow;
        return vorschlaege
            .Select(v => Info(v, preise.Berechnen(HelferprofilDienst.Preise(v.Helferprofil!), bezahlpflichtig, jetzt)))
            .ToList();
    }

    public async Task<VorschauErgebnis?> VorschauAsync(VorschauKriterien kriterien, CancellationToken abbruch)
    {
        var kategorie = await datenbank.Kategorien.AsNoTracking().FirstOrDefaultAsync(k => k.Id == kriterien.KategorieId, abbruch);
        var regionId = kriterien.RegionKennung is { } kennung
            ? await datenbank.Regionen.AsNoTracking()
                .Where(r => r.Kennung == kennung && r.Aktiv)
                .Select(r => (Guid?)r.Id)
                .FirstOrDefaultAsync(abbruch)
            : mandant.RegionId;
        if (regionId is not Guid region || kategorie is null)
        {
            return null;
        }

        // Ohne gewählte Dringlichkeit die übliche der Kategorie, wie beim Erfassen ohne Signalwort.
        var prioritaet = kriterien.Prioritaet ?? Dringlichkeit.Bestimmen(null, kategorie.Kennung).Prioritaet;
        var suchradius = Suchradius.FuerPrioritaet(prioritaet);
        var standort = await orte.PunktZuPlzAsync(kriterien.Plz, kriterien.Ort, abbruch);
        var kandidaten = await kandidatenSuche.SuchenAsync(new KandidatenKriterien(region, kategorie.Id,
            kategorie.Kostenpflichtig, kriterien.Plz, standort, suchradius), abbruch);

        var jetzt = DateTimeOffset.UtcNow;
        var eintraege = Punktzahl.Ordnen(kandidaten.Select(k => Punktzahl.Bewerten(k, suchradius, gewichte)))
            .Take(Punktzahl.AnzahlVorschlaege)
            .Select((b, index) => new VorschauEintrag(
                index + 1,
                b.Kandidat.Profil,
                (int)Math.Round(b.Kandidat.DistanzMeter),
                Math.Round(b.Punktzahl, 4),
                b.PartnerBevorzugt,
                Begruendung.Text(b.Kandidat.Profil, b.Kandidat.DistanzMeter, kategorie.Bezeichnung, b.PartnerBevorzugt),
                Begruendung.Kriterien(Gerundet(b.Teilpunkte), gewichte),
                preise.Berechnen(HelferprofilDienst.Preise(b.Kandidat.Profil), kategorie.Kostenpflichtig, jetzt)))
            .ToList();

        return new VorschauErgebnis(kategorie, prioritaet, suchradius, eintraege);
    }

    // Teilpunkte wie gespeichert, auf vier Stellen.
    private static Teilpunkte Gerundet(Teilpunkte t) => new(Math.Round(t.Distanz, 4), Math.Round(t.Faehigkeit, 4),
        Math.Round(t.Bewertung, 4), Math.Round(t.Verfuegbarkeit, 4), Math.Round(t.Erfahrung, 4));

    private Vorschlag Neu(Anfrage anfrage, BewerteterKandidat eintrag, int rang, DateTimeOffset jetzt) => new()
    {
        Id = Guid.NewGuid(),
        RegionId = anfrage.RegionId,
        AnfrageId = anfrage.Id,
        HelferprofilId = eintrag.Kandidat.Profil.Id,
        Rang = rang,
        Punktzahl = Math.Round(eintrag.Punktzahl, 4),
        DistanzMeter = (int)Math.Round(eintrag.Kandidat.DistanzMeter),
        TeilpunkteDistanz = Math.Round(eintrag.Teilpunkte.Distanz, 4),
        TeilpunkteFaehigkeit = Math.Round(eintrag.Teilpunkte.Faehigkeit, 4),
        TeilpunkteBewertung = Math.Round(eintrag.Teilpunkte.Bewertung, 4),
        TeilpunkteVerfuegbarkeit = Math.Round(eintrag.Teilpunkte.Verfuegbarkeit, 4),
        TeilpunkteErfahrung = Math.Round(eintrag.Teilpunkte.Erfahrung, 4),
        GewichtDistanz = gewichte.Distanz,
        GewichtFaehigkeit = gewichte.Faehigkeit,
        GewichtBewertung = gewichte.Bewertung,
        GewichtVerfuegbarkeit = gewichte.Verfuegbarkeit,
        GewichtErfahrung = gewichte.Erfahrung,
        PartnerBevorzugt = eintrag.PartnerBevorzugt,
        Begruendung = Begruendung.Text(eintrag.Kandidat.Profil, eintrag.Kandidat.DistanzMeter, anfrage.Kategorie!.Bezeichnung,
            eintrag.PartnerBevorzugt),
        ErstelltAm = jetzt,
    };

    private static VorschlagInfo Info(Vorschlag v, PreisInfo? preis)
    {
        var profil = v.Helferprofil!;
        var teilpunkte = new Teilpunkte(v.TeilpunkteDistanz, v.TeilpunkteFaehigkeit, v.TeilpunkteBewertung,
            v.TeilpunkteVerfuegbarkeit, v.TeilpunkteErfahrung);
        var verwendet = new MatchingGewichte(v.GewichtDistanz, v.GewichtFaehigkeit, v.GewichtBewertung,
            v.GewichtVerfuegbarkeit, v.GewichtErfahrung);
        return new VorschlagInfo(
            v.Id,
            v.HelferprofilId,
            v.Rang,
            v.Punktzahl,
            v.DistanzMeter,
            profil.Konto?.Anzeigename ?? string.Empty,
            profil.Typ,
            profil.Betriebsname,
            profil.Postleitzahl,
            profil.Ort,
            profil.Einsatzbereit,
            profil.Bewertungsschnitt,
            profil.AnzahlBewertungen,
            profil.AnzahlEinsaetze,
            v.PartnerBevorzugt,
            teilpunkte,
            verwendet,
            v.Begruendung,
            Begruendung.Kriterien(teilpunkte, verwendet),
            preis,
            v.ErstelltAm);
    }
}
