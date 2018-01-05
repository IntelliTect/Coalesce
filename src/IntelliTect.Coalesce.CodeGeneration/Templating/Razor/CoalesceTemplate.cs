using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace IntelliTect.Coalesce.CodeGeneration.Templating.Razor
{
    public abstract class CoalesceTemplate
    {
        private TextWriter Output { get; set; }
        /// <summary>
        /// Used to hold the closing task for attributes while the attribute is being written.
        /// </summary>
        private string AttributeSuffix { get; set; }
        
        public abstract Task ExecuteAsync();

        public abstract void SetModel(object model);

        public async Task<Stream> GetOutputAsync()
        {
            MemoryStream output = new MemoryStream();
            Output = new StreamWriter(output);
            await ExecuteAsync();
            await Output.FlushAsync();
            output.Seek(0, SeekOrigin.Begin);
            return output;
        }

        public void WriteLiteral(object value)
        {
            WriteLiteralTo(Output, value);
        }

        public virtual void WriteLiteralTo(TextWriter writer, object text)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (text != null)
            {
                writer.Write(text.ToString());
            }
        }

        public virtual void Write(object value)
        {
            WriteTo(Output, value);
        }

        public virtual void WriteTo(TextWriter writer, object content)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (content != null)
            {
                writer.Write(content.ToString());
            }
        }

        // This works around Razor's propensity to write attributes a special way.
        public virtual void BeginWriteAttribute(
            string name,
            string prefix,
            int prefixOffset,
            string suffix,
            int suffixOffset,
            int attributeValuesCount)
        {
            AttributeSuffix = suffix;
            WriteLiteral(prefix);
        }

        public void WriteAttributeValue(
            string prefix,
            int prefixOffset,
            object value,
            int valueOffset,
            int valueLength,
            bool isLiteral)
        {
            WriteLiteral($"{prefix}{value}");
        }

        public virtual void EndWriteAttribute()
        {
            WriteLiteral(AttributeSuffix);
        }
    }

    public abstract class CoalesceTemplate<TModel> : CoalesceTemplate
        where TModel : class
    {
        public TModel Model { get; private set; }

        public override void SetModel(object model) => Model = model as TModel ?? throw new ArgumentException("Incorrect model type");
    }
}
