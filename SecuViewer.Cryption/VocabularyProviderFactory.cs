using SecuViewer.Cryption.Implementations;

namespace SecuViewer.Cryption
{
    public class VocabularyProviderFactory
    {
        public static IVocabularyProvider CreateProvider(string password)
        {
            return new CryptoData(password);
        }
    }
}
