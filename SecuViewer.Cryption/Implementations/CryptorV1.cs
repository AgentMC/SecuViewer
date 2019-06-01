using System;
using System.IO;
using System.Linq;

namespace SecuViewer.Cryption.Implementations
{
    internal class CryptorV1 : ICryptor
    {
        protected virtual byte[] Magic { get; } = { (byte)'S', (byte)'F', (byte)'V', (byte)'1' };

        public virtual string Decrypt(IVocabularyProvider data, Stream reader)
        {
            var offset = (reader.ReadByte() ^ data.Next()) & 0xFF;
            reader.Position = offset;
            for (int i = 0; i < (offset - (Magic.Length + 1) & 0xF); i++)
            {
                data.Next();
            }
            return LegacyCryptor.Singleton.Decrypt(data, reader);
        }

        public virtual void Encrypt(IVocabularyProvider data, string what, Stream writer)
        {
            writer.Write(Magic, 0, Magic.Length);
            var rnd = new Random();
            var garbageCount = rnd.Next(10, 250);
            var totalPadding = (byte)((garbageCount + Magic.Length + 1 ^ data.Next()) & 0xFF);
            var buffer = new byte[garbageCount + 1];
            rnd.NextBytes(buffer);
            buffer[0] = totalPadding;
            writer.Write(buffer, 0, buffer.Length);
            for (int i = 0; i < (garbageCount & 0xF); i++)
            {
                data.Next();
            }
            LegacyCryptor.Singleton.Encrypt(data, what, writer);
        }

        public bool CanDecrypt(Stream reader)
        {
            var buffer = new byte[Magic.Length];
            var read = reader.Read(buffer, 0, Magic.Length);
            if (read == Magic.Length && buffer.SequenceEqual(Magic)) return true;
            reader.Position = 0;
            return false;
        }
    }
}
