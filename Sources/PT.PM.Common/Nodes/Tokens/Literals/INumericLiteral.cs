using System;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public interface INumericLiteral<T> where T 
        : IComparable, IComparable<T>, IEquatable<T>
    {
        T Value { get; set; }
    }
}
