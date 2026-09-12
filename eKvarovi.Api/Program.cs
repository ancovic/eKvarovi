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
