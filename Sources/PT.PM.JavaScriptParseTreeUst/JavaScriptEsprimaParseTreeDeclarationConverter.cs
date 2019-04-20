using Esprima.Ast;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.TypeMembers;
using System;
using UstLiterals = PT.PM.Common.Nodes.Tokens.Literals;
using UstTokens = PT.PM.Common.Nodes.Tokens;
using UstExprs = PT.PM.Common.Nodes.Expressions;
using Collections = System.Collections.Generic;

namespace PT.PM.JavaScriptParseTreeUst
{
    public partial class JavaScriptEsprimaParseTreeConverter
    {
        private Ust VisitDeclaration(IDeclaration declaration)
        {
            try
            {
                switch (declaration)
                {
                    case ClassDeclaration classDeclaration:
                        return VisitClassDeclaration(classDeclaration);
                    case ExportNamedDeclaration exportNamedDeclaration:
                        return VisitExportNamedDeclaration(exportNamedDeclaration);
                    case ExportDefaultDeclaration exportDefaultDeclaration:
                        return VisitExportDefaultDeclaration(exportDefaultDeclaration);
                    case ExportAllDeclaration exportAllDeclaration:
                        return VisitExportAllDeclaration(exportAllDeclaration);
                    case ImportDeclaration importDeclaration:
                        return VisitImportDeclaration(importDeclaration);
                    case Expression expression:
                        return VisitExpression(expression);
                    case Statement statement:
                        return VisitStatement(statement);
                    default:
                        return VisitUnknowDeclaration(declaration);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(new ConversionException(SourceFile, ex));
                return null;
            }
        }

        private TypeDeclaration VisitClassDeclaration(ClassDeclaration classDeclaration)
        {
            var name = classDeclaration.Id != null
                ? VisitIdentifier(classDeclaration.Id)
                : null;

            var baseTypes = new Collections.List<TypeToken>();
            if (classDeclaration.SuperClass != null)
            {
                if (VisitExpression(classDeclaration.SuperClass) is TypeToken superClassTypeToken)
                {
                    baseTypes.Add(superClassTypeToken);
                }
            }

            var properties = VisitClassBody(classDeclaration.Body);

            return new TypeDeclaration(null, name, properties, GetTextSpan(classDeclaration))
            {
                BaseTypes = baseTypes
            };
        }

        private Ust VisitPropertyValue(PropertyValue value)
        {
            return Visit(value);
        }

        private TypeDeclaration VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
        {
            var decl = exportNamedDeclaration.Declaration != null
                ? VisitStatementListItem(exportNamedDeclaration.Declaration)
                : null;

            foreach (ExportSpecifier exportSpecifier in exportNamedDeclaration.Specifiers)
            {
                VisitExportSpecifier(exportSpecifier);
            }

            Token source = exportNamedDeclaration.Source != null
                ? VisitLiteral(exportNamedDeclaration.Source)
               : null;

            Logger.LogDebug($"{nameof(ExportNamedDeclaration)} processing is not implemented"); // TODO

            return decl is TypeDeclaration typeDeclaration ? typeDeclaration : null;
        }

        private Ust VisitExportSpecifier(ExportSpecifier exportSpecifier)
        {
            var exported = VisitIdentifier(exportSpecifier.Exported);
            var local = VisitIdentifier(exportSpecifier.Local);
            Logger.LogDebug($"{nameof(ExportSpecifier)} processing is not implemented"); // TODO
            return null;
        }

        private Ust VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
        {
            // TODO: fix export default declaration
            return VisitDeclaration(exportDefaultDeclaration.Declaration);
        }

        private Ust VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
        {
            // TODO: fix export all declaration
            return VisitDeclaration(exportAllDeclaration.Source);
        }


        private Ust VisitImportDeclaration(ImportDeclaration importDeclaration)
        {
            var name = (UstLiterals.StringLiteral)VisitLiteral(importDeclaration.Source);
            Logger.LogDebug($"{nameof(ImportDeclaration)}.{nameof(ImportDeclaration.Specifiers)} are not supported for now");
            return new UsingDeclaration(name, GetTextSpan(importDeclaration));
        }

        private Collections.List<PropertyDeclaration> VisitClassBody(ClassBody classBody)
        {
            var properties = new Collections.List<PropertyDeclaration>(classBody.Body.Count);

            foreach (ClassProperty classProperty in classBody.Body)
            {
                properties.Add(VisitClassProperty(classProperty));
            }

            return properties;
        }

        private Collections.List<PropertyDeclaration> VisitProperties(NodeList<Property> properties)
        {
            var props = new Collections.List<PropertyDeclaration>(properties.Count);

            foreach (Property prop in properties)
            {
                props.Add(VisitClassProperty(prop));
            }

            return props;
        }

        private PropertyDeclaration VisitClassProperty(ClassProperty property)
        {
            Ust body = VisitPropertyValue(property.Value);
            IdToken name = null;

            if (property.Key is Identifier identifier)
            {
                name = VisitIdentifier(identifier);
            }
            else
            {
                if (property.Key is Literal literal)
                {
                    name = new IdToken(literal.StringValue ?? literal.Raw, GetTextSpan(literal));
                }
                else
                {
                    Logger.LogDebug($"Not implemented type of property key {property.Key.GetType()}"); // TODO
                }
            }

            return new PropertyDeclaration(null, name, body, GetTextSpan(property));
        }

        private PropertyDeclaration VisitMethodDefinition(MethodDefinition value)
        {
            var result = VisitClassProperty(value);

            if (value.Static)
            {
                result.Modifiers.Add(new UstLiterals.ModifierLiteral(Modifier.Static));
            }

            return result;
        }

        private Ust VisitUnknowDeclaration(IDeclaration declaration)
        {
            string message = declaration == null
                ? $"{nameof(declaration)} can not be null"
                : $"Unknow {nameof(IDeclaration)} type {declaration.GetType().Name}";
            Logger.LogError(new ConversionException(SourceFile, message: message));
            return null;
        }
    }
}
