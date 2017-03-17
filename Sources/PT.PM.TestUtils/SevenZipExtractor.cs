using PT.PM.Common;
using System;
using System.Diagnostics;

namespace PT.PM.TestUtils
{
    public class SevenZipExtractor
    {
        public static void Extract(string zipPath, string extractPath)
        {
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
                    string message = $"Error while extracting {extractPath}.";
                    Console.Error.WriteLine(message);
                    throw new ParsingException(message);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error while extracting {0}: {1}", extractPath, ex.Message);
                throw;
            }
        }
    }
}
