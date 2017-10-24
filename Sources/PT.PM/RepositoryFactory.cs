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
        public static SourceCodeRepository CreateSourceCodeRepository(string path, IEnumerable<Language> languages, string tempDir)
        {
            SourceCodeRepository sourceCodeRepository;
            if (Directory.Exists(path))
            {
                sourceCodeRepository = new FilesAggregatorCodeRepository(path);
            }
            else if (File.Exists(path))
            {
                sourceCodeRepository = new FileCodeRepository(path);
            }
            else
            {
                string url = path.Replace(@"\", "/");
                string projectName = null;
                if (!url.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    string urlWithoutHttp = TextUtils.HttpRegex.Replace(url, "");
                    if (urlWithoutHttp.StartsWith("github.com"))
                    {
                        projectName = urlWithoutHttp.Split('/').ElementAtOrDefault(2);
                        url = url + "/archive/master.zip";
                    }
                }
                var zipAtUrlCachedCodeRepository = new ZipAtUrlCachingRepository(url, projectName)
                {
                    DownloadPath = tempDir
                };
                sourceCodeRepository = zipAtUrlCachedCodeRepository;
            }
            sourceCodeRepository.Languages = new HashSet<Language>(languages);
            return sourceCodeRepository;
        }

        public static IPatternsRepository CreatePatternsRepository(string patternsString)
        {
            IPatternsRepository patternsRepository;
            if (string.IsNullOrEmpty(patternsString))
            {
                patternsRepository = new DefaultPatternRepository();
            }
            else if (patternsString.EndsWith(".json"))
            {
                patternsRepository = new FilePatternsRepository(patternsString);
            }
            else
            {
                var patterns = StringCompressorEscaper.UnescapeDecompress(patternsString);
                patternsRepository = new JsonPatternsRepository(patterns);
            }
            return patternsRepository;
        }
    }
}
