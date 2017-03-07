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

namespace PT.PM.CSharpAstConversion.RoslynAstVisitor
{
    public partial class RoslynAstCommonConverterVisitor : CSharpSyntaxVisitor<UstNode>
    {
        private CSharpRoslynSemanticsInfo semanticsInfo;

        protected SyntaxNode Root;

        protected SemanticModel SemanticModel;

        protected FileNode FileNode { get; set; }

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public CSharpRoslynSemanticsInfo SemanticsInfo
        {
            get { return semanticsInfo; }
            set
            {
                semanticsInfo = value;
                //SemanticModel = semanticsInfo.Compilation.GetSemanticModel(Root, true);
            }
        }

        public RoslynAstCommonConverterVisitor(SyntaxTree syntaxTree, string filePath)
        {
            Root = syntaxTree.GetRoot();
            FileNode = new FileNode(filePath, syntaxTree.GetText().ToString());
        }

        public RoslynAstCommonConverterVisitor(SyntaxTree syntaxTree, FileNode fileNode)
        {
            Root = syntaxTree.GetRoot();
            FileNode = fileNode;
        }

        public RoslynAstCommonConverterVisitor(SyntaxNode root, FileNode fileNode)
        {
            Root = root;
            FileNode = fileNode;
        }

        public FileNode Walk()
        {
            return (FileNode)Visit(Root);
        }

        public UstNode Walk(SyntaxNode node)
        {
            return Visit(node);
        }

        public override UstNode Visit(SyntaxNode node)
        {
            var children = new List<UstNode>();
            foreach (var child in node.ChildNodes())
            {
                var result = VisitAndReturnNullIfError(child);
                if (result != null)
                {
                    children.Add(result);
                }
            }
            FileNode.Root = children.CreateRootNamespace(Language.CSharp, FileNode);

            return FileNode;
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
            var name = new StringLiteral(node.Name.ToFullString(), nameSpan, FileNode);
            return new UsingDeclaration(name, node.GetTextSpan(), FileNode);
        }

        public override UstNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var nameSpan = node.Name.GetTextSpan();
            var name = new StringLiteral(node.Name.ToFullString(), nameSpan, FileNode);
            var members = node.Members.Select(member => VisitAndReturnNullIfError(member))
                .ToArray();

            var result = new NamespaceDeclaration(
                name, members,
                Language.CSharp, node.GetTextSpan(), FileNode
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
            var typeTypeToken = new TypeTypeLiteral(typeType, node.DelegateKeyword.GetTextSpan(), FileNode);
            var result = new TypeDeclaration(typeTypeToken, ConvertId(node.Identifier), new EntityDeclaration[0],
                node.GetTextSpan(), FileNode);
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
            var typeTypeToken = new TypeTypeLiteral(type, node.EnumKeyword.GetTextSpan(), FileNode);
            var members = node.Members.Select(m => (FieldDeclaration)VisitAndReturnNullIfError(m))
                .ToArray();

            var result = ConvertBaseTypeDeclaration(node, typeTypeToken, members);
            return result;
        }

        public override UstNode VisitPredefinedType(PredefinedTypeSyntax node)
        {
            var result = new TypeToken(node.Keyword.ValueText, node.GetTextSpan(), FileNode);
            return result;
        }

        public override UstNode VisitGenericName(GenericNameSyntax node)
        {
            var typeName = string.Join("", node.DescendantTokens().Select(t => t.ValueText));
            var result = new TypeToken(typeName, node.GetTextSpan(), FileNode);
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
            var typeTypeToken = new TypeTypeLiteral(type, node.Keyword.GetTextSpan(), FileNode);

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
                            new IdToken(name, default(TextSpan), FileNode), indexerSyntax.GetTextSpan()));
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
                            new ExpressionStatement(expressionBody, expressionBody.TextSpan, FileNode) },
                            expressionBody.TextSpan, FileNode);
                    }

                    var method = new MethodDeclaration(id, parameters, body, operatorSyntax.GetTextSpan(), FileNode)
                    {
                        Modifiers = operatorSyntax.Modifiers.Select(ConvertModifier)
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
                        id = new IdToken("To" + typeToken.TypeText, typeToken.TextSpan, FileNode);
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
                            new ExpressionStatement(expressionBody, expressionBody.TextSpan, FileNode) },
                            expressionBody.TextSpan, FileNode);
                    }

                    var parameters =
                        conversionOperatorSyntax.ParameterList.Parameters.Select(p => (ParameterDeclaration)VisitAndReturnNullIfError(p))
                        .ToArray();

                    var method = new MethodDeclaration(id, parameters, body, conversionOperatorSyntax.GetTextSpan(), FileNode)
                    {
                        Modifiers = conversionOperatorSyntax.Modifiers.Select(ConvertModifier)
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
                ex.TextSpan, FileNode);
            var method = new MethodDeclaration(
                new IdToken(name.TextValue + "_method", name.TextSpan, FileNode),
                Enumerable.Empty<ParameterDeclaration>(), body, textSpan, FileNode);
            return method;
        }

        public override UstNode VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            AssignmentExpression[] declarations = node.Declaration.Variables
                .Select(var => (AssignmentExpression)VisitAndReturnNullIfError(var))
                .ToArray();

            var result = new FieldDeclaration(declarations, node.GetTextSpan(), FileNode)
            {
                Modifiers = node.Modifiers.Select(ConvertModifier)
            };
            return result;
        }

        protected UstNode ConvertBaseTypeDeclaration(
            BaseTypeDeclarationSyntax node,
            TypeTypeLiteral typeTypeToken,
            IEnumerable<EntityDeclaration> typeMembers)
        {
            var nameLiteral = ConvertId(node.Identifier);
            var modifiers = node.Modifiers.Select(ConvertModifier).ToArray();

            var result = new TypeDeclaration(
                typeTypeToken,
                nameLiteral,
                typeMembers,
                node.GetTextSpan(),
                FileNode)
            {
                Modifiers = modifiers
            };
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
               node.Keyword.GetTextSpan(), FileNode);
            var body = node.Body == null ?
                new BlockStatement(new Statement[0], node.SemicolonToken.GetTextSpan(), FileNode) :
                (BlockStatement)VisitBlock(node.Body);

            var method = new MethodDeclaration(
                methodLiteral,
                parameters ?? new ParameterDeclaration[0],
                body ?? new BlockStatement(null, node.SemicolonToken.GetTextSpan(), FileNode),
                node.GetTextSpan(),
                FileNode)
            {
                Modifiers = node.Modifiers.Select(ConvertModifier)
            };
            return method;
        }

        #endregion
    }
}
