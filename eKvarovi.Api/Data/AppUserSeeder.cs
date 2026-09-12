using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Data;

public static class AppUserSeeder
{
    public static async Task SeedAsync(EKvaroviDbContext db)
    {
        if (await db.AppUsers.AnyAsync())
        {
            return;
        }

        var hasher = new PasswordHasher<AppUser>();

        var employeeId = await db.Employees
            .Where(employee => employee.IsActive)
            .OrderBy(employee => employee.Id)
            .Select(employee => (int?)employee.Id)
            .FirstOrDefaultAsync();

        var technicianId = await db.Technicians
            .Where(technician => technician.IsActive)
            .OrderBy(technician => technician.Id)
            .Select(technician => (int?)technician.Id)
            .FirstOrDefaultAsync();

        if (!employeeId.HasValue)
        {
            throw new InvalidOperationException(
                "Nema aktivnog djelatnika za demo Reporter korisnika.");
        }

        if (!technicianId.HasValue)
        {
            throw new InvalidOperationException(
                "Nema aktivnog izvršitelja za demo Technician korisnika.");
        }

        var admin = new AppUser
        {
            Email = "admin@ekvarovi.local",
            DisplayName = "eKvarovi Admin"
        };

        admin.PasswordHash =
            hasher.HashPassword(
                admin,
                "Admin123!");

        var manager = new AppUser
        {
            Email = "manager@ekvarovi.local",
            DisplayName = "Demo upravitelj"
        };

        manager.PasswordHash =
            hasher.HashPassword(
                manager,
                "Manager123!");

        var reporter = new AppUser
        {
            Email = "reporter@ekvarovi.local",
            DisplayName = "Demo prijavitelj",
            EmployeeId = employeeId
        };

        reporter.PasswordHash =
            hasher.HashPassword(
                reporter,
                "Reporter123!");

        var technician = new AppUser
        {
            Email = "technician@ekvarovi.local",
            DisplayName = "Demo izvršitelj",
            TechnicianId = technicianId
        };

        technician.PasswordHash =
            hasher.HashPassword(
                technician,
                "Technician123!");

        db.AppUsers.AddRange(
            admin,
            manager,
            reporter,
            technician);

        await db.SaveChangesAsync();

        db.AppUserRoles.AddRange(
            new AppUserRole
            {
                AppUserId = admin.Id,
                AppRoleId = 1
            },
            new AppUserRole
            {
                AppUserId = admin.Id,
                AppRoleId = 2
            },

            new AppUserRole
            {
                AppUserId = manager.Id,
                AppRoleId = 1
            },
            new AppUserRole
            {
                AppUserId = manager.Id,
                AppRoleId = 3
            },

            new AppUserRole
            {
                AppUserId = technician.Id,
                AppRoleId = 1
            },
            new AppUserRole
            {
                AppUserId = technician.Id,
                AppRoleId = 4
            },

            new AppUserRole
            {
                AppUserId = reporter.Id,
                AppRoleId = 1
            },
            new AppUserRole
            {
                AppUserId = reporter.Id,
                AppRoleId = 5
            }
        );

        await db.SaveChangesAsync();
    }
}