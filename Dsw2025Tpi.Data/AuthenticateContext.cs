using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Data
{
    public class AuthenticateContext : IdentityDbContext
    {
        public AuthenticateContext(DbContextOptions<AuthenticateContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUser>(b => { b.ToTable("Usuarios"); });
            builder.Entity<IdentityRole>(b => { b.ToTable("Roles"); });
            builder.Entity<IdentityUserRole<string>>(b => { b.ToTable("UsuariosRoles"); });
            builder.Entity<IdentityUserClaim<string>>(b => { b.ToTable("UsuariosClaims"); });
            builder.Entity<IdentityUserLogin<string>>(b => { b.ToTable("UsuariosLogins"); });
            builder.Entity<IdentityRoleClaim<string>>(b => { b.ToTable("RolesClaims"); });
            builder.Entity<IdentityUserToken<string>>(b => { b.ToTable("UsuariosTokens"); });

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
