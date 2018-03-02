using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecuViewer
{
    internal class Crypter
    {
        internal static string Decrypt(Password psw, string path)
        {
            var builder = new StringBuilder();
            var data = new CryptoData(psw);
            int i = 0, overhead = 0;
            using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var breader = new BinaryReader(reader, Encoding.Default))
            {
                while (builder.Length < (reader.Length - overhead*2)/8)
                {
                    char x = default(char);
                    for (int j = 0; j < 4; j++)
                    {
                        var z = breader.ReadInt16();
                        var y = (char) (z ^ data.Vocabulary[i++] ^ data.GlobalHash);
                        x |= y;
                        if (i == data.VocabularyLength) i = 0;
                    }
                    if (x%3 == 0)
                    {
                        breader.ReadInt16();
                        overhead++;
                    }
                    if (x%2 == 0)
                    {
                        breader.ReadInt16();
                        overhead++;
                    }
                    builder.Append(x);
                }
            }
            return builder.ToString();
        }

        internal static void Encrypt(Password psw, string what, string where)
        {
            var rnd = new Random();
            var data = new CryptoData(psw);
            int i = 0, overhead = 0;
            using (var writer = new FileStream(where, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            using (var briter = new BinaryWriter(writer, Encoding.Default))
            {
                foreach (char t in what)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        var ch = (ushort)((t & (0xF << (4 * j))) ^ data.Vocabulary[i++] ^ data.GlobalHash);
                        briter.Write(ch);
                        if (i == data.VocabularyLength) i = 0;
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
                writer.SetLength(what.Length * 8 + overhead * 2);
            }
        }

        private struct CryptoData
        {
            public readonly string Vocabulary;
            public readonly int GlobalHash;
            public readonly int VocabularyLength;

            public CryptoData(Password psw)
            {
                var builder = new StringBuilder();
                int k = 0;
                var source = psw.textBox1.Text;
                for (int j = 0; j < 5; j++)
                {
                    foreach (var t in source)
                    {
                        builder.Append((char)(t ^ (0x579D579D >> k++) ^ (source.GetHashCode())));
                        if (k == 16) k = 0;
                    }
                }
                Vocabulary = builder.ToString();
                GlobalHash = Vocabulary.GetHashCode() & 0xFFFF;
                VocabularyLength = Vocabulary.Length;
            }
        }
    }
}
