namespace PT.PM.Common.Json
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using PT.PM.Common.Nodes;
    using PT.PM.Common.Nodes.Collections;
    using PT.PM.Common.Nodes.Expressions;
    using PT.PM.Common.Nodes.GeneralScope;
    using PT.PM.Common.Nodes.Specific;
    using PT.PM.Common.Nodes.Statements;
    using PT.PM.Common.Nodes.Statements.Switch;
    using PT.PM.Common.Nodes.Statements.TryCatchFinally;
    using PT.PM.Common.Nodes.Tokens;
    using PT.PM.Common.Nodes.Tokens.Literals;
    using PT.PM.Common.Nodes.TypeMembers;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class UstJsonNodesConverter
    {
        public UstJsonConverterReader Reader
        {
            get;
            set;
        }

        public JsonSerializer Serializer
        {
            get;
            set;
        }

        public UstJsonNodesConverter(UstJsonConverterReader reader, JsonSerializer serializer)
        {
            Reader = reader;
            Serializer = serializer;
        }

        private void PopFromAncestors(JObject token, Ust ust = null)
        {
            if (!Reader.IgnoreExtraProcess)
                Reader.ExtraProcess(ust, token, Serializer);
            if (ust is RootUst)
            {
                Reader.rootAncestors.Pop();
            }
            Reader.ancestors.Pop();
        }

        private void PushToAncestors(JObject token, Ust ust = null)
        {
            if (Reader.rootAncestors.Count > 0)
            {
                ust.Root = Reader.rootAncestors.Peek();
            }

            if (Reader.ancestors.Count > 0)
            {
                ust.Parent = Reader.ancestors.Peek();
            }

            if (ust is RootUst rootUst)
            {
                Reader.rootAncestors.Push(rootUst);
            }

            Reader.ancestors.Push(ust);

        }

        private void GetTextSpans(JObject token, Ust ust = null)
        {
            List<TextSpan> textSpans = token[nameof(Ust.TextSpan)]?.ToTextSpans(Serializer).ToList();
            if (textSpans?.Count > 0)
            {
                if (textSpans.Count == 1)
                {
                    ust.TextSpan = textSpans[0];
                }
                else
                {
                    ust.InitialTextSpans = textSpans; ust.TextSpan = textSpans[0];
                }
            }
        }

        public Ust Convert(JObject token, Ust ust = null)
        {
            if (ust != null)
            {
                if (ust is NotImplementedUst NotImplementedUstUst)
                    return ConvertAsNotImplementedUst(token, NotImplementedUstUst);
                if (ust is RootUst RootUstUst)
                    return ConvertAsRootUst(token, RootUstUst);
                if (ust is ConstructorDeclaration ConstructorDeclarationUst)
                    return ConvertAsConstructorDeclaration(token, ConstructorDeclarationUst);
                if (ust is FieldDeclaration FieldDeclarationUst)
                    return ConvertAsFieldDeclaration(token, FieldDeclarationUst);
                if (ust is MethodDeclaration MethodDeclarationUst)
                    return ConvertAsMethodDeclaration(token, MethodDeclarationUst);
                if (ust is ParameterDeclaration ParameterDeclarationUst)
                    return ConvertAsParameterDeclaration(token, ParameterDeclarationUst);
                if (ust is StatementDeclaration StatementDeclarationUst)
                    return ConvertAsStatementDeclaration(token, StatementDeclarationUst);
                if (ust is BaseReferenceToken BaseReferenceTokenUst)
                    return ConvertAsBaseReferenceToken(token, BaseReferenceTokenUst);
                if (ust is IdToken IdTokenUst)
                    return ConvertAsIdToken(token, IdTokenUst);
                if (ust is TypeToken TypeTokenUst)
                    return ConvertAsTypeToken(token, TypeTokenUst);
                if (ust is BinaryOperatorLiteral BinaryOperatorLiteralUst)
                    return ConvertAsBinaryOperatorLiteral(token, BinaryOperatorLiteralUst);
                if (ust is BooleanLiteral BooleanLiteralUst)
                    return ConvertAsBooleanLiteral(token, BooleanLiteralUst);
                if (ust is CommentLiteral CommentLiteralUst)
                    return ConvertAsCommentLiteral(token, CommentLiteralUst);
                if (ust is FloatLiteral FloatLiteralUst)
                    return ConvertAsFloatLiteral(token, FloatLiteralUst);
                if (ust is InOutModifierLiteral InOutModifierLiteralUst)
                    return ConvertAsInOutModifierLiteral(token, InOutModifierLiteralUst);
                if (ust is IntLiteral IntLiteralUst)
                    return ConvertAsIntLiteral(token, IntLiteralUst);
                if (ust is ModifierLiteral ModifierLiteralUst)
                    return ConvertAsModifierLiteral(token, ModifierLiteralUst);
                if (ust is NullLiteral NullLiteralUst)
                    return ConvertAsNullLiteral(token, NullLiteralUst);
                if (ust is StringLiteral StringLiteralUst)
                    return ConvertAsStringLiteral(token, StringLiteralUst);
                if (ust is TypeTypeLiteral TypeTypeLiteralUst)
                    return ConvertAsTypeTypeLiteral(token, TypeTypeLiteralUst);
                if (ust is UnaryOperatorLiteral UnaryOperatorLiteralUst)
                    return ConvertAsUnaryOperatorLiteral(token, UnaryOperatorLiteralUst);
                if (ust is BlockStatement BlockStatementUst)
                    return ConvertAsBlockStatement(token, BlockStatementUst);
                if (ust is BreakStatement BreakStatementUst)
                    return ConvertAsBreakStatement(token, BreakStatementUst);
                if (ust is ContinueStatement ContinueStatementUst)
                    return ConvertAsContinueStatement(token, ContinueStatementUst);
                if (ust is DoWhileStatement DoWhileStatementUst)
                    return ConvertAsDoWhileStatement(token, DoWhileStatementUst);
                if (ust is EmptyStatement EmptyStatementUst)
                    return ConvertAsEmptyStatement(token, EmptyStatementUst);
                if (ust is ExpressionStatement ExpressionStatementUst)
                    return ConvertAsExpressionStatement(token, ExpressionStatementUst);
                if (ust is ForeachStatement ForeachStatementUst)
                    return ConvertAsForeachStatement(token, ForeachStatementUst);
                if (ust is ForStatement ForStatementUst)
                    return ConvertAsForStatement(token, ForStatementUst);
                if (ust is GotoStatement GotoStatementUst)
                    return ConvertAsGotoStatement(token, GotoStatementUst);
                if (ust is IfElseStatement IfElseStatementUst)
                    return ConvertAsIfElseStatement(token, IfElseStatementUst);
                if (ust is ReturnStatement ReturnStatementUst)
                    return ConvertAsReturnStatement(token, ReturnStatementUst);
                if (ust is ThrowStatement ThrowStatementUst)
                    return ConvertAsThrowStatement(token, ThrowStatementUst);
                if (ust is TypeDeclarationStatement TypeDeclarationStatementUst)
                    return ConvertAsTypeDeclarationStatement(token, TypeDeclarationStatementUst);
                if (ust is WhileStatement WhileStatementUst)
                    return ConvertAsWhileStatement(token, WhileStatementUst);
                if (ust is WithStatement WithStatementUst)
                    return ConvertAsWithStatement(token, WithStatementUst);
                if (ust is WrapperStatement WrapperStatementUst)
                    return ConvertAsWrapperStatement(token, WrapperStatementUst);
                if (ust is CatchClause CatchClauseUst)
                    return ConvertAsCatchClause(token, CatchClauseUst);
                if (ust is TryCatchStatement TryCatchStatementUst)
                    return ConvertAsTryCatchStatement(token, TryCatchStatementUst);
                if (ust is SwitchSection SwitchSectionUst)
                    return ConvertAsSwitchSection(token, SwitchSectionUst);
                if (ust is SwitchStatement SwitchStatementUst)
                    return ConvertAsSwitchStatement(token, SwitchStatementUst);
                if (ust is AsExpression AsExpressionUst)
                    return ConvertAsAsExpression(token, AsExpressionUst);
                if (ust is CheckedExpression CheckedExpressionUst)
                    return ConvertAsCheckedExpression(token, CheckedExpressionUst);
                if (ust is CheckedStatement CheckedStatementUst)
                    return ConvertAsCheckedStatement(token, CheckedStatementUst);
                if (ust is CommaExpression CommaExpressionUst)
                    return ConvertAsCommaExpression(token, CommaExpressionUst);
                if (ust is FixedStatement FixedStatementUst)
                    return ConvertAsFixedStatement(token, FixedStatementUst);
                if (ust is LockStatement LockStatementUst)
                    return ConvertAsLockStatement(token, LockStatementUst);
                if (ust is UnsafeStatement UnsafeStatementUst)
                    return ConvertAsUnsafeStatement(token, UnsafeStatementUst);
                if (ust is NamespaceDeclaration NamespaceDeclarationUst)
                    return ConvertAsNamespaceDeclaration(token, NamespaceDeclarationUst);
                if (ust is TypeDeclaration TypeDeclarationUst)
                    return ConvertAsTypeDeclaration(token, TypeDeclarationUst);
                if (ust is UsingDeclaration UsingDeclarationUst)
                    return ConvertAsUsingDeclaration(token, UsingDeclarationUst);
                if (ust is AnonymousMethodExpression AnonymousMethodExpressionUst)
                    return ConvertAsAnonymousMethodExpression(token, AnonymousMethodExpressionUst);
                if (ust is ArgumentExpression ArgumentExpressionUst)
                    return ConvertAsArgumentExpression(token, ArgumentExpressionUst);
                if (ust is ArrayCreationExpression ArrayCreationExpressionUst)
                    return ConvertAsArrayCreationExpression(token, ArrayCreationExpressionUst);
                if (ust is AssignmentExpression AssignmentExpressionUst)
                    return ConvertAsAssignmentExpression(token, AssignmentExpressionUst);
                if (ust is BinaryOperatorExpression BinaryOperatorExpressionUst)
                    return ConvertAsBinaryOperatorExpression(token, BinaryOperatorExpressionUst);
                if (ust is CastExpression CastExpressionUst)
                    return ConvertAsCastExpression(token, CastExpressionUst);
                if (ust is ConditionalExpression ConditionalExpressionUst)
                    return ConvertAsConditionalExpression(token, ConditionalExpressionUst);
                if (ust is IndexerExpression IndexerExpressionUst)
                    return ConvertAsIndexerExpression(token, IndexerExpressionUst);
                if (ust is InvocationExpression InvocationExpressionUst)
                    return ConvertAsInvocationExpression(token, InvocationExpressionUst);
                if (ust is MemberReferenceExpression MemberReferenceExpressionUst)
                    return ConvertAsMemberReferenceExpression(token, MemberReferenceExpressionUst);
                if (ust is MultichildExpression MultichildExpressionUst)
                    return ConvertAsMultichildExpression(token, MultichildExpressionUst);
                if (ust is ObjectCreateExpression ObjectCreateExpressionUst)
                    return ConvertAsObjectCreateExpression(token, ObjectCreateExpressionUst);
                if (ust is UnaryOperatorExpression UnaryOperatorExpressionUst)
                    return ConvertAsUnaryOperatorExpression(token, UnaryOperatorExpressionUst);
                if (ust is VariableDeclarationExpression VariableDeclarationExpressionUst)
                    return ConvertAsVariableDeclarationExpression(token, VariableDeclarationExpressionUst);
                if (ust is WrapperExpression WrapperExpressionUst)
                    return ConvertAsWrapperExpression(token, WrapperExpressionUst);
                if (ust is ThisReferenceToken ThisReferenceTokenUst)
                    return ConvertAsThisReferenceToken(token, ThisReferenceTokenUst);
                if (ust is ArgsUst ArgsUstUst)
                    return ConvertAsArgsUst(token, ArgsUstUst);
                if (ust is Collection CollectionUst)
                    return ConvertAsCollection(token, CollectionUst);
                if (ust is EntitiesUst EntitiesUstUst)
                    return ConvertAsEntitiesUst(token, EntitiesUstUst);
            }
            else
            {
                string kind = token[nameof(Ust.Kind)].ToString().ToLowerInvariant();
                if (kind == "notimplementedust")
                    return ConvertAsNotImplementedUst(token);
                if (kind == "rootust")
                    return ConvertAsRootUst(token);
                if (kind == "constructordeclaration")
                    return ConvertAsConstructorDeclaration(token);
                if (kind == "fielddeclaration")
                    return ConvertAsFieldDeclaration(token);
                if (kind == "methoddeclaration")
                    return ConvertAsMethodDeclaration(token);
                if (kind == "parameterdeclaration")
                    return ConvertAsParameterDeclaration(token);
                if (kind == "statementdeclaration")
                    return ConvertAsStatementDeclaration(token);
                if (kind == "basereferencetoken")
                    return ConvertAsBaseReferenceToken(token);
                if (kind == "idtoken")
                    return ConvertAsIdToken(token);
                if (kind == "typetoken")
                    return ConvertAsTypeToken(token);
                if (kind == "binaryoperatorliteral")
                    return ConvertAsBinaryOperatorLiteral(token);
                if (kind == "booleanliteral")
                    return ConvertAsBooleanLiteral(token);
                if (kind == "commentliteral")
                    return ConvertAsCommentLiteral(token);
                if (kind == "floatliteral")
                    return ConvertAsFloatLiteral(token);
                if (kind == "inoutmodifierliteral")
                    return ConvertAsInOutModifierLiteral(token);
                if (kind == "intliteral")
                    return ConvertAsIntLiteral(token);
                if (kind == "modifierliteral")
                    return ConvertAsModifierLiteral(token);
                if (kind == "nullliteral")
                    return ConvertAsNullLiteral(token);
                if (kind == "stringliteral")
                    return ConvertAsStringLiteral(token);
                if (kind == "typetypeliteral")
                    return ConvertAsTypeTypeLiteral(token);
                if (kind == "unaryoperatorliteral")
                    return ConvertAsUnaryOperatorLiteral(token);
                if (kind == "blockstatement")
                    return ConvertAsBlockStatement(token);
                if (kind == "breakstatement")
                    return ConvertAsBreakStatement(token);
                if (kind == "continuestatement")
                    return ConvertAsContinueStatement(token);
                if (kind == "dowhilestatement")
                    return ConvertAsDoWhileStatement(token);
                if (kind == "emptystatement")
                    return ConvertAsEmptyStatement(token);
                if (kind == "expressionstatement")
                    return ConvertAsExpressionStatement(token);
                if (kind == "foreachstatement")
                    return ConvertAsForeachStatement(token);
                if (kind == "forstatement")
                    return ConvertAsForStatement(token);
                if (kind == "gotostatement")
                    return ConvertAsGotoStatement(token);
                if (kind == "ifelsestatement")
                    return ConvertAsIfElseStatement(token);
                if (kind == "returnstatement")
                    return ConvertAsReturnStatement(token);
                if (kind == "throwstatement")
                    return ConvertAsThrowStatement(token);
                if (kind == "typedeclarationstatement")
                    return ConvertAsTypeDeclarationStatement(token);
                if (kind == "whilestatement")
                    return ConvertAsWhileStatement(token);
                if (kind == "withstatement")
                    return ConvertAsWithStatement(token);
                if (kind == "wrapperstatement")
                    return ConvertAsWrapperStatement(token);
                if (kind == "catchclause")
                    return ConvertAsCatchClause(token);
                if (kind == "trycatchstatement")
                    return ConvertAsTryCatchStatement(token);
                if (kind == "switchsection")
                    return ConvertAsSwitchSection(token);
                if (kind == "switchstatement")
                    return ConvertAsSwitchStatement(token);
                if (kind == "asexpression")
                    return ConvertAsAsExpression(token);
                if (kind == "checkedexpression")
                    return ConvertAsCheckedExpression(token);
                if (kind == "checkedstatement")
                    return ConvertAsCheckedStatement(token);
                if (kind == "commaexpression")
                    return ConvertAsCommaExpression(token);
                if (kind == "fixedstatement")
                    return ConvertAsFixedStatement(token);
                if (kind == "lockstatement")
                    return ConvertAsLockStatement(token);
                if (kind == "unsafestatement")
                    return ConvertAsUnsafeStatement(token);
                if (kind == "namespacedeclaration")
                    return ConvertAsNamespaceDeclaration(token);
                if (kind == "typedeclaration")
                    return ConvertAsTypeDeclaration(token);
                if (kind == "usingdeclaration")
                    return ConvertAsUsingDeclaration(token);
                if (kind == "anonymousmethodexpression")
                    return ConvertAsAnonymousMethodExpression(token);
                if (kind == "argumentexpression")
                    return ConvertAsArgumentExpression(token);
                if (kind == "arraycreationexpression")
                    return ConvertAsArrayCreationExpression(token);
                if (kind == "assignmentexpression")
                    return ConvertAsAssignmentExpression(token);
                if (kind == "binaryoperatorexpression")
                    return ConvertAsBinaryOperatorExpression(token);
                if (kind == "castexpression")
                    return ConvertAsCastExpression(token);
                if (kind == "conditionalexpression")
                    return ConvertAsConditionalExpression(token);
                if (kind == "indexerexpression")
                    return ConvertAsIndexerExpression(token);
                if (kind == "invocationexpression")
                    return ConvertAsInvocationExpression(token);
                if (kind == "memberreferenceexpression")
                    return ConvertAsMemberReferenceExpression(token);
                if (kind == "multichildexpression")
                    return ConvertAsMultichildExpression(token);
                if (kind == "objectcreateexpression")
                    return ConvertAsObjectCreateExpression(token);
                if (kind == "unaryoperatorexpression")
                    return ConvertAsUnaryOperatorExpression(token);
                if (kind == "variabledeclarationexpression")
                    return ConvertAsVariableDeclarationExpression(token);
                if (kind == "wrapperexpression")
                    return ConvertAsWrapperExpression(token);
                if (kind == "thisreferencetoken")
                    return ConvertAsThisReferenceToken(token);
                if (kind == "argsust")
                    return ConvertAsArgsUst(token);
                if (kind == "collection")
                    return ConvertAsCollection(token);
                if (kind == "entitiesust")
                    return ConvertAsEntitiesUst(token);
            }

            return ust;
        }

        public NotImplementedUst ConvertAsNotImplementedUst(JObject token, NotImplementedUst ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new NotImplementedUst();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            PopFromAncestors(token, ust);
            return ust;
        }

        public RootUst ConvertAsRootUst(JObject token, RootUst ust = null)
        {
            JToken propertyToken;
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Nodes", out propertyToken))
            {
                var elList = new List<Ust>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add((Ust)Convert((JObject)elToken));
                ust.Nodes = elList.ToArray();
            }

            if (token.TryGetValue("Comments", out propertyToken))
            {
                var elList = new List<CommentLiteral>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add(ConvertAsCommentLiteral((JObject)elToken));
                ust.Comments = elList.ToArray();
            }

            if (token.TryGetValue("Node", out propertyToken))
            {
                ust.Node = (Ust)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("LineOffset", out propertyToken))
            {
                ust.LineOffset = System.Int32.Parse(propertyToken.ToString());
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public ConstructorDeclaration ConvertAsConstructorDeclaration(JObject token, ConstructorDeclaration ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new ConstructorDeclaration();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("ReturnType", out propertyToken))
            {
                ust.ReturnType = ConvertAsTypeToken((JObject)propertyToken);
            }

            if (token.TryGetValue("Parameters", out propertyToken))
            {
                var elList = new List<ParameterDeclaration>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add(ConvertAsParameterDeclaration((JObject)elToken));
                ust.Parameters = elList;
            }

            if (token.TryGetValue("Body", out propertyToken))
            {
                ust.Body = ConvertAsBlockStatement((JObject)propertyToken);
            }

            if (token.TryGetValue("Modifiers", out propertyToken))
            {
                var elList = new List<ModifierLiteral>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add(ConvertAsModifierLiteral((JObject)elToken));
                ust.Modifiers = elList;
            }

            if (token.TryGetValue("Name", out propertyToken))
            {
                ust.Name = ConvertAsIdToken((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public FieldDeclaration ConvertAsFieldDeclaration(JObject token, FieldDeclaration ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new FieldDeclaration();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Type", out propertyToken))
            {
                ust.Type = ConvertAsTypeToken((JObject)propertyToken);
            }

            if (token.TryGetValue("Variables", out propertyToken))
            {
                var elList = new List<AssignmentExpression>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add(ConvertAsAssignmentExpression((JObject)elToken));
                ust.Variables = elList;
            }

            if (token.TryGetValue("Modifiers", out propertyToken))
            {
                var elList = new List<ModifierLiteral>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add(ConvertAsModifierLiteral((JObject)elToken));
                ust.Modifiers = elList;
            }

            if (token.TryGetValue("Name", out propertyToken))
            {
                ust.Name = ConvertAsIdToken((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public MethodDeclaration ConvertAsMethodDeclaration(JObject token, MethodDeclaration ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new MethodDeclaration();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("ReturnType", out propertyToken))
            {
                ust.ReturnType = ConvertAsTypeToken((JObject)propertyToken);
            }

            if (token.TryGetValue("Parameters", out propertyToken))
            {
                var elList = new List<ParameterDeclaration>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add(ConvertAsParameterDeclaration((JObject)elToken));
                ust.Parameters = elList;
            }

            if (token.TryGetValue("Body", out propertyToken))
            {
                ust.Body = ConvertAsBlockStatement((JObject)propertyToken);
            }

            if (token.TryGetValue("Modifiers", out propertyToken))
            {
                var elList = new List<ModifierLiteral>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add(ConvertAsModifierLiteral((JObject)elToken));
                ust.Modifiers = elList;
            }

            if (token.TryGetValue("Name", out propertyToken))
            {
                ust.Name = ConvertAsIdToken((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public ParameterDeclaration ConvertAsParameterDeclaration(JObject token, ParameterDeclaration ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new ParameterDeclaration();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Modifier", out propertyToken))
            {
                ust.Modifier = ConvertAsInOutModifierLiteral((JObject)propertyToken);
            }

            if (token.TryGetValue("Type", out propertyToken))
            {
                ust.Type = ConvertAsTypeToken((JObject)propertyToken);
            }

            if (token.TryGetValue("Name", out propertyToken))
            {
                ust.Name = ConvertAsIdToken((JObject)propertyToken);
            }

            if (token.TryGetValue("Initializer", out propertyToken))
            {
                ust.Initializer = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public StatementDeclaration ConvertAsStatementDeclaration(JObject token, StatementDeclaration ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new StatementDeclaration();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Statement", out propertyToken))
            {
                ust.Statement = (Statement)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("Modifiers", out propertyToken))
            {
                var elList = new List<ModifierLiteral>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add(ConvertAsModifierLiteral((JObject)elToken));
                ust.Modifiers = elList;
            }

            if (token.TryGetValue("Name", out propertyToken))
            {
                ust.Name = ConvertAsIdToken((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public BaseReferenceToken ConvertAsBaseReferenceToken(JObject token, BaseReferenceToken ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new BaseReferenceToken();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            PopFromAncestors(token, ust);
            return ust;
        }

        public IdToken ConvertAsIdToken(JObject token, IdToken ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new IdToken();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Id", out propertyToken))
            {
                ust.Id = propertyToken.ToString();
            }

            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public TypeToken ConvertAsTypeToken(JObject token, TypeToken ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new TypeToken();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("TypeText", out propertyToken))
            {
                ust.TypeText = propertyToken.ToString();
            }

            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public BinaryOperatorLiteral ConvertAsBinaryOperatorLiteral(JObject token, BinaryOperatorLiteral ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new BinaryOperatorLiteral();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("BinaryOperator", out propertyToken))
            {
                ust.BinaryOperator = (BinaryOperator)Enum.Parse(typeof(BinaryOperator), propertyToken.ToString());
            }

            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public BooleanLiteral ConvertAsBooleanLiteral(JObject token, BooleanLiteral ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new BooleanLiteral();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Value", out propertyToken))
            {
                ust.Value = System.Boolean.Parse(propertyToken.ToString());
            }

            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public CommentLiteral ConvertAsCommentLiteral(JObject token, CommentLiteral ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new CommentLiteral();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Comment", out propertyToken))
            {
                ust.Comment = propertyToken.ToString();
            }

            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public FloatLiteral ConvertAsFloatLiteral(JObject token, FloatLiteral ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new FloatLiteral();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Value", out propertyToken))
            {
                ust.Value = System.Double.Parse(propertyToken.ToString());
            }

            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public InOutModifierLiteral ConvertAsInOutModifierLiteral(JObject token, InOutModifierLiteral ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new InOutModifierLiteral();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("ModifierType", out propertyToken))
            {
                ust.ModifierType = (InOutModifier)Enum.Parse(typeof(InOutModifier), propertyToken.ToString());
            }

            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public IntLiteral ConvertAsIntLiteral(JObject token, IntLiteral ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new IntLiteral();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Value", out propertyToken))
            {
                ust.Value = System.Int64.Parse(propertyToken.ToString());
            }

            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public ModifierLiteral ConvertAsModifierLiteral(JObject token, ModifierLiteral ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new ModifierLiteral();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Modifier", out propertyToken))
            {
                ust.Modifier = (Modifier)Enum.Parse(typeof(Modifier), propertyToken.ToString());
            }

            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public NullLiteral ConvertAsNullLiteral(JObject token, NullLiteral ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new NullLiteral();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public StringLiteral ConvertAsStringLiteral(JObject token, StringLiteral ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new StringLiteral();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Text", out propertyToken))
            {
                ust.Text = propertyToken.ToString();
            }

            if (token.TryGetValue("EscapeCharsLength", out propertyToken))
            {
                ust.EscapeCharsLength = System.Int32.Parse(propertyToken.ToString());
            }

            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public TypeTypeLiteral ConvertAsTypeTypeLiteral(JObject token, TypeTypeLiteral ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new TypeTypeLiteral();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("TypeType", out propertyToken))
            {
                ust.TypeType = (TypeType)Enum.Parse(typeof(TypeType), propertyToken.ToString());
            }

            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public UnaryOperatorLiteral ConvertAsUnaryOperatorLiteral(JObject token, UnaryOperatorLiteral ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new UnaryOperatorLiteral();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("UnaryOperator", out propertyToken))
            {
                ust.UnaryOperator = (UnaryOperator)Enum.Parse(typeof(UnaryOperator), propertyToken.ToString());
            }

            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public BlockStatement ConvertAsBlockStatement(JObject token, BlockStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new BlockStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Statements", out propertyToken))
            {
                var elList = new List<Statement>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add((Statement)Convert((JObject)elToken));
                ust.Statements = elList;
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public BreakStatement ConvertAsBreakStatement(JObject token, BreakStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new BreakStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public ContinueStatement ConvertAsContinueStatement(JObject token, ContinueStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new ContinueStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public DoWhileStatement ConvertAsDoWhileStatement(JObject token, DoWhileStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new DoWhileStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("EmbeddedStatement", out propertyToken))
            {
                ust.EmbeddedStatement = (Statement)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("Condition", out propertyToken))
            {
                ust.Condition = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public EmptyStatement ConvertAsEmptyStatement(JObject token, EmptyStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new EmptyStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            PopFromAncestors(token, ust);
            return ust;
        }

        public ExpressionStatement ConvertAsExpressionStatement(JObject token, ExpressionStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new ExpressionStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public ForeachStatement ConvertAsForeachStatement(JObject token, ForeachStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new ForeachStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Type", out propertyToken))
            {
                ust.Type = ConvertAsTypeToken((JObject)propertyToken);
            }

            if (token.TryGetValue("VarName", out propertyToken))
            {
                ust.VarName = ConvertAsIdToken((JObject)propertyToken);
            }

            if (token.TryGetValue("InExpression", out propertyToken))
            {
                ust.InExpression = (Expression)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("EmbeddedStatement", out propertyToken))
            {
                ust.EmbeddedStatement = (Statement)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public ForStatement ConvertAsForStatement(JObject token, ForStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new ForStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Initializers", out propertyToken))
            {
                var elList = new List<Statement>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add((Statement)Convert((JObject)elToken));
                ust.Initializers = elList;
            }

            if (token.TryGetValue("Condition", out propertyToken))
            {
                ust.Condition = (Expression)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("Iterators", out propertyToken))
            {
                var elList = new List<Expression>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add((Expression)Convert((JObject)elToken));
                ust.Iterators = elList;
            }

            if (token.TryGetValue("Statement", out propertyToken))
            {
                ust.Statement = (Statement)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public GotoStatement ConvertAsGotoStatement(JObject token, GotoStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new GotoStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Id", out propertyToken))
            {
                ust.Id = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public IfElseStatement ConvertAsIfElseStatement(JObject token, IfElseStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new IfElseStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Condition", out propertyToken))
            {
                ust.Condition = (Expression)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("TrueStatement", out propertyToken))
            {
                ust.TrueStatement = (Statement)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("FalseStatement", out propertyToken))
            {
                ust.FalseStatement = (Statement)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public ReturnStatement ConvertAsReturnStatement(JObject token, ReturnStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new ReturnStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Return", out propertyToken))
            {
                ust.Return = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public ThrowStatement ConvertAsThrowStatement(JObject token, ThrowStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new ThrowStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("ThrowExpression", out propertyToken))
            {
                ust.ThrowExpression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public TypeDeclarationStatement ConvertAsTypeDeclarationStatement(JObject token, TypeDeclarationStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new TypeDeclarationStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("TypeDeclaration", out propertyToken))
            {
                ust.TypeDeclaration = ConvertAsTypeDeclaration((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public WhileStatement ConvertAsWhileStatement(JObject token, WhileStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new WhileStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Condition", out propertyToken))
            {
                ust.Condition = (Expression)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("Embedded", out propertyToken))
            {
                ust.Embedded = (Statement)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public WithStatement ConvertAsWithStatement(JObject token, WithStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new WithStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("WithNode", out propertyToken))
            {
                ust.WithNode = (Ust)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("Body", out propertyToken))
            {
                ust.Body = (Statement)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public WrapperStatement ConvertAsWrapperStatement(JObject token, WrapperStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new WrapperStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Node", out propertyToken))
            {
                ust.Node = (Ust)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public CatchClause ConvertAsCatchClause(JObject token, CatchClause ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new CatchClause();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Type", out propertyToken))
            {
                ust.Type = ConvertAsTypeToken((JObject)propertyToken);
            }

            if (token.TryGetValue("VarName", out propertyToken))
            {
                ust.VarName = ConvertAsIdToken((JObject)propertyToken);
            }

            if (token.TryGetValue("Body", out propertyToken))
            {
                ust.Body = ConvertAsBlockStatement((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public TryCatchStatement ConvertAsTryCatchStatement(JObject token, TryCatchStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new TryCatchStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("TryBlock", out propertyToken))
            {
                ust.TryBlock = ConvertAsBlockStatement((JObject)propertyToken);
            }

            if (token.TryGetValue("CatchClauses", out propertyToken))
            {
                var elList = new List<CatchClause>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add(ConvertAsCatchClause((JObject)elToken));
                ust.CatchClauses = elList;
            }

            if (token.TryGetValue("FinallyBlock", out propertyToken))
            {
                ust.FinallyBlock = ConvertAsBlockStatement((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public SwitchSection ConvertAsSwitchSection(JObject token, SwitchSection ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new SwitchSection();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("CaseLabels", out propertyToken))
            {
                var elList = new List<Expression>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add((Expression)Convert((JObject)elToken));
                ust.CaseLabels = elList;
            }

            if (token.TryGetValue("Statements", out propertyToken))
            {
                var elList = new List<Statement>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add((Statement)Convert((JObject)elToken));
                ust.Statements = elList;
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public SwitchStatement ConvertAsSwitchStatement(JObject token, SwitchStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new SwitchStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("Sections", out propertyToken))
            {
                var elList = new List<SwitchSection>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add(ConvertAsSwitchSection((JObject)elToken));
                ust.Sections = elList;
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public AsExpression ConvertAsAsExpression(JObject token, AsExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new AsExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("Type", out propertyToken))
            {
                ust.Type = ConvertAsTypeToken((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public CheckedExpression ConvertAsCheckedExpression(JObject token, CheckedExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new CheckedExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public CheckedStatement ConvertAsCheckedStatement(JObject token, CheckedStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new CheckedStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Body", out propertyToken))
            {
                ust.Body = ConvertAsBlockStatement((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public CommaExpression ConvertAsCommaExpression(JObject token, CommaExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new CommaExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Expressions", out propertyToken))
            {
                var elList = new List<Expression>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add((Expression)Convert((JObject)elToken));
                ust.Expressions = elList;
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public FixedStatement ConvertAsFixedStatement(JObject token, FixedStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new FixedStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Type", out propertyToken))
            {
                ust.Type = ConvertAsTypeToken((JObject)propertyToken);
            }

            if (token.TryGetValue("Variables", out propertyToken))
            {
                var elList = new List<AssignmentExpression>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add(ConvertAsAssignmentExpression((JObject)elToken));
                ust.Variables = elList;
            }

            if (token.TryGetValue("Embedded", out propertyToken))
            {
                ust.Embedded = (Statement)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public LockStatement ConvertAsLockStatement(JObject token, LockStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new LockStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Lock", out propertyToken))
            {
                ust.Lock = (Expression)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("Embedded", out propertyToken))
            {
                ust.Embedded = (Statement)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public UnsafeStatement ConvertAsUnsafeStatement(JObject token, UnsafeStatement ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new UnsafeStatement();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Body", out propertyToken))
            {
                ust.Body = ConvertAsBlockStatement((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public NamespaceDeclaration ConvertAsNamespaceDeclaration(JObject token, NamespaceDeclaration ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new NamespaceDeclaration();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Name", out propertyToken))
            {
                ust.Name = ConvertAsStringLiteral((JObject)propertyToken);
            }

            if (token.TryGetValue("Members", out propertyToken))
            {
                var elList = new List<Ust>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add((Ust)Convert((JObject)elToken));
                ust.Members = elList;
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public TypeDeclaration ConvertAsTypeDeclaration(JObject token, TypeDeclaration ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new TypeDeclaration();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Type", out propertyToken))
            {
                ust.Type = ConvertAsTypeTypeLiteral((JObject)propertyToken);
            }

            if (token.TryGetValue("BaseTypes", out propertyToken))
            {
                var elList = new List<TypeToken>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add(ConvertAsTypeToken((JObject)elToken));
                ust.BaseTypes = elList;
            }

            if (token.TryGetValue("TypeMembers", out propertyToken))
            {
                var elList = new List<EntityDeclaration>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add((EntityDeclaration)Convert((JObject)elToken));
                ust.TypeMembers = elList;
            }

            if (token.TryGetValue("Modifiers", out propertyToken))
            {
                var elList = new List<ModifierLiteral>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add(ConvertAsModifierLiteral((JObject)elToken));
                ust.Modifiers = elList;
            }

            if (token.TryGetValue("Name", out propertyToken))
            {
                ust.Name = ConvertAsIdToken((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public UsingDeclaration ConvertAsUsingDeclaration(JObject token, UsingDeclaration ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new UsingDeclaration();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Name", out propertyToken))
            {
                ust.Name = ConvertAsStringLiteral((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public AnonymousMethodExpression ConvertAsAnonymousMethodExpression(JObject token, AnonymousMethodExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new AnonymousMethodExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Parameters", out propertyToken))
            {
                var elList = new List<ParameterDeclaration>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add(ConvertAsParameterDeclaration((JObject)elToken));
                ust.Parameters = elList;
            }

            if (token.TryGetValue("Body", out propertyToken))
            {
                ust.Body = ConvertAsBlockStatement((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public ArgumentExpression ConvertAsArgumentExpression(JObject token, ArgumentExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new ArgumentExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Modifier", out propertyToken))
            {
                ust.Modifier = ConvertAsInOutModifierLiteral((JObject)propertyToken);
            }

            if (token.TryGetValue("Argument", out propertyToken))
            {
                ust.Argument = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public ArrayCreationExpression ConvertAsArrayCreationExpression(JObject token, ArrayCreationExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new ArrayCreationExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Type", out propertyToken))
            {
                ust.Type = ConvertAsTypeToken((JObject)propertyToken);
            }

            if (token.TryGetValue("Sizes", out propertyToken))
            {
                var elList = new List<Expression>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add((Expression)Convert((JObject)elToken));
                ust.Sizes = elList;
            }

            if (token.TryGetValue("Initializers", out propertyToken))
            {
                var elList = new List<Expression>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add((Expression)Convert((JObject)elToken));
                ust.Initializers = elList;
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public AssignmentExpression ConvertAsAssignmentExpression(JObject token, AssignmentExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new AssignmentExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Left", out propertyToken))
            {
                ust.Left = (Expression)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("Right", out propertyToken))
            {
                ust.Right = (Expression)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("Operator", out propertyToken))
            {
                ust.Operator = ConvertAsBinaryOperatorLiteral((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public BinaryOperatorExpression ConvertAsBinaryOperatorExpression(JObject token, BinaryOperatorExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new BinaryOperatorExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Left", out propertyToken))
            {
                ust.Left = (Expression)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("Operator", out propertyToken))
            {
                ust.Operator = ConvertAsBinaryOperatorLiteral((JObject)propertyToken);
            }

            if (token.TryGetValue("Right", out propertyToken))
            {
                ust.Right = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public CastExpression ConvertAsCastExpression(JObject token, CastExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new CastExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Type", out propertyToken))
            {
                ust.Type = ConvertAsTypeToken((JObject)propertyToken);
            }

            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public ConditionalExpression ConvertAsConditionalExpression(JObject token, ConditionalExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new ConditionalExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Condition", out propertyToken))
            {
                ust.Condition = (Expression)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("TrueExpression", out propertyToken))
            {
                ust.TrueExpression = (Expression)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("FalseExpression", out propertyToken))
            {
                ust.FalseExpression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public IndexerExpression ConvertAsIndexerExpression(JObject token, IndexerExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new IndexerExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Target", out propertyToken))
            {
                ust.Target = (Expression)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("Arguments", out propertyToken))
            {
                ust.Arguments = ConvertAsArgsUst((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public InvocationExpression ConvertAsInvocationExpression(JObject token, InvocationExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new InvocationExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Target", out propertyToken))
            {
                ust.Target = (Expression)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("Arguments", out propertyToken))
            {
                ust.Arguments = ConvertAsArgsUst((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public MemberReferenceExpression ConvertAsMemberReferenceExpression(JObject token, MemberReferenceExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new MemberReferenceExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Target", out propertyToken))
            {
                ust.Target = (Expression)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("Name", out propertyToken))
            {
                ust.Name = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public MultichildExpression ConvertAsMultichildExpression(JObject token, MultichildExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new MultichildExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Expressions", out propertyToken))
            {
                var elList = new List<Expression>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add((Expression)Convert((JObject)elToken));
                ust.Expressions = elList;
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public ObjectCreateExpression ConvertAsObjectCreateExpression(JObject token, ObjectCreateExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new ObjectCreateExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Type", out propertyToken))
            {
                ust.Type = (Token)Convert((JObject)propertyToken);
            }

            if (token.TryGetValue("Arguments", out propertyToken))
            {
                ust.Arguments = ConvertAsArgsUst((JObject)propertyToken);
            }

            if (token.TryGetValue("Initializers", out propertyToken))
            {
                var elList = new List<Expression>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add((Expression)Convert((JObject)elToken));
                ust.Initializers = elList;
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public UnaryOperatorExpression ConvertAsUnaryOperatorExpression(JObject token, UnaryOperatorExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new UnaryOperatorExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Operator", out propertyToken))
            {
                ust.Operator = ConvertAsUnaryOperatorLiteral((JObject)propertyToken);
            }

            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public VariableDeclarationExpression ConvertAsVariableDeclarationExpression(JObject token, VariableDeclarationExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new VariableDeclarationExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Type", out propertyToken))
            {
                ust.Type = ConvertAsTypeToken((JObject)propertyToken);
            }

            if (token.TryGetValue("Variables", out propertyToken))
            {
                var elList = new List<AssignmentExpression>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add(ConvertAsAssignmentExpression((JObject)elToken));
                ust.Variables = elList;
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public WrapperExpression ConvertAsWrapperExpression(JObject token, WrapperExpression ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new WrapperExpression();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Node", out propertyToken))
            {
                ust.Node = (Ust)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public ThisReferenceToken ConvertAsThisReferenceToken(JObject token, ThisReferenceToken ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new ThisReferenceToken();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Expression", out propertyToken))
            {
                ust.Expression = (Expression)Convert((JObject)propertyToken);
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public ArgsUst ConvertAsArgsUst(JObject token, ArgsUst ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new ArgsUst();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Collection", out propertyToken))
            {
                var elList = new List<Expression>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add((Expression)Convert((JObject)elToken));
                ust.Collection = elList;
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public Collection ConvertAsCollection(JObject token, Collection ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new Collection();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Collection", out propertyToken))
            {
                var elList = new List<Ust>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add((Ust)Convert((JObject)elToken));
                ust.Collection = elList;
            }

            PopFromAncestors(token, ust);
            return ust;
        }

        public EntitiesUst ConvertAsEntitiesUst(JObject token, EntitiesUst ust = null)
        {
            JToken propertyToken;
            ust = ust ?? new EntitiesUst();
            PushToAncestors(token, ust);
            GetTextSpans(token, ust);
            if (token.TryGetValue("Collection", out propertyToken))
            {
                var elList = new List<EntityDeclaration>();
                foreach (JToken elToken in propertyToken.ReadArray())
                    elList.Add((EntityDeclaration)Convert((JObject)elToken));
                ust.Collection = elList;
            }

            PopFromAncestors(token, ust);
            return ust;
        }
    }
}