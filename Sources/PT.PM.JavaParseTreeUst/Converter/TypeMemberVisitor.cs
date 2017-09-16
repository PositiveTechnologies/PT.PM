using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.TypeMembers;
using PT.PM.JavaParseTreeUst.Parser;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;
using System.Linq;
using PT.PM.AntlrUtils;
using Antlr4.Runtime.Misc;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.JavaParseTreeUst.Converter
{
    public partial class JavaAntlrParseTreeConverter
    {
        public UstNode VisitClassBodyDeclaration(JavaParser.ClassBodyDeclarationContext context)
        {
            var block = context.block();
            if (block != null)
            {
                var blockStatement = (BlockStatement)Visit(block);
                return new StatementDeclaration(blockStatement, context.GetTextSpan());
            }
            else
            {
                var result = (EntityDeclaration)Visit(context.memberDeclaration());
                result.Modifiers = context.modifier().Select(Visit).OfType<ModifierLiteral>().ToList();
                return result;
            }
        }

        public UstNode VisitInterfaceBodyDeclaration(JavaParser.InterfaceBodyDeclarationContext context)
        {
            var result = Visit(context.interfaceMemberDeclaration());
            return result;
        }

        public UstNode VisitInterfaceMemberDeclaration(JavaParser.InterfaceMemberDeclarationContext context)
        {
            return Visit(context.GetChild(0));
        }

        public UstNode VisitMemberDeclaration(JavaParser.MemberDeclarationContext context)
        {
            return Visit(context.GetChild(0));
        }

        public UstNode VisitInterfaceMethodDeclaration(JavaParser.InterfaceMethodDeclarationContext context)
        {
            JavaParser.TypeTypeOrVoidContext type = context.typeTypeOrVoid();
            ITerminalNode child0Terminal = context.GetChild<ITerminalNode>(0);
            ITerminalNode identifier = context.IDENTIFIER();
            JavaParser.FormalParametersContext formalParameters = context.formalParameters();
            JavaParser.BlockContext methodBody = context.methodBody().block();

            MethodDeclaration result = ConvertMethodDeclaration(type, child0Terminal, identifier, formalParameters, null,
                context.GetTextSpan());
            return result;
        }

        public UstNode VisitInterfaceMethodModifier([NotNull] JavaParser.InterfaceMethodModifierContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitMethodDeclaration(JavaParser.MethodDeclarationContext context)
        {
            JavaParser.TypeTypeOrVoidContext type = context.typeTypeOrVoid();
            ITerminalNode child0Terminal = context.GetChild<ITerminalNode>(0);
            ITerminalNode identifier = context.IDENTIFIER();
            JavaParser.FormalParametersContext formalParameters = context.formalParameters();
            JavaParser.BlockContext methodBody = context.methodBody().block();

            MethodDeclaration result = ConvertMethodDeclaration(type, child0Terminal, identifier, formalParameters, methodBody,
                context.GetTextSpan());
            return result;
        }

        public UstNode VisitMethodBody([NotNull] JavaParser.MethodBodyContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitGenericMethodDeclaration(JavaParser.GenericMethodDeclarationContext context)
        {
            return Visit(context.methodDeclaration());
        }

        public UstNode VisitFieldDeclaration(JavaParser.FieldDeclarationContext context)
        {
            var type = (TypeToken)VisitTypeType(context.typeType());
            AssignmentExpression[] varInits = context.variableDeclarators().variableDeclarator()
                .Select(varDec => (AssignmentExpression)Visit(varDec))
                .Where(varDec => varDec != null).ToArray();

            var result = new FieldDeclaration(type, varInits, context.GetTextSpan());
            return result;
        }

        public UstNode VisitConstructorDeclaration(JavaParser.ConstructorDeclarationContext context)
        {
            var id = (IdToken)Visit(context.IDENTIFIER());
            IEnumerable<ParameterDeclaration> parameters;
            JavaParser.FormalParameterListContext formalParameterList = context.formalParameters().formalParameterList();
            if (formalParameterList == null)
                parameters = Enumerable.Empty<ParameterDeclaration>();
            else
                parameters = formalParameterList.formalParameter()
                    .Select(param => (ParameterDeclaration)Visit(param))
                    .Where(p => p != null).ToArray();

            var body = (BlockStatement)Visit(context.constructorBody);

            var constructorDelaration = new ConstructorDeclaration(id, parameters, body, context.GetTextSpan());
            return constructorDelaration;
        }

        public UstNode VisitGenericConstructorDeclaration(JavaParser.GenericConstructorDeclarationContext context)
        {
            return Visit(context.constructorDeclaration());
        }

        public UstNode VisitVariableDeclarators(JavaParser.VariableDeclaratorsContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public UstNode VisitVariableDeclarator(JavaParser.VariableDeclaratorContext context)
        {
            var id = (IdToken)Visit(context.variableDeclaratorId());
            JavaParser.VariableInitializerContext variableInitializer = context.variableInitializer();
            Expression initializer = variableInitializer != null ?
                (Expression)Visit(variableInitializer) : null;

            var result = new AssignmentExpression(id, initializer, context.GetTextSpan());
            return result;
        }

        public UstNode VisitVariableDeclaratorId(JavaParser.VariableDeclaratorIdContext context)
        {
            var result = (IdToken)Visit(context.IDENTIFIER());
            return result;
        }

        public UstNode VisitVariableInitializer(JavaParser.VariableInitializerContext context)
        {
            var result = (Expression)Visit(context.GetChild(0));
            return result;
        }

        public UstNode VisitFormalParameters(JavaParser.FormalParametersContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitFormalParameterList(JavaParser.FormalParameterListContext context)
        {
            return VisitChildren(context);
        }

        public UstNode VisitFormalParameter(JavaParser.FormalParameterContext context)
        {
            var type = (TypeToken)Visit(context.typeType());
            var id = (IdToken)Visit(context.variableDeclaratorId());

            var result = new ParameterDeclaration(type, id, context.GetTextSpan());
            return result;
        }
    }
}
