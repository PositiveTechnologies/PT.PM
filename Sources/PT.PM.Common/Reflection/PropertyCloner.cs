namespace PT.PM.Common.Reflection
{
    public class PropertyCloner<T> : PropertyVisitor<T, T>
    {
        public override bool Clone => true;
    }
}
