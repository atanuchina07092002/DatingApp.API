using API.DTO;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task<bool>SaveAllAsync();
        Task<IEnumerable<AppUser>> GetUserAsync();
        Task<AppUser?>GetUserByIdAsync(int id);
        Task<PagedList<MemberDto>>GetmemberAsync(UserParams userParams);
        Task<AppUser?> GetUserByUsernameAsync(string username);
    }
}
