using API.DTO;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface ILikeRepository
    {
        Task<UserLike?>GetuserLike(int sourceUserId,int targetUserId);
        Task<PagedList<MemberDto>> GetUserLikes(LikeParams likeparams);
        Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId);
        void DeleteLike(UserLike like);
        void AddLike(UserLike like);
        Task<bool> SaveChanges();

    }
}
