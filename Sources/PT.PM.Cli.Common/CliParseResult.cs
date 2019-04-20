using System;
using System.Collections.Generic;

namespace PT.PM.Cli.Common
{
    public class CliParseResult<TParameters>
        where TParameters : new()
    {
        public TParameters Parameters { get; }

        public bool ShowHelp { get; }

        public bool ShowVersion { get; }

        public List<Exception> Errors { get; }

        public CliParseResult(TParameters parameters, bool showHelp, bool showVersion, List<Exception> errors)
        {
            Parameters = parameters;
            ShowHelp = showHelp;
            ShowVersion = showVersion;
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        public override string ToString()
        {
            return $"{Parameters}; {string.Join(", ", Errors)}";
        }
    }
}