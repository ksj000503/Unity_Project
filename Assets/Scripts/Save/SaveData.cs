using System;
using System.Collections.Generic;

namespace IdleGame.Save
{
    [Serializable]
    public class SaveData
    {
        public int level = 1;
        public int currentExp;
        public int gold;
        public int enhanceStones;
        public int stageIndex;
        public int killsInStage;
        public string lastSaveTimeUtc = "";
        public List<EquipmentSaveEntry> equippedItems = new List<EquipmentSaveEntry>();
    }

    [Serializable]
    public class EquipmentSaveEntry
    {
        public int slot;
        public int grade;
        public int enhanceLevel;
    }
}
