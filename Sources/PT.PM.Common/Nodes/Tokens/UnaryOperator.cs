﻿namespace PT.PM.Common.Nodes.Tokens
{
    /// <summary>
    /// TODO: possible replace with partial class. Specific constants will be defined in Specific folder.
    /// </summary>
    public enum UnaryOperator
    {
        None,          // No such operator
        Plus,          // +a
        Minus,         // -a
        Not,           // !a
        BitwiseNot,    // ~a
        Increment,     // ++a
        Decrement,     // --a
        PostIncrement, // a++
        PostDecrement, // a--
        Dereference,   // *a
        AddressOf,     // &a
        Delete,        // delete a
        DeleteArray,   // delete[] a
        TypeOf,        // typeof a

        // C#
        Await,          // await a

        // JavaScript
        Void
    }
}
