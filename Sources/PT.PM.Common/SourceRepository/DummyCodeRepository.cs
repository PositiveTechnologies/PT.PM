using System;
using System.Collections.Generic;
using PT.PM.Common.Files;

namespace PT.PM.Common.SourceRepository
{
    public class DummySourceRepository : SourceRepository
    {
        public static readonly DummySourceRepository Instance = new DummySourceRepository();

        public override IEnumerable<string> GetFileNames() => ArrayUtils<string>.EmptyArray;

        public override bool IsFileIgnored(string fileName, bool withParser, out Language language) => throw new InvalidOperationException();

        public override IFile ReadFile(string fileName) => throw new InvalidOperationException();
    }
}
