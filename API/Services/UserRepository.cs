using API.Data;
using API.DTO;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ServiceStack;
using System.Linq;

namespace API.Services
{
    public class UserRepository(DataContext context, IMapper mapper) : IUserRepository
    {
        public async Task<PagedList<MemberDto>> GetmemberAsync(UserParams userParams)
        {
            var query = context.Users.AsQueryable();
            query = query.Where(x => x.UserName != userParams.UserName);
            if(userParams.Gender!=null)
            {
                query = query.Where(x => x.Gender == userParams.Gender);
            }
           
            var minAge = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
            var maxAge = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));
            query = query.Where(x => x.DateOfBirth >= minAge && x.DateOfBirth <= maxAge);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(x => x.Created),
                _ => query.OrderByDescending(x => x.LastActive)
            };
            return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(mapper.ConfigurationProvider), userParams.PageNumber, userParams.PageSize);
        }

        public async Task<IEnumerable<AppUser>> GetUserAsync()
        {
            return await context.Users.Include(u => u.photos).ToListAsync();
        }

        public async Task<AppUser?> GetUserByIdAsync(int id)
        {
            return await context.Users.FindAsync(id);
        }

        public async Task<AppUser?> GetUserByUsernameAsync(string username)
        {

            return await context.Users.Include(u => u.photos).SingleOrDefaultAsync(u => u.UserName == username); //returns only one elemeat which one satisfied the condition
                                                                                                                 //And throw an error if has many matching
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            context.Entry(user).State = EntityState.Modified;
        }
    }
}
//*****Important Note*****
//We use userParams.OrderBy so the API can support dynamic sorting based on user input, making it flexible, reusable, and user-friendly.
//_: Default case if "created" is not matched.
//You can rename "created" to something like "newest" or "dateCreated" — just make sure your frontend sends the matching value.
//Then the frontend should call: GET /api/users?orderBy=newest