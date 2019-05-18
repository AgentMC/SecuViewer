using System.IO;

namespace SecuViewer.Cryption.Implementations
{
    internal interface ICryptor
    {
        string Decrypt(IVocabularyProvider data, Stream reader);
        void Encrypt(IVocabularyProvider data, string what, Stream writer);
        bool CanDecrypt(Stream reader);
    }
}