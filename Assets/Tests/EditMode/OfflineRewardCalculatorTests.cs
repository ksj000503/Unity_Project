using NUnit.Framework;
using IdleGame.Offline;

public class OfflineRewardCalculatorTests
{
    [Test]
    public void CalculateReward_WithinCap_UsesFullElapsedTime()
    {
        // TimeToKill = ceil(50/10)/1 = 5초. 50초 경과 -> 10킬.
        var (gold, exp) = OfflineRewardCalculator.CalculateReward(
            characterAttack: 10, attacksPerSecond: 1f, monsterHp: 50,
            monsterGold: 3, monsterExp: 2, elapsedSeconds: 50);

        Assert.AreEqual(30, gold);
        Assert.AreEqual(20, exp);
    }

    [Test]
    public void CalculateReward_BeyondCap_ClampsToMaxOfflineSeconds()
    {
        double farBeyondCap = OfflineRewardCalculator.MaxOfflineSeconds + 10_000;

        var (gold, _) = OfflineRewardCalculator.CalculateReward(
            characterAttack: 10, attacksPerSecond: 1f, monsterHp: 50,
            monsterGold: 3, monsterExp: 2, elapsedSeconds: farBeyondCap);

        var (cappedGold, _) = OfflineRewardCalculator.CalculateReward(
            characterAttack: 10, attacksPerSecond: 1f, monsterHp: 50,
            monsterGold: 3, monsterExp: 2, elapsedSeconds: OfflineRewardCalculator.MaxOfflineSeconds);

        Assert.AreEqual(cappedGold, gold);
    }

    [Test]
    public void CalculateReward_ZeroOrNegativeElapsed_ReturnsZero()
    {
        var (gold, exp) = OfflineRewardCalculator.CalculateReward(
            characterAttack: 10, attacksPerSecond: 1f, monsterHp: 50,
            monsterGold: 3, monsterExp: 2, elapsedSeconds: 0);

        Assert.AreEqual(0, gold);
        Assert.AreEqual(0, exp);
    }
}
