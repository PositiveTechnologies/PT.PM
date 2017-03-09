using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.TypeMembers;
using PT.PM.JavaAstConversion.Parser;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.JavaAstConversion.Converter
{
    public partial class JavaAntlrAstConverterVisitor
    {
        private MethodDeclaration ConvertMethodDeclaration(JavaParser.TypeTypeContext type, ITerminalNode child0Terminal,
            ITerminalNode identifier, JavaParser.FormalParametersContext formalParameters,
            JavaParser.MethodBodyContext methodBody, TextSpan textSpan)
        {
            TypeToken typeToken;
            if (type != null)
            {
                typeToken = (TypeToken)Visit(type);
            }
            else
            {
                var voidTerminal = child0Terminal;
                typeToken = new TypeToken(voidTerminal.GetText(), voidTerminal.GetTextSpan(), FileNode);
            }

            var id = (IdToken)Visit(identifier);

            // TODO: Fix with ParamsNode
            IEnumerable<ParameterDeclaration> parameters;
            JavaParser.FormalParameterListContext formalParameterList = formalParameters.formalParameterList();
            if (formalParameterList == null)
                parameters = Enumerable.Empty<ParameterDeclaration>();
            else
                parameters = formalParameterList.formalParameter()
                    .Select(param => (ParameterDeclaration)Visit(param))
                    .Where(p => p != null).ToArray();

            BlockStatement body = methodBody != null
                ? (BlockStatement)Visit(methodBody)
                /*: new BlockStatement(Enumerable.Empty<Statement>(),
                    GetAndConvertTextSpan((ITerminalNode)context.GetChild(context.ChildCount - 1)), FileNode);*/
                : null;

            var result = new MethodDeclaration(id, parameters, body, textSpan, FileNode);
            return result;
        }
    }
}
