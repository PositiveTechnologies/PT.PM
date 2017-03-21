using System;

namespace PT.PM
{
    public class StageHelper<T> where T : struct, IConvertible
    {
        public T Stage { get; private set; }

        public StageHelper(T stage)
        {
            Stage = stage;
        }

        public bool IsRead => Convert.ToInt32(Stage) == (int)PM.Stage.Read;

        public bool IsParse => Convert.ToInt32(Stage) == (int)PM.Stage.Parse;

        public bool IsConvert => Convert.ToInt32(Stage) == (int)PM.Stage.Convert;

        public bool IsPreprocess => Convert.ToInt32(Stage) == (int)PM.Stage.Preprocess;

        public bool IsPatterns => Convert.ToInt32(Stage) == (int)PM.Stage.Patterns;

        public bool IsContainsRead => Convert.ToInt32(Stage) >= (int)PM.Stage.Read;

        public bool IsContainsParse => Convert.ToInt32(Stage) >= (int)PM.Stage.Parse;

        public bool IsContainsConvert => Convert.ToInt32(Stage) >= (int)PM.Stage.Convert;

        public bool IsLessThanMatch => Convert.ToInt32(Stage) < (int)PM.Stage.Match;
    }
}
