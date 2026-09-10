using System;
using NUnit.Framework;
using IdleGame.Economy;

public class CurrencyWalletTests
{
    [Test]
    public void NewWallet_StartsAtZero()
    {
        var wallet = new CurrencyWallet();

        Assert.AreEqual(0, wallet.Gold);
        Assert.AreEqual(0, wallet.EnhanceStones);
    }

    [Test]
    public void AddGold_IncreasesBalance()
    {
        var wallet = new CurrencyWallet();

        wallet.AddGold(100);

        Assert.AreEqual(100, wallet.Gold);
    }

    [Test]
    public void AddGold_Negative_Throws()
    {
        var wallet = new CurrencyWallet();

        Assert.Throws<ArgumentOutOfRangeException>(() => wallet.AddGold(-1));
    }

    [Test]
    public void TrySpendGold_EnoughBalance_SpendsAndReturnsTrue()
    {
        var wallet = new CurrencyWallet();
        wallet.AddGold(100);

        bool spent = wallet.TrySpendGold(60);

        Assert.IsTrue(spent);
        Assert.AreEqual(40, wallet.Gold);
    }

    [Test]
    public void TrySpendGold_NotEnoughBalance_ReturnsFalseAndKeepsBalance()
    {
        var wallet = new CurrencyWallet();
        wallet.AddGold(10);

        bool spent = wallet.TrySpendGold(60);

        Assert.IsFalse(spent);
        Assert.AreEqual(10, wallet.Gold);
    }

    [Test]
    public void TrySpendEnhanceStones_EnoughBalance_SpendsAndReturnsTrue()
    {
        var wallet = new CurrencyWallet();
        wallet.AddEnhanceStones(20);

        bool spent = wallet.TrySpendEnhanceStones(5);

        Assert.IsTrue(spent);
        Assert.AreEqual(15, wallet.EnhanceStones);
    }

    [Test]
    public void LoadState_RestoresBalances()
    {
        var wallet = new CurrencyWallet();

        wallet.LoadState(gold: 500, enhanceStones: 12);

        Assert.AreEqual(500, wallet.Gold);
        Assert.AreEqual(12, wallet.EnhanceStones);
    }
}
