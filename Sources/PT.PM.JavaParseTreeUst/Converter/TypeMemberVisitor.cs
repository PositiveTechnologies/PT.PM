using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.TypeMembers;
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
        public Ust VisitClassBodyDeclaration(JavaParser.ClassBodyDeclarationContext context)
        {
            var block = context.block();
            if (block != null)
            {
                var blockStatement = (BlockStatement)Visit(block);
                return new StatementDeclaration(blockStatement, context.GetTextSpan());
            }
            else
            {
                var result = Visit(context.memberDeclaration()) as EntityDeclaration;
                if (result != null)
                {
                    result.Modifiers = context.modifier().Select(Visit).OfType<ModifierLiteral>().ToList();
                    return result;
                }
                else
                {
                    return VisitChildren(context);
                }
            }
        }

        public Ust VisitInterfaceBodyDeclaration(JavaParser.InterfaceBodyDeclarationContext context)
        {
            var result = Visit(context.interfaceMemberDeclaration());
            return result;
        }

        public Ust VisitInterfaceMemberDeclaration(JavaParser.InterfaceMemberDeclarationContext context)
        {
            return Visit(context.GetChild(0));
        }

        public Ust VisitMemberDeclaration(JavaParser.MemberDeclarationContext context)
        {
            return Visit(context.GetChild(0));
        }

        public Ust VisitInterfaceMethodDeclaration(JavaParser.InterfaceMethodDeclarationContext context)
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

        public Ust VisitInterfaceMethodModifier([NotNull] JavaParser.InterfaceMethodModifierContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitMethodDeclaration(JavaParser.MethodDeclarationContext context)
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

        public Ust VisitMethodBody([NotNull] JavaParser.MethodBodyContext context)
        {
            return VisitShouldNotBeVisited(context);
        }

        public Ust VisitGenericMethodDeclaration(JavaParser.GenericMethodDeclarationContext context)
        {
            return Visit(context.methodDeclaration());
        }

        public Ust VisitFieldDeclaration(JavaParser.FieldDeclarationContext context)
        {
            var type = (TypeToken)VisitTypeType(context.typeType());
            AssignmentExpression[] varInits = context.variableDeclarators().variableDeclarator()
                .Select(varDec => (AssignmentExpression)Visit(varDec))
                .Where(varDec => varDec != null).ToArray();

            var result = new FieldDeclaration(type, varInits, context.GetTextSpan());
            return result;
        }

        public Ust VisitConstructorDeclaration(JavaParser.ConstructorDeclarationContext context)
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

        public Ust VisitGenericConstructorDeclaration(JavaParser.GenericConstructorDeclarationContext context)
        {
            return Visit(context.constructorDeclaration());
        }

        public Ust VisitVariableDeclarators(JavaParser.VariableDeclaratorsContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitVariableDeclarator(JavaParser.VariableDeclaratorContext context)
        {
            var id = (IdToken)Visit(context.variableDeclaratorId());
            JavaParser.VariableInitializerContext variableInitializer = context.variableInitializer();
            Expression initializer = variableInitializer != null ?
                (Expression)Visit(variableInitializer) : null;

            var result = new AssignmentExpression(id, initializer, context.GetTextSpan());
            return result;
        }

        public Ust VisitVariableDeclaratorId(JavaParser.VariableDeclaratorIdContext context)
        {
            var result = (IdToken)Visit(context.IDENTIFIER());
            return result;
        }

        public Ust VisitVariableInitializer(JavaParser.VariableInitializerContext context)
        {
            var result = (Expression)Visit(context.GetChild(0));
            return result;
        }

        public Ust VisitFormalParameters(JavaParser.FormalParametersContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFormalParameterList(JavaParser.FormalParameterListContext context)
        {
            return VisitChildren(context);
        }

        public Ust VisitFormalParameter(JavaParser.FormalParameterContext context)
        {
            var type = (TypeToken)Visit(context.typeType());
            var id = (IdToken)Visit(context.variableDeclaratorId());

            var result = new ParameterDeclaration(null, type, id, context.GetTextSpan());
            return result;
        }
    }
}
