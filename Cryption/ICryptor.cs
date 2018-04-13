using System.IO;

namespace SecuViewer.Cryption
{
    internal interface ICryptor
    {
        string Decrypt(CryptoData data, Stream reader);
        void Encrypt(CryptoData data, string what, Stream writer);
        bool CanDecrypt(Stream reader);
    }
}