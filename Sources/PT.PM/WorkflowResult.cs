using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Matching;
using PT.PM.Patterns;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PT.PM
{
    public class WorkflowResult
    {
        private List<SourceCodeFile> sourceCodeFiles = new List<SourceCodeFile>();
        private List<ParseTree> parseTrees = new List<ParseTree>();
        private List<Ust> usts = new List<Ust>();
        private List<MatchingResult> matchingResults = new List<MatchingResult>();
        private List<Pattern> patterns = new List<Pattern>();

        private long totalReadTicks;
        private long totalParseTicks;
        private long totalConvertTicks;
        private long totalPreprocessTicks;
        private long totalMatchTicks;
        private long totalPatternsTicks;

        private int totalProcessedFileCount;
        private int totalProcessedCharsCount;
        private int totalProcessedLinesCount;

        private long totalLexerTicks;
        private long totalParserTicks;

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
                    ThrowInvalidStageException(Stage.Parse);
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
                    ThrowInvalidStageException(Stage.Parse);
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
                    ThrowInvalidStageException(Stage.Convert);
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
                    ThrowInvalidStageException(Stage.Match);
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
                    ThrowInvalidStageException(Stage.Patterns);
                }

                return patterns;
            }
        }

        public long TotalReadTicks => totalReadTicks;
        public long TotalParseTicks => totalParseTicks;
        public long TotalConvertTicks => totalConvertTicks;
        public long TotalPreprocessTicks => totalPreprocessTicks;
        public long TotalMatchTicks => totalMatchTicks;
        public long TotalPatternsTicks => totalPatternsTicks;
        public long TotalLexerTicks => totalLexerTicks;
        public long TotalParserTicks => totalParserTicks;

        public int TotalProcessedFilesCount => totalProcessedFileCount;
        public int TotalProcessedCharsCount => totalProcessedCharsCount;
        public int TotalProcessedLinesCount => totalProcessedLinesCount;

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

        public void AddStageTime(Stage stage, long ticks)
        {
            switch (stage)
            {
                case Stage.Read:
                    Interlocked.Add(ref totalReadTicks, ticks);
                    break;
                case Stage.Parse:
                    Interlocked.Add(ref totalParserTicks, ticks);
                    break;
                case Stage.Convert:
                    Interlocked.Add(ref totalConvertTicks, ticks);
                    break;
                case Stage.Preprocess:
                    Interlocked.Add(ref totalPreprocessTicks, ticks);
                    break;
                case Stage.Match:
                    Interlocked.Add(ref totalMatchTicks, ticks);
                    break;
                case Stage.Patterns:
                    Interlocked.Add(ref totalPatternsTicks, ticks);
                    break;
            }
        }

        public void AddLexerTime(long ticks)
        {
            Interlocked.Add(ref totalLexerTicks, ticks);
        }

        public void AddParserTicks(long ticks)
        {
            Interlocked.Add(ref totalParserTicks, ticks);
        }

        public void AddProcessedFilesCount(int filesCount)
        {
            Interlocked.Add(ref totalProcessedFileCount, filesCount);
        }

        public void AddProcessedCharsCount(int charsCount)
        {
            Interlocked.Add(ref totalProcessedCharsCount, charsCount);
        }

        public void AddProcessedLinesCount(int linesCount)
        {
            Interlocked.Add(ref totalProcessedLinesCount, linesCount);
        }

        public long GetTotalTimeTicks()
        {
            return totalReadTicks + totalParseTicks + totalConvertTicks +
                   totalPreprocessTicks + totalMatchTicks + totalPatternsTicks;
        }

        private void ThrowInvalidStageException(Stage stage)
        {
            throw new InvalidOperationException($"Set {stage} as a final Stage or activate {nameof(IsIncludeIntermediateResult)} property");
        }
    }
}
