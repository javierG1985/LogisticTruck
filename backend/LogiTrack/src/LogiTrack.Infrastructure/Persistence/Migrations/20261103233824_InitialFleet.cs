using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogiTrack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialFleet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fleet");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "trucks",
                schema: "fleet",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plate = table.Column<string>(type: "varchar(6)", nullable: false),
                    brand = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    model_year = table.Column<int>(type: "integer", nullable: false),
                    capacity_kg = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    volume_m3 = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    soat_expires_on = table.Column<DateOnly>(type: "date", nullable: false),
                    tech_review_expires_on = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trucks", x => x.id);
                    table.CheckConstraint("ck_trucks_capacity_kg_positive", "capacity_kg > 0");
                    table.CheckConstraint("ck_trucks_volume_m3_positive", "volume_m3 > 0");
                });

            migrationBuilder.CreateIndex(
                name: "ix_trucks_plate",
                schema: "fleet",
                table: "trucks",
                column: "plate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trucks",
                schema: "fleet");
        }
    }
}
