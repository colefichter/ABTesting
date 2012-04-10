using ABTesting.Helpers;

namespace ABTesting.Helpers.FilePathProviders
{
    public sealed class DebugFilePathProvider : IFilePathProvider
    {
        public string GetFilePath()
        {
            return @"C:\mailout_interactive\debug_tests.ab";
        }
    }
}
