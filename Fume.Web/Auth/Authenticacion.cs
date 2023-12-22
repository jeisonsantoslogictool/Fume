using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Fume.Web.Auth
{
    public class Authenticacion : AuthenticationStateProvider
    {
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var anonimous = new ClaimsIdentity();
            var jei = new ClaimsIdentity(new List<Claim>
            {
                new Claim("FirstName", "Juan"),
                new Claim ("LastName", "Zulu"),
                new Claim(ClaimTypes.Name, "jei@yopmail.com"),
                new Claim(ClaimTypes.Role, "Admin")
            },                                
            authenticationType: "test");
            return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(jei)));
        }
    }
}
