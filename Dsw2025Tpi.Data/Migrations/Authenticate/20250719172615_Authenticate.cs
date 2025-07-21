using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Dsw2025Tpi.Data.Migrations.Authenticate
{
    /// <inheritdoc />
    public partial class Authenticate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Crear tabla Roles para gestionar roles de usuarios
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false), // PK Id como string
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true), // Nombre del rol
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true), // Nombre normalizado (mayúsculas)
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true) // Control de concurrencia
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            // Crear tabla Usuarios para almacenar datos de usuarios
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false), // PK Id como string
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true), // Nombre de usuario
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true), // Nombre de usuario normalizado
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true), // Email
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true), // Email normalizado
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false), // Confirmación de email
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true), // Hash de la contraseña
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true), // Marca de seguridad para validar sesiones
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true), // Control de concurrencia
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true), // Teléfono
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false), // Confirmación teléfono
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false), // 2FA activado
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true), // Fecha fin de bloqueo
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false), // Si está bloqueado
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false) // Conteo de accesos fallidos
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            // Tabla RolesClaims para almacenar claims asociados a roles
            migrationBuilder.CreateTable(
                name: "RolesClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"), // PK autoincremental
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false), // FK a Roles
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true), // Tipo de claim
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true) // Valor del claim
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolesClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Tabla UsuariosClaims para claims asociados a usuarios
            migrationBuilder.CreateTable(
                name: "UsuariosClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"), // PK autoincremental
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false), // FK a Usuarios
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true), // Tipo de claim
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true) // Valor del claim
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosClaims_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Tabla UsuariosLogins para proveedores externos de login (ej. Google, Facebook)
            migrationBuilder.CreateTable(
                name: "UsuariosLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false), // PK compuesta
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false), // PK compuesta
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true), // Nombre proveedor
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false) // FK a Usuarios
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UsuariosLogins_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Tabla UsuariosRoles para relacionar usuarios con roles (muchos a muchos)
            migrationBuilder.CreateTable(
                name: "UsuariosRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false), // FK a Usuarios
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false) // FK a Roles
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosRoles", x => new { x.UserId, x.RoleId }); // PK compuesta
                    table.ForeignKey(
                        name: "FK_UsuariosRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuariosRoles_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Tabla UsuariosTokens para tokens asociados a usuarios (ej. refresh tokens)
            migrationBuilder.CreateTable(
                name: "UsuariosTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false), // FK a Usuarios
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false), // PK compuesta
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false), // PK compuesta
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true) // Valor del token
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosTokens", x => new { x.UserId, x.LoginProvider, x.Name }); // PK compuesta
                    table.ForeignKey(
                        name: "FK_UsuariosTokens_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Insertar roles iniciales en la tabla Roles
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "4632eea2-4d43-47ed-b736-0ccd85664371", null, "Customer", "CUSTOMER" },
                    { "f936a0de-4c11-4c82-b2f9-38cd193514ed", null, "Admin", "ADMIN" }
                });

            // Índice único para evitar roles con nombre duplicado
            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            // Índices para mejorar rendimiento en consultas con claves foráneas
            migrationBuilder.CreateIndex(
                name: "IX_RolesClaims_RoleId",
                table: "RolesClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Usuarios",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Usuarios",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosClaims_UserId",
                table: "UsuariosClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosLogins_UserId",
                table: "UsuariosLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosRoles_RoleId",
                table: "UsuariosRoles",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar tablas en orden inverso para evitar conflictos FK
            migrationBuilder.DropTable(name: "RolesClaims");
            migrationBuilder.DropTable(name: "UsuariosClaims");
            migrationBuilder.DropTable(name: "UsuariosLogins");
            migrationBuilder.DropTable(name: "UsuariosRoles");
            migrationBuilder.DropTable(name: "UsuariosTokens");
            migrationBuilder.DropTable(name: "Roles");
            migrationBuilder.DropTable(name: "Usuarios");
        }
    }
}
