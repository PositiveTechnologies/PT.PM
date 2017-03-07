namespace PT.PM.Common
{
    /// <summary>
    /// Source: Roslyn, http://source.roslyn.codeplex.com/#Microsoft.CodeAnalysis/publicUtilities/Hash.cs
    /// </summary>
    public static class Hash
    {
        public static int Combine(int newKey, int currentKey)
        {
            return unchecked((currentKey * (int)0xA5555529) + newKey);
        }
    }
}
