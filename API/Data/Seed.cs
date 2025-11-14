using API.Entities;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(UserManager<AppUser> usermanager, RoleManager<AppRoles> roleManager)
        {
            if (await usermanager.Users.AnyAsync()) return;
            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);
            if (users == null) return;
            var roles = new List<AppRoles>//All roles
            {
                new AppRoles{Name="Admin" },
                new(){Name="Member"},
                new(){Name="Moderator"}
            };
            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);//Importing the role in AppRole Table
            }
            foreach (var user in users)
            {

                //user.passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("password123"));
                //user.passwordSalt = hmac.Key;
                await usermanager.CreateAsync(user, "Password@123");
                await usermanager.AddToRoleAsync(user, "Member");//Assign member role to all users
            }
            var admin = new AppUser//Creating admin
            {
                UserName = "Atanu",
                KnownAs = "admin",
                Gender = "Male",
                City = "Kolkata",
                Country = "India",
            };
            await usermanager.CreateAsync(admin, "Admin123");//adding the admin in app user table
            await usermanager.AddToRolesAsync(admin, ["Admin","Moderator"]);

        }
    }
}
