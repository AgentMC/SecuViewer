namespace SecuViewer.Cryption.Implementations
{
    internal class CryptoDataV2 : CryptoData
    {
        private readonly string Source;
        private bool _v2Upgraded = false;

        public CryptoDataV2(string source) : base(source)
        {
            Source = source;
        }

        public CryptoDataV2 UpgradeToV2()
        {
            _v2Upgraded = true;
            Initialize(Source);
            return this;
        }

        protected override int Hash(string input)
        {
            return _v2Upgraded ? GetDeterministicHashCode(input) : base.Hash(input);
        }

        private static int GetDeterministicHashCode(string str)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}
