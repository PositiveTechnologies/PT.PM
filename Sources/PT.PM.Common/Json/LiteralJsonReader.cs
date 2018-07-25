using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System;

namespace PT.PM.Common.Json
{
    public class LiteralJsonReader
    {
        public Ust Read(JObject token, Ust ust)
        {
            switch (ust)
            {
                case StringLiteral stringLiteral:
                    return ReadAsStringLiteral(token, stringLiteral);

                case IntLiteral intLiteral:
                    return ReadAsIntLiteral(token, intLiteral);

                case BinaryOperatorLiteral binaryLiteral:
                    return ReadAsBinaryOperatorLiteral(token, binaryLiteral);

                case BooleanLiteral boolLiteral:
                    return ReadAsBooleanLiteral(token, boolLiteral);

                case CommentLiteral commentLiteral:
                    return ReadAsCommentLiteral(token, commentLiteral);

                case ModifierLiteral modifierLiteral:
                    return ReadAsModifierLiteral(token, modifierLiteral);

                case FloatLiteral floatLiteral:
                    return ReadAsFloatLiteral(token, floatLiteral);

                case InOutModifierLiteral inOutLiteral:
                    return ReadAsInOutModifierLiteral(token, inOutLiteral);

                case UnaryOperatorLiteral unaryLiteral:
                    return ReadAsUnaryOperatorLiteral(token, unaryLiteral);

                default:
                    return ust;
            }
        }

        public UnaryOperatorLiteral ReadAsUnaryOperatorLiteral(JObject token, UnaryOperatorLiteral unaryLiteral)
        {
            string operatorText = token[nameof(UnaryOperatorLiteral.UnaryOperator)]?.ToString();
            if (operatorText != null)
            {
                Enum.TryParse(operatorText, out UnaryOperator op);
                unaryLiteral.UnaryOperator = op;
            }
            return unaryLiteral;
        }

        public InOutModifierLiteral ReadAsInOutModifierLiteral(JObject token, InOutModifierLiteral inOutLiteral)
        {
            string modifierTypeText = token[nameof(InOutModifierLiteral.ModifierType)]?.ToString();
            if (modifierTypeText != null)
            {
                Enum.TryParse(modifierTypeText, out InOutModifier mod);
                inOutLiteral.ModifierType = mod;
            }
            return inOutLiteral;
        }

        public FloatLiteral ReadAsFloatLiteral(JObject token, FloatLiteral floatLiteral)
        {
            float value = 0;
            string valueText = token[nameof(FloatLiteral.Value)]?.ToString();
            if (!string.IsNullOrEmpty(valueText))
            {
                float.TryParse(valueText, out value);
            }
            floatLiteral.Value = value;
            return floatLiteral;
        }

        public ModifierLiteral ReadAsModifierLiteral(JObject token, ModifierLiteral modifierLiteral)
        {
            string modifierTypeText = token[nameof(ModifierLiteral.Modifier)]?.ToString();
            if (modifierTypeText != null)
            {
                Enum.TryParse(modifierTypeText, out Modifier mod);
                modifierLiteral.Modifier = mod;
            }
            return modifierLiteral;
        }

        public CommentLiteral ReadAsCommentLiteral(JObject token, CommentLiteral commentLiteral)
        {
            commentLiteral.Comment = token[nameof(CommentLiteral.Comment)]?.ToString()
                ?? commentLiteral.Comment;

            return commentLiteral;
        }

        public BooleanLiteral ReadAsBooleanLiteral(JObject token, BooleanLiteral boolLiteral)
        {
            string valueText = token[nameof(BooleanLiteral.Value)]?.ToString();
            if (!string.IsNullOrEmpty(valueText))
            {
                boolLiteral.Value = bool.TrueString.Equals(valueText, StringComparison.InvariantCultureIgnoreCase);
            }
            return boolLiteral;
        }

        public BinaryOperatorLiteral ReadAsBinaryOperatorLiteral(JObject token, BinaryOperatorLiteral binaryLiteral)
        {
            string operatorText = token[nameof(BinaryOperatorLiteral.BinaryOperator)]?.ToString();
            if (operatorText != null)
            {
                Enum.TryParse(operatorText, out BinaryOperator op);
                binaryLiteral.BinaryOperator = op;
            }

            return binaryLiteral;
        }

        public IntLiteral ReadAsIntLiteral(JObject token, IntLiteral intLiteral)
        {
            int value = 0;
            string valueText = token[nameof(IntLiteral.Value)]?.ToString();
            if (!string.IsNullOrEmpty(valueText))
            {
                int.TryParse(valueText, out value);
                intLiteral.Value = value;
            }
            return intLiteral;
        }

        public StringLiteral ReadAsStringLiteral(JObject token, StringLiteral stringLiteral)
        {
            stringLiteral.Text = token[nameof(StringLiteral.Text)]?.ToString();
            string valueText = token[nameof(StringLiteral.EscapeCharsLength)]?.ToString();
            if (!string.IsNullOrEmpty(valueText))
            {
                int.TryParse(valueText, out int escapeCharsLength);
                stringLiteral.EscapeCharsLength = escapeCharsLength;
            }
            return stringLiteral;
        }
    }
}
