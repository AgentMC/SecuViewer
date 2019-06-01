using SecuViewer.Cryption.Implementations;
using System.IO;

namespace SecuViewer.Cryption
{
    public static class Crypter
    {
        public static string Decrypt(IPasswordProvider psw, string path)
        {
            var data = VocabularyProviderFactory.CreateProvider(psw.RawPassword);
            using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Decrypt(data, reader);
            }
        }

        public static string Decrypt(IVocabularyProvider data, Stream reader)
        {
            foreach (var cryptor in Cryptors)
            {
                if (cryptor.CanDecrypt(reader)) return cryptor.Decrypt(data, reader);
            }
            return null;
        }

        public static void Encrypt(IPasswordProvider psw, string what, string where)
        {
            var data = VocabularyProviderFactory.CreateProvider(psw.RawPassword);
            using (var writer = new FileStream(where, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                Encrypt(data, what, writer);
            }   
        }

        public static void Encrypt(IVocabularyProvider data, string what, Stream writer)
        {
            Cryptors[0].Encrypt(data, what, writer);
        }

        private static readonly ICryptor[] Cryptors =
        {
            new CryptorV2(),
            new CryptorV1(), 
            LegacyCryptor.Singleton
        };
    }
}
