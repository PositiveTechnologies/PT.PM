using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using PT.PM.Common;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrParserConverter : AntlrParserBase, IParserConverter
    {
        private const double ParseRatio = 0.5;

        protected override bool IsParseTreeFree => true;

        protected abstract AntlrListenerConverter InitListener(TextFile sourceFile);

        public RootUst ParseConvert(IList<IToken> tokens, out TimeSpan parserTimeSpan, out TimeSpan converterTimeSpan)
        {
            if (tokens.Count == 0)
            {
                return null;
            }

            var listener = InitListener(((LightToken)tokens[0]).TextFile);

            if (listener is ILoggable loggable)
            {
                loggable.Logger = Logger;
            }

            var rootUst = (RootUst)ProcessTokens(tokens, listener, out _, out TimeSpan parserConverterTimeSpan);

            long ticks = parserConverterTimeSpan.Ticks;
            parserTimeSpan = new TimeSpan((long)Math.Round(ticks * ParseRatio));
            converterTimeSpan = new TimeSpan((long)Math.Round(ticks * (1 - ParseRatio)));

            return rootUst;
        }
    }
}
