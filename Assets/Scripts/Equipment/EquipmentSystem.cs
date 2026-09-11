using System;
using System.Collections.Generic;
using UnityEngine;
using IdleGame.Core;
using IdleGame.Save;

namespace IdleGame.Equipment
{
    public class EquipmentSystem : MonoBehaviour
    {
        [SerializeField] private EquipmentBaseStatsTable baseStatsTable;

        private readonly Dictionary<EquipmentSlot, EquippedItem> _equipped = new Dictionary<EquipmentSlot, EquippedItem>();
        private readonly IRandomSource _random = new SystemRandomSource();

        public int TotalBonusAttack => Sum(item => item.TotalAttack);
        public int TotalBonusHealth => Sum(item => item.TotalHealth);

        public EquippedItem GetEquipped(EquipmentSlot slot)
        {
            return _equipped.TryGetValue(slot, out var item) ? item : null;
        }

        // 드롭된 장비는 인벤토리 없이 즉시 해당 슬롯에 장착한다 (MVP 단순화, 스펙 4절).
        public void RollDropOnKill(double dropChance)
        {
            if (!EquipmentDropRoller.TryRollDrop(dropChance, _random, out var grade)) return;

            var slot = EquipmentDropRoller.RollSlot(_random);
            Equip(slot, grade);
        }

        public void Equip(EquipmentSlot slot, EquipmentGrade grade)
        {
            var entry = baseStatsTable.Find(slot, grade);
            _equipped[slot] = new EquippedItem
            {
                slot = slot,
                grade = grade,
                enhanceLevel = 0,
                baseAttack = entry.bonusAttack,
                baseHealth = entry.bonusHealth
            };
            GameEvents.RaiseEquipmentDropped(slot, grade);
        }

        public void LoadState(List<EquipmentSaveEntry> savedItems)
        {
            if (savedItems == null) return;

            foreach (var saved in savedItems)
            {
                var slot = (EquipmentSlot)saved.slot;
                var grade = (EquipmentGrade)saved.grade;
                var entry = baseStatsTable.Find(slot, grade);
                _equipped[slot] = new EquippedItem
                {
                    slot = slot,
                    grade = grade,
                    enhanceLevel = saved.enhanceLevel,
                    baseAttack = entry.bonusAttack,
                    baseHealth = entry.bonusHealth
                };
            }
        }

        private int Sum(Func<EquippedItem, int> selector)
        {
            int total = 0;
            foreach (var item in _equipped.Values) total += selector(item);
            return total;
        }
    }
}
