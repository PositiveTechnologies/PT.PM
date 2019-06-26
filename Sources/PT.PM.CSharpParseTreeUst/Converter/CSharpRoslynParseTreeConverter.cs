using System;
using System.Collections.Generic;
using System.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.TypeMembers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Exceptions;
using System.Threading;

namespace PT.PM.CSharpParseTreeUst.RoslynUstVisitor
{
    public partial class CSharpRoslynParseTreeConverter : CSharpSyntaxVisitor<Ust>, IParseTreeToUstConverter
    {
        private RootUst root;
        private ConvertHelper convertHelper;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Language Language => Language.CSharp;

        public HashSet<Language> AnalyzedLanguages { get; set; }

        public RootUst ParentRoot { get; set; }

        public static CSharpRoslynParseTreeConverter Create() => new CSharpRoslynParseTreeConverter();

        public CSharpRoslynParseTreeConverter()
        {
            AnalyzedLanguages = Language.GetSelfAndSublanguages();
        }

        public RootUst Convert(ParseTree langParseTree)
        {
            var roslynParseTree = (CSharpRoslynParseTree)langParseTree;
            SyntaxTree syntaxTree = roslynParseTree.SyntaxTree;

            if (syntaxTree == null)
            {
                return null;
            }

            RootUst result;
            try
            {
                root = new RootUst(langParseTree.SourceFile, Language.CSharp);
                convertHelper = new ConvertHelper(root) {Logger = Logger};
                Ust visited = Visit(roslynParseTree.SyntaxTree.GetRoot());
                if (visited is RootUst rootUst)
                {
                    result = rootUst;
                }
                else
                {
                    result = new RootUst(langParseTree.SourceFile, Language);
                    result.Node = visited;
                }
                result.SourceFile = langParseTree.SourceFile;
                result.Comments = roslynParseTree.Comments.Select(c =>
                    new Comment(c.GetTextSpan(), result)
                    {
                        Root = result
                    })
                    .ToArray();
                result.FillParentAndRootForDescendantsAndSelf();

                return result;
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ConversionException(langParseTree.SourceFile, ex));
                return null;
            }
        }

        public override Ust Visit(SyntaxNode node)
        {
            var children = new List<Ust>();
            foreach (SyntaxNode child in node.ChildNodes())
            {
                var result = VisitAndReturnNullIfError(child);
                if (result != null)
                {
                    children.Add(result);
                }
            }

            if (root == null)
            {
                root = new RootUst(null, Language.CSharp);
            }
            root.Nodes = children.ToArray();
            root.TextSpan = node.GetTextSpan();
            return root;
        }

        public override Ust DefaultVisit(SyntaxNode node)
        {
            return null;
        }

        public override Ust VisitExternAliasDirective(ExternAliasDirectiveSyntax node)
        {
            return null;
        }

        public override Ust VisitUsingDirective(UsingDirectiveSyntax node)
        {
            var nameSpan = node.Name.GetTextSpan();
            var name = new StringLiteral(node.Name.ToFullString(), nameSpan, 0);
            return new UsingDeclaration(name, node.GetTextSpan());
        }

        public override Ust VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var nameSpan = node.Name.GetTextSpan();
            var name = new StringLiteral(node.Name.ToFullString(), nameSpan);
            var members = node.Members.Select(member => VisitAndReturnNullIfError(member))
                .ToArray();

            var result = new NamespaceDeclaration(
                name, members,
                node.GetTextSpan()
            );
            return result;
        }

        public override Ust VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            return ConvertTypeDeclaration(node);
        }

        public override Ust VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            var typeType = ConvertTypeType(TypeType.Delegate);
            var typeTypeToken = new TypeTypeLiteral(typeType, node.DelegateKeyword.GetTextSpan());
            var result = new TypeDeclaration(typeTypeToken, ConvertId(node.Identifier), new EntityDeclaration[0],
                node.GetTextSpan());
            return result;
        }

        public override Ust VisitStructDeclaration(StructDeclarationSyntax node)
        {
            return ConvertTypeDeclaration(node);
        }

        public override Ust VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            return ConvertTypeDeclaration(node);
        }

        public override Ust VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            var type = ConvertTypeType(TypeType.Enum);
            var typeTypeToken = new TypeTypeLiteral(type, node.EnumKeyword.GetTextSpan());
            var members = node.Members.Select(m => (FieldDeclaration)VisitAndReturnNullIfError(m))
                .ToArray();

            var result = ConvertBaseTypeDeclaration(node, typeTypeToken, members);
            return result;
        }

        public override Ust VisitPredefinedType(PredefinedTypeSyntax node)
        {
            var result = new TypeToken(node.Keyword.ValueText, node.GetTextSpan());
            return result;
        }

        public override Ust VisitGenericName(GenericNameSyntax node)
        {
            var typeName = string.Join("", node.DescendantTokens().Select(t => t.ValueText));
            var result = new TypeToken(typeName, node.GetTextSpan());
            return result;
        }

        #region Utils

        /// <summary>
        /// TODO: Split method.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected Ust ConvertTypeDeclaration(TypeDeclarationSyntax node)
        {
            TypeType type;
            Enum.TryParse(node.Keyword.ValueText, true, out type);
            type = ConvertTypeType(type);
            var typeTypeToken = new TypeTypeLiteral(type, node.Keyword.GetTextSpan());

            var typeMembers = new List<EntityDeclaration>();
            foreach (var member in node.Members)
            {
                PropertyDeclarationSyntax propertySyntax;
                OperatorDeclarationSyntax operatorSyntax;
                IndexerDeclarationSyntax indexerSyntax;
                EventDeclarationSyntax eventSyntax;
                ConversionOperatorDeclarationSyntax conversionOperatorSyntax;

                if ((indexerSyntax = member as IndexerDeclarationSyntax) != null)
                {
                    var name = "Indexer";
                    var parameters =
                           indexerSyntax.ParameterList.Parameters.Select(p => (ParameterDeclaration)VisitAndReturnNullIfError(p))
                           .ToArray();

                    if (indexerSyntax.AccessorList == null)
                    {
                        typeMembers.Add(ConvertToExpressionBodyMethod(indexerSyntax.ExpressionBody,
                            new IdToken(name, default(TextSpan)), indexerSyntax.GetTextSpan()));
                    }
                    else
                    {
                        foreach (var accessor in indexerSyntax.AccessorList.Accessors)
                        {
                            var method = ConvertAccessorToMethodDef(name, accessor, parameters);
                            typeMembers.Add(method);
                        }
                    }
                }
                else if ((eventSyntax = member as EventDeclarationSyntax) != null)
                {
                    if (eventSyntax.AccessorList == null)
                    {
                        typeMembers.Add(ConvertToExpressionBodyMethod(indexerSyntax.ExpressionBody,
                            ConvertId(eventSyntax.Identifier), eventSyntax.GetTextSpan()));
                    }
                    else
                    {
                        var name = eventSyntax.Identifier.ValueText;
                        foreach (var accessor in eventSyntax.AccessorList.Accessors)
                        {
                            var method = ConvertAccessorToMethodDef(name, accessor);
                            typeMembers.Add(method);
                        }
                    }
                }
                else if ((operatorSyntax = member as OperatorDeclarationSyntax) != null)
                {
                    var id = ConvertId(operatorSyntax.OperatorToken);
                    var parameters =
                        operatorSyntax.ParameterList.Parameters.Select(p => (ParameterDeclaration)VisitAndReturnNullIfError(p))
                        ;

                    BlockStatement body;
                    if (operatorSyntax.Body != null)
                    {
                        body = (BlockStatement)VisitBlock(operatorSyntax.Body);
                    }
                    else
                    {
                        var expressionBody = (Expression)VisitArrowExpressionClause(operatorSyntax.ExpressionBody);
                        body = new BlockStatement(new Statement[] {
                            new ExpressionStatement(expressionBody, expressionBody.TextSpan) },
                            expressionBody.TextSpan);
                    }

                    var method = new MethodDeclaration(id, parameters, body, operatorSyntax.GetTextSpan())
                    {
                        Modifiers = operatorSyntax.Modifiers.Select(ConvertModifier).ToList()
                    };
                    typeMembers.Add(method);
                }
                else if ((conversionOperatorSyntax = member as ConversionOperatorDeclarationSyntax) != null)
                {
                    IdToken id;
                    if (conversionOperatorSyntax.Type is IdentifierNameSyntax)
                    {
                        id = (IdToken)VisitIdentifierName((IdentifierNameSyntax)conversionOperatorSyntax.Type);
                    }
                    else
                    {
                        var typeToken = (TypeToken)base.Visit(conversionOperatorSyntax.Type);
                        id = new IdToken("To" + typeToken.TypeText, typeToken.TextSpan);
                    }

                    BlockStatement body;
                    if (conversionOperatorSyntax.Body != null)
                    {
                        body = (BlockStatement)VisitBlock(conversionOperatorSyntax.Body);
                    }
                    else
                    {
                        var expressionBody = (Expression)VisitArrowExpressionClause(conversionOperatorSyntax.ExpressionBody);
                        body = new BlockStatement(new Statement[] {
                            new ExpressionStatement(expressionBody, expressionBody.TextSpan) },
                            expressionBody.TextSpan);
                    }

                    var parameters =
                        conversionOperatorSyntax.ParameterList.Parameters.Select(p => (ParameterDeclaration)VisitAndReturnNullIfError(p))
                        .ToArray();

                    var method = new MethodDeclaration(id, parameters, body, conversionOperatorSyntax.GetTextSpan())
                    {
                        Modifiers = conversionOperatorSyntax.Modifiers.Select(ConvertModifier).ToList()
                    };
                    typeMembers.Add(method);
                }
                else if ((propertySyntax = member as PropertyDeclarationSyntax) != null)
                {
                    if (propertySyntax.AccessorList == null)
                    {
                        IdToken id = ConvertId(propertySyntax.Identifier);
                        typeMembers.Add(ConvertToExpressionBodyMethod(propertySyntax.ExpressionBody, id, propertySyntax.GetTextSpan()));
                    }
                    else
                    {
                        var name = propertySyntax.Identifier.ValueText;
                        foreach (var accessor in propertySyntax.AccessorList.Accessors)
                        {
                            var method = ConvertAccessorToMethodDef(name, accessor);
                            typeMembers.Add(method);
                        }
                    }
                }
                else
                {
                    var entity = (EntityDeclaration)base.Visit(member);
                    typeMembers.Add(entity);
                }
            }

            var result = ConvertBaseTypeDeclaration(node, typeTypeToken, typeMembers);
            return result;
        }

        private MethodDeclaration ConvertToExpressionBodyMethod(ArrowExpressionClauseSyntax expressionBody,
            IdToken name, TextSpan textSpan)
        {
            var ex = (Expression)VisitArrowExpressionClause(expressionBody);
            var body = new BlockStatement(new Statement[] { new ExpressionStatement(ex) },
                ex.TextSpan);
            var method = new MethodDeclaration(
                new IdToken(name.TextValue + "_method", name.TextSpan),
                Enumerable.Empty<ParameterDeclaration>(), body, textSpan);
            return method;
        }

        public override Ust VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            List<AssignmentExpression> declarations = node.Declaration.Variables
                .Select(var => (AssignmentExpression)VisitAndReturnNullIfError(var))
                .ToList();

            var result = new FieldDeclaration(declarations, node.GetTextSpan())
            {
                Modifiers = node.Modifiers.Select(ConvertModifier).ToList()
            };
            return result;
        }

        protected Ust ConvertBaseTypeDeclaration(
            BaseTypeDeclarationSyntax node,
            TypeTypeLiteral typeTypeToken,
            IEnumerable<EntityDeclaration> typeMembers)
        {
            var nameLiteral = ConvertId(node.Identifier);
            var modifiers = node.Modifiers.Select(ConvertModifier).ToList();
            var baseTypes = node.BaseList?.Types.Select(t =>
            {
                var name = t.Type is IdentifierNameSyntax id ? id.Identifier.ValueText : t.ToString();
                return new TypeToken(name, t.GetTextSpan());
            }).ToList() ?? new List<TypeToken>();

            var result = new TypeDeclaration(
                typeTypeToken,
                nameLiteral,
                baseTypes,
                typeMembers,
                node.GetTextSpan())
            {
                Modifiers = modifiers,
            };

            return result;
        }

        protected virtual TypeType ConvertTypeType(TypeType typeType)
        {
            switch (typeType)
            {
                default:
                    return TypeType.Class;
                case TypeType.Interface:
                    return TypeType.Interface;
            }
        }

        protected MethodDeclaration ConvertAccessorToMethodDef(string name,
            AccessorDeclarationSyntax node,
            IEnumerable<ParameterDeclaration> parameters = null)
        {
            var methodLiteral = new IdToken(name + "_" + node.Keyword.ValueText,
               node.Keyword.GetTextSpan());
            var body = node.Body == null ?
                new BlockStatement(new Statement[0], node.SemicolonToken.GetTextSpan()) :
                (BlockStatement)VisitBlock(node.Body);

            var method = new MethodDeclaration(
                methodLiteral,
                parameters ?? new ParameterDeclaration[0],
                body ?? new BlockStatement(null, node.SemicolonToken.GetTextSpan()),
                node.GetTextSpan())
            {
                Modifiers = node.Modifiers.Select(ConvertModifier).ToList()
            };
            return method;
        }

        #endregion
    }
}
