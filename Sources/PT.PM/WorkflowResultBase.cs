using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Matching;
using PT.PM.Patterns;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PT.PM
{
    public abstract class WorkflowResultBase<TStage, TPattern, TMatchingResult> 
        where TStage : struct, IConvertible
        where TPattern : PatternBase
        where TMatchingResult : MatchingResultBase<TPattern>
    {
        private List<SourceCodeFile> sourceCodeFiles = new List<SourceCodeFile>();
        private List<ParseTree> parseTrees = new List<ParseTree>();
        private List<Ust> usts = new List<Ust>();
        private List<TPattern> patterns = new List<TPattern>();
        private List<TMatchingResult> matchingResults = new List<TMatchingResult>();

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

        protected StageHelper<TStage> stageExt;

        public WorkflowResultBase(TStage stage, bool isIncludeIntermediateResult)
        {
            Stage = stage;
            stageExt = new StageHelper<TStage>(stage);
            IsIncludeIntermediateResult = isIncludeIntermediateResult;
        }

        public TStage Stage { get; private set; }

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

        public IReadOnlyList<TMatchingResult> MatchingResults => ValidateStageAndReturn(PM.Stage.Match.ToString(), matchingResults);

        public IReadOnlyList<TPattern> Patterns
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
            if (IsIncludeIntermediateResult ||
                (convert && stageExt.IsConvert) ||
                (!convert && stageExt.IsPreprocess))
            {
                
                lock (usts)
                {
                    int ustIndex = usts.FindIndex(tree => tree.FileName == ust.FileName);
                    if (ustIndex == -1)
                    {
                        usts.Add(ust);
                    }
                    else
                    {
                        usts.RemoveAt(ustIndex);
                        usts.Insert(ustIndex, ust);
                    }
                }
            }
        }

        public void AddResultEntity(IEnumerable<TMatchingResult> matchingResults)
        {
            lock (this.matchingResults)
            {
                this.matchingResults.AddRange(matchingResults);
            }
        }

        public void AddResultEntity(TPattern[] patterns)
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
