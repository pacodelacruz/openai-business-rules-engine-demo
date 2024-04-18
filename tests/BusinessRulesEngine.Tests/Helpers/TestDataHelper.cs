using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessRulesEngine.Tests.Helpers
{
    internal static class TestDataHelper
    {
        /// <summary>
        /// Gets test data as a string from a file
        /// </summary>
        /// <param name="subfolder"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static string GetTestDataStringFromFile(string fileName, string subfolder = "")
        {
            // Gets the file path depending on the operating system
            string path = Path.Combine("TestData", subfolder, fileName);

            if (!File.Exists(path))
            {
                throw new ArgumentException($"Could not find file at path: {path}");
            }

            return File.ReadAllText(path);
        }
    }
}
