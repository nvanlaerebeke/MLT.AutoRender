using System.IO;
using System.Text;

namespace CrazyUtils {
    public sealed class StringWriterWithEncoding : StringWriter {
        private readonly Encoding encoding;

        public StringWriterWithEncoding(Encoding encoding, StringBuilder sb) {
            this.encoding = encoding;
        }

        public StringWriterWithEncoding() { }

        public StringWriterWithEncoding(Encoding encoding) {
            this.encoding = encoding;
        }

        public override Encoding Encoding {
            get { return encoding; }
        }
    }
}