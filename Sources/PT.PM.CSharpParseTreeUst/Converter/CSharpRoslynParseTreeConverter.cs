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

namespace PT.PM.CSharpParseTreeUst.RoslynUstVisitor
{
    public partial class CSharpRoslynParseTreeConverter : CSharpSyntaxVisitor<UstNode>, IParseTreeToUstConverter
    {
        private RootNode root { get; set; }

        public Language Language => Language.CSharp;

        public HashSet<Language> AnalyzedLanguages { get; set; }

        public RootNode ParentRoot { get; set; }

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public CSharpRoslynParseTreeConverter()
        {
            AnalyzedLanguages = Language.GetSelfAndSublanguages();
        }

        public RootNode Convert(ParseTree langParseTree)
        {
            var roslynParseTree = (CSharpRoslynParseTree)langParseTree;
            SyntaxTree syntaxTree = roslynParseTree.SyntaxTree;
            RootNode result;
            if (syntaxTree != null)
            {
                string filePath = syntaxTree.FilePath;
                try
                {
                    UstNode visited = Visit(roslynParseTree.SyntaxTree.GetRoot());
                    if (visited is RootNode rootUstNode)
                    {
                        result = rootUstNode;
                    }
                    else
                    {
                        result = new RootNode(langParseTree.SourceCodeFile, Language);
                        result.Node = visited;
                    }
                    result.SourceCodeFile = langParseTree.SourceCodeFile;
                    result.Comments = roslynParseTree.Comments.Select(c =>
                        new CommentLiteral(c.ToString(), c.GetTextSpan(), result)).ToArray();
                    result.FillAscendants();
                }
                catch (Exception ex)
                {
                    Logger.LogError(new ConversionException(filePath, ex));
                    result = new RootNode(langParseTree.SourceCodeFile, Language);
                }
            }
            else
            {
                result = new RootNode(langParseTree.SourceCodeFile, Language);
            }

            return result;
        }

        public override UstNode Visit(SyntaxNode node)
        {
            var children = new List<UstNode>();
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
                root = new RootNode(children.FirstOrDefault()?.Root?.SourceCodeFile, Language.CSharp);
            }
            root.Nodes = children.ToArray();
            return root;
        }

        public override UstNode DefaultVisit(SyntaxNode node)
        {
            return null;
        }

        public override UstNode VisitExternAliasDirective(ExternAliasDirectiveSyntax node)
        {
            return null;
        }

        public override UstNode VisitUsingDirective(UsingDirectiveSyntax node)
        {
            var nameSpan = node.Name.GetTextSpan();
            var name = new StringLiteral(node.Name.ToFullString(), nameSpan, root);
            return new UsingDeclaration(name, node.GetTextSpan(), root);
        }

        public override UstNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var nameSpan = node.Name.GetTextSpan();
            var name = new StringLiteral(node.Name.ToFullString(), nameSpan, root);
            var members = node.Members.Select(member => VisitAndReturnNullIfError(member))
                .ToArray();

            var result = new NamespaceDeclaration(
                name, members,
                node.GetTextSpan(), root
            );
            return result;
        }

        public override UstNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            return ConvertTypeDeclaration(node);
        }

        public override UstNode VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            var typeType = ConvertTypeType(TypeType.Delegate);
            var typeTypeToken = new TypeTypeLiteral(typeType, node.DelegateKeyword.GetTextSpan(), root);
            var result = new TypeDeclaration(typeTypeToken, ConvertId(node.Identifier), new EntityDeclaration[0],
                node.GetTextSpan(), root);
            return result;
        }

        public override UstNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            return ConvertTypeDeclaration(node);
        }

        public override UstNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            return ConvertTypeDeclaration(node);
        }

        public override UstNode VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            var type = ConvertTypeType(TypeType.Enum);
            var typeTypeToken = new TypeTypeLiteral(type, node.EnumKeyword.GetTextSpan(), root);
            var members = node.Members.Select(m => (FieldDeclaration)VisitAndReturnNullIfError(m))
                .ToArray();

            var result = ConvertBaseTypeDeclaration(node, typeTypeToken, members);
            return result;
        }

        public override UstNode VisitPredefinedType(PredefinedTypeSyntax node)
        {
            var result = new TypeToken(node.Keyword.ValueText, node.GetTextSpan(), root);
            return result;
        }

        public override UstNode VisitGenericName(GenericNameSyntax node)
        {
            var typeName = string.Join("", node.DescendantTokens().Select(t => t.ValueText));
            var result = new TypeToken(typeName, node.GetTextSpan(), root);
            return result;
        }

        #region Utils

        /// <summary>
        /// TODO: Split method.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected UstNode ConvertTypeDeclaration(TypeDeclarationSyntax node)
        {
            TypeType type;
            Enum.TryParse(node.Keyword.ValueText, true, out type);
            type = ConvertTypeType(type);
            var typeTypeToken = new TypeTypeLiteral(type, node.Keyword.GetTextSpan(), root);

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
                            new IdToken(name, default(TextSpan), root), indexerSyntax.GetTextSpan()));
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
                            new ExpressionStatement(expressionBody, expressionBody.TextSpan, root) },
                            expressionBody.TextSpan, root);
                    }

                    var method = new MethodDeclaration(id, parameters, body, operatorSyntax.GetTextSpan(), root)
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
                        id = new IdToken("To" + typeToken.TypeText, typeToken.TextSpan, root);
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
                            new ExpressionStatement(expressionBody, expressionBody.TextSpan, root) },
                            expressionBody.TextSpan, root);
                    }

                    var parameters =
                        conversionOperatorSyntax.ParameterList.Parameters.Select(p => (ParameterDeclaration)VisitAndReturnNullIfError(p))
                        .ToArray();

                    var method = new MethodDeclaration(id, parameters, body, conversionOperatorSyntax.GetTextSpan(), root)
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
                ex.TextSpan, root);
            var method = new MethodDeclaration(
                new IdToken(name.TextValue + "_method", name.TextSpan, root),
                Enumerable.Empty<ParameterDeclaration>(), body, textSpan, root);
            return method;
        }

        public override UstNode VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            List<AssignmentExpression> declarations = node.Declaration.Variables
                .Select(var => (AssignmentExpression)VisitAndReturnNullIfError(var))
                .ToList();

            var result = new FieldDeclaration(declarations, node.GetTextSpan(), root)
            {
                Modifiers = node.Modifiers.Select(ConvertModifier).ToList()
            };
            return result;
        }

        protected UstNode ConvertBaseTypeDeclaration(
            BaseTypeDeclarationSyntax node,
            TypeTypeLiteral typeTypeToken,
            IEnumerable<EntityDeclaration> typeMembers)
        {
            var nameLiteral = ConvertId(node.Identifier);
            var modifiers = node.Modifiers.Select(ConvertModifier).ToList();

            var result = new TypeDeclaration(
                typeTypeToken,
                nameLiteral,
                typeMembers,
                node.GetTextSpan(),
                root)
            {
                Modifiers = modifiers,
            };

            if(node.BaseList?.Types != null)
            {
                var bases = node
                    .BaseList
                    .Types
                    .Select(x => x.Type)
                    .OfType<IdentifierNameSyntax>()
                    .Select(x => ConvertId(x.Identifier))
                    .ToList();
                result.BaseTypes = bases;
            }

            return result;
        }

        protected virtual TypeType ConvertTypeType(TypeType typeType)
        {
            switch (typeType)
            {
                default:
                case TypeType.Class:
                case TypeType.Enum:
                case TypeType.Struct:
                    return TypeType.Class;
                case TypeType.Interface:
                    return  TypeType.Interface;
            }
        }

        protected MethodDeclaration ConvertAccessorToMethodDef(string name,
            AccessorDeclarationSyntax node,
            IEnumerable<ParameterDeclaration> parameters = null)
        {
            var methodLiteral = new IdToken(name + "_" + node.Keyword.ValueText,
               node.Keyword.GetTextSpan(), root);
            var body = node.Body == null ?
                new BlockStatement(new Statement[0], node.SemicolonToken.GetTextSpan(), root) :
                (BlockStatement)VisitBlock(node.Body);

            var method = new MethodDeclaration(
                methodLiteral,
                parameters ?? new ParameterDeclaration[0],
                body ?? new BlockStatement(null, node.SemicolonToken.GetTextSpan(), root),
                node.GetTextSpan(),
                root)
            {
                Modifiers = node.Modifiers.Select(ConvertModifier).ToList()
            };
            return method;
        }

        #endregion
    }
}
