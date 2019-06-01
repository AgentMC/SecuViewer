using System;
using System.IO;

namespace SecuViewer.Cryption.Implementations
{
    internal class CryptorV2 : CryptorV1
    {
        protected override byte[] Magic { get; } = { (byte)'S', (byte)'F', (byte)'V', (byte)'2' };
        public override string Decrypt(IVocabularyProvider data, Stream reader)
        {
            return base.Decrypt(CheckCryptoData(data).UpgradeToV2(), reader);
        }

        private static CryptoDataV2 CheckCryptoData(IVocabularyProvider data)
        {
            if (!(data is CryptoDataV2 datav2))
            {
                throw new Exception("CryptorV2 expects CryptoDataV2 or later version as IVocabularyProvider");
            }
            return datav2;
        }

        public override void Encrypt(IVocabularyProvider data, string what, Stream writer)
        {
            base.Encrypt(CheckCryptoData(data).UpgradeToV2(), what, writer);
        }
    }
}
