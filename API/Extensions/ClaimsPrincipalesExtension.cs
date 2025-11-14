using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipalesExtension
    {
        ///**Retrieves the user ID from the JWT or claims-based identity.
        public static string GetUserAsync(this ClaimsPrincipal user)
        {
            var username = user.FindFirstValue(ClaimTypes.Name)
            ?? throw new Exception("Cannot get username from token");
            return username;
        }
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? throw new Exception("Cannot get username from token"));

            return userId;
        }

    }
}
