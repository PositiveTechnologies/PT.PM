using System;
using System.Collections.Generic;
using PT.PM.Common;

namespace PT.PM.Patterns.PatternsRepository
{
    public class FilesPatternsRepository : IPatternsRepository
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public string Path { get; set; }

        public FilesPatternsRepository(string filePath)
        {
            Path = filePath;
        }

        public IEnumerable<PatternDto> GetAll()
        {
            throw new NotImplementedException();
        }

        public void Add(IEnumerable<PatternDto> pattern)
        {
            throw new NotImplementedException();
        }

        public PatternDto GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
