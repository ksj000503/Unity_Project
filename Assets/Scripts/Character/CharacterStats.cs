using System;

namespace IdleGame.Character
{
    public class CharacterStats
    {
        private const int BaseAttack = 10;
        private const int BaseHealth = 100;
        private const int AttackPerLevel = 2;
        private const int HealthPerLevel = 15;
        private const int BaseExpToLevel = 100;
        private const int ExpToLevelIncreasePerLevel = 50;

        public int Level { get; private set; } = 1;
        public long CurrentExp { get; private set; }

        public int Attack => BaseAttack + (Level - 1) * AttackPerLevel;
        public int MaxHealth => BaseHealth + (Level - 1) * HealthPerLevel;

        public static long RequiredExpForLevel(int level)
        {
            return BaseExpToLevel + (long)(level - 1) * ExpToLevelIncreasePerLevel;
        }

        // 여러 레벨을 한 번에 넘기는 경험치도 while로 정확히 처리해야 해서 루프를 쓴다.
        public int AddExp(long amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));

            CurrentExp += amount;
            return NormalizeLevel();
        }

        public void LoadState(int level, long currentExp)
        {
            if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
            if (currentExp < 0) throw new ArgumentOutOfRangeException(nameof(currentExp));

            Level = level;
            CurrentExp = currentExp;
            NormalizeLevel();
        }

        // O(1) 폐쇄형 공식도 가능하지만(이차 방정식), 현재 루프가 더 단순하고 정확하므로 YAGNI.
        private int NormalizeLevel()
        {
            int levelsGained = 0;
            while (CurrentExp >= RequiredExpForLevel(Level))
            {
                CurrentExp -= RequiredExpForLevel(Level);
                Level++;
                levelsGained++;
            }
            return levelsGained;
        }
    }
}
