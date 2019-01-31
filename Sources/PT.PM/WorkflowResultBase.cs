using Newtonsoft.Json;
using PT.PM.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using PT.PM.Common.Files;
using PT.PM.Matching;

namespace PT.PM
{
    public abstract class WorkflowResultBase<TStage, TPattern, TRenderStage>
        where TStage : Enum
        where TRenderStage : Enum
    {
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

        protected int totalSuppressedCount;
        protected int totalMatchesCount;

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
        public HashSet<IFile> SourceFiles { get; } = new HashSet<IFile>();

        [JsonIgnore]
        public List<TPattern> Patterns { get; set; } = new List<TPattern>();

        public long TotalReadTicks => totalReadTicks;
        public long TotalLexerParserTicks => totalLexerTicks + totalParserTicks;
        public long TotalConvertTicks => totalConvertTicks;
        public long TotalMatchTicks => totalMatchTicks;
        public long TotalPatternsTicks => totalPatternsTicks;
        public long TotalLexerTicks => totalLexerTicks;
        public long TotalParserTicks => totalParserTicks;

        public int TotalSuppressedCount => totalSuppressedCount;
        public int TotalMatchesCount => totalMatchesCount;

        public int TotalProcessedFilesCount => totalProcessedFilesCount;
        public int TotalProcessedCharsCount => totalProcessedCharsCount;
        public int TotalProcessedLinesCount => totalProcessedLinesCount;

        public int TotalProcessedPatternsCount => totalProcessedPatternsCount;

        public int TotalTerminatedFilesCount => totalTerminatedFilesCount;

        public int TotalFilesCount { get; set; }

        public void AddResultEntity(IFile sourceFile)
        {
            AddEntity(SourceFiles, sourceFile);
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

        public virtual void ProcessMatchResult(IMatchResultBase matchResult)
        {
            int matchedResultCount = 0;
            int suppressedCount = 0;

            GetMatchesCount(matchResult, ref matchedResultCount, ref suppressedCount);

            Interlocked.Add(ref totalMatchesCount, matchedResultCount);
            Interlocked.Add(ref totalSuppressedCount, suppressedCount);
        }

        protected static void GetMatchesCount(IMatchResultBase matchResult, ref int matchedResultCount, ref int suppressedCount)
        {
            int patternsCount = PatternRoot.ExtractKeys(matchResult.PatternKey).Length;
            if (patternsCount == 0)
            {
                patternsCount = 1;
            }

            matchedResultCount += patternsCount;
            if (matchResult.Suppressed)
            {
                suppressedCount += patternsCount;
            }
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
