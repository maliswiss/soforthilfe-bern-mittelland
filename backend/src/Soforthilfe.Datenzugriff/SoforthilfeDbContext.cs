using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff.Mandanten;
using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Datenzugriff;

/// <summary>
/// Datenbankkontext der Plattform (PostgreSQL mit PostGIS).
/// Konten, Helferprofile und Anfragen sind strikt nach Region (Mandant) getrennt.
/// </summary>
public sealed class SoforthilfeDbContext : DbContext
{
    private readonly IMandantenKontext _mandant;

    public SoforthilfeDbContext(DbContextOptions<SoforthilfeDbContext> optionen, IMandantenKontext mandant)
        : base(optionen)
    {
        _mandant = mandant;
    }

    public DbSet<Region> Regionen => Set<Region>();

    public DbSet<Konto> Konten => Set<Konto>();

    public DbSet<Kategorie> Kategorien => Set<Kategorie>();

    public DbSet<Skill> Skills => Set<Skill>();

    public DbSet<Helferprofil> Helferprofile => Set<Helferprofil>();

    public DbSet<Anfrage> Anfragen => Set<Anfrage>();

    protected override void OnModelCreating(ModelBuilder modell)
    {
        // PostGIS wird über die Migrationen in der Datenbank aktiviert, nicht von Hand.
        modell.HasPostgresExtension("postgis");

        modell.Entity<Region>(region =>
        {
            region.ToTable("Regionen");
            region.Property(r => r.Kennung).HasMaxLength(50);
            region.Property(r => r.Name).HasMaxLength(100);
            region.HasIndex(r => r.Kennung).IsUnique();
            region.HasData(Stammdaten.Regionen);
        });

        modell.Entity<Konto>(konto =>
        {
            konto.ToTable("Konten");
            konto.Property(k => k.EMail).HasMaxLength(254);
            konto.Property(k => k.Anzeigename).HasMaxLength(100);
            konto.Property(k => k.Telefon).HasMaxLength(30);

            // Rollen als Text speichern, damit die Daten auch ohne Code lesbar bleiben.
            konto.Property(k => k.Rolle).HasConversion<string>().HasMaxLength(30);

            // Eine E-Mail-Adresse gehört zu genau einem Konto.
            konto.HasIndex(k => k.EMail).IsUnique();

            // Regionen mit Konten dürfen nicht gelöscht werden.
            konto.HasOne(k => k.Region).WithMany().HasForeignKey(k => k.RegionId).OnDelete(DeleteBehavior.Restrict);

            // Mandantenfilter: Gelesen wird nur die Region der laufenden Anfrage.
            konto.HasQueryFilter(k => _mandant.Uebergreifend || k.RegionId == _mandant.RegionId);
        });

        modell.Entity<Kategorie>(kategorie =>
        {
            kategorie.ToTable("Kategorien");
            kategorie.Property(k => k.Kennung).HasMaxLength(50);
            kategorie.Property(k => k.Bezeichnung).HasMaxLength(100);
            kategorie.HasIndex(k => k.Kennung).IsUnique();
            kategorie.HasMany(k => k.Skills).WithOne(s => s.Kategorie).HasForeignKey(s => s.KategorieId);
            kategorie.HasData(Stammdaten.Kategorien);
        });

        modell.Entity<Skill>(skill =>
        {
            skill.ToTable("Skills");
            skill.Property(s => s.Kennung).HasMaxLength(50);
            skill.Property(s => s.Bezeichnung).HasMaxLength(100);
            skill.HasIndex(s => s.Kennung).IsUnique();
            skill.HasData(Stammdaten.Skills);
        });

        modell.Entity<Helferprofil>(profil =>
        {
            profil.ToTable("Helferprofile");
            profil.Property(h => h.Postleitzahl).HasMaxLength(4);
            profil.Property(h => h.Ort).HasMaxLength(100);

            // Ein Konto hat höchstens ein Helferprofil.
            profil.HasOne(h => h.Konto).WithOne().HasForeignKey<Helferprofil>(h => h.KontoId).OnDelete(DeleteBehavior.Cascade);
            profil.HasOne(h => h.Region).WithMany().HasForeignKey(h => h.RegionId).OnDelete(DeleteBehavior.Restrict);

            // Zuordnung Profil zu Fähigkeiten in einer eigenen Tabelle.
            profil.HasMany(h => h.Skills).WithMany().UsingEntity(zuordnung => zuordnung.ToTable("HelferprofilSkills"));

            profil.HasQueryFilter(h => _mandant.Uebergreifend || h.RegionId == _mandant.RegionId);
        });

        modell.Entity<Anfrage>(anfrage =>
        {
            anfrage.ToTable("Anfragen");
            anfrage.Property(a => a.Freitext).HasMaxLength(2000);
            anfrage.Property(a => a.Postleitzahl).HasMaxLength(4);
            anfrage.Property(a => a.Ort).HasMaxLength(120);
            anfrage.Property(a => a.Adresse).HasMaxLength(250);
            anfrage.Property(a => a.Zeitfenster).HasMaxLength(120);
            anfrage.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            anfrage.Property(a => a.Prioritaet).HasConversion<string>().HasMaxLength(10);

            // Abfragen laufen immer je Region; der Index unterstützt die Mandantentrennung.
            anfrage.HasIndex(a => a.RegionId);

            anfrage.HasOne(a => a.Region).WithMany().HasForeignKey(a => a.RegionId).OnDelete(DeleteBehavior.Restrict);
            anfrage.HasOne(a => a.Konto).WithMany().HasForeignKey(a => a.KontoId).OnDelete(DeleteBehavior.Restrict);
            anfrage.HasOne(a => a.Kategorie).WithMany().HasForeignKey(a => a.KategorieId).OnDelete(DeleteBehavior.Restrict);

            anfrage.HasQueryFilter(a => _mandant.Uebergreifend || a.RegionId == _mandant.RegionId);
        });
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        KontrolliereMandant();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        KontrolliereMandant();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    // Schreiben ist nur innerhalb der Region der laufenden Anfrage erlaubt; der Filter allein schützt nur das Lesen.
    private void KontrolliereMandant()
    {
        if (_mandant.Uebergreifend)
        {
            return;
        }

        foreach (var eintrag in ChangeTracker.Entries())
        {
            if (eintrag.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            Guid? region;
            switch (eintrag.Entity)
            {
                case Konto konto:
                    region = konto.RegionId;
                    break;
                case Helferprofil profil:
                    region = profil.RegionId;
                    break;
                case Anfrage anfrage:
                    region = anfrage.RegionId;
                    break;
                default:
                    continue;
            }

            if (region is null || region != _mandant.RegionId)
            {
                throw new MandantenVerletzung("Schreibzugriff ausserhalb der Region der Anfrage.");
            }
        }
    }
}
