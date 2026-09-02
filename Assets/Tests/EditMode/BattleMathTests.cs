using System;
using NUnit.Framework;
using IdleGame.Battle;

public class BattleMathTests
{
    [Test]
    public void TimeToKillSeconds_ExactMultiple_ReturnsExpectedSeconds()
    {
        // 공격력 10, 초당 1회 공격, 몬스터 체력 50 -> 5회 타격 필요 -> 5초
        double time = BattleMath.TimeToKillSeconds(attack: 10, attacksPerSecond: 1f, monsterHp: 50);

        Assert.AreEqual(5d, time, 0.0001d);
    }

    [Test]
    public void TimeToKillSeconds_NonExactMultiple_RoundsUpHits()
    {
        // 공격력 10, 초당 2회 공격, 몬스터 체력 45 -> 5회 타격 필요(올림) -> 2.5초
        double time = BattleMath.TimeToKillSeconds(attack: 10, attacksPerSecond: 2f, monsterHp: 45);

        Assert.AreEqual(2.5d, time, 0.0001d);
    }

    [Test]
    public void TimeToKillSeconds_NonPositiveAttack_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BattleMath.TimeToKillSeconds(0, 1f, 10));
    }

    [Test]
    public void TimeToKillSeconds_NonPositiveAttacksPerSecond_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BattleMath.TimeToKillSeconds(10, 0f, 10));
    }

    [Test]
    public void TimeToKillSeconds_NonPositiveMonsterHp_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BattleMath.TimeToKillSeconds(10, 1f, 0));
    }
}
