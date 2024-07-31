using System;
using System.Reflection;

namespace IntelliTect.Analyzer
{
    public static class AnalyzerBlockMixins
    {
        private static string GetEnumAttributeValue<TAttribute>(Enum value, Func<TAttribute, string> valueAccessor)
            where TAttribute : Attribute
        {
            FieldInfo fi = value.GetType().GetRuntimeField(value.ToString());

            var attributes = (TAttribute[])fi?.GetCustomAttributes(typeof(TAttribute), false);

            return attributes?.Length > 0 ? valueAccessor(attributes[0]) : null;
        }

        public static string GetDescription(this AnalyzerBlock value)
        {
            if (value == AnalyzerBlock.None)
            {
                throw new ArgumentException("AnalyzerBlock must be specified", nameof(value));
            }

            return GetEnumAttributeValue<DescriptionAttribute>(value, x => x.Description);
        }
    }
}
