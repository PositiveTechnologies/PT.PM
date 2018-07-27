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
    public abstract class WorkflowResultBase<TStage, TPattern, TMatchResult, TRenderStage>
        where TStage : Enum
        where TMatchResult : MatchResultBase<TPattern>
        where TRenderStage : Enum
    {
        // Erase parse trees if required because they are intermediate objects.
        private List<WeakReference<ParseTree>> parseTrees = new List<WeakReference<ParseTree>>();

        private List<RootUst> usts = new List<RootUst>();
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
        }

        public string RootPath { get; set; }

        public IReadOnlyList<Language> AnalyzedLanguages { get; private set; }

        public IReadOnlyList<Language> BaseLanguages { get; set; }

        public HashSet<TRenderStage> RenderStages { get; set; } = new HashSet<TRenderStage>();

        public int ThreadCount { get; private set; }

        public TStage Stage { get; private set; }

        public bool IsSimplifyUst { get; set; }

        public int ErrorCount { get; set; }

        [JsonIgnore]
        public HashSet<CodeFile> SourceCodeFiles { get; } = new HashSet<CodeFile>();

        [JsonIgnore]
        public IReadOnlyList<ParseTree> ParseTrees =>
            parseTrees.Select(parseTree => parseTree.TryGetTarget(out ParseTree target) ? target : null)
            .Where(parseTree => parseTree != null)
            .ToList();

        [JsonIgnore]
        public IReadOnlyList<RootUst> Usts => usts;

        [JsonIgnore]
        public IReadOnlyList<IMatchResultBase> MatchResults => matchResults;

        [JsonIgnore]
        public List<TPattern> Patterns { get; set; } = new List<TPattern>();

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
            AddEntity(SourceCodeFiles, sourceCodeFile);
        }

        public void AddResultEntity(ParseTree parseTree)
        {
            if (ThreadCount == 1)
            {
                parseTrees.Add(new WeakReference<ParseTree>(parseTree));
            }
            else
            {
                lock (parseTrees)
                {
                    parseTrees.Add(new WeakReference<ParseTree>(parseTree));
                }
            }
        }

        public void AddResultEntity(RootUst ust)
        {
            AddEntity(usts, ust);
        }

        public void AddResultEntity(IEnumerable<IMatchResultBase> matchResults)
        {
            AddEntities(this.matchResults, matchResults);
        }

        public void AddResultEntity(IEnumerable<TPattern> patterns)
        {
            AddEntities(Patterns, patterns);
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

        public void AddReadTime(TimeSpan readTime)
        {
            AddTime(ref totalReadTicks, readTime);
        }

        public void AddParseTime(TimeSpan parseTime)
        {
            AddTime(ref totalParseTicks, parseTime);
        }

        public void AddConvertTime(TimeSpan convertTime)
        {
            AddTime(ref totalConvertTicks, convertTime);
        }

        public void AddSimplifyTime(TimeSpan simplifyTime)
        {
            AddTime(ref totalSimplifyTicks, simplifyTime);
        }

        public void AddMatchTime(TimeSpan matchTime)
        {
            AddTime(ref totalMatchTicks, matchTime);
        }

        public void AddPatternsTime(TimeSpan patternsTime)
        {
            AddTime(ref totalPatternsTicks, patternsTime);
        }

        public void AddProcessedPatternsCount(int patternsCount)
        {
            AddInt(ref totalProcessedPatternsCount, patternsCount);
        }

        public void AddLexerTime(TimeSpan lexerTime)
        {
            AddTime(ref totalLexerTicks, lexerTime);
        }

        public void AddParserTicks(TimeSpan parserTime)
        {
            AddTime(ref totalParserTicks, parserTime);
        }

        public virtual long TotalTimeTicks =>
            totalReadTicks +
            totalParseTicks +
            totalConvertTicks +
            totalSimplifyTicks +
            totalMatchTicks +
            totalPatternsTicks;

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

        protected void AddTime(ref long total, TimeSpan timeSpan)
        {
            if (ThreadCount == 1)
            {
                total += timeSpan.Ticks;
            }
            else
            {
                Interlocked.Add(ref total, timeSpan.Ticks);
            }
        }
    }
}
