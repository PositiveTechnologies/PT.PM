using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Matching.PatternsRepository;
using PT.PM.Patterns.PatternsRepository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM
{
    public static class RepositoryFactory
    {
        public static SourceCodeRepository CreateSourceCodeRepository(string path, IEnumerable<Language> languages, string tempDir, bool isStartUstStage)
        {
            SourceCodeRepository sourceCodeRepository;
            if (Directory.Exists(path))
            {
                sourceCodeRepository = new DirectoryCodeRepository(path);
            }
            else if (File.Exists(path))
            {
                if (Path.GetExtension(path) == ".zip")
                {
                    sourceCodeRepository = new ZipCachingRepository(path)
                    {
                        ExtractPath = tempDir
                    };
                }
                else
                {
                    sourceCodeRepository = new FileCodeRepository(path);
                }
            }
            else
            {
                string url = path;
                string projectName = null;
                string urlWithoutHttp = TextUtils.HttpRegex.Replace(url, "");

                if (!url.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    if (urlWithoutHttp.StartsWith("github.com"))
                    {
                        url = url + "/archive/master.zip";
                    }
                }

                if (urlWithoutHttp.StartsWith("github.com"))
                {
                    projectName = urlWithoutHttp.Split('/').ElementAtOrDefault(2);
                }

                var zipAtUrlCachedCodeRepository = new ZipAtUrlCachingRepository(url, projectName)
                {
                    DownloadPath = tempDir
                };
                sourceCodeRepository = zipAtUrlCachedCodeRepository;
            }

            sourceCodeRepository.Languages = new HashSet<Language>(languages);
            sourceCodeRepository.LoadJson = isStartUstStage;

            return sourceCodeRepository;
        }

        public static IPatternsRepository CreatePatternsRepository(string patternsString)
        {
            IPatternsRepository patternsRepository;
            if (string.IsNullOrEmpty(patternsString))
            {
                patternsRepository = new DefaultPatternRepository();
            }
            else if (patternsString.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                patternsRepository = new FilePatternsRepository(patternsString);
            }
            else
            {
                throw new FormatException("Patterns should be loaded from *.json file");
            }
            return patternsRepository;
        }
    }
}
