using System;

namespace IdleGame.Battle
{
    public class StageProgress
    {
        public int CurrentStageIndex { get; private set; }
        public int KillsInStage { get; private set; }

        public bool RegisterKill(int killsRequiredForStage)
        {
            if (killsRequiredForStage <= 0) throw new ArgumentOutOfRangeException(nameof(killsRequiredForStage));

            KillsInStage++;
            if (KillsInStage < killsRequiredForStage) return false;

            KillsInStage = 0;
            CurrentStageIndex++;
            return true;
        }

        public void LoadState(int stageIndex, int killsInStage)
        {
            if (stageIndex < 0) throw new ArgumentOutOfRangeException(nameof(stageIndex));
            if (killsInStage < 0) throw new ArgumentOutOfRangeException(nameof(killsInStage));

            CurrentStageIndex = stageIndex;
            KillsInStage = killsInStage;
        }
    }
}
