using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Datenzugriff;

/// <summary>
/// Datenbankkontext der Plattform (PostgreSQL mit PostGIS).
/// </summary>
public sealed class SoforthilfeDbContext(DbContextOptions<SoforthilfeDbContext> optionen) : DbContext(optionen)
{
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
        });

        modell.Entity<Anfrage>(anfrage =>
        {
            anfrage.ToTable("Anfragen");
            anfrage.Property(a => a.Freitext).HasMaxLength(2000);
            anfrage.Property(a => a.Postleitzahl).HasMaxLength(4);
            anfrage.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);

            // Abfragen laufen immer je Region; der Index unterstützt die Mandantentrennung.
            anfrage.HasIndex(a => a.RegionId);

            anfrage.HasOne(a => a.Region).WithMany().HasForeignKey(a => a.RegionId).OnDelete(DeleteBehavior.Restrict);
            anfrage.HasOne(a => a.Konto).WithMany().HasForeignKey(a => a.KontoId).OnDelete(DeleteBehavior.Restrict);
            anfrage.HasOne(a => a.Kategorie).WithMany().HasForeignKey(a => a.KategorieId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
