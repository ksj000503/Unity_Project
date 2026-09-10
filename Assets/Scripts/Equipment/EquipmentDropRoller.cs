using System;

namespace IdleGame.Equipment
{
    public static class EquipmentDropRoller
    {
        // 등급 가중치: 일반 70%, 희귀 20%, 영웅 8%, 전설 2%.
        private const double RareThreshold = 0.70;
        private const double EpicThreshold = 0.90;
        private const double LegendaryThreshold = 0.98;

        public static bool TryRollDrop(double dropChance, IRandomSource random, out EquipmentGrade grade)
        {
            if (dropChance < 0 || dropChance > 1) throw new ArgumentOutOfRangeException(nameof(dropChance));

            grade = default;
            if (random.NextDouble() >= dropChance) return false;

            grade = RollGrade(random);
            return true;
        }

        public static EquipmentSlot RollSlot(IRandomSource random)
        {
            var slots = (EquipmentSlot[])Enum.GetValues(typeof(EquipmentSlot));
            int index = (int)(random.NextDouble() * slots.Length);
            if (index >= slots.Length) index = slots.Length - 1;
            return slots[index];
        }

        private static EquipmentGrade RollGrade(IRandomSource random)
        {
            double roll = random.NextDouble();
            if (roll < RareThreshold) return EquipmentGrade.Common;
            if (roll < EpicThreshold) return EquipmentGrade.Rare;
            if (roll < LegendaryThreshold) return EquipmentGrade.Epic;
            return EquipmentGrade.Legendary;
        }
    }
}
