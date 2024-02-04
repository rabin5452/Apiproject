using Microsoft.AspNetCore.Identity;
using Practiseproject.Constraint;
using Practiseproject.Model;
using System.Data;

namespace Practiseproject.Data
{
    public class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            var userManager = service.GetService<UserManager<User>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()))!;
            await roleManager.CreateAsync(new IdentityRole(Roles.User.ToString()));

            var user = new User
            {
                UserName = "admin12",
                Email = "admin12@gmail.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            var userInDb = await userManager.FindByEmailAsync(user.Email);
            if (userInDb == null)
            {
                await userManager.CreateAsync(user, "Admin@12");
                await userManager.AddToRoleAsync(user, Roles.Admin.ToString());
            }
        }
    }
}
