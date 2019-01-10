using System;
using System.Collections.Generic;
using PT.PM.Common.Files;

namespace PT.PM.Common.CodeRepository
{
    public class DummyCodeRepository : SourceCodeRepository
    {
        public static DummyCodeRepository Instance = new DummyCodeRepository();

        public DummyCodeRepository()
            : base(null)
        {
        }

        public override IEnumerable<string> GetFileNames() => ArrayUtils<string>.EmptyArray;

        public override bool IsFileIgnored(string fileName, bool withParser) => throw new InvalidOperationException();

        public override IFile ReadFile(string fileName) => throw new InvalidOperationException();
    }
}
