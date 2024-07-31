using System;

namespace IntelliTect.Analyzer
{
    [AttributeUsage(AttributeTargets.All)]
    internal class DescriptionAttribute(string description) : Attribute
    {
        public static DescriptionAttribute Default { get; } = new DescriptionAttribute();

        public string Description { get; } = description;

        public DescriptionAttribute() : this(string.Empty)
        {
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
            {
                return true;
            }

            if (obj is DescriptionAttribute descriptionAttribute)
            {
                return descriptionAttribute.Description == Description;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Description.GetHashCode();
        }
    }
}
