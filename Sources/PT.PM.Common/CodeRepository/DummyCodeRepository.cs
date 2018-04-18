using System;
using System.Collections.Generic;

namespace PT.PM.Common.CodeRepository
{
    public class DummyCodeRepository : SourceCodeRepository
    {
        public override IEnumerable<string> GetFileNames() => ArrayUtils<string>.EmptyArray;

        public override bool IsFileIgnored(string fileName) => throw new InvalidOperationException("Should not be called");

        public override CodeFile ReadFile(string fileName) => throw new InvalidOperationException("Should not be called");
    }
}
