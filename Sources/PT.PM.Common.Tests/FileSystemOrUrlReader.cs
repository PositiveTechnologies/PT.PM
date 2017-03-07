using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common.Tests
{
    public class FileSystemOrUrlReader
    {
        public static string ReadAllText(string fsPathOrUrl)
        {
            string result;
            if (fsPathOrUrl.StartsWith("http"))
            {
                using (var client = new WebClient())
                {
                    result = client.DownloadString(fsPathOrUrl);
                }
            }
            else
            {
                result = File.ReadAllText(fsPathOrUrl);
            }
            return result;
        }

        public static byte[] ReadAllBytes(string fsPathOrUrl)
        {
            byte[] result;
            if (fsPathOrUrl.StartsWith("http"))
            {
                using (var client = new WebClient())
                {
                    result = client.DownloadData(fsPathOrUrl);
                }
            }
            else
            {
                result = File.ReadAllBytes(fsPathOrUrl);
            }
            return result;
        }
    }
}
