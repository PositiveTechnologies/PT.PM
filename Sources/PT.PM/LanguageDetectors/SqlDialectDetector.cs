using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.LanguageDetectors;

namespace PT.PM
{
    public class SqlDialectDetector
    {
        public static List<Language> Detect(string data)
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
                int channel = token.Channel;

                if (token.Type == SqlDialectsLexer.CMD)
                {
                    string tokensText = token.Text;
                    if (!tokensText.Contains("/") && !tokensText.Contains("\\") && !tokensText.Contains("."))
                    {
                        channel = SqlDialectsLexer.T_SQL;
                    }
                }

                switch (channel)
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
    }
}