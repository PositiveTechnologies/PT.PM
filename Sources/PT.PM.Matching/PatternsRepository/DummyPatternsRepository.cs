using PT.PM.Common;
using System;
using System.Collections.Generic;

namespace PT.PM.Matching.PatternsRepository
{
    public class DummyPatternsRepository : IPatternsRepository
    {
        public static DummyPatternsRepository Instance = new DummyPatternsRepository();

        public string Path
        {
            get => "";
            set => new InvalidOperationException($"Unable to change {nameof(DummyPatternsRepository)}");
        }

        public IEnumerable<string> Identifiers
        {
            get => ArrayUtils<string>.EmptyArray;
            set => new InvalidOperationException($"Unable to change {nameof(DummyPatternsRepository)}");
        }

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public void Add(IEnumerable<PatternDto> pattern) =>
            throw new InvalidOperationException($"Unable to change {nameof(DummyPatternsRepository)}");

        public void Clear() =>
            throw new InvalidOperationException($"Unable to change {nameof(DummyPatternsRepository)}");

        public IEnumerable<PatternDto> GetAll() => ArrayUtils<PatternDto>.EmptyArray;
    }
}
