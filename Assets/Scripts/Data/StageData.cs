using UnityEngine;

namespace IdleGame.Data
{
    [CreateAssetMenu(fileName = "StageData", menuName = "IdleGame/Stage Data")]
    public class StageData : ScriptableObject
    {
        public string stageName = "Stage 1";
        public MonsterData monster;
        public int killsToAdvance = 10;
    }
}
