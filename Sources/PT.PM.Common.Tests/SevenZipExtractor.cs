using System;
using System.Diagnostics;
using System.IO;

namespace PT.PM.Common.Tests
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
                    FileName = Path.GetFullPath(TestHelper.SevenZipPath),
                    Arguments = "x \"" + zipPath + "\" -o\"" + extractPath + "\" -y"
                };
                Process process = Process.Start(processStartInfo);
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    System.Console.Error.WriteLine("Error while extracting {0}.", extractPath);
                }
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine("Error extracting {0}: {1}", extractPath, ex.Message);
                throw;
            }
        }
    }
}
