using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogiTrack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTruckAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "truck_assignments",
                schema: "fleet",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    released_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    release_reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_truck_assignments", x => x.id);
                    table.CheckConstraint("ck_truck_assignments_release_consistent", "(released_at IS NULL) = (release_reason IS NULL)");
                    table.ForeignKey(
                        name: "fk_truck_assignments_drivers_driver_id",
                        column: x => x.driver_id,
                        principalSchema: "fleet",
                        principalTable: "drivers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_truck_assignments_trucks_truck_id",
                        column: x => x.truck_id,
                        principalSchema: "fleet",
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_truck_assignments_active_driver_id",
                schema: "fleet",
                table: "truck_assignments",
                column: "driver_id",
                unique: true,
                filter: "released_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_truck_assignments_active_truck_id",
                schema: "fleet",
                table: "truck_assignments",
                column: "truck_id",
                unique: true,
                filter: "released_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "truck_assignments",
                schema: "fleet");
        }
    }
}
