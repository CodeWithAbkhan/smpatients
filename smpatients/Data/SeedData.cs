using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using smpatients.Data;

namespace smpatients;

public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                if (context == null || context.Users == null)
                {
                    throw new ArgumentNullException("Null ApplicationDbContext");
                }

                // Look for any users or roles.
                if (context.Users.Any() || context.Roles.Any())
                {
                    return;   // DB has been seeded
                }

                string ADMIN_ROLE_ID = Guid.NewGuid().ToString();
                string partner_ROLE_ID = Guid.NewGuid().ToString();
                string ASSISTANT_ROLE_ID = Guid.NewGuid().ToString();
                string PARTNER_DR_ROLE_ID = Guid.NewGuid().ToString();

                // Add roles
                context.Roles.AddRange(
                    new IdentityRole
                    {
                        Name = "admin",
                        NormalizedName = "ADMIN",
                        Id = ADMIN_ROLE_ID,
                        ConcurrencyStamp = ADMIN_ROLE_ID
                    },
                    new IdentityRole
                    {
                        Name = "partner",
                        NormalizedName = "partner",
                        Id = partner_ROLE_ID,
                        ConcurrencyStamp = partner_ROLE_ID
                    },
                   
                    new IdentityRole
                    {
                        Name = "assistant",
                        NormalizedName = "ASSISTANT",
                        Id = ASSISTANT_ROLE_ID,
                        ConcurrencyStamp = ASSISTANT_ROLE_ID
                    },
                    new IdentityRole
                    {
                        Name = "partner_dr",
                        NormalizedName = "PARTNER_DR",
                        Id = PARTNER_DR_ROLE_ID,
                        ConcurrencyStamp = PARTNER_DR_ROLE_ID
                    }
                );

                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var adminUser = new ApplicationUser
                {
                    Email = "admin@gmail.com",
                    EmailConfirmed = true,
                    UserName = "admin@gmail.com",
                };

                // Use PasswordHasher to hash the password
                var password = "Password@123d";
                var passwordHash = new PasswordHasher<ApplicationUser>().HashPassword(adminUser, password);
                adminUser.PasswordHash = passwordHash;

                context.Users.AddRange(
                    adminUser
                );

              try
                {
                    context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // Handle concurrency conflict, e.g., by reloading the entity and trying again
                    var entry = ex.Entries.Single();
                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                    context.SaveChanges();
                }


                // Assign roles to the admin user using UserManager
                userManager.AddToRolesAsync(adminUser, new[] { "admin", "partner", "partner", "assistant", "partner_dr" }).Wait();

                try
                {
                    context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // Handle concurrency conflict, e.g., by reloading the entity and trying again
                    var entry = ex.Entries.Single();
                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                    context.SaveChanges();
                }

            }
        }
   
    }