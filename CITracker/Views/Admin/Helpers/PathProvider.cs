namespace CITracker.Helpers
{
    public class PathProvider : IPathProvider
    {
        private IWebHostEnvironment _hostEnvironment;

        public PathProvider(IWebHostEnvironment environment)
        {
            _hostEnvironment = environment;
        }

        public string MapPath(string path)
        {
            string filePath = Path.Combine(_hostEnvironment.ContentRootPath, path);

            return filePath;
        }
    }
}
