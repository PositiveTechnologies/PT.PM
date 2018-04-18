using PT.PM.Common;
using System;
using System.Collections.Generic;

namespace PT.PM.Matching.PatternsRepository
{
    public class DummyPatternsRepository : IPatternsRepository
    {
        public string Path { get; set; } = "";

        public IEnumerable<string> Identifiers { get; set; } = ArrayUtils<string>.EmptyArray;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public void Add(IEnumerable<PatternDto> pattern) => throw new InvalidOperationException("Should not be called");

        public void Clear() => throw new InvalidOperationException("Should not be called");

        public IEnumerable<PatternDto> GetAll() => ArrayUtils<PatternDto>.EmptyArray;
    }
}
