using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace API.Controllers
{
    public class LikesController(ILikeRepository likeRepository) : BaseApiController
    {
        [HttpPost("{targetUserId:int}")]
        public async Task<ActionResult> ToggleLike(int targetUserid)
        {
            var sourceUserId = User.GetUserId();

            if (sourceUserId == targetUserid)
            {
                return BadRequest("You cannot like yourself");
            }
            var exsistingLike = await likeRepository.GetuserLike(sourceUserId, targetUserid);
            if (exsistingLike == null)
            {
                var like = new UserLike
                {
                    SourceUserId = sourceUserId,
                    TargetUserId = targetUserid,
                };
                likeRepository.AddLike(like);
            }
            else
            {
                likeRepository.DeleteLike(exsistingLike);

            }
            if (await likeRepository.SaveChanges())
            {
                return Ok();
            }
            return BadRequest("Faild to update");
        }
        [HttpGet("list")]
        public async Task<ActionResult<int>> GetCurrentUserLikeIds()
        {
            return Ok(await likeRepository.GetCurrentUserLikeIds(User.GetUserId()));
        }
        [HttpGet]
        public async Task<ActionResult<MemberDto>> GetUserLikes([FromQuery] LikeParams likeParams)
        {
            likeParams.UserId = User.GetUserId();
            var users = await likeRepository.GetUserLikes(likeParams); 
            Response.AddPaginationHeader(users);
            return Ok(users);
        }


    }
}
