using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Matching;
using PT.PM.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PT.PM
{
    public abstract class WorkflowResultBase<T> where T : struct, IConvertible
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

        private long totalLexerTicks;
        private long totalParserTicks;

        private int totalProcessedFileCount;
        private int totalProcessedCharsCount;
        private int totalProcessedLinesCount;

        protected StageHelper<T> stageExt;

        public WorkflowResultBase(T stage, bool isIncludeIntermediateResult)
        {
            Stage = stage;
            stageExt = new StageHelper<T>(stage);
            IsIncludeIntermediateResult = isIncludeIntermediateResult;
        }

        public T Stage { get; private set; }

        public bool IsIncludeIntermediateResult { get; private set; }

        public int ErrorCount { get; set; }

        public IReadOnlyList<SourceCodeFile> SourceCodeFiles => ValidateStageAndReturn(PM.Stage.Read.ToString(), sourceCodeFiles);

        public IReadOnlyList<ParseTree> ParseTrees => ValidateStageAndReturn(PM.Stage.Parse.ToString(), parseTrees);

        public IReadOnlyList<Ust> Usts
        {
            get
            {
                if (!stageExt.IsConvert && !stageExt.IsPreprocess && !IsIncludeIntermediateResult)
                {
                    ThrowInvalidStageException(PM.Stage.Convert.ToString());
                }
                return usts;
            }
        }

        public ParseTree LastParseTree => ParseTrees.Last();

        public Ust LastUst => Usts.Last();

        public IReadOnlyList<MatchingResult> MatchingResults => ValidateStageAndReturn(PM.Stage.Match.ToString(), matchingResults);

        public List<Pattern> Patterns
        {
            get
            {
                if (!stageExt.IsPatterns && (stageExt.IsLessThanMatch || !IsIncludeIntermediateResult))
                {
                    ThrowInvalidStageException(PM.Stage.Patterns.ToString());
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

        public void AddResultEntity(SourceCodeFile sourceCodeFile)
        {
            if (stageExt.IsRead || IsIncludeIntermediateResult)
            {
                lock (sourceCodeFiles)
                {
                    sourceCodeFiles.Add(sourceCodeFile);
                }
            }
        }

        public void AddResultEntity(ParseTree parseTree)
        {
            if (stageExt.IsParse || IsIncludeIntermediateResult)
            {
                lock (parseTrees)
                {
                    parseTrees.Add(parseTree);
                }
            }
        }

        public void AddResultEntity(Ust ust, bool convert)
        {
            if ((convert && (stageExt.IsConvert || IsIncludeIntermediateResult)) ||
                (!convert && (stageExt.IsPreprocess || IsIncludeIntermediateResult)))
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

        public void AddReadTime(long readTicks)
        {
            Interlocked.Add(ref totalReadTicks, readTicks);
        }

        public void AddParseTime(long parseTicks)
        {
            Interlocked.Add(ref totalParseTicks, parseTicks);
        }

        public void AddConvertTime(long convertTicks)
        {
            Interlocked.Add(ref totalConvertTicks, convertTicks);
        }

        public void AddPreprocessTime(long preprocessTicks)
        {
            Interlocked.Add(ref totalPreprocessTicks, preprocessTicks);
        }

        public void AddMatchTime(long matchTicks)
        {
            Interlocked.Add(ref totalMatchTicks, matchTicks);
        }

        public void AddPatternsTime(long patternsTicks)
        {
            Interlocked.Add(ref totalPatternsTicks, patternsTicks);
        }

        public void AddLexerTime(long ticks)
        {
            Interlocked.Add(ref totalLexerTicks, ticks);
        }

        public void AddParserTicks(long ticks)
        {
            Interlocked.Add(ref totalParserTicks, ticks);
        }

        public long GetTotalTimeTicks()
        {
            return totalReadTicks + totalParseTicks + totalConvertTicks +
                   totalPreprocessTicks + totalMatchTicks + totalPatternsTicks;
        }

        protected Result ValidateStageAndReturn<Result>(string stage, Result result)
        {
            if (Stage.ToString() != stage && !IsIncludeIntermediateResult)
                ThrowInvalidStageException(stage);
            return result;
        }

        protected void ThrowInvalidStageException(string stage)
        {
            throw new InvalidOperationException($"Set {stage} as a final Stage or activate {nameof(IsIncludeIntermediateResult)} property");
        }
    }
}
