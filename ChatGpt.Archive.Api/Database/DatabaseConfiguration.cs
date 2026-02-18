using System.IO.Abstractions;

namespace ChatGpt.Archive.Api.Database
{
    public class DatabaseConfiguration
    {
        private const string DatabaseFileName = "archive.db";
        private readonly string _dataDirectory;
        private readonly string _databasePath;

        public DatabaseConfiguration(ArchiveSourcesOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.DataDirectory))
            {
                throw new ArgumentException("DataDirectory must be configured", nameof(options));
            }

            _dataDirectory = options.DataDirectory;
            _databasePath = Path.Combine(_dataDirectory, DatabaseFileName);
        }

        public string DataDirectory => _dataDirectory;
        public string DatabasePath => _databasePath;
        public string ConnectionString => $"Data Source={_databasePath}";

        public void EnsureDataDirectoryExists(IFileSystem fileSystem)
        {
            if (!fileSystem.Directory.Exists(_dataDirectory))
            {
                fileSystem.Directory.CreateDirectory(_dataDirectory);
            }
        }
    }
}
