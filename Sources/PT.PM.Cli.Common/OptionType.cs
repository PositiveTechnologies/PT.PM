using System;
using System.Reflection;

namespace PT.PM.Cli.Common
{
    class OptionProperty
    {
        public OptionAttribute Option { get; }

        public PropertyInfo PropertyInfo { get; }

        public string LongName => string.IsNullOrEmpty(Option.LongName)
            ? PropertyInfo.Name.ToLowerInvariant()
            : Option.LongName;

        public OptionProperty(OptionAttribute option, PropertyInfo propertyInfo)
        {
            Option = option ?? throw new ArgumentNullException(nameof(option));
            PropertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
        }

        public override string ToString()
        {
            return $"{Option}; {PropertyInfo}";
        }
    }
}
