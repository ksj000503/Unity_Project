using System;
using NUnit.Framework;
using IdleGame.Battle;

public class StageProgressTests
{
    [Test]
    public void NewProgress_StartsAtStage0WithNoKills()
    {
        var progress = new StageProgress();

        Assert.AreEqual(0, progress.CurrentStageIndex);
        Assert.AreEqual(0, progress.KillsInStage);
    }

    [Test]
    public void RegisterKill_BelowTarget_IncrementsWithoutAdvancing()
    {
        var progress = new StageProgress();

        bool advanced = progress.RegisterKill(killsRequiredForStage: 3);

        Assert.IsFalse(advanced);
        Assert.AreEqual(1, progress.KillsInStage);
        Assert.AreEqual(0, progress.CurrentStageIndex);
    }

    [Test]
    public void RegisterKill_ReachesTarget_AdvancesStageAndResetsKills()
    {
        var progress = new StageProgress();
        progress.RegisterKill(3);
        progress.RegisterKill(3);

        bool advanced = progress.RegisterKill(3);

        Assert.IsTrue(advanced);
        Assert.AreEqual(1, progress.CurrentStageIndex);
        Assert.AreEqual(0, progress.KillsInStage);
    }

    [Test]
    public void RegisterKill_NonPositiveTarget_Throws()
    {
        var progress = new StageProgress();

        Assert.Throws<ArgumentOutOfRangeException>(() => progress.RegisterKill(0));
    }

    [Test]
    public void LoadState_RestoresStageAndKills()
    {
        var progress = new StageProgress();

        progress.LoadState(stageIndex: 4, killsInStage: 2);

        Assert.AreEqual(4, progress.CurrentStageIndex);
        Assert.AreEqual(2, progress.KillsInStage);
    }
}
