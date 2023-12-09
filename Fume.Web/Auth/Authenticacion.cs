using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Fume.Web.Auth
{
    public class Authenticacion : AuthenticationStateProvider
    {
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            await Task.Delay(3000);
            var anonimous = new ClaimsIdentity();
            return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonimous)));
        }
    }
}
