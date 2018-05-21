using System;

namespace PT.PM
{
    public static class Utils
    {
        public const int DefaultMaxStackSize = 0;

        public static bool Is<TStage>(this TStage stage, Stage pmStage)
            where TStage : struct, IConvertible
        {
            return Convert.ToInt32(stage) == (int)pmStage;
        }

        public static bool IsGreaterOrEqual<TStage>(this TStage stage, Stage pmStage)
            where TStage : struct, IConvertible
        {
            return Convert.ToInt32(stage) >= (int)pmStage;
        }

        public static bool IsLess<TStage>(this TStage stage, Stage pmStage)
            where TStage : struct, IConvertible
        {
            return Convert.ToInt32(stage) < (int)pmStage;
        }
    }
}
