using API.Data;
using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    public class UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService) : BaseApiController
    {
        //[Authorize(Roles ="Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetAllUsers([FromQuery]UserParams userParams)
        {
            userParams.UserName = User.GetUserAsync();
            var users = await userRepository.GetmemberAsync(userParams);
            //var usersToReturn = mapper.Map<IEnumerable<MemberDto>>(users);//Mapping from source object to destination object
            Response.AddPaginationHeader(users);
            return Ok(users);
        }
        //[Authorize(Roles ="Member")]
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUsers(string username)
        {
            var user = await userRepository.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }
            return mapper.Map<MemberDto>(user);
        }
        [HttpGet("{id:int}")]//Define the i d datatype is the correct way to route
        public async Task<ActionResult<MemberDto>> GetUserById(int id)
        {
            var user = await userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return mapper.Map<MemberDto>(user);
        }
        [HttpPut]
        public async Task<ActionResult> MemberUpdate(MemberUpdateDto memberUpdateDto)
        {

            var user = await userRepository.GetUserByUsernameAsync(User.GetUserAsync());
            if (user == null) return BadRequest("User Not Found");
            mapper.Map(memberUpdateDto, user);
            userRepository.Update(user);
            if (await userRepository.SaveAllAsync()) { return NoContent(); }
            return BadRequest("Faild To Update");
        }
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> PhotoUpload(IFormFile file)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUserAsync());

            if (user == null) { return BadRequest("User Not Found"); }

            var result = await photoService.AddPhotoAsync(file);
            if (result == null) return BadRequest("Photo Not Uploaded Successfully");
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,//uniform resource identifier 
                PublicId = result.PublicId,

            };
            if (user.photos.Count == 0)
            {
                photo.IsMain = true;
            }
            user.photos.Add(photo);
            
            if (await userRepository.SaveAllAsync())
            {

                return CreatedAtAction(nameof(GetUsers), new { username = user.UserName }, mapper.Map<PhotoDto>(photo));
            }
            return BadRequest();
        }
        [HttpPut("set-main-photo/{photoId:int}")]
        public async Task<ActionResult> SetIsMainPhoto(int photoId)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUserAsync());
            if (user == null) return BadRequest();
            var photo = user.photos.FirstOrDefault(u => u.Id == photoId);
            if (photo == null) return BadRequest();
            var currentIsMain = user.photos.FirstOrDefault(x => x.IsMain);
            if (currentIsMain != null)
            {
                currentIsMain.IsMain = false;
            }
            photo.IsMain = true;
            if (await userRepository.SaveAllAsync()) { return NoContent(); }
            return BadRequest("Problem to setting main photo");
        }
        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUserAsync());// Get the user
            if(user==null)
            {
                return BadRequest("User Not Found");
            }
            var photo = user.photos.FirstOrDefault(u => u.Id == photoId); //Get the photo using photoId
            if (photo == null || photo.IsMain)
            {
                return BadRequest("Photo will be not deleted");
            }
            if(photo.PublicId!=null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId); //Delete the photo details from cloudinary.
                if(result.Error!=null)
                {
                    return BadRequest(result.Error.Message);
                }
            }
            user.photos.Remove(photo);// Remove the photo from user photos array;
            if (await userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Photo Not Deleted");
        }
    }

}
