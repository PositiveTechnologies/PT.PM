using System;

namespace PT.PM
{
    public class StageHelper<T> where T : struct, IConvertible
    {
        private int stageInt;

        public T Stage { get; }

        public StageHelper(T stage)
        {
            Stage = stage;
            stageInt = Convert.ToInt32(Stage);
        }

        public bool IsRead => stageInt == (int)PM.Stage.Read;

        public bool IsParse => stageInt == (int)PM.Stage.Parse;

        public bool IsConvert => stageInt == (int)PM.Stage.Convert;

        public bool IsPreprocess => stageInt == (int)PM.Stage.Preprocess;

        public bool IsPatterns => stageInt == (int)PM.Stage.Patterns;

        public bool IsContainsRead => stageInt >= (int)PM.Stage.Read;

        public bool IsContainsParse => stageInt >= (int)PM.Stage.Parse;

        public bool IsContainsConvert => stageInt >= (int)PM.Stage.Convert;

        public bool IsLessThanMatch => stageInt < (int)PM.Stage.Match;
    }
}
