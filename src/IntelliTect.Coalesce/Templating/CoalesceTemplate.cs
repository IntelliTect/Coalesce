using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace IntelliTect.Coalesce.Templating
{
    public abstract class CoalesceTemplate
    {
        private TextWriter Output { get; set; }

        public dynamic Model { get; set; }
        public object FileName { get; internal set; }

        public abstract Task ExecuteAsync();

        public async Task<string> ExecuteTemplate()
        {
            StringBuilder output = new StringBuilder();
            using (var writer = new StringWriter(output))
            {
                Output = writer;
                await ExecuteAsync();
            }
            return output.ToString();
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
    }
}
