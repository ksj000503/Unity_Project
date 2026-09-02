using System;
using NUnit.Framework;
using IdleGame.Character;

public class CharacterStatsTests
{
    [Test]
    public void NewCharacter_StartsAtLevel1WithBaseStats()
    {
        var stats = new CharacterStats();

        Assert.AreEqual(1, stats.Level);
        Assert.AreEqual(0, stats.CurrentExp);
        Assert.AreEqual(10, stats.Attack);
        Assert.AreEqual(100, stats.MaxHealth);
    }

    [Test]
    public void RequiredExpForLevel_GrowsLinearly()
    {
        Assert.AreEqual(100, CharacterStats.RequiredExpForLevel(1));
        Assert.AreEqual(150, CharacterStats.RequiredExpForLevel(2));
        Assert.AreEqual(200, CharacterStats.RequiredExpForLevel(3));
    }

    [Test]
    public void AddExp_BelowThreshold_NoLevelUp()
    {
        var stats = new CharacterStats();

        int levelsGained = stats.AddExp(50);

        Assert.AreEqual(0, levelsGained);
        Assert.AreEqual(1, stats.Level);
        Assert.AreEqual(50, stats.CurrentExp);
    }

    [Test]
    public void AddExp_ExactlyAtThreshold_LevelsUpAndClearsExp()
    {
        var stats = new CharacterStats();

        int levelsGained = stats.AddExp(100);

        Assert.AreEqual(1, levelsGained);
        Assert.AreEqual(2, stats.Level);
        Assert.AreEqual(0, stats.CurrentExp);
        Assert.AreEqual(12, stats.Attack);
        Assert.AreEqual(115, stats.MaxHealth);
    }

    [Test]
    public void AddExp_EnoughForMultipleLevels_LevelsUpMultipleTimes()
    {
        var stats = new CharacterStats();

        int levelsGained = stats.AddExp(100 + 150 + 30);

        Assert.AreEqual(2, levelsGained);
        Assert.AreEqual(3, stats.Level);
        Assert.AreEqual(30, stats.CurrentExp);
    }

    [Test]
    public void AddExp_NegativeAmount_Throws()
    {
        var stats = new CharacterStats();

        Assert.Throws<ArgumentOutOfRangeException>(() => stats.AddExp(-1));
    }

    [Test]
    public void LoadState_RestoresLevelAndExp()
    {
        var stats = new CharacterStats();

        stats.LoadState(5, 42);

        Assert.AreEqual(5, stats.Level);
        Assert.AreEqual(42, stats.CurrentExp);
        Assert.AreEqual(18, stats.Attack);
    }

    [Test]
    public void LoadState_ExpExceedsRequirement_AutoLevelsUp()
    {
        var stats = new CharacterStats();

        stats.LoadState(1, 120);

        Assert.AreEqual(2, stats.Level);
        Assert.AreEqual(20, stats.CurrentExp);
        Assert.AreEqual(12, stats.Attack);
        Assert.AreEqual(115, stats.MaxHealth);
    }
}
