using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LogiTrack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersRolesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "security");

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "security",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "security",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "security",
                columns: table => new
                {
                    role_code = table.Column<string>(type: "character varying(60)", nullable: false),
                    permission_code = table.Column<string>(type: "character varying(60)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => new { x.role_code, x.permission_code });
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_code",
                        column: x => x.permission_code,
                        principalSchema: "security",
                        principalTable: "permissions",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_code",
                        column: x => x.role_code,
                        principalSchema: "security",
                        principalTable: "roles",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    role_code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.CheckConstraint("ck_users_email_lowercase", "email = lower(email)");
                    table.ForeignKey(
                        name: "fk_users_roles_role_code",
                        column: x => x.role_code,
                        principalSchema: "security",
                        principalTable: "roles",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "security",
                table: "permissions",
                columns: new[] { "code", "description" },
                values: new object[,]
                {
                    { "assignments.read", "Consultar asignaciones" },
                    { "assignments.write", "Asignar y liberar conductores de camiones" },
                    { "drivers.read", "Consultar conductores" },
                    { "drivers.write", "Registrar, editar, renovar licencia, cambiar estado y dar de baja conductores" },
                    { "trucks.read", "Consultar camiones" },
                    { "trucks.write", "Registrar, editar, cambiar estado y dar de baja camiones" },
                    { "users.manage", "Crear usuarios" }
                });

            migrationBuilder.InsertData(
                schema: "security",
                table: "roles",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "Administrator", "Administrador" },
                    { "Driver", "Conductor" },
                    { "Operator", "Operador" }
                });

            migrationBuilder.InsertData(
                schema: "security",
                table: "role_permissions",
                columns: new[] { "permission_code", "role_code" },
                values: new object[,]
                {
                    { "assignments.read", "Administrator" },
                    { "assignments.write", "Administrator" },
                    { "drivers.read", "Administrator" },
                    { "drivers.write", "Administrator" },
                    { "trucks.read", "Administrator" },
                    { "trucks.write", "Administrator" },
                    { "users.manage", "Administrator" },
                    { "assignments.read", "Driver" },
                    { "drivers.read", "Driver" },
                    { "trucks.read", "Driver" },
                    { "assignments.read", "Operator" },
                    { "assignments.write", "Operator" },
                    { "drivers.read", "Operator" },
                    { "drivers.write", "Operator" },
                    { "trucks.read", "Operator" },
                    { "trucks.write", "Operator" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_code",
                schema: "security",
                table: "role_permissions",
                column: "permission_code");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "security",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_role_code",
                schema: "security",
                table: "users",
                column: "role_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "security");

            migrationBuilder.DropTable(
                name: "users",
                schema: "security");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "security");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "security");
        }
    }
}
