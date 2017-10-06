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

        public bool IsFile => stageInt == (int)PM.Stage.File;

        public bool IsParseTree => stageInt == (int)PM.Stage.ParseTree;

        public bool IsUst => stageInt == (int)PM.Stage.Ust;

        public bool IsSimpleParseTree => stageInt == (int)PM.Stage.SimpleParseTree;

        public bool IsPattern => stageInt == (int)PM.Stage.Pattern;

        public bool IsContainsFile => stageInt >= (int)PM.Stage.File;

        public bool IsContainsParseTree => stageInt >= (int)PM.Stage.ParseTree;

        public bool IsContainsUst => stageInt >= (int)PM.Stage.Ust;

        public bool IsContainsMatch => stageInt >= (int)PM.Stage.Match;

        public bool IsLessThanMatch => stageInt < (int)PM.Stage.Match;
    }
}
