using System;
using System.IO;
using System.Text;

namespace SecuViewer
{
    internal class Crypter
    {
        internal static string Decrypt(Password psw, string path)
        {
            var data = new CryptoData(psw);
            using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Decrypt(data, reader);
            }
        }

        internal static string Decrypt(CryptoData data, Stream reader)
        {
            int overhead = 0;
            var builder = new StringBuilder();
            using (var breader = new BinaryReader(reader, Encoding.Default))
            {
                while (builder.Length < (reader.Length - overhead * 2) / 8)
                {
                    char x = default(char);
                    for (int j = 0; j < 4; j++)
                    {
                        var z = breader.ReadInt16();
                        var y = (char)(z ^ data.Next());
                        x |= y;
                    }
                    if (x % 3 == 0)
                    {
                        breader.ReadInt16();
                        overhead++;
                    }
                    if (x % 2 == 0)
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
            var data = new CryptoData(psw);
            using (var writer = new FileStream(where, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                Encrypt(data, what, writer);
            }   
        }

        internal static void Encrypt(CryptoData data, string what, Stream writer)
        {
            int overhead = 0;
            var rnd = new Random();
            using (var briter = new BinaryWriter(writer, Encoding.Default, true))
            {
                foreach (char t in what)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        var ch = (ushort)((t & (0xF << (4 * j))) ^ data.Next());
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
            writer.SetLength(what.Length * 8 + overhead * 2);
        }

        internal struct CryptoData
        {
            private readonly int _vocabularyLength;
            private readonly int[] _vocabulary;
            private int _ptr;

            public CryptoData(Password psw)
            {
                //1. Get voco srting
                var voco = GetInitialVoco(psw.textBox1.Text);
                //2. Get GlobalHash
                var globalHash = voco.GetHashCode() & 0xFFFF;
                //3. Build hashed array
                _vocabularyLength = voco.Length;
                _vocabulary = new int[_vocabularyLength];
                for (int i = 0; i < _vocabularyLength; i++)
                {
                    _vocabulary[i] = voco[i] ^ globalHash;
                }
                //4. Init array pointer
                _ptr = 0;
            }

            private static string GetInitialVoco(string source)
            {
                const int multiplier = 5;
                var builder = new StringBuilder(source.Length * multiplier);
                int k = 0, sourceHash = source.GetHashCode();
                for (int j = 0; j < multiplier; j++)
                {
                    foreach (var t in source)
                    {
                        builder.Append((char) (t ^ (0x579D579D >> k++) ^ sourceHash));
                        if (k == 16) k = 0;
                    }
                }
                var voco = builder.ToString();
                return voco;
            }

            public int Next()
            {
                if (_ptr == _vocabularyLength) _ptr = 0;
                return _vocabulary[_ptr++];
            }
        }
    }
}
