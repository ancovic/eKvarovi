using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace eKvarovi.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaultPriorities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaultPriorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaultStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaultStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaultTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaultTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterventionStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterventionStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaterialUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Technicians",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Specialization = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Technicians", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    City = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LocationTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_LocationTypes_LocationTypeId",
                        column: x => x.LocationTypeId,
                        principalTable: "LocationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaterialUnitId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Materials_MaterialUnits_MaterialUnitId",
                        column: x => x.MaterialUnitId,
                        principalTable: "MaterialUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmployeeId = table.Column<int>(type: "INTEGER", nullable: true),
                    TechnicianId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUsers_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AppUsers_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FaultReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReporterId = table.Column<int>(type: "INTEGER", nullable: false),
                    FaultStatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    FaultTypeId = table.Column<int>(type: "INTEGER", nullable: true),
                    FaultPriorityId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaultReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaultReports_Employees_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaultReports_FaultPriorities_FaultPriorityId",
                        column: x => x.FaultPriorityId,
                        principalTable: "FaultPriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaultReports_FaultStatuses_FaultStatusId",
                        column: x => x.FaultStatusId,
                        principalTable: "FaultStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaultReports_FaultTypes_FaultTypeId",
                        column: x => x.FaultTypeId,
                        principalTable: "FaultTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaultReports_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppUserRoles",
                columns: table => new
                {
                    AppUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    AppRoleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserRoles", x => new { x.AppUserId, x.AppRoleId });
                    table.ForeignKey(
                        name: "FK_AppUserRoles_AppRoles_AppRoleId",
                        column: x => x.AppRoleId,
                        principalTable: "AppRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppUserRoles_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssignedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    FaultReportId = table.Column<int>(type: "INTEGER", nullable: false),
                    TechnicianId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkAssignments_FaultReports_FaultReportId",
                        column: x => x.FaultReportId,
                        principalTable: "FaultReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkAssignments_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Interventions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    WorkNote = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WorkAssignmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    InterventionStatusId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interventions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interventions_InterventionStatuses_InterventionStatusId",
                        column: x => x.InterventionStatusId,
                        principalTable: "InterventionStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interventions_WorkAssignments_WorkAssignmentId",
                        column: x => x.WorkAssignmentId,
                        principalTable: "WorkAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FaultMedia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OriginalFileName = table.Column<string>(type: "TEXT", nullable: false),
                    StoredFileName = table.Column<string>(type: "TEXT", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    AttachmentPurpose = table.Column<int>(type: "INTEGER", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FaultReportId = table.Column<int>(type: "INTEGER", nullable: false),
                    InterventionId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaultMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaultMedia_FaultReports_FaultReportId",
                        column: x => x.FaultReportId,
                        principalTable: "FaultReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaultMedia_Interventions_InterventionId",
                        column: x => x.InterventionId,
                        principalTable: "Interventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InterventionMaterials",
                columns: table => new
                {
                    InterventionId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaterialId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterventionMaterials", x => new { x.InterventionId, x.MaterialId });
                    table.ForeignKey(
                        name: "FK_InterventionMaterials_Interventions_InterventionId",
                        column: x => x.InterventionId,
                        principalTable: "Interventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterventionMaterials_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AppRoles",
                columns: new[] { "Id", "DisplayName", "Name" },
                values: new object[,]
                {
                    { 1, "Korisnik", "User" },
                    { 2, "Administrator", "Admin" },
                    { 3, "Upravitelj", "Manager" },
                    { 4, "Izvršitelj", "Technician" },
                    { 5, "Prijavitelj", "Reporter" }
                });

            migrationBuilder.InsertData(
                table: "FaultPriorities",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Nizak" },
                    { 2, "Srednji" },
                    { 3, "Visok" },
                    { 4, "Kritičan" }
                });

            migrationBuilder.InsertData(
                table: "FaultStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Zaprimljeno" },
                    { 2, "Pregledano" },
                    { 3, "Dodijeljeno" },
                    { 4, "U radu" },
                    { 5, "Riješeno" },
                    { 6, "Zatvoreno" }
                });

            migrationBuilder.InsertData(
                table: "FaultTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Elektrika" },
                    { 2, "Voda" },
                    { 3, "Grijanje" },
                    { 4, "Mreža" },
                    { 5, "Građevinski radovi" },
                    { 6, "Ostalo" }
                });

            migrationBuilder.InsertData(
                table: "InterventionStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Planirana" },
                    { 2, "U tijeku" },
                    { 3, "Završena" },
                    { 4, "Neuspješna" }
                });

            migrationBuilder.InsertData(
                table: "LocationTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Upravna zgrada" },
                    { 2, "Škola" },
                    { 3, "Zdravstvena ustanova" },
                    { 4, "Skladište" }
                });

            migrationBuilder.InsertData(
                table: "MaterialUnits",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Komad" },
                    { 2, "Metar" },
                    { 3, "Litra" },
                    { 4, "Kilogram" },
                    { 5, "Paket" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppRoles_Name",
                table: "AppRoles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRoles_AppRoleId",
                table: "AppUserRoles",
                column: "AppRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_Email",
                table: "AppUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_EmployeeId",
                table: "AppUsers",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_TechnicianId",
                table: "AppUsers",
                column: "TechnicianId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_LocationId",
                table: "Employees",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultMedia_FaultReportId",
                table: "FaultMedia",
                column: "FaultReportId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultMedia_InterventionId",
                table: "FaultMedia",
                column: "InterventionId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultReports_FaultPriorityId",
                table: "FaultReports",
                column: "FaultPriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultReports_FaultStatusId",
                table: "FaultReports",
                column: "FaultStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultReports_FaultTypeId",
                table: "FaultReports",
                column: "FaultTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultReports_LocationId",
                table: "FaultReports",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultReports_ReporterId",
                table: "FaultReports",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_InterventionMaterials_MaterialId",
                table: "InterventionMaterials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_InterventionStatusId",
                table: "Interventions",
                column: "InterventionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_WorkAssignmentId",
                table: "Interventions",
                column: "WorkAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_LocationTypeId",
                table: "Locations",
                column: "LocationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_MaterialUnitId",
                table: "Materials",
                column: "MaterialUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkAssignments_FaultReportId",
                table: "WorkAssignments",
                column: "FaultReportId",
                unique: true,
                filter: "\"IsActive\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_WorkAssignments_TechnicianId",
                table: "WorkAssignments",
                column: "TechnicianId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUserRoles");

            migrationBuilder.DropTable(
                name: "FaultMedia");

            migrationBuilder.DropTable(
                name: "InterventionMaterials");

            migrationBuilder.DropTable(
                name: "AppRoles");

            migrationBuilder.DropTable(
                name: "AppUsers");

            migrationBuilder.DropTable(
                name: "Interventions");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "InterventionStatuses");

            migrationBuilder.DropTable(
                name: "WorkAssignments");

            migrationBuilder.DropTable(
                name: "MaterialUnits");

            migrationBuilder.DropTable(
                name: "FaultReports");

            migrationBuilder.DropTable(
                name: "Technicians");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "FaultPriorities");

            migrationBuilder.DropTable(
                name: "FaultStatuses");

            migrationBuilder.DropTable(
                name: "FaultTypes");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "LocationTypes");
        }
    }
}
