namespace Fume.Web.Repositories
{
    public interface IRepository
    {
        Task<HttpResponseWrapper<T>> Get <T>(string url);
        Task<HttpResponseWrapper<Object>> post<T>(string url, T model);

        Task<HttpResponseWrapper<Tresponse>> post<T, Tresponse>(string url, T model);

        Task<HttpResponseWrapper<object>> Delete(string url);
        Task<HttpResponseWrapper<object>> Put<T>(string url, T model);

        Task<HttpResponseWrapper<Tresponse>> Put<T, Tresponse>(string url, T model);
    }
}
