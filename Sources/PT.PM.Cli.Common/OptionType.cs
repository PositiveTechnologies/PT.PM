using CommandLine;
using System;
using System.Reflection;

namespace PT.PM.Cli.Common
{
    public class OptionType
    {
        public OptionAttribute Option { get; }

        public Type Type { get; }

        public string ParamName {get;}

        public OptionType(OptionAttribute option, PropertyInfo prop)
        {
            Option = option ?? throw new ArgumentNullException(nameof(option));
            Type = prop?.PropertyType ?? throw new ArgumentNullException(nameof(prop));
            if (string.IsNullOrEmpty(Option.ShortName) && string.IsNullOrEmpty(Option.LongName))
            {
                ParamName = prop.Name.ToLowerInvariant();
            }
        }

        public override string ToString()
        {
            return $"{Option.ShortName}; {Option.LongName}; {Type}";
        }
    }
}
