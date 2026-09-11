using System;
using IdleGame.Equipment;

namespace IdleGame.Core
{
    // 시스템 간 직접 참조 대신 이벤트로 느슨하게 연결하기 위한 전역 이벤트 버스.
    public static class GameEvents
    {
        public static event Action<int> OnLevelUp;
        public static event Action<EquipmentSlot, EquipmentGrade> OnEquipmentDropped;
        public static event Action<int, int> OnMonsterKilled;
        public static event Action<bool, int> OnEnhanceResult;
        public static event Action<int> OnStageAdvanced;

        public static void RaiseLevelUp(int level) => OnLevelUp?.Invoke(level);
        public static void RaiseEquipmentDropped(EquipmentSlot slot, EquipmentGrade grade) => OnEquipmentDropped?.Invoke(slot, grade);
        public static void RaiseMonsterKilled(int gold, int exp) => OnMonsterKilled?.Invoke(gold, exp);
        public static void RaiseEnhanceResult(bool success, int newLevel) => OnEnhanceResult?.Invoke(success, newLevel);
        public static void RaiseStageAdvanced(int stageIndex) => OnStageAdvanced?.Invoke(stageIndex);
    }
}
