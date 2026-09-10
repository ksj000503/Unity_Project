using System;

namespace IdleGame.Economy
{
    public class CurrencyWallet
    {
        public int Gold { get; private set; }
        public int EnhanceStones { get; private set; }

        public void AddGold(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Gold += amount;
        }

        public void AddEnhanceStones(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            EnhanceStones += amount;
        }

        public bool TrySpendGold(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (Gold < amount) return false;
            Gold -= amount;
            return true;
        }

        public bool TrySpendEnhanceStones(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (EnhanceStones < amount) return false;
            EnhanceStones -= amount;
            return true;
        }

        public void LoadState(int gold, int enhanceStones)
        {
            if (gold < 0) throw new ArgumentOutOfRangeException(nameof(gold));
            if (enhanceStones < 0) throw new ArgumentOutOfRangeException(nameof(enhanceStones));
            Gold = gold;
            EnhanceStones = enhanceStones;
        }
    }
}
