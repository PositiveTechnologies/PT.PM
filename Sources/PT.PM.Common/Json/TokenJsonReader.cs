using Newtonsoft.Json;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System;

namespace PT.PM.Common.Json
{
    public class TokenJsonReader
    {

        public Ust Read(JsonReader reader, Ust ust)
        {
            if (ust is IdToken idToken)
            {
                ust = ReadAsIdToken(reader, idToken);
            }
            else if (ust is TypeToken typeToken)
            {
                ust = ReadAsTypeToken(reader, typeToken);
            }
            else if (ust is TypeTypeLiteral typeTypeLiteral)
            {
                ust = ReadAsTypeType(reader, typeTypeLiteral);
            }
            return ust;
        }

        private Ust ReadAsTypeType(JsonReader reader, TypeTypeLiteral typeTypeLiteral)
        {
            string currentProperty = string.Empty;
            while (reader.Read())
            {
                if (reader.Value != null)
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        currentProperty = reader.Value.ToString();
                        reader.Read();
                    }
                    if (reader.TokenType == JsonToken.String && currentProperty == nameof(TypeTypeLiteral.TypeType))
                    {
                        Enum.TryParse(reader.Value.ToString(), out TypeType type);
                        typeTypeLiteral.TypeType = type;
                        break;
                    }
                }
            }
            return typeTypeLiteral;
        }

        private Ust ReadAsTypeToken(JsonReader reader, TypeToken typeToken)
        {
            string currentProperty = string.Empty;
            while (reader.Read())
            {
                if (reader.Value != null)
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        currentProperty = reader.Value.ToString();
                        reader.Read();
                    }
                    if (reader.TokenType == JsonToken.String && currentProperty == nameof(TypeToken.TypeText))
                    {
                        typeToken.TypeText = reader.Value.ToString();
                        break;
                    }
                }
            }
            return typeToken;
        }

        private Ust ReadAsIdToken(JsonReader reader, IdToken idToken)
        {
            string currentProperty = string.Empty;
            while (reader.Read())
            {
                if (reader.Value != null)
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        currentProperty = reader.Value.ToString();
                        reader.Read();
                    }
                    if (reader.TokenType == JsonToken.String && currentProperty == nameof(IdToken.Id))
                    {
                        idToken.Id = reader.Value.ToString();
                        break;
                    }
                }
            }
            return idToken;
        }
    }
}
