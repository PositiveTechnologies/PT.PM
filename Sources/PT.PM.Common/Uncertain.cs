namespace PT.PM.Common
{
    public static class Uncertain
    {
        public readonly static Language Language =
            new Language(nameof(Uncertain), ".*", false, "Uncertain", haveAntlrParser: false);
    }
}
