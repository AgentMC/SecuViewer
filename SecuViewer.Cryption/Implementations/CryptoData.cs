using System.Text;

namespace SecuViewer.Cryption.Implementations
{
    internal class CryptoData : IVocabularyProvider
    {
        private int _vocabularyLength;
        private int[] _vocabulary;
        private int _ptr;

        public CryptoData(string source)
        {
            Initialize(source);
        }

        protected void Initialize(string source)
        {
            //1. Get voco srting
            var voco = GetInitialVoco(source);
            //2. Get GlobalHash
            var globalHash = Hash(voco) & 0xFFFF;
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

        private string GetInitialVoco(string source)
        {
            const int multiplier = 5;
            var builder = new StringBuilder(source.Length * multiplier);
            int k = 0, sourceHash = Hash(source);
            for (int j = 0; j < multiplier; j++)
            {
                foreach (var t in source)
                {
                    builder.Append((char)(t ^ 0x579D579D >> k++ ^ sourceHash));
                    if (k == 16) k = 0;
                }
            }
            var voco = builder.ToString();
            return voco;
        }

        protected virtual int Hash(string input)
        {
            return input.GetHashCode();
        }

        public int Next()
        {
            if (_ptr == _vocabularyLength) _ptr = 0;
            return _vocabulary[_ptr++];
        }
    }
}