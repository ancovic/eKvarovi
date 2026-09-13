using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<EKvaroviDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Unesi JWT token."
        });

    options.AddSecurityRequirement(
        document =>
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference(
                        "Bearer",
                        document)
                ] = []
            });
});

var jwtSection =
    builder.Configuration.GetSection(
        JwtOptions.SectionName);

var jwtOptions =
    jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "Nedostaje Jwt konfiguracija.");

if (jwtOptions.SigningKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:SigningKey mora imati najmanje 32 znaka.");
}

builder.Services.Configure<JwtOptions>(
    jwtSection);

builder.Services.AddScoped<JwtTokenService>();

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer =
                    jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience =
                    jwtOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtOptions.SigningKey)),

                ValidateLifetime = true,

                ClockSkew =
                    TimeSpan.FromSeconds(30)
            };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

    options.AddPolicy(
        AuthorizationPolicies.AdminOnly,
        policy =>
            policy.RequireRole("Admin"));

    options.AddPolicy(
        AuthorizationPolicies.Management,
        policy =>
            policy.RequireRole(
                "Admin",
                "Manager"));

    options.AddPolicy(
        AuthorizationPolicies.ReporterOnly,
        policy =>
            policy.RequireRole(
                "Reporter"));

    options.AddPolicy(
        AuthorizationPolicies.TechnicianOnly,
        policy =>
            policy.RequireRole(
                "Technician"));
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EKvaroviDbContext>();

    await db.Database.MigrateAsync();

    if (!await db.Locations.AnyAsync())
    {
        db.Locations.AddRange(

            new Location
            {
                Name = "Gradska uprava Zagreb",
                Address = "Trg Stjepana Radića 1",
                City = "Zagreb",
                IsActive = true,
                LocationTypeId = 1
            },
            new Location
            {
                Name = "Osnovna škola Centar",
                Address = "Vukovarska 15",
                City = "Split",
                IsActive = true,
                LocationTypeId = 2
            },
            new Location
            {
                Name = "Dom zdravlja Maksimir",
                Address = "Maksimirska 81",
                City = "Zagreb",
                IsActive = true,
                LocationTypeId = 3
            },
            new Location
            {
                Name = "Centralno skladište",
                Address = "Industrijska cesta 10",
                City = "Velika Gorica",
                IsActive = true,
                LocationTypeId = 4
            },
            new Location
            {
                Name = "Stara upravna zgrada",
                Address = "Savska cesta 25",
                City = "Zagreb",
                IsActive = false,
                LocationTypeId = 1
            }
        );
    }

    await db.SaveChangesAsync();

    var locationIds = await db.Locations
        .Where(location => location.IsActive)
        .OrderBy(location => location.Name)
        .Select(location => location.Id)
        .Take(4)
        .ToListAsync();

    if (!await db.Employees.AnyAsync() && locationIds.Count >= 4)
    {
        db.Employees.AddRange(
            new Employee
            {
                FirstName = "Ivan",
                LastName = "Horvat",
                Email = "ivan.horvat@ekvarovi.local",
                Phone = "091 111 2233",
                IsActive = true,
                LocationId = locationIds[0]
            },
            new Employee
            {
                FirstName = "Marija",
                LastName = "Kovač",
                Email = "marija.kovac@ekvarovi.local",
                Phone = "098 222 3344",
                IsActive = true,
                LocationId = locationIds[1]
            },
            new Employee
            {
                FirstName = "Petar",
                LastName = "Marić",
                Email = "petar.maric@ekvarovi.local",
                Phone = "095 333 4455",
                IsActive = true,
                LocationId = locationIds[2]
            },
            new Employee
            {
                FirstName = "Ana",
                LastName = "Jurić",
                Email = "ana.juric@ekvarovi.local",
                Phone = "092 444 5566",
                IsActive = true,
                LocationId = locationIds[3]
            },
            new Employee
            {
                FirstName = "Marko",
                LastName = "Radić",
                Email = "marko.radic@ekvarovi.local",
                Phone = "099 555 6677",
                IsActive = false,
                LocationId = locationIds[0]
            }
        );
    }

    if (!await db.Technicians.AnyAsync())
    {
        db.Technicians.AddRange(
            new Technician
            {
                FirstName = "Tomislav",
                LastName = "Babić",
                Email = "tomislav.babic@ekvarovi.local",
                Phone = "091 222 1100",
                Specialization = "Elektrika",
                IsActive = true
            },
            new Technician
            {
                FirstName = "Nikola",
                LastName = "Perić",
                Email = "nikola.peric@ekvarovi.local",
                Phone = "098 333 2200",
                Specialization = "Vodoinstalacije",
                IsActive = true
            },
            new Technician
            {
                FirstName = "Mario",
                LastName = "Jurić",
                Email = "mario.juric@ekvarovi.local",
                Phone = "095 444 3300",
                Specialization = "Grijanje",
                IsActive = true
            },
            new Technician
            {
                FirstName = "Luka",
                LastName = "Radić",
                Email = "luka.radic@ekvarovi.local",
                Phone = "092 555 4400",
                Specialization = "Računalstvo i mreža",
                IsActive = true
            },
            new Technician
            {
                FirstName = "Davor",
                LastName = "Kovač",
                Email = "davor.kovac@ekvarovi.local",
                Phone = "099 666 5500",
                Specialization = "Građevinski radovi",
                IsActive = false
            }
        );
    }

    await db.SaveChangesAsync();

    var reportEmployees = await db.Employees
    .Include(employee => employee.Location)
    .Where(employee =>
        employee.IsActive &&
        employee.Location != null &&
        employee.Location.IsActive)
    .OrderBy(employee => employee.LastName)
    .ThenBy(employee => employee.FirstName)
    .Take(4)
    .ToListAsync();

    if (!await db.FaultReports.AnyAsync() &&
        reportEmployees.Count >= 3)
    {
        var receivedStatusId = await db.FaultStatuses
            .Where(status => status.Name == "Zaprimljeno")
            .Select(status => status.Id)
            .FirstAsync();

        var reviewedStatusId = await db.FaultStatuses
            .Where(status => status.Name == "Pregledano")
            .Select(status => status.Id)
            .FirstAsync();

        var waterTypeId = await db.FaultTypes
            .Where(type => type.Name == "Voda")
            .Select(type => type.Id)
            .FirstAsync();

        var networkTypeId = await db.FaultTypes
            .Where(type => type.Name == "Mreža")
            .Select(type => type.Id)
            .FirstAsync();

        var heatingTypeId = await db.FaultTypes
            .Where(type => type.Name == "Grijanje")
            .Select(type => type.Id)
            .FirstAsync();

        var mediumPriorityId = await db.FaultPriorities
            .Where(priority => priority.Name == "Srednji")
            .Select(priority => priority.Id)
            .FirstAsync();

        var highPriorityId = await db.FaultPriorities
            .Where(priority => priority.Name == "Visok")
            .Select(priority => priority.Id)
            .FirstAsync();

        var criticalPriorityId = await db.FaultPriorities
            .Where(priority => priority.Name == "Kritičan")
            .Select(priority => priority.Id)
            .FirstAsync();

        db.FaultReports.AddRange(
            new FaultReport
            {
                Title = "Ne radi klima uređaj",
                Description = "Klima uređaj u uredu se ne uključuje.",
                CreatedAt = new DateTime(
                    2026, 9, 5, 8, 30, 0,
                    DateTimeKind.Utc),
                LocationId = reportEmployees[0].LocationId,
                ReporterId = reportEmployees[0].Id,
                FaultStatusId = receivedStatusId
            },
            new FaultReport
            {
                Title = "Neispravna rasvjeta na stubištu",
                Description = "Rasvjeta između prvog i drugog kata ne radi.",
                CreatedAt = new DateTime(
                    2026, 9, 6, 10, 15, 0,
                    DateTimeKind.Utc),
                LocationId = reportEmployees[1].LocationId,
                ReporterId = reportEmployees[1].Id,
                FaultStatusId = receivedStatusId
            },
            new FaultReport
            {
                Title = "Curenje vode u sanitarnom čvoru",
                Description = "Voda curi ispod umivaonika.",
                CreatedAt = new DateTime(
                    2026, 9, 7, 7, 45, 0,
                    DateTimeKind.Utc),
                DueDate = new DateTime(2026, 9, 14),
                LocationId = reportEmployees[2].LocationId,
                ReporterId = reportEmployees[2].Id,
                FaultStatusId = reviewedStatusId,
                FaultTypeId = waterTypeId,
                FaultPriorityId = highPriorityId
            },
            new FaultReport
            {
                Title = "Prekid mrežne veze",
                Description = "Računala u jednom uredu nemaju pristup mreži.",
                CreatedAt = new DateTime(
                    2026, 9, 8, 12, 20, 0,
                    DateTimeKind.Utc),
                DueDate = new DateTime(2026, 9, 18),
                LocationId = reportEmployees[0].LocationId,
                ReporterId = reportEmployees[0].Id,
                FaultStatusId = reviewedStatusId,
                FaultTypeId = networkTypeId,
                FaultPriorityId = mediumPriorityId
            },
            new FaultReport
            {
                Title = "Kvar sustava grijanja",
                Description = "Grijanje ne radi u većem dijelu objekta.",
                CreatedAt = new DateTime(
                    2026, 9, 9, 6, 50, 0,
                    DateTimeKind.Utc),
                DueDate = new DateTime(2026, 9, 13),
                LocationId = reportEmployees[2].LocationId,
                ReporterId = reportEmployees[2].Id,
                FaultStatusId = reviewedStatusId,
                FaultTypeId = heatingTypeId,
                FaultPriorityId = criticalPriorityId
            }
        );
    }

    if (!await db.Materials.AnyAsync())
    {
        db.Materials.AddRange(
            new Material
            {
                Name = "LED žarulja",
                Description = "LED žarulja za unutarnju rasvjetu.",
                IsActive = true,
                MaterialUnitId = 1
            },
            new Material
            {
                Name = "Osigurač 16 A",
                Description = "Električni automatski osigurač.",
                IsActive = true,
                MaterialUnitId = 1
            },
            new Material
            {
                Name = "Električni kabel",
                Description = "Električni instalacijski kabel.",
                IsActive = true,
                MaterialUnitId = 2
            },
            new Material
            {
                Name = "Termostatski ventil",
                Description = "Ventil za sustav grijanja.",
                IsActive = true,
                MaterialUnitId = 1
            },
            new Material
            {
                Name = "Bakrena cijev",
                Description = "Cijev za vodovodne i grijaće instalacije.",
                IsActive = true,
                MaterialUnitId = 2
            },
            new Material
            {
                Name = "Rashladna tekućina",
                Description = "Tekućina za servis rashladnih sustava.",
                IsActive = true,
                MaterialUnitId = 3
            },
            new Material
            {
                Name = "Građevinska masa",
                Description = "Masa za manje građevinske popravke.",
                IsActive = true,
                MaterialUnitId = 4
            },
            new Material
            {
                Name = "Set vijaka",
                Description = "Paket vijaka za tehničke popravke.",
                IsActive = true,
                MaterialUnitId = 5
            }
        );
    }

    await db.SaveChangesAsync();

    await AppUserSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
