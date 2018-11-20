using Newtonsoft.Json;
using PT.PM.Common;
using PT.PM.Matching;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PT.PM
{
    public abstract class WorkflowResultBase<TStage, TPattern, TRenderStage>
        where TStage : Enum
        where TRenderStage : Enum
    {
        private List<IMatchResultBase> matchResults = new List<IMatchResultBase>();

        private long totalReadTicks;
        private long totalLexerTicks;
        private long totalParserTicks;
        private long totalConvertTicks;
        private long totalMatchTicks;

        private long totalPatternsTicks;

        private int totalProcessedFilesCount;
        private int totalProcessedCharsCount;
        private int totalProcessedLinesCount;

        private int totalProcessedPatternsCount;

        private int totalTerminatedFilesCount;

        public WorkflowResultBase(List<Language> languages, int threadCount, TStage stage)
        {
            AnalyzedLanguages = languages;
            ThreadCount = threadCount;
            Stage = stage;
        }

        public string RootPath { get; set; }

        public IReadOnlyList<Language> AnalyzedLanguages { get; }

        public IReadOnlyList<Language> BaseLanguages { get; set; }

        public HashSet<TRenderStage> RenderStages { get; set; } = new HashSet<TRenderStage>();

        public int ThreadCount { get; }

        public TStage Stage { get; }

        public bool IsFoldConstants { get; set; }

        public int ErrorCount { get; set; }

        [JsonIgnore]
        public HashSet<CodeFile> SourceCodeFiles { get; } = new HashSet<CodeFile>();

        [JsonIgnore]
        public IReadOnlyList<IMatchResultBase> MatchResults => matchResults;

        [JsonIgnore]
        public List<TPattern> Patterns { get; set; } = new List<TPattern>();

        public long TotalReadTicks => totalReadTicks;
        public long TotalLexerParserTicks => totalLexerTicks + totalParserTicks;
        public long TotalConvertTicks => totalConvertTicks;
        public long TotalMatchTicks => totalMatchTicks;
        public long TotalPatternsTicks => totalPatternsTicks;
        public long TotalLexerTicks => totalLexerTicks;
        public long TotalParserTicks => totalParserTicks;

        public int TotalMatchesCount => MatchResults.Count;

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

        public void AddConvertTime(TimeSpan convertTime)
        {
            AddTime(ref totalConvertTicks, convertTime);
        }

        public void AddMatchTime(TimeSpan matchTime)
        {
            AddTime(ref totalMatchTicks, matchTime);
        }

        public void AddPatternsTime(TimeSpan patternsTime)
        {
            Interlocked.Add(ref totalPatternsTicks, patternsTime.Ticks);
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
            totalReadTicks + totalLexerTicks + totalParserTicks + totalConvertTicks + totalMatchTicks + totalPatternsTicks;

        protected void AddEntity<T>(ICollection<T> collection, T entity)
        {
            lock (collection)
            {
                collection.Add(entity);
            }
        }

        protected void AddEntities<T>(List<T> collection, IEnumerable<T> entities)
        {
            lock (collection)
            {
                collection.AddRange(entities);
            }
        }

        protected void AddInt(ref int total, int value)
        {
            Interlocked.Add(ref total, value);
        }

        protected void AddTime(ref long totalTicks, TimeSpan timeSpan)
        {
            Interlocked.Add(ref totalTicks, timeSpan.Ticks);
        }
    }
}
