namespace PT.PM.Common
{
    public static class Uncertain
    {
        public readonly static Language Language =
            new Language("Uncertain", ".*", false, "Uncertain", haveAntlrParser: false);
    }
}
