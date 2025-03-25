using System;

namespace XTuleap.API2
{
    /// <summary>
    /// Attribute used to declare an attribute on a field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TuleapField : Attribute
    {
        /// <summary>
        /// Gets the field name.
        /// </summary>
        public string FieldName
        {
            get;
            private set;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="pFieldName"></param>
        public TuleapField(string pFieldName)
        {
            this.FieldName = pFieldName;
        }
    }
}
