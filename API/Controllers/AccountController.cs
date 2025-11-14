using API.Data;
using API.DTO;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper) : BaseApiController
    {
        [HttpPost("register")] // account/register[-
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");
            var user = mapper.Map<AppUser>(registerDto);
            user.UserName = registerDto.Username.ToUpper();
            var result = userManager.CreateAsync(user, registerDto.Password);
            if(result==null)BadRequest("Password Is Not Matched");

            return new UserDto
            {
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Gender=user.Gender,
                Token = await tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await userManager.Users.Include(u => u.photos).FirstOrDefaultAsync(x =>
                x.NormalizedUserName == loginDto.Username.ToUpper());

            if (user == null || user.UserName==null) return Unauthorized("Invalid username");

            //using var hmac = new HMACSHA512(user.passwordSalt);

            //var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            //for (int i = 0; i < computedHash.Length; i++)
            //{
            //    if (computedHash[i] != user.passwordHash[i]) return Unauthorized("Invalid password");
            //}

            var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!result) return Unauthorized();

            return new UserDto
            {
                Username = user.UserName,
                Token = await tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                photoUrl = user.photos.FirstOrDefault(x => x.IsMain)?.Url,
                Gender=user.Gender,
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await userManager.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper()); // Bob != bob
        }
    }
}
