using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleGame.Equipment
{
    [Serializable]
    public class EquipmentBaseStatsEntry
    {
        public EquipmentSlot slot;
        public EquipmentGrade grade;
        public int bonusAttack;
        public int bonusHealth;
    }

    [CreateAssetMenu(fileName = "EquipmentBaseStatsTable", menuName = "IdleGame/Equipment Base Stats Table")]
    public class EquipmentBaseStatsTable : ScriptableObject
    {
        public List<EquipmentBaseStatsEntry> entries = new List<EquipmentBaseStatsEntry>();

        public EquipmentBaseStatsEntry Find(EquipmentSlot slot, EquipmentGrade grade)
        {
            foreach (var entry in entries)
            {
                if (entry.slot == slot && entry.grade == grade) return entry;
            }
            throw new InvalidOperationException($"No base stats entry for {slot}/{grade}");
        }
    }
}
