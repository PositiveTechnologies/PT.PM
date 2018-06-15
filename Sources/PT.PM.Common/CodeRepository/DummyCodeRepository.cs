using System;
using System.Collections.Generic;

namespace PT.PM.Common.CodeRepository
{
    public class DummyCodeRepository : SourceCodeRepository
    {
        public static DummyCodeRepository Instance = new DummyCodeRepository();

        public override IEnumerable<string> GetFileNames() => ArrayUtils<string>.EmptyArray;

        public override bool IsFileIgnored(string fileName, bool withParser) => throw new InvalidOperationException();

        public override CodeFile ReadFile(string fileName) => throw new InvalidOperationException();
    }
}
