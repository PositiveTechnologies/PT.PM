using NUnit.Framework;
using PT.PM.CSharpParseTreeUst;
using PT.PM.JavaParseTreeUst;
using PT.PM.JavaScriptParseTreeUst;
using PT.PM.PhpParseTreeUst;
using PT.PM.SqlParseTreeUst;
using static System.Console;

namespace PT.PM.Tests
{
    [SetUpFixture]
    public class SetUp
    {
        [OneTimeSetUp]
        public static void AssemblyInitialize()
        {
            WriteLine("Included languages:");
            WriteLine(Aspx.Language);
            WriteLine(CSharp.Language);
            WriteLine(Java.Language);
            WriteLine(Php.Language);
            WriteLine(Html.Language);
            WriteLine(JavaScript.Language);
            WriteLine(PlSql.Language);
            WriteLine(TSql.Language);
        }
    }
}
