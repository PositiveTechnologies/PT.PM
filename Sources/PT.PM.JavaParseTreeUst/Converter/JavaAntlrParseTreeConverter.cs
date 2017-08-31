using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.TypeMembers;
using PT.PM.JavaParseTreeUst.Parser;
using PT.PM.AntlrUtils;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Misc;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.JavaParseTreeUst.Converter
{
    public partial class JavaAntlrParseTreeConverter : AntlrConverter, IJavaParserVisitor<UstNode>
    {
        public override Language Language => Language.Java;

        public UstNode VisitCompilationUnit(JavaParser.CompilationUnitContext context)
        {
            var packageDeclaration = context.packageDeclaration();
            EntityDeclaration[] typeDecs = context.typeDeclaration()
                .Select(typeDec => Visit(typeDec) as EntityDeclaration)
                .Where(typeDec => typeDec != null)
                .ToArray();
            UsingDeclaration[] importDecs = context.importDeclaration()
                .Select(importDec => (UsingDeclaration)Visit(importDec))
                .Where(importDec => importDec != null)
                .ToArray();

            var roots = new List<UstNode>(importDecs);
            if (packageDeclaration != null)
            {
                var name = (StringLiteral)Visit(packageDeclaration.qualifiedName());
                var ns = new NamespaceDeclaration(name, typeDecs, context.GetTextSpan(), root);
                roots.Add(ns);
            }
            else
            {
                roots.AddRange(typeDecs);
            }
            root.Nodes = roots.ToArray();

            return root;
        }

        public UstNode VisitPackageDeclaration(JavaParser.PackageDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitImportDeclaration(JavaParser.ImportDeclarationContext context)
        {
            StringLiteral name = (StringLiteral)Visit(context.qualifiedName());
            TextSpan textSpan = context.GetTextSpan();
            var result = new UsingDeclaration(name, textSpan, root);

            return result;
        }

        public UstNode VisitTypeDeclaration(JavaParser.TypeDeclarationContext context)
        {
            var child0Terminal = context.GetChild(0) as ITerminalNode;
            if (child0Terminal != null) // ignore ';'
                return null;

            // TODO: implement modifiers.
            var result = Visit(context.GetChild(context.ChildCount - 1));
            return result;
        }

        public UstNode VisitClassOrInterfaceModifier(JavaParser.ClassOrInterfaceModifierContext context)
        {
            JavaParser.AnnotationContext annotation = context.annotation();
            if (annotation != null)
            {
                return VisitChildren(context);
            }

            Modifier modifier;
            ModifierLiteral result = null;
            if (Enum.TryParse<Modifier>(context.GetChild<ITerminalNode>(0).GetText(), true, out modifier))
                result = new ModifierLiteral(modifier, context.GetTextSpan(), root);
            return result;
        }

        public UstNode VisitClassDeclaration(JavaParser.ClassDeclarationContext context)
        {
            var typeTypeToken = new TypeTypeLiteral(TypeType.Class,
                context.GetChild<ITerminalNode>(0).Symbol.GetTextSpan(), root);

            var id = (IdToken)Visit(context.IDENTIFIER());

            EntityDeclaration[] typeMembers = context.classBody().classBodyDeclaration()
                .Select(dec => Visit(dec) as EntityDeclaration)
                .Where(dec => dec != null).ToArray();

            var result = new TypeDeclaration(typeTypeToken, id, typeMembers, context.GetTextSpan(), root);
            return result;
        }

        public UstNode VisitEnumDeclaration(JavaParser.EnumDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitInterfaceDeclaration(JavaParser.InterfaceDeclarationContext context)
        {
            var typeTypeToken = new TypeTypeLiteral(TypeType.Interface,
                context.GetChild<ITerminalNode>(0).Symbol.GetTextSpan(), root);

            var id = (IdToken)Visit(context.IDENTIFIER());

            // TODO: Modifiers
            EntityDeclaration[] typeMembers = context.interfaceBody().interfaceBodyDeclaration()
                .Select(dec => Visit(dec) as EntityDeclaration)
                .Where(dec => dec != null).ToArray();
            
            var result = new TypeDeclaration(typeTypeToken, id, typeMembers, context.GetTextSpan(), root);
            return result;
        }

        public UstNode VisitAnnotationTypeDeclaration(JavaParser.AnnotationTypeDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitClassOrInterfaceType(JavaParser.ClassOrInterfaceTypeContext context)
        {
            var id = (IdToken)Visit(context.IDENTIFIER(0));
            var typeArguments = context.typeArguments();
            var typeNodes = new StringBuilder();
            foreach (var typeArgument in typeArguments)
            {
                for (int i = 0; i < typeArgument.ChildCount; i++)
                {
                    var terminalNode = typeArgument.GetChild(i) as ITerminalNode;
                    if (terminalNode != null)
                    {
                        typeNodes.Append(terminalNode.Symbol.Text);
                        continue;
                    }

                    var typeNode = typeArgument.GetChild(i) as JavaParser.TypeArgumentContext;
                    if (typeNode != null)
                    {
                        typeNodes.Append(((TypeToken)Visit(typeNode))?.TypeText);
                    }
                }
            }

            var result = new TypeToken(id.Id + typeNodes.ToString(), context.GetTextSpan(), root);
            return result;
        }

        public UstNode VisitTypeArguments(JavaParser.TypeArgumentsContext context)
        {
            TypeToken[] typeArgs = context.typeArgument()
                .Select(arg => (TypeToken)Visit(arg))
                .Where(type => type != null).ToArray();

            string resultString =
                ((ITerminalNode)context.GetChild(0)).Symbol.Text +
                string.Join(",", typeArgs.Select(arg => arg.TypeText)) +
                ((ITerminalNode)context.GetChild(context.ChildCount - 1)).Symbol.Text;

            var result = new TypeToken(resultString.ToString(), context.GetTextSpan(), root);
            return result;
        }

        public UstNode VisitTypeArgument(JavaParser.TypeArgumentContext context)
        {
            TypeToken result;
            if (context.typeType() != null)
            {
                result = (TypeToken)Visit(context.typeType());
            }
            else
            {
                result = new TypeToken("object", context.GetTextSpan(), root);
            }
            return result;
        }

        #region Not implemented

        public UstNode VisitClassBody(JavaParser.ClassBodyContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitModifier([NotNull] JavaParser.ModifierContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitVariableModifier([NotNull] JavaParser.VariableModifierContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitTypeParameters([NotNull] JavaParser.TypeParametersContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitTypeParameter([NotNull] JavaParser.TypeParameterContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitTypeBound([NotNull] JavaParser.TypeBoundContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitEnumConstants([NotNull] JavaParser.EnumConstantsContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitEnumConstant([NotNull] JavaParser.EnumConstantContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitEnumBodyDeclarations([NotNull] JavaParser.EnumBodyDeclarationsContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitInterfaceBody([NotNull] JavaParser.InterfaceBodyContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitConstDeclaration([NotNull] JavaParser.ConstDeclarationContext context)
        {
            var assignments = context.constantDeclarator()
                .Select(declarator => (AssignmentExpression)Visit(declarator));
            return new FieldDeclaration(assignments, context.GetTextSpan(), root);
        }

        public UstNode VisitConstantDeclarator([NotNull] JavaParser.ConstantDeclaratorContext context)
        {
            return new AssignmentExpression(
                (Expression)Visit(context.IDENTIFIER()),
                (Expression)Visit(context.variableInitializer()),
                context.GetTextSpan(), root);
        }

        public UstNode VisitGenericInterfaceMethodDeclaration([NotNull] JavaParser.GenericInterfaceMethodDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitArrayInitializer([NotNull] JavaParser.ArrayInitializerContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitLastFormalParameter([NotNull] JavaParser.LastFormalParameterContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitAnnotation([NotNull] JavaParser.AnnotationContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitElementValuePairs([NotNull] JavaParser.ElementValuePairsContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitElementValuePair([NotNull] JavaParser.ElementValuePairContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitElementValue([NotNull] JavaParser.ElementValueContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitElementValueArrayInitializer([NotNull] JavaParser.ElementValueArrayInitializerContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitAnnotationTypeBody([NotNull] JavaParser.AnnotationTypeBodyContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitAnnotationTypeElementDeclaration([NotNull] JavaParser.AnnotationTypeElementDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitAnnotationTypeElementRest([NotNull] JavaParser.AnnotationTypeElementRestContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitAnnotationMethodOrConstantRest([NotNull] JavaParser.AnnotationMethodOrConstantRestContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitAnnotationMethodRest([NotNull] JavaParser.AnnotationMethodRestContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitAnnotationConstantRest([NotNull] JavaParser.AnnotationConstantRestContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitDefaultValue([NotNull] JavaParser.DefaultValueContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitResourceSpecification([NotNull] JavaParser.ResourceSpecificationContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitResources([NotNull] JavaParser.ResourcesContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitEnhancedForControl([NotNull] JavaParser.EnhancedForControlContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitArrayCreatorRest([NotNull] JavaParser.ArrayCreatorRestContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitClassCreatorRest([NotNull] JavaParser.ClassCreatorRestContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitNonWildcardTypeArgumentsOrDiamond([NotNull] JavaParser.NonWildcardTypeArgumentsOrDiamondContext context)
        {
            return VisitChildren(context);
        }

        #endregion
    }
}
