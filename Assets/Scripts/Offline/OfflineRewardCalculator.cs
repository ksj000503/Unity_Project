using System;
using IdleGame.Battle;

namespace IdleGame.Offline
{
    public static class OfflineRewardCalculator
    {
        // 스펙 7절: 오프라인 보상은 최대 8시간까지만 인정한다.
        public const int MaxOfflineSeconds = 8 * 60 * 60;

        public static (int gold, int exp) CalculateReward(
            int characterAttack, float attacksPerSecond, int monsterHp,
            int monsterGold, int monsterExp, double elapsedSeconds)
        {
            if (elapsedSeconds <= 0) return (0, 0);

            double cappedSeconds = Math.Min(elapsedSeconds, MaxOfflineSeconds);
            double timeToKill = BattleMath.TimeToKillSeconds(characterAttack, attacksPerSecond, monsterHp);
            int kills = (int)(cappedSeconds / timeToKill);

            return (kills * monsterGold, kills * monsterExp);
        }
    }
}
