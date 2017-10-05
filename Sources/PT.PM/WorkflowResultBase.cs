using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Matching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PT.PM
{
    public abstract class WorkflowResultBase<TStage, TPattern, TMatchingResult>
        where TStage : struct, IConvertible
        where TMatchingResult : MatchingResultBase<TPattern>
    {
        private List<SourceCodeFile> sourceCodeFiles = new List<SourceCodeFile>();
        private List<ParseTree> parseTrees = new List<ParseTree>();
        private List<RootUst> usts = new List<RootUst>();
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

        private int totalProcessedFilesCount;
        private int totalProcessedCharsCount;
        private int totalProcessedLinesCount;

        protected StageHelper<TStage> stageExt;

        public WorkflowResultBase(IEnumerable<LanguageInfo> languages, int threadCount, TStage stage, bool isIncludeIntermediateResult)
        {
            AnalyzedLanguages = languages.ToList();
            ThreadCount = threadCount;
            Stage = stage;
            stageExt = new StageHelper<TStage>(stage);
            IsIncludeIntermediateResult = isIncludeIntermediateResult;
        }

        public IReadOnlyList<LanguageInfo> AnalyzedLanguages { get; private set; }

        public IReadOnlyList<LanguageInfo> BaseLanguages { get; set; }

        public HashSet<TStage> RenderStages { get; set; } = new HashSet<TStage>();

        public int ThreadCount { get; private set; }

        public TStage Stage { get; private set; }

        public bool IsIncludeIntermediateResult { get; private set; }

        public int ErrorCount { get; set; }

        public IReadOnlyList<SourceCodeFile> SourceCodeFiles => ValidateStageAndReturn(PM.Stage.Read.ToString(), sourceCodeFiles);

        public IReadOnlyList<ParseTree> ParseTrees => ValidateStageAndReturn(PM.Stage.Parse.ToString(), parseTrees);

        public IReadOnlyList<RootUst> Usts
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

        public int TotalProcessedFilesCount => totalProcessedFilesCount;
        public int TotalProcessedCharsCount => totalProcessedCharsCount;
        public int TotalProcessedLinesCount => totalProcessedLinesCount;

        public int TotalFilesCount { get; set; }

        public void AddResultEntity(SourceCodeFile sourceCodeFile)
        {
            if (stageExt.IsRead || IsIncludeIntermediateResult)
            {
                AddEntity(sourceCodeFiles, sourceCodeFile);
            }
        }

        public void AddResultEntity(ParseTree parseTree)
        {
            if (stageExt.IsParse || IsIncludeIntermediateResult)
            {
                AddEntity(parseTrees, parseTree);
            }
        }

        public void AddResultEntity(RootUst ust, bool convert)
        {
            if (IsIncludeIntermediateResult || (convert && stageExt.IsConvert) || (!convert && stageExt.IsPreprocess))
            {
                int ustIndex = usts.FindIndex(tree => tree.SourceCodeFile == ust.SourceCodeFile);
                lock (usts)
                {
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
            AddEntities(this.matchingResults, matchingResults);
        }

        public void AddResultEntity(IEnumerable<TPattern> patterns)
        {
            AddEntities(this.patterns, patterns);
        }

        public void AddProcessedFilesCount(int filesCount)
        {
            AddInt(ref totalProcessedFilesCount, filesCount);
        }

        public void AddProcessedCharsCount(int charsCount)
        {
            AddInt(ref totalProcessedCharsCount, charsCount);
        }

        public void AddProcessedLinesCount(int linesCount)
        {
            AddInt(ref totalProcessedLinesCount, linesCount);
        }

        public void AddReadTime(long readTicks)
        {
            AddTicks(ref totalReadTicks, readTicks);
        }

        public void AddParseTime(long parseTicks)
        {
            AddTicks(ref totalParseTicks, parseTicks);
        }

        public void AddConvertTime(long convertTicks)
        {
            AddTicks(ref totalConvertTicks, convertTicks);
        }

        public void AddPreprocessTime(long preprocessTicks)
        {
            AddTicks(ref totalPreprocessTicks, preprocessTicks);
        }

        public void AddMatchTime(long matchTicks)
        {
            AddTicks(ref totalMatchTicks, matchTicks);
        }

        public void AddPatternsTime(long patternsTicks)
        {
            AddTicks(ref totalPatternsTicks, patternsTicks);
        }

        public void AddLexerTime(long ticks)
        {
            AddTicks(ref totalLexerTicks, ticks);
        }

        public void AddParserTicks(long ticks)
        {
            AddTicks(ref totalParserTicks, ticks);
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

        protected void AddEntity<T>(List<T> collection, T entity)
        {
            if (ThreadCount == 1)
            {
                collection.Add(entity);
            }
            else
            {
                lock (collection)
                {
                    collection.Add(entity);
                }
            }
        }

        protected void AddEntities<T>(List<T> collection, IEnumerable<T> entities)
        {
            if (ThreadCount == 1)
            {
                collection.AddRange(entities);
            }
            else
            {
                lock (collection)
                {
                    collection.AddRange(entities);
                }
            }
        }

        protected void AddInt(ref int total, int value)
        {
            if (ThreadCount == 1)
            {
                total += value;
            }
            else
            {
                Interlocked.Add(ref total, value);
            }
        }

        protected void AddTicks(ref long total, long ticks)
        {
            if (ThreadCount == 1)
            {
                total += ticks;
            }
            else
            {
                Interlocked.Add(ref total, ticks);
            }
        }
    }
}
