using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogiTrack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTruckType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "truck_type",
                schema: "fleet",
                table: "trucks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Rigid");

            // El default solo rellena los camiones existentes; después se quita para que todo alta envíe su tipo.
            migrationBuilder.Sql("ALTER TABLE fleet.trucks ALTER COLUMN truck_type DROP DEFAULT;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "truck_type",
                schema: "fleet",
                table: "trucks");
        }
    }
}
