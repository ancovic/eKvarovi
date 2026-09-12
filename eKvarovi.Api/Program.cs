using eKvarovi.Api.Data;
using eKvarovi.Shared.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<EKvaroviDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
