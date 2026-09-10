using System;
using NUnit.Framework;
using IdleGame.Equipment;

public class EquipmentDropRollerTests
{
    private class FixedRandomSource : IRandomSource
    {
        private readonly double _value;
        public FixedRandomSource(double value) => _value = value;
        public double NextDouble() => _value;
    }

    [Test]
    public void TryRollDrop_RollBelowChance_ReturnsTrue()
    {
        var random = new FixedRandomSource(0.1);

        bool dropped = EquipmentDropRoller.TryRollDrop(0.3, random, out _);

        Assert.IsTrue(dropped);
    }

    [Test]
    public void TryRollDrop_RollAtOrAboveChance_ReturnsFalse()
    {
        var random = new FixedRandomSource(0.3);

        bool dropped = EquipmentDropRoller.TryRollDrop(0.3, random, out var grade);

        Assert.IsFalse(dropped);
        Assert.AreEqual(default(EquipmentGrade), grade);
    }

    [Test]
    public void TryRollDrop_InvalidChance_Throws()
    {
        var random = new FixedRandomSource(0.1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => EquipmentDropRoller.TryRollDrop(1.1, random, out _));
    }

    [TestCase(0.0, EquipmentGrade.Common)]
    [TestCase(0.69, EquipmentGrade.Common)]
    [TestCase(0.70, EquipmentGrade.Rare)]
    [TestCase(0.89, EquipmentGrade.Rare)]
    [TestCase(0.90, EquipmentGrade.Epic)]
    [TestCase(0.97, EquipmentGrade.Epic)]
    [TestCase(0.98, EquipmentGrade.Legendary)]
    [TestCase(0.999, EquipmentGrade.Legendary)]
    public void TryRollDrop_GradeWeightBoundaries(double gradeRoll, EquipmentGrade expectedGrade)
    {
        // 첫 NextDouble() 호출은 드롭 여부, 두 번째 호출은 등급 결정에 쓰인다.
        var random = new SequenceRandomSource(0.0, gradeRoll);

        EquipmentDropRoller.TryRollDrop(1.0, random, out var grade);

        Assert.AreEqual(expectedGrade, grade);
    }

    private class SequenceRandomSource : IRandomSource
    {
        private readonly double[] _values;
        private int _index;
        public SequenceRandomSource(params double[] values) => _values = values;
        public double NextDouble() => _values[_index++];
    }

    [Test]
    public void RollSlot_ReturnsValueWithinEnumRange()
    {
        var random = new FixedRandomSource(0.99);

        EquipmentSlot slot = EquipmentDropRoller.RollSlot(random);

        Assert.IsTrue(Enum.IsDefined(typeof(EquipmentSlot), slot));
    }
}
