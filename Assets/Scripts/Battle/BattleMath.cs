using System;

namespace IdleGame.Battle
{
    public static class BattleMath
    {
        // 몬스터 체력을 공격력으로 나눠 필요한 타격 횟수를 올림 계산하고,
        // 공격속도로 나눠 처치까지 걸리는 시간을 구한다. (오프라인 보상 계산에도 재사용)
        public static double TimeToKillSeconds(int attack, float attacksPerSecond, long monsterHp)
        {
            if (attack <= 0) throw new ArgumentOutOfRangeException(nameof(attack));
            if (attacksPerSecond <= 0) throw new ArgumentOutOfRangeException(nameof(attacksPerSecond));
            if (monsterHp <= 0) throw new ArgumentOutOfRangeException(nameof(monsterHp));

            long hitsToKill = (long)Math.Ceiling((double)monsterHp / attack);
            return hitsToKill / (double)attacksPerSecond;
        }
    }
}
