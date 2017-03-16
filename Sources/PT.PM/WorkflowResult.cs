using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Matching;
using PT.PM.Patterns;
using System;
using System.Collections.Generic;

namespace PT.PM
{
    public class WorkflowResult
    {
        private List<SourceCodeFile> sourceCodeFiles = new List<SourceCodeFile>();
        private List<ParseTree> parseTrees = new List<ParseTree>();
        private List<Ust> usts = new List<Ust>();
        private List<MatchingResult> matchingResults = new List<MatchingResult>();
        private List<Pattern> patterns = new List<Pattern>();

        public WorkflowResult(Stage stage, bool isIncludeIntermediateResult)
        {
            Stage = stage;
            IsIncludeIntermediateResult = isIncludeIntermediateResult;
        }

        public Stage Stage { get; private set; }

        public bool IsIncludeIntermediateResult { get; private set; }

        public IReadOnlyList<SourceCodeFile> SourceCodeFiles
        {
            get
            {
                if (Stage != Stage.Parse && !IsIncludeIntermediateResult)
                {
                    ThrowException(Stage.Parse);
                }
                return sourceCodeFiles;
            }
        }

        public IReadOnlyList<ParseTree> ParseTrees
        {
            get
            {
                if (Stage != Stage.Parse && !IsIncludeIntermediateResult)
                {
                    ThrowException(Stage.Parse);
                }
                return parseTrees;
            }
        }

        public ParseTree LastParseTree => ParseTrees?[0];

        public IReadOnlyList<Ust> Usts
        {
            get
            {
                if (Stage != Stage.Convert && !IsIncludeIntermediateResult)
                {
                    ThrowException(Stage.Convert);
                }
                return usts;
            }
        }

        public Ust LastUst => Usts?[0];

        public IReadOnlyList<MatchingResult> MatchingResults
        {
            get
            {
                if (Stage != Stage.Match && !IsIncludeIntermediateResult)
                {
                    ThrowException(Stage.Match);
                }
                return matchingResults;
            }
        }

        public List<Pattern> Patterns
        {
            get
            {
                if (Stage != Stage.Patterns && (Stage < Stage.Match || !IsIncludeIntermediateResult))
                {
                    ThrowException(Stage.Patterns);
                }

                return patterns;
            }
        }

        public int ErrorCount { get; set; }

        public void AddResultEntity(SourceCodeFile sourceCodeFile)
        {
            if (Stage == Stage.Read || IsIncludeIntermediateResult)
            {
                lock (sourceCodeFiles)
                {
                    sourceCodeFiles.Add(sourceCodeFile);
                }
            }
        }

        public void AddResultEntity(ParseTree parseTree)
        {
            if (Stage == Stage.Parse || IsIncludeIntermediateResult)
            {
                lock (parseTrees)
                {
                    parseTrees.Add(parseTree);
                }
            }
        }

        public void AddResultEntity(Ust ust, bool convert)
        {
            if ((convert && (Stage == Stage.Convert || IsIncludeIntermediateResult)) ||
                (!convert && (Stage == Stage.Preprocess || IsIncludeIntermediateResult)))
            {
                lock (usts)
                {
                    usts.Add(ust);
                }
            }
        }

        public void AddResultEntity(IEnumerable<MatchingResult> matchingResults)
        {
            lock (this.matchingResults)
            {
                this.matchingResults.AddRange(matchingResults);
            }
        }

        public void AddResultEntity(IEnumerable<Pattern> patterns)
        {
            lock (this.patterns)
            {
                this.patterns.AddRange(patterns);
            }
        }

        private void ThrowException(Stage stage)
        {
            throw new InvalidOperationException($"Set {stage} as a final Stage or activate {nameof(IsIncludeIntermediateResult)} property");
        }
    }
}
