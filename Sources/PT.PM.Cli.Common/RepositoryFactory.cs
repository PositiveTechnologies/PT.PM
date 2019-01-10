using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Dsl;
using PT.PM.Matching;
using PT.PM.Matching.PatternsRepository;
using PT.PM.Patterns.PatternsRepository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PT.PM.Common.Files;
using PT.PM.Common.Utils;

namespace PT.PM.Cli.Common
{
    public static class RepositoryFactory
    {
        public static SourceCodeRepository CreateSourceCodeRepository(string path, string tempDir)
        {
            SourceCodeRepository sourceCodeRepository;
            if (string.IsNullOrWhiteSpace(path))
            {
                sourceCodeRepository = DummyCodeRepository.Instance;
            }
            else if (DirectoryExt.Exists(path))
            {
                sourceCodeRepository = new DirectoryCodeRepository(path);
            }
            else if (FileExt.Exists(path))
            {
                string extension = Path.GetExtension(path);
                if (extension.EqualsIgnoreCase(".zip"))
                {
                    var zipCachingRepository = new ZipCachingRepository(path);
                    if (tempDir != null)
                    {
                        zipCachingRepository.ExtractPath = tempDir;
                    }
                    sourceCodeRepository = zipCachingRepository;
                }
                else
                {
                    sourceCodeRepository = new FileCodeRepository(path, format: CommonUtils.GetFormatByExtension(extension));
                }
            }
            else
            {
                string url = path;
                string projectName = null;
                string urlWithoutHttp = TextUtils.HttpRegex.Replace(url, "");

                if (!url.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    if (urlWithoutHttp.StartsWith("github.com", StringComparison.OrdinalIgnoreCase))
                    {
                        url = url + "/archive/master.zip";
                    }
                }

                if (urlWithoutHttp.StartsWith("github.com", StringComparison.OrdinalIgnoreCase))
                {
                    projectName = urlWithoutHttp.Split('/').ElementAtOrDefault(2);
                }

                var zipAtUrlCachedCodeRepository = new ZipAtUrlCachingRepository(url, projectName);
                if (tempDir != null)
                {
                    zipAtUrlCachedCodeRepository.DownloadPath = tempDir;
                }
                sourceCodeRepository = zipAtUrlCachedCodeRepository;
            }

            return sourceCodeRepository;
        }

        public static IPatternsRepository CreatePatternsRepository(string patternsString, IEnumerable<string> patternIds,
            ILogger logger)
        {
            IPatternsRepository patternsRepository;

            if (string.IsNullOrEmpty(patternsString) || patternsString == "default")
            {
                patternsRepository = new DefaultPatternRepository();
            }
            else if (patternsString.EqualsIgnoreCase("no"))
            {
                patternsRepository = DummyPatternsRepository.Instance;
            }
            else if (patternsString.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                patternsRepository = new JsonPatternsRepository(FileExt.ReadAllText(patternsString));
            }
            else
            {
                CodeFile patternsFile;
                if (patternsString.EndsWith(".pattern", StringComparison.OrdinalIgnoreCase))
                {
                    patternsFile = new CodeFile(FileExt.ReadAllText(patternsString))
                    {
                        PatternKey = patternsString,
                        Name = patternsString
                    };
                }
                else
                {
                    patternsFile = new CodeFile(patternsString);
                }

                var processor = new DslProcessor();
                if (logger != null)
                {
                    processor.Logger = logger;
                }
                PatternRoot patternRoot = processor.Deserialize(patternsFile);
                var patternConverter = new PatternConverter();
                if (logger != null)
                {
                    patternConverter.Logger = logger;
                }
                List<PatternDto> dtos = patternConverter.ConvertBack(new[] { patternRoot });

                var memoryPatternsRepository = new MemoryPatternsRepository();
                memoryPatternsRepository.Add(dtos);

                patternsRepository = memoryPatternsRepository;
            }

            if (logger != null)
            {
                patternsRepository.Logger = logger;
            }
            
            if (patternsRepository is MemoryPatternsRepository MemoryPatternsRepository)
            {
                patternsRepository.Identifiers = patternIds as List<string> ?? patternIds?.ToList();
            }

            return patternsRepository;
        }
    }
}
