using System;
using ABTesting.Helpers;

namespace ABTesting.Helpers.FilePathProviders
{
    public sealed class AutomaticFilePathProvider : IFilePathProvider
    {
        public string GetFilePath()
        {
            //Figure out if we are testing or live... always default to test mode
            bool test = true;
            try
            {
                System.Web.HttpContext hc = System.Web.HttpContext.Current;
                if (hc != null && hc.Request != null && !hc.Request.Url.Host.Contains(@"cmsedit.")) // cmsedit is the subdomain where the CMS system lives...
                {
                    test = false; //production mode...
                }
            }
            catch (Exception)
            {
            }

            if (test)
            {
                return @"C:\mailout_interactive\debug_tests.ab";
            }
            else
            {
                return @"C:\mailout_interactive\production_tests.ab";
            }
        }
    }
}
