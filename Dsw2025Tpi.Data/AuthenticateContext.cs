using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Dsw2025Tpi.Data
{
    // Contexto de EF para manejar la autenticación y autorización con Identity
    public class AuthenticateContext : IdentityDbContext
    {
        // Constructor que recibe opciones de configuración para el contexto
        public AuthenticateContext(DbContextOptions<AuthenticateContext> options)
            : base(options)
        {
        }

        // Método para configurar el modelo y la estructura de las tablas en la base de datos
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cambiar los nombres de las tablas predeterminadas de Identity a nombres personalizados
            builder.Entity<IdentityUser>(b => { b.ToTable("Usuarios"); });               // Tabla de usuarios
            builder.Entity<IdentityRole>(b => { b.ToTable("Roles"); });                   // Tabla de roles
            builder.Entity<IdentityUserRole<string>>(b => { b.ToTable("UsuariosRoles"); });       // Tabla de relación usuario-rol
            builder.Entity<IdentityUserClaim<string>>(b => { b.ToTable("UsuariosClaims"); });     // Tabla de claims de usuario
            builder.Entity<IdentityUserLogin<string>>(b => { b.ToTable("UsuariosLogins"); });     // Tabla de logins externos
            builder.Entity<IdentityRoleClaim<string>>(b => { b.ToTable("RolesClaims"); });         // Tabla de claims de roles
            builder.Entity<IdentityUserToken<string>>(b => { b.ToTable("UsuariosTokens"); });     // Tabla de tokens de usuario


        // Seed inicial para roles predefinidos (Admin y Customer) con IDs fijos para referencia
        builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "f936a0de-4c11-4c82-b2f9-38cd193514ed",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = "4632eea2-4d43-47ed-b736-0ccd85664371",
                    Name = "Customer",
                    NormalizedName = "CUSTOMER"
                }
            );
        }
    }
}
