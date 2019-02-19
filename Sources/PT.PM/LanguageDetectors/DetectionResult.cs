using PT.PM.Common;
using System;
using System.Collections.Generic;

namespace PT.PM
{
    public class DetectionResult
    {
        public Language Language { get; }

        public ParseTree ParseTree { get; }

        public List<Exception> Errors { get; }

        public List<object> Infos { get; }

        public List<string> Debugs { get; }

        public DetectionResult(Language language)
        {
            Language = language;
            Errors = new List<Exception>();
            Infos = new List<object>();
            Debugs = new List<string>();
        }

        public DetectionResult(Language language, ParseTree parseTree,
            List<Exception> errors, List<object> infos, List<string> debugs)
        {
            Language = language;
            ParseTree = parseTree;
            Errors = errors;
            Infos = infos;
            Debugs = debugs;
        }
    }
}
