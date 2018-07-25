using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System;

namespace PT.PM.Common.Json
{
    public class TokenJsonReader
    {

        public Ust Read(JObject token, Ust ust)
        {
            if (ust is IdToken idToken)
            {
                ust = ReadAsIdToken(token, idToken);
            }
            else if (ust is TypeToken typeToken)
            {
                ust = ReadAsTypeToken(token, typeToken);
            }
            else if (ust is TypeTypeLiteral typeTypeLiteral)
            {
                ust = ReadAsTypeType(token, typeTypeLiteral);
            }
            return ust;
        }

        private Ust ReadAsTypeType(JObject token, TypeTypeLiteral typeTypeLiteral)
        {
            string typeText = token[nameof(TypeTypeLiteral.TypeType)]?.ToString();
            if (!string.IsNullOrEmpty(typeText))
            {
                Enum.TryParse(typeText, out TypeType type);
                typeTypeLiteral.TypeType = type;
            }

            return typeTypeLiteral;
        }

        private Ust ReadAsTypeToken(JObject token, TypeToken typeToken)
        {
            typeToken.TypeText = token[nameof(TypeToken.TypeText)]?.ToString()
                ?? typeToken.TypeText;
            return typeToken;
        }

        private Ust ReadAsIdToken(JObject token, IdToken idToken)
        {
            idToken.Id = token[nameof(IdToken.Id)]?.ToString()
                ?? idToken.Id;
            return idToken;
        }
    }
}
