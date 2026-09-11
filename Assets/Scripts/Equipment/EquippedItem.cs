namespace IdleGame.Equipment
{
    public class EquippedItem
    {
        // 강화 1단계당 붙는 고정 보너스. 밸런스 수치라 여기서만 상수로 관리한다.
        private const int EnhanceBonusPerLevel = 2;

        public EquipmentSlot slot;
        public EquipmentGrade grade;
        public int enhanceLevel;
        public int baseAttack;
        public int baseHealth;

        public int TotalAttack => baseAttack + enhanceLevel * EnhanceBonusPerLevel;
        public int TotalHealth => baseHealth + enhanceLevel * EnhanceBonusPerLevel;
    }
}
