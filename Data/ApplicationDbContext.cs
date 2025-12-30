using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using IdentityCoreCustomization.Areas.Users.Models;
using IdentityCoreCustomization.Models;
using IdentityCoreCustomization.Models.Identity;

namespace IdentityCoreCustomization.Data
{
    public class ApplicationDbContext : IdentityDbContext<
    ApplicationUser,
    ApplicationRole,
    int,
    ApplicationUserClaim,
    ApplicationUserRole,
    ApplicationUserLogin,
    ApplicationRoleClaim,
    ApplicationUserToken> // Mehdi: Class declaration modified
    {
        // Table Names
        private string usersTableName = "Users";
        private string userClaimsTableName = "UserClaims";
        private string userLoginsTableName = "UserLogins";
        private string userTokensTableName = "UserTokens";
        private string rolesTableName = "Roles";
        private string roleClaimsTableName = "RoleClaims";
        private string userRolesTableName = "UserRoles";
        

        // Column Names
        private string userIDColumnName = "UserID";
        private string userClaimIDColumnName = "UserClaimID";
        private string userNameColumnName = "Username";
        private string normalizedUserNameColumnName = "NormalizedUsername";
        private string roleIDColumnName = "RoleID";
        private string roleClaimIDColumnName = "RoleClaimID";
        private string roleNameColumnName = "RoleName";
        private string roleNormalizedNameColumnName = "RoleNormalizedName";


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public DbSet<UserLoginWithSms> UserLoginWithSms { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<UserPhoneToken> UserPhoneTokens { get; set; }
        public DbSet<AuthenticationTicket> AuthenticationTickets { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(b =>
            {
                // Each User can have many UserClaims
                b.HasMany(e => e.Claims)
                    .WithOne(e => e.User)
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                // Each User can have many UserLogins
                b.HasMany(e => e.Logins)
                    .WithOne(e => e.User)
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                // Each User can have many UserTokens
                b.HasMany(e => e.Tokens)
                    .WithOne(e => e.User)
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();

                // Each User can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<ApplicationRole>(b =>
            {
                // Each Role can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                // Each Role can have many associated RoleClaims
                b.HasMany(e => e.RoleClaims)
                    .WithOne(e => e.Role)
                    .HasForeignKey(rc => rc.RoleId)
                    .IsRequired();
            });

            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.ToTable(usersTableName);
                b.Property(e => e.Id).HasColumnName(userIDColumnName);
                b.Property(e => e.UserName).HasColumnName(userNameColumnName);
                b.Property(e => e.NormalizedUserName).HasColumnName(normalizedUserNameColumnName);
            });

            modelBuilder.Entity<ApplicationUserClaim>(b =>
            {
                b.ToTable(userClaimsTableName);
                b.Property(e => e.Id).HasColumnName(userClaimIDColumnName);
                b.Property(e => e.UserId).HasColumnName(userIDColumnName);
            });

            modelBuilder.Entity<ApplicationUserLogin>(b =>
            {
                b.ToTable(userLoginsTableName);
                b.Property(e => e.UserId).HasColumnName(userIDColumnName);
            });

            modelBuilder.Entity<ApplicationUserToken>(b =>
            {
                b.ToTable(userTokensTableName);
                b.Property(e => e.UserId).HasColumnName(userIDColumnName);
            });

            modelBuilder.Entity<ApplicationRole>(b =>
            {
                b.ToTable(rolesTableName);
                b.Property(e => e.Id).HasColumnName(roleIDColumnName);
                b.Property(e => e.Name).HasColumnName(roleNameColumnName);
                b.Property(e => e.NormalizedName).HasColumnName(roleNormalizedNameColumnName);
            });

            modelBuilder.Entity<ApplicationRoleClaim>(b =>
            {
                b.ToTable(roleClaimsTableName);
                b.Property(e => e.Id).HasColumnName(roleClaimIDColumnName);
                b.Property(e => e.RoleId).HasColumnName(roleIDColumnName);
            });

            modelBuilder.Entity<ApplicationUserRole>(b =>
            {
                b.ToTable(userRolesTableName);
            });
        }
    }
}
