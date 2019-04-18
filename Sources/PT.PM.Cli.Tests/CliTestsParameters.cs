using PT.PM.Cli.Common;

namespace PT.PM.Cli.Tests
{
    public class CliTestsParameters
    {
        [Option('s', "string")]
        public string File { get; set; }

        [Option('b', "bool1")]
        public bool? Bool1 { get; set; }

        [Option("bool2")]
        public bool? Bool2 { get; set; }

        [Option('i', "int1")]
        public int? Int1 { get; set; }

        [Option]
        public string Option { get; set; }

        [Option]
        public int Int { get; set; }

        [Option]
        public uint UInt { get; set; }

        [Option]
        public byte Byte { get; set; }

        [Option]
        public sbyte SByte { get; set; }

        [Option]
        public short Short { get; set; }

        [Option]
        public ushort UShort { get; set; }

        [Option]
        public long Long { get; set; }

        [Option]
        public ulong ULong { get; set; }

        [Option]
        public float Float { get; set; }

        [Option]
        public double Double { get; set; }

        [Option]
        public decimal Decimal { get; set; }

        [Option]
        public bool Bool { get; set; }

        [Option]
        public Stage Enum { get; set; }

        [Option]
        public string[] Array { get; set; }
    }
}
