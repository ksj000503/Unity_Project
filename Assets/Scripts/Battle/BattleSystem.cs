using UnityEngine;
using IdleGame.Character;
using IdleGame.Core;
using IdleGame.Data;
using IdleGame.Equipment;

namespace IdleGame.Battle
{
    public class BattleSystem : MonoBehaviour
    {
        [SerializeField] private StageData[] stages;
        [SerializeField] private float attacksPerSecond = 1f;
        [SerializeField] private EquipmentSystem equipmentSystem;

        private CharacterStats _characterStats;
        private StageProgress _stageProgress;
        private int _currentMonsterHealth;
        private float _attackTimer;

        private StageData CurrentStage => stages[_stageProgress.CurrentStageIndex];

        public void Initialize(CharacterStats characterStats, StageProgress stageProgress)
        {
            _characterStats = characterStats;
            _stageProgress = stageProgress;
            SpawnMonster();
        }

        private void SpawnMonster()
        {
            _currentMonsterHealth = CurrentStage.monster.maxHealth;
        }

        private void Update()
        {
            if (_characterStats == null) return;

            _attackTimer += Time.deltaTime;
            float interval = 1f / attacksPerSecond;
            while (_attackTimer >= interval)
            {
                _attackTimer -= interval;
                Attack();
            }
        }

        private void Attack()
        {
            int attack = _characterStats.Attack + equipmentSystem.TotalBonusAttack;
            _currentMonsterHealth -= attack;
            if (_currentMonsterHealth <= 0) HandleKill();
        }

        private void HandleKill()
        {
            var monster = CurrentStage.monster;
            GameEvents.RaiseMonsterKilled(monster.goldReward, monster.expReward);
            equipmentSystem.RollDropOnKill(monster.dropChance);

            bool advanced = _stageProgress.RegisterKill(CurrentStage.killsToAdvance);
            if (advanced) GameEvents.RaiseStageAdvanced(_stageProgress.CurrentStageIndex);

            SpawnMonster();
        }
    }
}
