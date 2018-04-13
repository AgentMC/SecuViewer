using System.IO;

namespace SecuViewer.Cryption
{
    internal static class Crypter
    {
        internal static string Decrypt(Password psw, string path)
        {
            var data = new CryptoData(psw.textBox1.Text);
            using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Decrypt(data, reader);
            }
        }

        internal static string Decrypt(CryptoData data, Stream reader)
        {
            foreach (var cryptor in Cryptors)
            {
                if (cryptor.CanDecrypt(reader)) return cryptor.Decrypt(data, reader);
            }
            return null;
        }

        internal static void Encrypt(Password psw, string what, string where)
        {
            var data = new CryptoData(psw.textBox1.Text);
            using (var writer = new FileStream(where, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                Encrypt(data, what, writer);
            }   
        }

        internal static void Encrypt(CryptoData data, string what, Stream writer)
        {
            Cryptors[0].Encrypt(data, what, writer);
        }

        private static readonly ICryptor[] Cryptors =
        {
            new CryptorV1(), 
            LegacyCryptor.Singleton
        };
    }
}
