using System;
using System.IO;
using System.Text;

namespace SecuViewer.Cryption.Implementations
{
    internal class LegacyCryptor : ICryptor
    {
        internal static readonly LegacyCryptor Singleton = new LegacyCryptor();

        public string Decrypt(IVocabularyProvider data, Stream reader)
        {
            int overhead = 0;
            var builder = new StringBuilder();
            long readerLength = reader.Length, streamLength = readerLength - reader.Position;
            using (var breader = new BinaryReader(reader, Encoding.Default, true))
            {
                while (builder.Length < (streamLength - overhead * 2) / 8 && readerLength >= reader.Position + 8)
                {
                    char x = default;
                    for (int j = 0; j < 4; j++)
                    {
                        var z = breader.ReadInt16();
                        var y = (char)(z ^ data.Next());
                        x |= y;
                    }
                    if (x % 3 == 0 && readerLength >= reader.Position + 2)
                    {
                        breader.ReadInt16();
                        overhead++;
                    }
                    if (x % 2 == 0 && readerLength >= reader.Position + 2)
                    {
                        breader.ReadInt16();
                        overhead++;
                    }
                    builder.Append(x);
                }
            }
            return builder.ToString();
        }

        public void Encrypt(IVocabularyProvider data, string what, Stream writer)
        {
            int overhead = 0;
            long padding = writer.Position;
            var rnd = new Random();
            using (var briter = new BinaryWriter(writer, Encoding.Default, true))
            {
                foreach (char t in what)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        var ch = (ushort)(t & 0xF << 4 * j ^ data.Next());
                        briter.Write(ch);
                    }
                    if (t % 3 == 0)
                    {
                        briter.Write((short)rnd.Next(1023, 65535));
                        overhead++;
                    }
                    if (t % 2 == 0)
                    {
                        briter.Write((short)rnd.Next(1023, 65535));
                        overhead++;
                    }
                }
            }
            writer.SetLength(padding + what.Length * 8 + overhead * 2);
        }

        public bool CanDecrypt(Stream reader)
        {
            return true;
        }
    }
}
