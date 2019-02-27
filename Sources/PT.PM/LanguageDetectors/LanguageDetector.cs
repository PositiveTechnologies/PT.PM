using System;
using PT.PM.Common;
using PT.PM.Common.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common.Files;
using PT.PM.LanguageDetectors;

namespace PT.PM
{
    public abstract class LanguageDetector
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public int MaxStackSize { get; set; }

        public DetectionResult DetectIfRequired(string sourceFileName, out TimeSpan detectionTimeSpan)
        {
            var sourceFile = new TextFile(FileExt.ReadAllText(sourceFileName))
            {
                Name = sourceFileName
            };
            return DetectIfRequired(sourceFile, out detectionTimeSpan);
        }

        public DetectionResult DetectIfRequired(TextFile sourceFile, out TimeSpan detectionTimeSpan, ICollection<Language> languages = null)
        {
            DetectionResult result = null;

            if ((languages?.Count ?? 0) == 1)
            {
                result = new DetectionResult(languages.First());
            }
            else if (!string.IsNullOrEmpty(sourceFile.Name))
            {
                string[] extensions = GetExtensions(sourceFile.Name);
                List<Language> finalLanguages = GetLanguagesIntersection(extensions, languages);
                if (finalLanguages.Count == 1 || finalLanguages.Any(final => final == Language.CSharp))
                {
                    result = new DetectionResult(finalLanguages[0]);
                }
                else if (finalLanguages.Count > 1)
                {
                    if (finalLanguages.Count == 2 && finalLanguages.Contains(Language.Html) && finalLanguages.Contains(Language.Php))
                    {
                        result = new DetectionResult(Language.Php);
                    }
                    else
                    {
                        var stopwatch = Stopwatch.StartNew();

                        if (finalLanguages.Any(lang => lang.IsSql()))
                        {
                            List<Language> sqls = DetectSqlDialect(sourceFile.Data);

                            if (sqls.Count == 1)
                            {
                                stopwatch.Stop();
                                detectionTimeSpan = stopwatch.Elapsed;
                                return new DetectionResult(sqls[0]);
                            }

                            finalLanguages.RemoveAll(lang => lang.IsSql());
                            finalLanguages.AddRange(sqls);
                        }

                        result = Detect(sourceFile, finalLanguages);

                        stopwatch.Stop();
                        detectionTimeSpan = stopwatch.Elapsed;

                        LogDetection(result, finalLanguages, sourceFile);
                    }
                }
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();
                result = Detect(sourceFile, languages);
                stopwatch.Stop();
                detectionTimeSpan = stopwatch.Elapsed;

                LogDetection(result, languages ?? LanguageUtils.Languages, sourceFile);
            }

            return result;
        }

        public abstract DetectionResult Detect(TextFile sourceFile, IEnumerable<Language> languages = null);

        private void LogDetection(DetectionResult detectionResult, IEnumerable<Language> languages, TextFile sourceFile)
        {
            string languagesString = string.Join(", ", languages.Select(lang => LanguageUtils.LanguageInfos[lang].Title));

            if (detectionResult != null)
            {
                Logger.LogInfo($"Language {detectionResult.Language} (from {languagesString}) detected for file \"{sourceFile}\". ");
            }
            else
            {
                Logger.LogInfo($"Language is not detected from ({languagesString}) for file \"{sourceFile}\". ");
            }
        }

        public static List<Language> DetectSqlDialect(string data)
        {
            var inputStream = new AntlrCaseInsensitiveInputStream(data, CaseInsensitiveType.UPPER);
            var sqlLexer = new SqlDialectsLexer(inputStream);
            IList<IToken> tokens = sqlLexer.GetAllTokens();

            var sqlDialectTokensCount = new Dictionary<Language, int>
            {
                [Language.TSql] = 0,
                [Language.MySql] = 0,
                [Language.PlSql] = 0
            };

            foreach (IToken token in tokens)
            {
                switch (token.Channel)
                {
                    case SqlDialectsLexer.T_SQL:
                        sqlDialectTokensCount[Language.TSql]++;
                        break;

                    case SqlDialectsLexer.MY_SQL:
                        sqlDialectTokensCount[Language.MySql]++;
                        break;

                    case SqlDialectsLexer.PL_SQL:
                        sqlDialectTokensCount[Language.PlSql]++;
                        break;

                    case SqlDialectsLexer.MY_PL_SQL:
                        sqlDialectTokensCount[Language.MySql]++;
                        sqlDialectTokensCount[Language.PlSql]++;
                        break;

                    case SqlDialectsLexer.PL_T_SQL:
                        sqlDialectTokensCount[Language.PlSql]++;
                        sqlDialectTokensCount[Language.TSql]++;
                        break;
                }
            }

            int maxTokensCount = -1;

            var result = new List<Language>(3);

            var pairs = sqlDialectTokensCount.OrderByDescending(pair => pair.Value);

            foreach (KeyValuePair<Language,int> pair in pairs)
            {
                if (maxTokensCount == -1)
                {
                    maxTokensCount = pair.Value;
                }

                if (pair.Value == maxTokensCount)
                {
                    result.Add(pair.Key);
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private static string[] GetExtensions(string fileName)
        {
            var result = new List<string>();
            string extension = Path.GetExtension(fileName);
            while (extension != "")
            {
                result.Add(extension);
                fileName = Path.GetFileNameWithoutExtension(fileName);
                extension = Path.GetExtension(fileName);
            }
            return result.ToArray();
        }

        private static List<Language> GetLanguagesIntersection(string[] extensions, IEnumerable<Language> languages)
        {
            var result = new List<Language>();
            if (extensions.Length == 0)
            {
                return languages.ToList();
            }
            foreach (var extension in extensions)
            {
                var normalizedExtension = extension.ToLowerInvariant();
                foreach (Language language in LanguageUtils.Languages)
                {
                    var extensionIsFine = string.IsNullOrEmpty(normalizedExtension) ||
                        language.GetExtensions().Contains(normalizedExtension);
                    var languageIsFine = languages == null || languages.Contains(language);
                    if (extensionIsFine && languageIsFine)
                    {
                        result.Add(language);
                    }
                }
            }
            return result;
        }
    }
}
