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
using Antlr4.Runtime;

namespace PT.PM.JavaParseTreeUst.Converter
{
    public partial class JavaAntlrParseTreeConverter : AntlrConverter, IJavaParserVisitor<Ust>
    {
        public override Language Language => Java.Language;

        public Ust VisitCompilationUnit(JavaParser.CompilationUnitContext context)
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

            var roots = new List<Ust>(importDecs);
            if (packageDeclaration != null)
            {
                var name = (StringLiteral)Visit(packageDeclaration.qualifiedName());
                var ns = new NamespaceDeclaration(name, typeDecs, context.GetTextSpan());
                roots.Add(ns);
            }
            else
            {
                roots.AddRange(typeDecs);
            }
            root.Nodes = roots.ToArray();

            return root;
        }

        public Ust VisitPackageDeclaration(JavaParser.PackageDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitImportDeclaration(JavaParser.ImportDeclarationContext context)
        {
            StringLiteral name = (StringLiteral)Visit(context.qualifiedName());
            TextSpan textSpan = context.GetTextSpan();
            var result = new UsingDeclaration(name, textSpan);

            return result;
        }

        public Ust VisitTypeDeclaration(JavaParser.TypeDeclarationContext context)
        {
            return ProcessTypeDeclaration(context, context.classOrInterfaceModifier());
        }

        public Ust VisitLocalTypeDeclaration([NotNull] JavaParser.LocalTypeDeclarationContext context)
        {
            return ProcessTypeDeclaration(context, context.classOrInterfaceModifier());
        }

        public Ust VisitClassOrInterfaceModifier(JavaParser.ClassOrInterfaceModifierContext context)
        {
            JavaParser.AnnotationContext annotation = context.annotation();
            if (annotation != null)
            {
                return VisitChildren(context);
            }
            return new ModifierLiteral(context.GetChild<ITerminalNode>(0).GetText(), context.GetTextSpan());
        }

        public Ust VisitClassDeclaration(JavaParser.ClassDeclarationContext context)
        {
            var typeTypeToken = new TypeTypeLiteral(TypeType.Class,
                context.GetChild<ITerminalNode>(0).Symbol.GetTextSpan());

            var id = (IdToken)Visit(context.IDENTIFIER());

            EntityDeclaration[] typeMembers = context.classBody().classBodyDeclaration()
                 .Select(dec => Visit(dec) as EntityDeclaration)
                 .Where(dec => dec != null).ToArray();

            var baseTypes = new List<TypeToken>();
            if (context.typeType() != null)
            {
                baseTypes.Add((TypeToken)Visit(context.typeType()));
            }
            if (context.typeList() != null)
            {
                baseTypes.AddRange(context.typeList().typeType().Select(t => (TypeToken)Visit(t)));
            }

            var result = new TypeDeclaration(typeTypeToken, id, baseTypes, typeMembers, context.GetTextSpan());
            return result;
        }

        public Ust VisitEnumDeclaration(JavaParser.EnumDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitInterfaceDeclaration(JavaParser.InterfaceDeclarationContext context)
        {
            var typeTypeToken = new TypeTypeLiteral(TypeType.Interface,
                context.GetChild<ITerminalNode>(0).Symbol.GetTextSpan());

            var id = (IdToken)Visit(context.IDENTIFIER());

            EntityDeclaration[] typeMembers = context.interfaceBody().interfaceBodyDeclaration()
                .Select(dec => Visit(dec) as EntityDeclaration)
                .Where(dec => dec != null).ToArray();

            var baseTypes = new List<TypeToken>();
            if (context.typeList() != null)
            {
                baseTypes = context.typeList().typeType().Select(t => (TypeToken)Visit(t)).ToList();
            }

            var result = new TypeDeclaration(typeTypeToken, id, baseTypes, typeMembers, context.GetTextSpan());
            return result;
        }

        public Ust VisitAnnotationTypeDeclaration(JavaParser.AnnotationTypeDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitClassOrInterfaceType(JavaParser.ClassOrInterfaceTypeContext context)
        {
            var id = (IdToken)Visit(context.IDENTIFIER(0));
            var typeArguments = context.typeArguments();
            var typeNodes = new StringBuilder();
            foreach (var typeArgument in typeArguments)
            {
                for (int i = 0; i < typeArgument.ChildCount; i++)
                {
                    if (typeArgument.GetChild(i) is ITerminalNode terminalNode)
                    {
                        typeNodes.Append(terminalNode.Symbol.Text);
                        continue;
                    }

                    if (typeArgument.GetChild(i) is JavaParser.TypeArgumentContext typeNode)
                    {
                        typeNodes.Append(((TypeToken)Visit(typeNode))?.TypeText);
                    }
                }
            }

            var result = new TypeToken(id.Id + typeNodes.ToString(), context.GetTextSpan());
            return result;
        }

        public Ust VisitTypeArguments(JavaParser.TypeArgumentsContext context)
        {
            TypeToken[] typeArgs = context.typeArgument()
                .Select(arg => (TypeToken)Visit(arg))
                .Where(type => type != null).ToArray();

            string resultString =
                ((ITerminalNode)context.GetChild(0)).Symbol.Text +
                string.Join(",", typeArgs.Select(arg => arg.TypeText)) +
                ((ITerminalNode)context.GetChild(context.ChildCount - 1)).Symbol.Text;

            var result = new TypeToken(resultString.ToString(), context.GetTextSpan());
            return result;
        }

        public Ust VisitTypeArgument(JavaParser.TypeArgumentContext context)
        {
            TypeToken result;
            if (context.typeType() != null)
            {
                result = (TypeToken)Visit(context.typeType());
            }
            else
            {
                result = new TypeToken("object", context.GetTextSpan());
            }
            return result;
        }

        public Ust VisitModifier([NotNull] JavaParser.ModifierContext context)
        {
            if (context.classOrInterfaceModifier() != null)
            {
                return VisitClassOrInterfaceModifier(context.classOrInterfaceModifier());
            }
            return VisitChildren(context);
        }

        public Ust VisitConstDeclaration([NotNull] JavaParser.ConstDeclarationContext context)
        {
            var type = (TypeToken)Visit(context.typeType());
            var assignments = context.constantDeclarator()
                .Select(declarator => (AssignmentExpression)Visit(declarator));
            return new FieldDeclaration(type, assignments, context.GetTextSpan());
        }

        public Ust VisitConstantDeclarator([NotNull] JavaParser.ConstantDeclaratorContext context)
        {
            return new AssignmentExpression(
                (Expression)Visit(context.IDENTIFIER()),
                (Expression)Visit(context.variableInitializer()),
                context.GetTextSpan());
        }

        #region Default implementation

        public Ust VisitClassBody(JavaParser.ClassBodyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitVariableModifier([NotNull] JavaParser.VariableModifierContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTypeParameters([NotNull] JavaParser.TypeParametersContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTypeParameter([NotNull] JavaParser.TypeParameterContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitTypeBound([NotNull] JavaParser.TypeBoundContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitEnumConstants([NotNull] JavaParser.EnumConstantsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitEnumConstant([NotNull] JavaParser.EnumConstantContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitEnumBodyDeclarations([NotNull] JavaParser.EnumBodyDeclarationsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitInterfaceBody([NotNull] JavaParser.InterfaceBodyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitGenericInterfaceMethodDeclaration([NotNull] JavaParser.GenericInterfaceMethodDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitArrayInitializer([NotNull] JavaParser.ArrayInitializerContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitLastFormalParameter([NotNull] JavaParser.LastFormalParameterContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAnnotation([NotNull] JavaParser.AnnotationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitElementValuePairs([NotNull] JavaParser.ElementValuePairsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitElementValuePair([NotNull] JavaParser.ElementValuePairContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitElementValue([NotNull] JavaParser.ElementValueContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitElementValueArrayInitializer([NotNull] JavaParser.ElementValueArrayInitializerContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAnnotationTypeBody([NotNull] JavaParser.AnnotationTypeBodyContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAnnotationTypeElementDeclaration([NotNull] JavaParser.AnnotationTypeElementDeclarationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAnnotationTypeElementRest([NotNull] JavaParser.AnnotationTypeElementRestContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAnnotationMethodOrConstantRest([NotNull] JavaParser.AnnotationMethodOrConstantRestContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAnnotationMethodRest([NotNull] JavaParser.AnnotationMethodRestContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAnnotationConstantRest([NotNull] JavaParser.AnnotationConstantRestContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitDefaultValue([NotNull] JavaParser.DefaultValueContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitResourceSpecification([NotNull] JavaParser.ResourceSpecificationContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitResources([NotNull] JavaParser.ResourcesContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitEnhancedForControl([NotNull] JavaParser.EnhancedForControlContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitArrayCreatorRest([NotNull] JavaParser.ArrayCreatorRestContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitClassCreatorRest([NotNull] JavaParser.ClassCreatorRestContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitNonWildcardTypeArgumentsOrDiamond([NotNull] JavaParser.NonWildcardTypeArgumentsOrDiamondContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitAccessorOrMethodRefCall([NotNull] JavaParser.AccessorOrMethodRefCallContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitSuperOrThis([NotNull] JavaParser.SuperOrThisContext context)
        {
            return VisitChildren(context);
        }

        #endregion

        private Ust ProcessTypeDeclaration(ParserRuleContext context,
            JavaParser.ClassOrInterfaceModifierContext[] modifiers)
        {
            if (context.GetChild(0) is ITerminalNode child0Terminal) // ignore ';'
                return null;

            var result = Visit(context.GetChild(context.ChildCount - 1));

            if (modifiers.Any() && result is TypeDeclaration typeDeclaration)
            {
                typeDeclaration.Modifiers = modifiers
                    .Select(m => VisitClassOrInterfaceModifier(m) as ModifierLiteral)
                    .Where(m => m != null).ToList();
            }

            return result;
        }
    }
}
