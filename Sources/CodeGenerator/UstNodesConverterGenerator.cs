using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CodeGenerator
{
    public class UstNodesConverterGenerator
    {
        public string[] Usings = new string[]
        {
            "Newtonsoft.Json",
            "Newtonsoft.Json.Linq",
            "PT.PM.Common.Nodes",
            "PT.PM.Common.Nodes.Collections",
            "PT.PM.Common.Nodes.Expressions",
            "PT.PM.Common.Nodes.GeneralScope",
            "PT.PM.Common.Nodes.Specific",
            "PT.PM.Common.Nodes.Statements",
            "PT.PM.Common.Nodes.Statements.Switch",
            "PT.PM.Common.Nodes.Statements.TryCatchFinally",
            "PT.PM.Common.Nodes.Tokens",
            "PT.PM.Common.Nodes.Tokens.Literals",
            "PT.PM.Common.Nodes.TypeMembers",
            "System",
            "System.Collections.Generic",
            "System.Linq"
        };

        private readonly string popFromAncestorsCodeBlock = "if (!Reader.IgnoreExtraProcess)" +
            "Reader.ExtraProcess(ust, token, Serializer);\n" +
            "if (ust is RootUst)" +
            "{Reader.rootAncestors.Pop();}\nReader.ancestors.Pop();";
        private readonly string pushToAncestorsCodeBlock = "if (Reader.rootAncestors.Count > 0){ust.Root = Reader.rootAncestors.Peek();}\n" +
            "if (Reader.ancestors.Count > 0){ust.Parent = Reader.ancestors.Peek();}\n" +
            "if (ust is RootUst rootUst){Reader.rootAncestors.Push(rootUst);}\n" +
            "Reader.ancestors.Push(ust);";
        private readonly string textSpanConverterBlock = "List<TextSpan> textSpans = token[nameof(Ust.TextSpan)]?.ToTextSpans(Serializer).ToList();\n" +
            "if (textSpans?.Count > 0){if (textSpans.Count == 1){ust.TextSpan = textSpans[0];}\n" +
            "else{ust.InitialTextSpans = textSpans;\n" +
            "ust.TextSpan = textSpans[0];}}";

        public string TokenParameterName { get; } = "token";
        public string UstParameterName { get; } = "ust";

        private string[] excludedProperties = new string[] { nameof(Ust.Parent), nameof(Ust.Root), nameof(Ust.TextSpan), nameof(RootUst.Language), nameof(RootUst.SourceCodeFile) };

        /// <summary>
        /// Generate code for filling All objects that have base type Ust
        /// </summary>
        /// <returns>Generated code string</returns>
        public string Generate()
        {
            var ustType = typeof(Ust);
            var types = GetAllSubtypesOf(ustType);

            var namespaceDeclaration = NamespaceDeclaration(ParseName("PT.PM.Common.Json")).NormalizeWhitespace();
            namespaceDeclaration = namespaceDeclaration.AddUsings(Usings.Select(u => UsingDirective(ParseName(u))).ToArray());
            var classDeclaration = ClassDeclaration("UstJsonNodesConverter").AddModifiers(Token(SyntaxKind.PublicKeyword));
            classDeclaration.AddModifiers(Token(SyntaxKind.PublicKeyword));
            List<MemberDeclarationSyntax> members = new List<MemberDeclarationSyntax>(types.Count + 1);

            members.AddRange(CreatePublicMembers());
            members.AddRange(CreatePrivateMembers());

            var mainMethod = CreateMainConverterMethod(ustType, types);

            members.Add(mainMethod);

            foreach (Type type in types)
            {
                members.Add(CreateConverterMethod(type));
            }

            classDeclaration = classDeclaration.AddMembers(members.ToArray());
            namespaceDeclaration = namespaceDeclaration.AddMembers(new MemberDeclarationSyntax[] { classDeclaration });

            return namespaceDeclaration.NormalizeWhitespace().ToFullString();
        }

        private IEnumerable<MemberDeclarationSyntax> CreatePublicMembers()
        {
            var members = new List<MemberDeclarationSyntax>
            {
                CreateReadWriteProperty("Reader", "UstJsonConverterReader", SyntaxKind.PublicKeyword),
                CreateReadWriteProperty("Serializer", "JsonSerializer", SyntaxKind.PublicKeyword),
            };

            var ctor = ConstructorDeclaration(
                new SyntaxList<AttributeListSyntax>(),
                SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword)),
                ParseToken("UstJsonNodesConverter"),
                ParameterList(new SeparatedSyntaxList<ParameterSyntax>()),
                null,
                Block())
                .AddParameterListParameters
                (
                    Parameter(ParseToken("reader")).WithType(ParseTypeName("UstJsonConverterReader")),
                    Parameter(ParseToken("serializer")).WithType(ParseTypeName("JsonSerializer"))
                );

            ctor = ctor.WithBody(Block(
                ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, ParseExpression("Reader"), ParseExpression("reader"))),
                ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, ParseExpression("Serializer"), ParseExpression("serializer")))
                ));
            members.Add(ctor);

            return members;
        }

        List<MemberDeclarationSyntax> CreatePrivateMembers()
        {
            var popFromAncestorsMethod = MethodDeclaration(ParseTypeName("void"), "PopFromAncestors")
                .AddModifiers(Token(SyntaxKind.PrivateKeyword))
                .WithBody(Block(ParseStatement(popFromAncestorsCodeBlock)))
                .AddParameterListParameters(CreateFillerMethodParameters(typeof(Ust)));
            var pushToAncestorsMethod = MethodDeclaration(ParseTypeName("void"), "PushToAncestors")
                .AddModifiers(Token(SyntaxKind.PrivateKeyword))
                .WithBody(Block(ParseStatement(pushToAncestorsCodeBlock)))
                .AddParameterListParameters(CreateFillerMethodParameters(typeof(Ust)));
            var textSpanReadMethod = MethodDeclaration(ParseTypeName("void"), "GetTextSpans")
                .AddModifiers(Token(SyntaxKind.PrivateKeyword))
                .WithBody(Block(ParseStatement(textSpanConverterBlock)))
                .AddParameterListParameters(CreateFillerMethodParameters(typeof(Ust)));

            return new List<MemberDeclarationSyntax>() { popFromAncestorsMethod, pushToAncestorsMethod, textSpanReadMethod };
        }

        private PropertyDeclarationSyntax CreateReadWriteProperty(string name, string typeName, SyntaxKind propertyModifier,
            bool isSetterPrivate = false)
        {
            var result = PropertyDeclaration(
                    ParseTypeName(typeName),
                    ParseToken(name))
                .WithModifiers(SyntaxTokenList.Create(Token(propertyModifier)))
                .AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

            if (isSetterPrivate)
            {
                result = result.AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithModifiers(SyntaxTokenList.Create(Token(SyntaxKind.PrivateKeyword)))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
            }
            else
            {
                result = result.AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
            }

            return result;
        }

        private List<Type> GetAllSubtypesOf(Type type)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s =>
                {
                    try
                    {
                        return s.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        return ex.Types;
                    }
                })
                .Where(p => (p?.IsSubclassOf(type) ?? false)
                && !p.IsAbstract).ToList();
        }

        private MethodDeclarationSyntax CreateMainConverterMethod(Type baseType, List<Type> subtypes)
        {
            var mainMethod = MethodDeclaration(ParseTypeName("Ust"), "Convert");

            mainMethod = mainMethod.AddParameterListParameters(CreateFillerMethodParameters(baseType))
                .AddModifiers(Token(SyntaxKind.PublicKeyword));

            var bodyBlocks = new List<StatementSyntax>();

            foreach (Type type in subtypes)
            {
                bodyBlocks.Add(IfStatement(
                    ParseExpression($"{UstParameterName} is {type.Name} {type.Name}Ust"),
                    ParseStatement($"return ConvertAs{type.Name}({TokenParameterName},{type.Name}Ust);")));
            }
            var mainElseblocks = new List<StatementSyntax>();
            mainElseblocks.Add(ParseStatement("string kind = token[nameof(Ust.Kind)].ToString().ToLowerInvariant();"));
            foreach (Type type in subtypes)
            {
                mainElseblocks.Add(IfStatement(
                    ParseExpression($"kind == \"{type.Name.ToLowerInvariant()}\""),
                    ParseStatement($"return ConvertAs{type.Name}({TokenParameterName});")));
            }
            var mainIf = IfStatement(ParseExpression("ust != null"),
                Block(bodyBlocks), ElseClause(Block(mainElseblocks)));
            bodyBlocks.Add(ReturnStatement(ParseExpression(UstParameterName)));
            return mainMethod.WithBody(Block(mainIf, ParseStatement("return ust;")));
        }

        private MethodDeclarationSyntax CreateConverterMethod(Type type)
        {
            var methodDeclaration = MethodDeclaration(ParseTypeName(type.Name), $"ConvertAs{type.Name}");

            ParameterSyntax[] parameters = CreateFillerMethodParameters(type);

            methodDeclaration = methodDeclaration.AddParameterListParameters(parameters)
                .AddModifiers(Token(SyntaxKind.PublicKeyword));

            List<StatementSyntax> bodyStatements = new List<StatementSyntax>();
            bodyStatements.Add(ParseStatement("JToken propertyToken;"));

            if (type != typeof(RootUst))
            {
                bodyStatements.Add(ParseStatement($"ust = ust ?? new {type.Name}();"));
            }

            bodyStatements.Add(ParseStatement("PushToAncestors(token, ust);"));
            bodyStatements.Add(ParseStatement("GetTextSpans(token, ust);"));
            foreach (var property in type.GetProperties()
                .Where(p => p.CanRead
                && p.CanWrite
                && (!p.SetMethod?.IsPrivate ?? true)
                && !excludedProperties.Contains(p.Name)))
            {
                var propertyTypeInfo = property.PropertyType.GetTypeInfo();
                var tryGetExpression = ParseExpression($"{TokenParameterName}.TryGetValue(\"{property.Name}\", out propertyToken)");
                var ifStatement = IfStatement(tryGetExpression, EmptyStatement());
                BlockSyntax block = Block();
                string propertyAccess = $"{UstParameterName}.{property.Name}";

                if (propertyTypeInfo.IsArray || propertyTypeInfo.IsGenericType)
                {
                    Type elementType = property.PropertyType.GetElementType() ?? propertyTypeInfo.GenericTypeArguments[0];
                    if (elementType == typeof(TextSpan))
                    {
                        continue;
                    }
                    StatementSyntax elementList = ParseStatement($"var elList = new List<{elementType.Name}>();");

                    StatementSyntax foreachBody = null;
                    if (elementType.IsAbstract)
                    {
                        foreachBody = ParseStatement($"elList.Add(({elementType.Name})Convert((JObject)elToken));");
                    }
                    else
                    {
                        foreachBody = ParseStatement($"elList.Add(ConvertAs{elementType.Name}((JObject)elToken));");
                    }

                    var foreachStatement = ForEachStatement(ParseTypeName("JToken"), "elToken", ParseExpression("propertyToken.ReadArray()"), foreachBody);
                    var assignment = ParseStatement($"{UstParameterName}.{property.Name} = elList{(propertyTypeInfo.IsArray ? ".ToArray()" : "")};");
                    block = Block(elementList, foreachStatement, assignment);
                }
                else if (property.PropertyType.IsSubclassOf(typeof(Ust)) || property.PropertyType == typeof(Ust))
                {
                    if (property.PropertyType.IsAbstract)
                    {
                        block = Block(ParseStatement($"{propertyAccess} = ({property.PropertyType.Name})Convert((JObject)propertyToken);"));
                    }
                    else
                    {
                        block = Block(ParseStatement($"{propertyAccess} = ConvertAs{property.PropertyType.Name}((JObject)propertyToken);"));
                    }
                }
                else if (propertyTypeInfo.IsPrimitive)
                {
                    block = Block(ParseStatement($"{propertyAccess} = {property.PropertyType}.Parse(propertyToken.ToString());"));
                }
                else if (propertyTypeInfo.Name == nameof(String))
                {
                    block = Block(ParseStatement($"{propertyAccess} = propertyToken.ToString();"));
                }
                else if (propertyTypeInfo.IsEnum)
                {
                    block = Block(ParseStatement($"{propertyAccess} = ({property.PropertyType.Name})Enum.Parse(typeof({property.PropertyType.Name}), propertyToken.ToString());"));
                }
                else
                {
                    block = Block(ParseStatement($"{propertyAccess} = ({property.PropertyType.Name})propertyToken.ToObject(typeof({property.PropertyType.Name}), Serializer);"));
                }

                bodyStatements.Add(ifStatement.WithStatement(block));
            }
            bodyStatements.Add(ParseStatement("PopFromAncestors(token, ust);"));
            bodyStatements.Add(ReturnStatement(ParseExpression(UstParameterName)));

            return methodDeclaration.WithBody(Block(bodyStatements.ToArray()));
        }


        private ParameterSyntax[] CreateFillerMethodParameters(Type type)
        {
            return new ParameterSyntax[]
            {
                Parameter(ParseToken(TokenParameterName)).WithType(ParseTypeName("JObject")),
                Parameter(ParseToken(UstParameterName)).WithType(ParseTypeName(type.Name)).WithDefault(EqualsValueClause(ParseExpression("null")))
            };
        }

    }
}
