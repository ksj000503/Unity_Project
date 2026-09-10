using UnityEngine;

namespace IdleGame.Data
{
    [CreateAssetMenu(fileName = "MonsterData", menuName = "IdleGame/Monster Data")]
    public class MonsterData : ScriptableObject
    {
        public string monsterName = "Monster";
        public int maxHealth = 50;
        public int goldReward = 3;
        public int expReward = 10;
        [Range(0, 1)] public double dropChance = 0.3;
    }
}
