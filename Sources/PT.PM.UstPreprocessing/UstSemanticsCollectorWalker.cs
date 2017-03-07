using System.Collections.Generic;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.TypeMembers;
using PT.PM.Common.Symbols;

namespace PT.PM.UstPreprocessing
{
    public class UstSemanticsCollectorWalker : UstListener
    {
        private string DefaultTypeName = "default_type";

        private Dictionary<string, TypeSymbol> typeSymbols = new Dictionary<string, TypeSymbol>();

        private string currentTypeName;
        private string currentMethodName;

        public IReadOnlyDictionary<string, TypeSymbol> TypeSymbols => typeSymbols;

        public UstSemanticsCollectorWalker()
        {
            typeSymbols.Add(DefaultTypeName, new TypeSymbol(DefaultTypeName));
        }

        public override void Enter(TypeDeclaration typeDeclaration)
        {
            currentTypeName = typeDeclaration.Name.Id;
            TypeSymbol typeSymbol;
            if (!typeSymbols.TryGetValue(currentTypeName, out typeSymbol))
            {
                typeSymbol = new TypeSymbol(currentTypeName) { TypeDeclaration = typeDeclaration };
                typeSymbols[currentTypeName] = typeSymbol;
            }
        }

        public override void Exit(TypeDeclaration typeDeclaration)
        {
            currentTypeName = null;
        }

        public override void Enter(MethodDeclaration methodDeclaration)
        {
            var methodSymbol = new MethodSymbol(methodDeclaration.Signature);
            var typeName = currentTypeName;
            if (typeName == null)
            {
                typeName = DefaultTypeName;
            }

            currentMethodName = methodSymbol.Name;
            typeSymbols[typeName].MethodSymbols[methodSymbol.Name] = methodSymbol;
        }

        public override void Exit(MethodDeclaration methodDeclaration)
        {
            currentMethodName = null;
        }
    }
}
