using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectApprovalSystem.Models;
using ProjectApprovalSystem.Models.Enums;

namespace ProjectApprovalSystem.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Seed Roles
            var roles = Enum.GetNames(typeof(UserRole));
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Research Areas if none exist
            if (!context.ResearchAreas.Any())
            {
                var areas = new List<ResearchArea>
                {
                    new ResearchArea { Name = "Artificial Intelligence", Description = "Machine Learning, Neural Networks, etc." },
                    new ResearchArea { Name = "Web Development", Description = "Front-end, Back-end, Cloud technologies" },
                    new ResearchArea { Name = "Cybersecurity", Description = "Network security, Cryptography" },
                    new ResearchArea { Name = "Data Science", Description = "Big Data, Analytics" }
                };
                context.ResearchAreas.AddRange(areas);
                await context.SaveChangesAsync();
            }

            // 1. Seed Module Leader (Admin)
            await CreateUserWithRole(userManager, context, "admin@pas.com", "Admin@123", "System Administrator", UserRole.ModuleLeader);

            // 2. Seed Student
            await CreateUserWithRole(userManager, context, "student@pas.com", "Student@123", "Test Student", UserRole.Student);

            // 3. Seed Supervisor
            await CreateUserWithRole(userManager, context, "supervisor@pas.com", "Supervisor@123", "Test Supervisor", UserRole.Supervisor);
        }

        private static async Task CreateUserWithRole(
            UserManager<ApplicationUser> userManager, 
            ApplicationDbContext context,
            string email, 
            string password, 
            string fullName, 
            UserRole role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role.ToString());
                    
                    // Also seed the role-specific profile if it's Student or Supervisor
                    if (role == UserRole.Student)
                    {
                        if (!await context.StudentProfiles.AnyAsync(s => s.UserId == user.Id))
                        {
                            context.StudentProfiles.Add(new StudentProfile { UserId = user.Id, StudentId = "S123456" });
                        }
                    }
                    else if (role == UserRole.Supervisor)
                    {
                        if (!await context.SupervisorProfiles.AnyAsync(s => s.UserId == user.Id))
                        {
                            context.SupervisorProfiles.Add(new SupervisorProfile { UserId = user.Id, StaffId = "T1234" });
                        }
                    }
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
