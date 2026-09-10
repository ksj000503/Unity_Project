using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using IdleGame.Save;

public class SaveSystemTests
{
    private string _tempFilePath;

    [SetUp]
    public void SetUp()
    {
        _tempFilePath = Path.Combine(Path.GetTempPath(), $"idlegame-save-test-{Path.GetRandomFileName()}.json");
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_tempFilePath)) File.Delete(_tempFilePath);
    }

    [Test]
    public void Load_NoFileExists_ReturnsNull()
    {
        var saveSystem = new SaveSystem(_tempFilePath);

        SaveData loaded = saveSystem.Load();

        Assert.IsNull(loaded);
    }

    [Test]
    public void Save_ThenLoad_RoundTripsAllFields()
    {
        var saveSystem = new SaveSystem(_tempFilePath);
        var data = new SaveData
        {
            level = 5,
            currentExp = 42,
            gold = 1000,
            enhanceStones = 7,
            stageIndex = 2,
            killsInStage = 3,
            lastSaveTimeUtc = "2026-09-02T00:00:00.0000000Z",
            equippedItems = new List<EquipmentSaveEntry>
            {
                new EquipmentSaveEntry { slot = 0, grade = 1, enhanceLevel = 3 }
            }
        };

        saveSystem.Save(data);
        SaveData loaded = saveSystem.Load();

        Assert.IsNotNull(loaded);
        Assert.AreEqual(5, loaded.level);
        Assert.AreEqual(42, loaded.currentExp);
        Assert.AreEqual(1000, loaded.gold);
        Assert.AreEqual(7, loaded.enhanceStones);
        Assert.AreEqual(2, loaded.stageIndex);
        Assert.AreEqual(3, loaded.killsInStage);
        Assert.AreEqual("2026-09-02T00:00:00.0000000Z", loaded.lastSaveTimeUtc);
        Assert.AreEqual(1, loaded.equippedItems.Count);
        Assert.AreEqual(0, loaded.equippedItems[0].slot);
        Assert.AreEqual(1, loaded.equippedItems[0].grade);
        Assert.AreEqual(3, loaded.equippedItems[0].enhanceLevel);
    }
}
