using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LogiTrack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "catalog_items",
                schema: "fleet",
                columns: table => new
                {
                    catalog_code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_catalog_items", x => new { x.catalog_code, x.code });
                });

            migrationBuilder.InsertData(
                schema: "fleet",
                table: "catalog_items",
                columns: new[] { "catalog_code", "code", "label", "sort_order" },
                values: new object[,]
                {
                    { "assignment_release_reasons", "DriverDeleted", "Conductor dado de baja", 5 },
                    { "assignment_release_reasons", "DriverStatusChanged", "Cambio de estado del conductor", 4 },
                    { "assignment_release_reasons", "Manual", "Liberación manual", 1 },
                    { "assignment_release_reasons", "TruckDeleted", "Camión dado de baja", 3 },
                    { "assignment_release_reasons", "TruckStatusChanged", "Cambio de estado del camión", 2 },
                    { "driver_statuses", "Active", "Activo", 1 },
                    { "driver_statuses", "Inactive", "Retirado", 3 },
                    { "driver_statuses", "OnLeave", "Vacaciones o incapacidad", 2 },
                    { "license_categories", "C1", "C1", 1 },
                    { "license_categories", "C2", "C2", 2 },
                    { "license_categories", "C3", "C3", 3 },
                    { "license_statuses", "Expired", "Vencida", 3 },
                    { "license_statuses", "ExpiringSoon", "Por vencer", 2 },
                    { "license_statuses", "Valid", "Vigente", 1 },
                    { "truck_statuses", "Available", "Disponible", 1 },
                    { "truck_statuses", "Inactive", "Inactivo", 4 },
                    { "truck_statuses", "InRoute", "En ruta", 2 },
                    { "truck_statuses", "Maintenance", "En mantenimiento", 3 },
                    { "truck_types", "Articulated", "Articulado", 3 },
                    { "truck_types", "Light", "Liviano", 1 },
                    { "truck_types", "Rigid", "Rígido", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "catalog_items",
                schema: "fleet");
        }
    }
}
