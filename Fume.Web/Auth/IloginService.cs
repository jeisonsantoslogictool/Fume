namespace Fume.Web.Auth
{
    public interface IloginService
    {
        Task LoginAsync(string token);

        Task LogoutAsync();

    }
}
