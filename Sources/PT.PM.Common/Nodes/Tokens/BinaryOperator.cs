namespace PT.PM.Common.Nodes.Tokens
{
    /// <summary>
    /// TODO: possible replace with partial class. Specific constants will be defined in Specific folder.
    /// </summary>
    public enum BinaryOperator
    {
        None,             // No such operator
        Plus,             // +
        Minus,            // -
        Multiply,         // *
        Divide,           // /
        Mod,              // %
        BitwiseAnd,       // &
        BitwiseOr,        // |
        LogicalAnd,       // &&
        LogicalOr,        // ||
        BitwiseXor,       // ^
        ShiftLeft,        // <<
        ShiftRight,       // >>
        Equal,            // ==
        NotEqual,         // !=
        Greater,          // >
        Less,             // <
        GreaterOrEqual,   // >=
        LessOrEqual,      // <=

        /// C#
        NullCoalescing,   // ??

        // Java
        LogicalShift,     // >>>

        // PHP
        Concat,           // .
        Power,            // **

        // PHP & JavaScript
        StrictEqual,      // ===
        StrictNotEqual,   // !==

        // JavaScript
        In,               // in
        InstanceOf        // instanceof

    }
}
