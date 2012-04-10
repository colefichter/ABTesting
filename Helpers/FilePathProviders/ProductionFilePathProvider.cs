using ABTesting.Helpers;

namespace ABTesting.Helpers.FilePathProviders
{
    public sealed class ProductionFilePathProvider: IFilePathProvider
    {
        public string GetFilePath()
        {
            return @"C:\mailout_interactive\production_tests.ab";
        }
    }
}
