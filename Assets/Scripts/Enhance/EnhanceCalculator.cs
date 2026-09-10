using System;
using IdleGame.Equipment;

namespace IdleGame.Enhance
{
    public static class EnhanceCalculator
    {
        private const double BaseSuccessRate = 0.9;
        private const double SuccessRateDropPerLevel = 0.05;
        private const double MinSuccessRate = 0.05;
        private const int BaseEnhanceCost = 10;
        private const int CostIncreasePerLevel = 5;

        public static double SuccessRate(int currentLevel)
        {
            if (currentLevel < 0) throw new ArgumentOutOfRangeException(nameof(currentLevel));
            double rate = BaseSuccessRate - currentLevel * SuccessRateDropPerLevel;
            return Math.Max(rate, MinSuccessRate);
        }

        public static int CostForLevel(int currentLevel)
        {
            if (currentLevel < 0) throw new ArgumentOutOfRangeException(nameof(currentLevel));
            return BaseEnhanceCost + currentLevel * CostIncreasePerLevel;
        }

        // 실패해도 장비가 파괴되지 않고 한 단계만 하락한다 (스펙 5절).
        public static int TryEnhance(int currentLevel, IRandomSource random)
        {
            if (currentLevel < 0) throw new ArgumentOutOfRangeException(nameof(currentLevel));

            bool success = random.NextDouble() < SuccessRate(currentLevel);
            if (success) return currentLevel + 1;
            return Math.Max(0, currentLevel - 1);
        }
    }
}
