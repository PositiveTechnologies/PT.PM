namespace PT.PM.Patterns
{
    public class PatternIdGenerator
    {
        private int currentId = 1;

        public void Reset()
        {
            currentId = 1;
        }

        public string NextId()
        {
            return currentId++.ToString();
        }
    }
}
