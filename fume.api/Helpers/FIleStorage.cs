namespace fume.api.Helpers
{
    public class FIleStorage : IFileStorage
    {
        private readonly string connectionString;

        public FIleStorage(IConfiguration configuration)
        {
            connectionString = "DockerConnection";
        }
        public Task RemoveFileAsync(string path, string nombreContenedor)
        {
            throw new NotImplementedException();
        }

        public Task<string> SaveFileAsync(byte[] content, string extention, string containerName)
        {
            throw new NotImplementedException();
        }
    }
}
