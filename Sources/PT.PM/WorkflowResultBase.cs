using Newtonsoft.Json;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Matching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PT.PM
{
    public abstract class WorkflowResultBase<TStage, TPattern, TMatchResult>
        where TStage : struct, IConvertible
        where TMatchResult : MatchResultBase<TPattern>
    {
        private HashSet<CodeFile> sourceCodeFiles = new HashSet<CodeFile>();
        private List<ParseTree> parseTrees = new List<ParseTree>();
        private List<RootUst> usts = new List<RootUst>();
        private List<TPattern> patterns = new List<TPattern>();
        private List<IMatchResultBase> matchResults = new List<IMatchResultBase>();

        private long totalReadTicks;
        private long totalParseTicks;
        private long totalConvertTicks;
        private long totalSimplifyTicks;
        private long totalMatchTicks;
        private long totalPatternsTicks;

        private long totalLexerTicks;
        private long totalParserTicks;

        private int totalProcessedFilesCount;
        private int totalProcessedCharsCount;
        private int totalProcessedLinesCount;

        private int totalProcessedPatternsCount;

        private int totalTerminatedFilesCount;

        public WorkflowResultBase(IEnumerable<Language> languages, int threadCount, TStage stage, bool isIncludeIntermediateResult)
        {
            AnalyzedLanguages = languages.ToList();
            ThreadCount = threadCount;
            Stage = stage;
            IsIncludeIntermediateResult = isIncludeIntermediateResult;
        }

        public IReadOnlyList<Language> AnalyzedLanguages { get; private set; }

        public IReadOnlyList<Language> BaseLanguages { get; set; }

        public HashSet<TStage> RenderStages { get; set; } = new HashSet<TStage>();

        public int ThreadCount { get; private set; }

        public TStage Stage { get; private set; }

        public bool IsIncludeIntermediateResult { get; private set; }

        public int ErrorCount { get; set; }

        [JsonIgnore]
        public HashSet<CodeFile> SourceCodeFiles => sourceCodeFiles;

        [JsonIgnore]
        public IReadOnlyList<ParseTree> ParseTrees => ValidateStageAndReturn(PM.Stage.ParseTree.ToString(), parseTrees);

        [JsonIgnore]
        public IReadOnlyList<RootUst> Usts
        {
            get
            {
                if (!Stage.Is(PM.Stage.Ust) && !Stage.Is(PM.Stage.SimplifiedUst) && !IsIncludeIntermediateResult &&
                    RenderStages.All(stage => Convert.ToInt32(stage) != (int)PM.Stage.Ust))
                {
                    ThrowInvalidStageException(PM.Stage.Ust.ToString());
                }
                return usts;
            }
        }

        [JsonIgnore]
        public IReadOnlyList<IMatchResultBase> MatchResults => matchResults;

        [JsonIgnore]
        public IReadOnlyList<TPattern> Patterns
        {
            get
            {
                if (!Stage.Is(PM.Stage.Pattern) && (Stage.IsLess(PM.Stage.Match) || !IsIncludeIntermediateResult))
                {
                    ThrowInvalidStageException(PM.Stage.Pattern.ToString());
                }

                return patterns;
            }
        }

        public long TotalReadTicks => totalReadTicks;
        public long TotalParseTicks => totalParseTicks;
        public long TotalConvertTicks => totalConvertTicks;
        public long TotalSimplifyTicks => totalSimplifyTicks;
        public long TotalMatchTicks => totalMatchTicks;
        public long TotalPatternsTicks => totalPatternsTicks;

        public long TotalLexerTicks => totalLexerTicks;
        public int TotalMatchesCount => MatchResults.Count;
        public long TotalParserTicks => totalParserTicks;

        public int TotalProcessedFilesCount => totalProcessedFilesCount;
        public int TotalProcessedCharsCount => totalProcessedCharsCount;
        public int TotalProcessedLinesCount => totalProcessedLinesCount;

        public int TotalProcessedPatternsCount => totalProcessedPatternsCount;

        public int TotalTerminatedFilesCount => totalTerminatedFilesCount;

        public int TotalFilesCount { get; set; }

        public void AddResultEntity(CodeFile sourceCodeFile)
        {
            AddEntity(sourceCodeFiles, sourceCodeFile);
        }

        public void AddResultEntity(ParseTree parseTree)
        {
            if (Stage.Is(PM.Stage.ParseTree) || IsIncludeIntermediateResult ||
                RenderStages.Any(stage => Convert.ToInt32(stage) == (int)PM.Stage.ParseTree))
            {
                AddEntity(parseTrees, parseTree);
            }
        }

        public void AddResultEntity(RootUst ust, bool convert)
        {
            if (IsIncludeIntermediateResult || (convert && Stage.Is(PM.Stage.Ust)) || (!convert && Stage.Is(PM.Stage.SimplifiedUst)) ||
                Stage.Is(PM.Stage.Match) ||
                RenderStages.Any(stage => Convert.ToInt32(stage) == (int)PM.Stage.Ust))
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

        public void AddResultEntity(IEnumerable<IMatchResultBase> matchResults)
        {
            AddEntities(this.matchResults, matchResults);
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

        public void AddTerminatedFilesCount(int filesCount)
        {
            AddInt(ref totalTerminatedFilesCount, filesCount);
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

        public void AddSimplifyTime(long simplifyTicks)
        {
            AddTicks(ref totalSimplifyTicks, simplifyTicks);
        }

        public void AddMatchTime(long matchTicks)
        {
            AddTicks(ref totalMatchTicks, matchTicks);
        }

        public void AddPatternsTime(long patternsTicks)
        {
            AddTicks(ref totalPatternsTicks, patternsTicks);
        }

        public void AddProcessedPatternsCount(int patternsCount)
        {
            AddInt(ref totalProcessedPatternsCount, patternsCount);
        }

        public void AddLexerTime(long ticks)
        {
            AddTicks(ref totalLexerTicks, ticks);
        }

        public void AddParserTicks(long ticks)
        {
            AddTicks(ref totalParserTicks, ticks);
        }

        public virtual long TotalTimeTicks =>
            totalReadTicks +
            totalParseTicks +
            totalConvertTicks +
            totalSimplifyTicks +
            totalMatchTicks +
            totalPatternsTicks;

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

        protected void AddEntity<T>(ICollection<T> collection, T entity)
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
