using System;
using NUnit.Framework;
using IdleGame.Enhance;
using IdleGame.Equipment;

public class EnhanceCalculatorTests
{
    private class FixedRandomSource : IRandomSource
    {
        private readonly double _value;
        public FixedRandomSource(double value) => _value = value;
        public double NextDouble() => _value;
    }

    [Test]
    public void SuccessRate_DecreasesWithLevelAndClampsToMinimum()
    {
        Assert.AreEqual(0.9, EnhanceCalculator.SuccessRate(0), 0.0001);
        Assert.AreEqual(0.7, EnhanceCalculator.SuccessRate(4), 0.0001);
        Assert.AreEqual(0.05, EnhanceCalculator.SuccessRate(100), 0.0001);
    }

    [Test]
    public void SuccessRate_NegativeLevel_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EnhanceCalculator.SuccessRate(-1));
    }

    [Test]
    public void CostForLevel_IncreasesLinearly()
    {
        Assert.AreEqual(10, EnhanceCalculator.CostForLevel(0));
        Assert.AreEqual(30, EnhanceCalculator.CostForLevel(4));
    }

    [Test]
    public void TryEnhance_RollBelowSuccessRate_ReturnsLevelPlusOne()
    {
        var random = new FixedRandomSource(0.0);

        int newLevel = EnhanceCalculator.TryEnhance(currentLevel: 2, random);

        Assert.AreEqual(3, newLevel);
    }

    [Test]
    public void TryEnhance_RollAtOrAboveSuccessRate_ReturnsLevelMinusOne()
    {
        var random = new FixedRandomSource(0.999);

        int newLevel = EnhanceCalculator.TryEnhance(currentLevel: 2, random);

        Assert.AreEqual(1, newLevel);
    }

    [Test]
    public void TryEnhance_FailAtLevelZero_StaysAtZero()
    {
        var random = new FixedRandomSource(0.999);

        int newLevel = EnhanceCalculator.TryEnhance(currentLevel: 0, random);

        Assert.AreEqual(0, newLevel);
    }
}
