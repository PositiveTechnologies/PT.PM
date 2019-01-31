namespace PT.PM.Common.Nodes
{
    public enum NodeType
    {
        RootUst = 0,

        // Collections
        ArgsUst,
        Collection,
        EntitiesUst,

        // Expressions
        AnonymousMethodExpression,
        AnonymousObjectExpression,
        ArgumentExpression,
        ArrayCreationExpression,
        AssignmentExpression,
        BinaryOperatorExpression,
        CastExpression,
        ConditionalExpression,
        IndexerExpression,
        InvocationExpression,
        MemberReferenceExpression,
        MultichildExpression,
        ObjectCreateExpression,
        TupleCreateExpression,
        UnaryOperatorExpression,
        VariableDeclarationExpression,
        WrapperExpression,
        YieldExpression,

        // Generate Scope
        NamespaceDeclaration,
        TypeDeclaration,
        UsingDeclaration,

        // Specific
        ArrayPatternExpression,
        AsExpression,
        CheckedExpression,
        CommaExpression,
        DebuggerStatement,
        FixedStatement,
        LockStatement,
        UnsafeStatement,
        WithStatement,

        // Sql
        QueryArgs,
        SqlBlockStatement,
        SqlQuery,

        // Statements
        SwitchSection,
        SwitchStatement,
        CatchClause,
        TryCatchStatement,
        BlockStatement,
        BreakStatement,
        ContinueStatement,
        DoWhileStatement,
        EmptyStatement,
        ExpressionStatement,
        ForeachStatement,
        ForStatement,
        GotoStatement,
        IfElseStatement,
        LabelStatement,
        ReturnStatement,
        ThrowStatement,
        TypeDeclarationStatement,
        WhileStatement,
        WrapperStatement,

        // Literals
        BinaryOperatorLiteral,
        BooleanLiteral,
        CommentLiteral,
        FloatLiteral,
        InOutModifierLiteral,
        IntLiteral,
        ModifierLiteral,
        NullLiteral,
        StringLiteral,
        TypeTypeLiteral,
        UnaryOperatorLiteral,

        // Tokens
        BaseReferenceToken,
        IdToken,
        ThisReferenceToken,
        TypeToken,

        // Type Members
        ConstructorDeclaration,
        FieldDeclaration,
        MethodDeclaration,
        ParameterDeclaration,
        PropertyDeclaration,
        StatementDeclaration,
    }
}