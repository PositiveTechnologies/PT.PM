using PT.PM.Common.Exceptions;
using System;
using System.Diagnostics;

namespace PT.PM.TestUtils
{
    public class SevenZipExtractor
    {
        public static void Extract(string zipPath, string extractPath)
        {
            string errorMessage = null;
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = TestHelper.SevenZipPath,
                    Arguments = "x \"" + zipPath + "\" -o\"" + extractPath + "\" -y"
                };
                Process process = Process.Start(processStartInfo);
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    errorMessage = $"Error while extracting {extractPath}.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error while extracting {extractPath}: {ex.Message}";
            }
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Console.Error.WriteLine(errorMessage);
                throw new ReadException(zipPath, message: errorMessage);
            }
        }
    }
}
