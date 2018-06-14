using PT.PM.Common;

namespace PT.PM.CLangsParseTreeUst
{
    public static class ObjectiveC
    {
        public readonly static Language Language =
            new Language("Objective-C", new[] { ".m", ".mm" }, false, "Objective-C", new[] { C.Language });
    }
}