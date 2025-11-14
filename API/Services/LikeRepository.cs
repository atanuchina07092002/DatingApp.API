using API.Data;
using API.DTO;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class LikeRepository(DataContext context, IMapper mapper) : ILikeRepository
    {
        public void AddLike(UserLike like) // Add the each like info to database
        {

            context.Likes.Add(like);
        }

        public void DeleteLike(UserLike like)// Remove the each like from database
        {
            context.Likes.Remove(like);
        }

        public async Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId) //Get the list of targetId from database of current User.
        {
            return await context.Likes
                   .Where(l => l.SourceUserId == currentUserId)
                   .Select(l => l.TargetUserId)
                   .ToListAsync();
        }

        public async Task<UserLike?> GetuserLike(int sourceUserId, int targetUserId) //Found the existing SourceUserId and TargetUserId
        {
            return await context.Likes.FindAsync(sourceUserId, targetUserId);
        }

        public async Task<PagedList<MemberDto>> GetUserLikes(LikeParams likeParams)//this is for based on query string here predicate is belonged from query string
        {
            var likes = context.Likes.AsQueryable();//t's not strictly necessary to call .AsQueryable() on context.Likes in this method because context.Likes is already an IQueryable<Like> if it’s a DbSet<Like> (which it usually is in Entity Framework).
            IQueryable<MemberDto> query;

            switch (likeParams.Predicate)
            {
                //List of liked users
                case "liked":
                    query = likes
                         .Where(l => l.SourceUserId == likeParams.UserId)
                         .Select(x => x.TargetUser)
                         .ProjectTo<MemberDto>(mapper.ConfigurationProvider);//which directly converts User entity to MemberDto in the SQL query itself.
                    break;

                //List of users who liked you
                case "likedBy":
                    query = likes
                        .Where(l => l.TargetUserId == likeParams.UserId)
                        .Select(x => x.SourceUser)
                        .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                    break;

                //mutual: users who liked you and you liked them back
                default:
                    var likeIds = await GetCurrentUserLikeIds(likeParams.UserId);
                    query = likes
                       .Where(x => x.TargetUserId == likeParams.UserId && likeIds.Contains(x.SourceUserId))
                       .Select(x => x.SourceUser)
                       .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                    break;

            }
            return await PagedList<MemberDto>.CreateAsync(query, likeParams.PageNumber, likeParams.PageSize);
        }

        public async Task<bool> SaveChanges()
        {
            return await context.SaveChangesAsync() > 0;//
        }
    }
}
