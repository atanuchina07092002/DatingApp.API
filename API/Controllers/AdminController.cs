using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;

namespace API.Controllers
{
    public class AdminController(UserManager<AppUser> usermanager): BaseApiController
    {
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUserName()
        {
            var usernames = await usermanager.Users
                .Select(x=> new 
                { 
                    x.Id,
                    x.UserName,
                    x.Gender,
                    Roles=x.UsersRoles.Select(u=>u.Role.Name).ToList()
                }).OrderBy(x=>x.UserName).ToListAsync();
            return Ok(usernames);
        }
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, string roles)
        {
            if (string.IsNullOrEmpty(roles)) { return BadRequest("Please Enter Valid Roles"); }

            var selectedRoles = roles.Split(',').ToArray();//split the roles basis of , then convert to array

            var user = await usermanager.FindByNameAsync(username);

            if (user == null) { return BadRequest("User Not Found"); }

            var userRoles = await usermanager.GetRolesAsync(user);

            var result = await usermanager.AddToRolesAsync(user, selectedRoles.Except(userRoles));// Add new roles

            if (!result.Succeeded) { return BadRequest("Roles not added successfully"); }

            result = await usermanager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));//Remove roles that are not selected anymore

            if (!result.Succeeded) { BadRequest("Roles remove not succeded"); }

            return Ok(await usermanager.GetRolesAsync(user));
        }
    }
}
