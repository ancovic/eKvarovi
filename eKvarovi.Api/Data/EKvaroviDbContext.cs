using eKvarovi.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Data;

public class EKvaroviDbContext : DbContext
{
    public EKvaroviDbContext(DbContextOptions<EKvaroviDbContext> options)
        : base(options)
    {
    }

    public DbSet<Location> Locations => Set<Location>();
    public DbSet<LocationType> LocationTypes => Set<LocationType>();

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Technician> Technicians => Set<Technician>();

    public DbSet<FaultReport> FaultReports => Set<FaultReport>();
    public DbSet<FaultType> FaultTypes => Set<FaultType>();
    public DbSet<FaultPriority> FaultPriorities => Set<FaultPriority>();
    public DbSet<FaultStatus> FaultStatuses => Set<FaultStatus>();

    public DbSet<WorkAssignment> WorkAssignments => Set<WorkAssignment>();

    public DbSet<Intervention> Interventions => Set<Intervention>();
    public DbSet<InterventionStatus> InterventionStatuses => Set<InterventionStatus>();

    public DbSet<Material> Materials => Set<Material>();
    public DbSet<MaterialUnit> MaterialUnits => Set<MaterialUnit>();
    public DbSet<InterventionMaterial> InterventionMaterials => Set<InterventionMaterial>();

    public DbSet<FaultMedia> FaultMedia => Set<FaultMedia>();

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppRole> AppRoles => Set<AppRole>();
    public DbSet<AppUserRole> AppUserRoles => Set<AppUserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Location>()
            .HasOne(location => location.LocationType)
            .WithMany(type => type.Locations)
            .HasForeignKey(location => location.LocationTypeId)
            .OnDelete(DeleteBehavior.Restrict);



        modelBuilder.Entity<Employee>()
            .HasOne(employee => employee.Location)
            .WithMany(location => location.Employees)
            .HasForeignKey(employee => employee.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        

        modelBuilder.Entity<FaultReport>()
            .HasOne(report => report.Location)
            .WithMany(location => location.FaultReports)
            .HasForeignKey(report => report.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FaultReport>()
            .HasOne(report => report.Reporter)
            .WithMany(employee => employee.ReportedFaults)
            .HasForeignKey(report => report.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FaultReport>()
            .HasOne(report => report.FaultStatus)
            .WithMany(status => status.FaultReports)
            .HasForeignKey(report => report.FaultStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FaultReport>()
            .HasOne(report => report.FaultType)
            .WithMany(type => type.FaultReports)
            .HasForeignKey(report => report.FaultTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FaultReport>()
            .HasOne(report => report.FaultPriority)
            .WithMany(priority => priority.FaultReports)
            .HasForeignKey(report => report.FaultPriorityId)
            .OnDelete(DeleteBehavior.Restrict);

        

        modelBuilder.Entity<WorkAssignment>()
            .HasOne(assignment => assignment.FaultReport)
            .WithMany(report => report.Assignments)
            .HasForeignKey(assignment => assignment.FaultReportId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkAssignment>()
            .HasOne(assignment => assignment.Technician)
            .WithMany(technician => technician.WorkAssignments)
            .HasForeignKey(assignment => assignment.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict);

        // Jedna prijava smije imati samo jednu aktivnu dodjelu.
        modelBuilder.Entity<WorkAssignment>()
            .HasIndex(assignment => assignment.FaultReportId)
            .IsUnique()
            .HasFilter("\"IsActive\" = 1");

        

        modelBuilder.Entity<Intervention>()
            .HasOne(intervention => intervention.WorkAssignment)
            .WithMany(assignment => assignment.Interventions)
            .HasForeignKey(intervention => intervention.WorkAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Intervention>()
            .HasOne(intervention => intervention.InterventionStatus)
            .WithMany(status => status.Interventions)
            .HasForeignKey(intervention => intervention.InterventionStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        

        modelBuilder.Entity<Material>()
            .HasOne(material => material.MaterialUnit)
            .WithMany(unit => unit.Materials)
            .HasForeignKey(material => material.MaterialUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InterventionMaterial>()
            .HasKey(interventionMaterial => new
            {
                interventionMaterial.InterventionId,
                interventionMaterial.MaterialId
            });

        modelBuilder.Entity<InterventionMaterial>()
            .HasOne(interventionMaterial => interventionMaterial.Intervention)
            .WithMany(intervention => intervention.Materials)
            .HasForeignKey(interventionMaterial => interventionMaterial.InterventionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InterventionMaterial>()
            .HasOne(interventionMaterial => interventionMaterial.Material)
            .WithMany(material => material.Interventions)
            .HasForeignKey(interventionMaterial => interventionMaterial.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        

        modelBuilder.Entity<FaultMedia>()
            .HasOne(media => media.FaultReport)
            .WithMany(report => report.Media)
            .HasForeignKey(media => media.FaultReportId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FaultMedia>()
            .HasOne(media => media.Intervention)
            .WithMany(intervention => intervention.Media)
            .HasForeignKey(media => media.InterventionId)
            .OnDelete(DeleteBehavior.Restrict);

        

        modelBuilder.Entity<AppUser>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasIndex(user => user.EmployeeId)
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasIndex(user => user.TechnicianId)
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasOne(user => user.Employee)
            .WithMany()
            .HasForeignKey(user => user.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AppUser>()
            .HasOne(user => user.Technician)
            .WithMany()
            .HasForeignKey(user => user.TechnicianId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AppRole>()
            .HasIndex(role => role.Name)
            .IsUnique();

        modelBuilder.Entity<AppUserRole>()
            .HasKey(userRole => new
            {
                userRole.AppUserId,
                userRole.AppRoleId
            });

        modelBuilder.Entity<AppUserRole>()
            .HasOne(userRole => userRole.AppUser)
            .WithMany(user => user.UserRoles)
            .HasForeignKey(userRole => userRole.AppUserId);

        modelBuilder.Entity<AppUserRole>()
            .HasOne(userRole => userRole.AppRole)
            .WithMany(role => role.UserRoles)
            .HasForeignKey(userRole => userRole.AppRoleId);

        //------------- SEEDS ------------------------------

        modelBuilder.Entity<LocationType>().HasData(
            new LocationType { Id = 1, Name = "Upravna zgrada" },
            new LocationType { Id = 2, Name = "Škola" },
            new LocationType { Id = 3, Name = "Zdravstvena ustanova" },
            new LocationType { Id = 4, Name = "Skladište" }
        );

        modelBuilder.Entity<FaultType>().HasData(
            new FaultType { Id = 1, Name = "Elektrika" },
            new FaultType { Id = 2, Name = "Voda" },
            new FaultType { Id = 3, Name = "Grijanje" },
            new FaultType { Id = 4, Name = "Mreža" },
            new FaultType { Id = 5, Name = "Građevinski radovi" },
            new FaultType { Id = 6, Name = "Ostalo" }
        );

        modelBuilder.Entity<FaultPriority>().HasData(
            new FaultPriority { Id = 1, Name = "Nizak" },
            new FaultPriority { Id = 2, Name = "Srednji" },
            new FaultPriority { Id = 3, Name = "Visok" },
            new FaultPriority { Id = 4, Name = "Kritičan" }
        );

        modelBuilder.Entity<FaultStatus>().HasData(
            new FaultStatus { Id = 1, Name = "Zaprimljeno" },
            new FaultStatus { Id = 2, Name = "Pregledano" },
            new FaultStatus { Id = 3, Name = "Dodijeljeno" },
            new FaultStatus { Id = 4, Name = "U radu" },
            new FaultStatus { Id = 5, Name = "Riješeno" },
            new FaultStatus { Id = 6, Name = "Zatvoreno" }
        );

        modelBuilder.Entity<InterventionStatus>().HasData(
            new InterventionStatus { Id = 1, Name = "Planirana" },
            new InterventionStatus { Id = 2, Name = "U tijeku" },
            new InterventionStatus { Id = 3, Name = "Završena" },
            new InterventionStatus { Id = 4, Name = "Neuspješna" }
        );

        modelBuilder.Entity<MaterialUnit>().HasData(
            new MaterialUnit { Id = 1, Name = "Komad" },
            new MaterialUnit { Id = 2, Name = "Metar" },
            new MaterialUnit { Id = 3, Name = "Litra" },
            new MaterialUnit { Id = 4, Name = "Kilogram" },
            new MaterialUnit { Id = 5, Name = "Paket" }
        );

        modelBuilder.Entity<AppRole>().HasData(
            new AppRole { Id = 1, Name = "User", DisplayName = "Korisnik" },
            new AppRole { Id = 2, Name = "Admin", DisplayName = "Administrator" },
            new AppRole { Id = 3, Name = "Manager", DisplayName = "Upravitelj" },
            new AppRole { Id = 4, Name = "Technician", DisplayName = "Izvršitelj" },
            new AppRole { Id = 5, Name = "Reporter", DisplayName = "Prijavitelj" }
        );
    }
}