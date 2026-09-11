using UnityEngine;
using IdleGame.Battle;
using IdleGame.Character;
using IdleGame.Economy;

namespace IdleGame.Core
{
    public class GameManager : MonoBehaviour
    {
        // 스테이지 클리어 시 지급하는 강화석 보상 (스펙 6절: 강화석은 스테이지 클리어 보상으로 획득).
        private const int EnhanceStonesPerStageClear = 5;

        public CharacterStats CharacterStats { get; private set; }
        public StageProgress StageProgress { get; private set; }
        public CurrencyWallet Wallet { get; private set; }

        private void Awake()
        {
            CharacterStats = new CharacterStats();
            StageProgress = new StageProgress();
            Wallet = new CurrencyWallet();

            GameEvents.OnMonsterKilled += HandleMonsterKilled;
            GameEvents.OnStageAdvanced += HandleStageAdvanced;
        }

        private void OnDestroy()
        {
            GameEvents.OnMonsterKilled -= HandleMonsterKilled;
            GameEvents.OnStageAdvanced -= HandleStageAdvanced;
        }

        private void HandleMonsterKilled(int gold, int exp)
        {
            Wallet.AddGold(gold);
            int levelsGained = CharacterStats.AddExp(exp);
            if (levelsGained > 0) GameEvents.RaiseLevelUp(CharacterStats.Level);
        }

        private void HandleStageAdvanced(int stageIndex)
        {
            Wallet.AddEnhanceStones(EnhanceStonesPerStageClear);
        }
    }
}
