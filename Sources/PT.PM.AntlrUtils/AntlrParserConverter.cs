using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PT.PM.Common;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrParserConverter : AntlrParser, IParserConverter
    {
        private const double ParseRatio = 0.5;

        protected abstract AntlrListenerConverter InitListener(TextFile sourceFile);

        public RootUst ParseConvert(IList<IToken> tokens, out TimeSpan parserTimeSpan, out TimeSpan converterTimeSpan)
        {
            if (tokens.Count == 0)
            {
                return null;
            }

            var listener = InitListener(((LightToken)tokens.First(token => token is LightToken)).TextFile);

            if (listener is ILoggable loggable)
            {
                loggable.Logger = Logger;
            }

            ProcessTokens(tokens, listener, out _, out RootUst rootUst, out TimeSpan parserConverterTimeSpan);

            long ticks = parserConverterTimeSpan.Ticks;
            parserTimeSpan = new TimeSpan((long)Math.Round(ticks * ParseRatio));
            converterTimeSpan = new TimeSpan((long)Math.Round(ticks * (1 - ParseRatio)));

            return rootUst;
        }

        public RootUst Convert(AntlrParseTree parseTree)
        {
            var listener = InitListener(((LightToken)parseTree.Tokens.First(token => token is LightToken)).TextFile);

            if (listener is ILoggable loggable)
            {
                loggable.Logger = Logger;
            }

            listener.ParseTreeIsExisted = true;
            var parseTreeWalker = new ParseTreeWalker();
            parseTreeWalker.Walk(listener, parseTree.SyntaxTree);

            return listener.Complete();
        }
    }
}
