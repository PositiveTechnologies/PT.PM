using System;
using System.IO;

namespace CodeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = new UstNodesConverterGenerator().Generate();
            File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "Generated.cs"), code);
        }
    }
}
