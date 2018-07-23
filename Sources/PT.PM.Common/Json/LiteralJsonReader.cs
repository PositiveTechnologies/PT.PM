using Newtonsoft.Json;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System;

namespace PT.PM.Common.Json
{
    public class LiteralJsonReader
    {
        public Ust Read(JsonReader reader, Ust ust)
        {
            switch (ust)
            {
                case StringLiteral stringLiteral:
                    return ReadAsStringLiteral(reader, stringLiteral);

                case IntLiteral intLiteral:
                    return ReadAsIntLiteral(reader, intLiteral);

                case BinaryOperatorLiteral binaryLiteral:
                    return ReadAsBinaryOperatorLiteral(reader, binaryLiteral);

                case BooleanLiteral boolLiteral:
                    return ReadAsBooleanLiteral(reader, boolLiteral);

                case CommentLiteral commentLiteral:
                    return ReadAsCommentLiteral(reader, commentLiteral);

                case ModifierLiteral modifierLiteral:
                    return ReadAsModifierLiteral(reader, modifierLiteral);

                case FloatLiteral floatLiteral:
                    return ReadAsFloatLiteral(reader, floatLiteral);

                case InOutModifierLiteral inOutLiteral:
                    return ReadAsInOutModifierLiteral(reader, inOutLiteral);

                case UnaryOperatorLiteral unaryLiteral:
                    return ReadAsUnaryOperatorLiteral(reader, unaryLiteral);

                default:
                    return ust;
            }
        }

        private Ust ReadAsUnaryOperatorLiteral(JsonReader reader, UnaryOperatorLiteral unaryLiteral)
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
                    if (reader.TokenType == JsonToken.String && currentProperty == nameof(UnaryOperatorLiteral.UnaryOperator))
                    {
                        Enum.TryParse(reader.Value.ToString(), out UnaryOperator op);
                        unaryLiteral.UnaryOperator = op;
                        break;
                    }
                }
            }
            return unaryLiteral;
        }

        private Ust ReadAsInOutModifierLiteral(JsonReader reader, InOutModifierLiteral inOutLiteral)
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
                    if (reader.TokenType == JsonToken.String && currentProperty == nameof(InOutModifierLiteral.ModifierType))
                    {
                        Enum.TryParse(reader.Value.ToString(), out InOutModifier mod);
                        inOutLiteral.ModifierType = mod;
                        break;
                    }
                }
            }
            return inOutLiteral;
        }

        private Ust ReadAsFloatLiteral(JsonReader reader, FloatLiteral floatLiteral)
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
                    if (reader.TokenType == JsonToken.Float && currentProperty == nameof(FloatLiteral.Value))
                    {
                        floatLiteral.Value = float.Parse(reader.Value.ToString());
                        break;
                    }
                }
            }
            return floatLiteral;
        }

        private Ust ReadAsModifierLiteral(JsonReader reader, ModifierLiteral modifierLiteral)
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
                    if (reader.TokenType == JsonToken.String && currentProperty == nameof(ModifierLiteral.Modifier))
                    {
                        Enum.TryParse(reader.Value.ToString(), out Modifier mod);
                        modifierLiteral.Modifier = mod;
                        break;
                    }
                }
            }
            return modifierLiteral;
        }

        private Ust ReadAsCommentLiteral(JsonReader reader, CommentLiteral commentLiteral)
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
                    if (reader.TokenType == JsonToken.String && currentProperty == nameof(CommentLiteral.Comment))
                    {
                        commentLiteral.Comment = reader.Value.ToString();
                        break;
                    }
                }
            }
            return commentLiteral;
        }

        private Ust ReadAsBooleanLiteral(JsonReader reader, BooleanLiteral boolLiteral)
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
                    if (reader.TokenType == JsonToken.Boolean && currentProperty == nameof(BooleanLiteral.Value))
                    {
                        boolLiteral.Value = bool.Parse(reader.Value.ToString());
                        break;
                    }
                }
            }
            return boolLiteral;
        }

        private Ust ReadAsBinaryOperatorLiteral(JsonReader reader, BinaryOperatorLiteral binaryLiteral)
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
                    if (reader.TokenType == JsonToken.String && currentProperty == nameof(BinaryOperatorLiteral.BinaryOperator))
                    {
                        Enum.TryParse(reader.Value.ToString(), out BinaryOperator op);
                        binaryLiteral.BinaryOperator = op;
                        break;
                    }
                }
            }

            return binaryLiteral;
        }

        private Ust ReadAsIntLiteral(JsonReader reader, IntLiteral intLiteral)
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
                    if (reader.TokenType == JsonToken.Integer && currentProperty == nameof(IntLiteral.Value))
                    {
                        intLiteral.Value = int.Parse(reader.Value.ToString());
                        break;
                    }
                }
            }
            return intLiteral;
        }

        private Ust ReadAsStringLiteral(JsonReader reader, StringLiteral stringLiteral)
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
                    if (reader.TokenType == JsonToken.String && currentProperty == nameof(StringLiteral.Text))
                    {
                        stringLiteral.Text = reader.Value.ToString();
                    }
                    else if (reader.TokenType == JsonToken.Integer && currentProperty == nameof(StringLiteral.EscapeCharsLength))
                    {
                        stringLiteral.EscapeCharsLength = int.Parse(reader.Value.ToString());
                    }
                }
            }
            return stringLiteral;
        }
    }
}
