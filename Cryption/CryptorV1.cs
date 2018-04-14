using System;
using System.IO;
using System.Linq;

namespace SecuViewer.Cryption
{
    internal class CryptorV1 : ICryptor
    {
        private readonly byte[] _magic = {(byte) 'S', (byte) 'F', (byte) 'V', (byte) '1'};

        public string Decrypt(CryptoData data, Stream reader)
        {
            var offset = (reader.ReadByte() ^ data.Next()) & 0xFF;
            reader.Position = offset;
            for (int i = 0; i < ((offset - (_magic.Length + 1)) & 0xF); i++)
            {
                data.Next();
            }
            return LegacyCryptor.Singleton.Decrypt(data, reader);
        }

        public void Encrypt(CryptoData data, string what, Stream writer)
        {
            writer.Write(_magic, 0, _magic.Length);
            var rnd = new Random();
            var garbageCount = rnd.Next(10, 250);
            var totalPadding = (byte) (((garbageCount + _magic.Length + 1) ^ data.Next()) & 0xFF);
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
            var buffer = new byte[4];
            var read = reader.Read(buffer, 0, _magic.Length);
            if (read == _magic.Length && buffer.SequenceEqual(_magic)) return true;
            reader.Position = 0;
            return false;
        }
    }
}
