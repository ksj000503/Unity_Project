# 방치형 게임(메이플키우기 모작) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Unity 2D 모바일 방치형 게임의 MVP(자동전투 + 장비 + 강화 + 오프라인 보상 + 로컬 저장 + 3탭 UI)를 처음부터 구현한다.

**Architecture:** GameManager가 BattleSystem/EquipmentSystem/EnhanceSystem/SaveSystem을 보유하고, 시스템 간 통신은 `GameEvents`(정적 C# 이벤트)로 한다. 데미지 계산, 레벨 곡선, 드롭 확률, 강화 확률, 오프라인 보상 계산은 UnityEngine에 의존하지 않는 순수 C# 클래스로 분리해 EditMode 유닛 테스트로 검증한다. 몬스터/스테이지/장비 기본 스탯은 ScriptableObject 데이터 에셋으로 관리한다.

**Tech Stack:** Unity 2D URP (기존 프로젝트), Unity Test Framework (EditMode/NUnit), Unity UI (uGUI, `UnityEngine.UI`), JsonUtility 기반 로컬 JSON 저장.

**Spec:** `docs/superpowers/specs/2026-09-02-idle-maple-clone-design.md`

## Global Constraints

- 코드 스타일: 단순하고 읽기 쉽게, 그러나 기능 생략·예외처리 누락 절대 금지 (project.txt / 스펙 2절).
- 불필요한 추상화·디자인패턴·미래대비 코드 금지 (YAGNI). 함수는 짧게, 스크립트는 한 가지 역할만.
- 주석은 '왜' 그렇게 했는지만 짧게. 뻔한 내용은 달지 않음.
- 여러 구현 가능성이 있으면 시간복잡도를 계산해 더 빠른 알고리즘을 채택한다.
- 백엔드/계정/결제/광고 없음 — 로컬 저장만 사용한다 (스펙 1, 11절).
- 순수 로직(전투 계산/레벨 곡선/드롭/강화/오프라인 보상)은 EditMode 유닛 테스트로 검증하고, MonoBehaviour/UI 연동부는 에디터에서 수동 확인한다 (스펙 10절).

---

## 파일 구조

```
Assets/
  Scripts/
    IdleGame.asmdef
    Character/
      CharacterStats.cs
    Battle/
      BattleMath.cs
      StageProgress.cs
      BattleSystem.cs
    Economy/
      CurrencyWallet.cs
    Equipment/
      EquipmentTypes.cs        (EquipmentSlot, EquipmentGrade enum)
      IRandomSource.cs
      SystemRandomSource.cs
      EquipmentDropRoller.cs
      EquippedItem.cs
      EquipmentBaseStatsTable.cs (ScriptableObject)
      EquipmentSystem.cs
    Enhance/
      EnhanceCalculator.cs
      EnhanceSystem.cs
    Offline/
      OfflineRewardCalculator.cs
    Save/
      SaveData.cs
      SaveSystem.cs
      SaveSystemBehaviour.cs
    Data/
      MonsterData.cs (ScriptableObject)
      StageData.cs (ScriptableObject)
    Core/
      GameEvents.cs
      GameManager.cs
    UI/
      TabBarController.cs
      BattleScreenUI.cs
      EquipmentScreenUI.cs
      EnhanceScreenUI.cs
  Tests/
    EditMode/
      IdleGame.Tests.asmdef
      CharacterStatsTests.cs
      BattleMathTests.cs
      StageProgressTests.cs
      CurrencyWalletTests.cs
      EquipmentDropRollerTests.cs
      EnhanceCalculatorTests.cs
      OfflineRewardCalculatorTests.cs
      SaveSystemTests.cs
```

---

### Task 1: Git 저장소 초기화 + Unity .gitignore

**Files:**
- Create: `.gitignore`

**Interfaces:** 없음 (저장소 설정 작업).

- [ ] **Step 1: 저장소 초기화**

Run: `git init`

- [ ] **Step 2: Unity용 .gitignore 작성**

`.gitignore`:
```gitignore
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Mm]emoryCaptures/
[Uu]serSettings/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db
.vs/
sysinfo.txt
*.apk
*.aab
*.unitypackage
*.app
crashlytics-build.properties
```

- [ ] **Step 3: 최초 커밋**

```bash
git add .gitignore project.txt docs
git commit -m "chore: init repo with gitignore and design docs"
```

---

### Task 2: 스크립트/테스트 어셈블리 설정

**Files:**
- Create: `Assets/Scripts/IdleGame.asmdef`
- Create: `Assets/Tests/EditMode/IdleGame.Tests.asmdef`

**Interfaces:**
- Produces: 이후 모든 스크립트가 속할 `IdleGame` 어셈블리, 테스트가 속할 `IdleGame.Tests` 어셈블리.

- [ ] **Step 1: 게임 코드 어셈블리 정의 작성**

`Assets/Scripts/IdleGame.asmdef`:
```json
{
    "name": "IdleGame",
    "rootNamespace": "IdleGame",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 2: 테스트 어셈블리 정의 작성**

`Assets/Tests/EditMode/IdleGame.Tests.asmdef`:
```json
{
    "name": "IdleGame.Tests",
    "rootNamespace": "IdleGame.Tests",
    "references": [
        "IdleGame",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": true,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 3: Unity 에디터에서 컴파일 확인**

Unity 에디터를 열고(또는 이미 열려 있으면 포커스), 콘솔에 컴파일 에러가 없는지 확인한다. `Window > General > Test Runner`를 열어 `EditMode` 탭에 `IdleGame.Tests` 어셈블리가 보이는지 확인한다 (테스트 0개, 에러 없음).

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scripts/IdleGame.asmdef Assets/Scripts/IdleGame.asmdef.meta Assets/Tests
git commit -m "chore: set up IdleGame script and test assemblies"
```

---

### Task 3: CharacterStats (레벨/경험치/스탯 공식)

**Files:**
- Create: `Assets/Scripts/Character/CharacterStats.cs`
- Test: `Assets/Tests/EditMode/CharacterStatsTests.cs`

**Interfaces:**
- Produces: `class CharacterStats { int Level; int CurrentExp; int Attack; int MaxHealth; static int RequiredExpForLevel(int level); int AddExp(int amount) -> int levelsGained; void LoadState(int level, int currentExp); }`

- [ ] **Step 1: 실패하는 테스트 작성**

`Assets/Tests/EditMode/CharacterStatsTests.cs`:
```csharp
using System;
using NUnit.Framework;
using IdleGame.Character;

public class CharacterStatsTests
{
    [Test]
    public void NewCharacter_StartsAtLevel1WithBaseStats()
    {
        var stats = new CharacterStats();

        Assert.AreEqual(1, stats.Level);
        Assert.AreEqual(0, stats.CurrentExp);
        Assert.AreEqual(10, stats.Attack);
        Assert.AreEqual(100, stats.MaxHealth);
    }

    [Test]
    public void RequiredExpForLevel_GrowsLinearly()
    {
        Assert.AreEqual(100, CharacterStats.RequiredExpForLevel(1));
        Assert.AreEqual(150, CharacterStats.RequiredExpForLevel(2));
        Assert.AreEqual(200, CharacterStats.RequiredExpForLevel(3));
    }

    [Test]
    public void AddExp_BelowThreshold_NoLevelUp()
    {
        var stats = new CharacterStats();

        int levelsGained = stats.AddExp(50);

        Assert.AreEqual(0, levelsGained);
        Assert.AreEqual(1, stats.Level);
        Assert.AreEqual(50, stats.CurrentExp);
    }

    [Test]
    public void AddExp_ExactlyAtThreshold_LevelsUpAndClearsExp()
    {
        var stats = new CharacterStats();

        int levelsGained = stats.AddExp(100);

        Assert.AreEqual(1, levelsGained);
        Assert.AreEqual(2, stats.Level);
        Assert.AreEqual(0, stats.CurrentExp);
        Assert.AreEqual(12, stats.Attack);
        Assert.AreEqual(115, stats.MaxHealth);
    }

    [Test]
    public void AddExp_EnoughForMultipleLevels_LevelsUpMultipleTimes()
    {
        var stats = new CharacterStats();

        int levelsGained = stats.AddExp(100 + 150 + 30);

        Assert.AreEqual(2, levelsGained);
        Assert.AreEqual(3, stats.Level);
        Assert.AreEqual(30, stats.CurrentExp);
    }

    [Test]
    public void AddExp_NegativeAmount_Throws()
    {
        var stats = new CharacterStats();

        Assert.Throws<ArgumentOutOfRangeException>(() => stats.AddExp(-1));
    }

    [Test]
    public void LoadState_RestoresLevelAndExp()
    {
        var stats = new CharacterStats();

        stats.LoadState(5, 42);

        Assert.AreEqual(5, stats.Level);
        Assert.AreEqual(42, stats.CurrentExp);
        Assert.AreEqual(18, stats.Attack);
    }
}
```

- [ ] **Step 2: 테스트 실패 확인**

Unity Test Runner의 EditMode 탭에서 `CharacterStatsTests`를 실행한다.
Expected: FAIL (컴파일 에러 — `IdleGame.Character` 네임스페이스/`CharacterStats` 타입 없음).

- [ ] **Step 3: 최소 구현 작성**

`Assets/Scripts/Character/CharacterStats.cs`:
```csharp
using System;

namespace IdleGame.Character
{
    public class CharacterStats
    {
        private const int BaseAttack = 10;
        private const int BaseHealth = 100;
        private const int AttackPerLevel = 2;
        private const int HealthPerLevel = 15;
        private const int BaseExpToLevel = 100;
        private const int ExpToLevelIncreasePerLevel = 50;

        public int Level { get; private set; } = 1;
        public int CurrentExp { get; private set; }

        public int Attack => BaseAttack + (Level - 1) * AttackPerLevel;
        public int MaxHealth => BaseHealth + (Level - 1) * HealthPerLevel;

        public static int RequiredExpForLevel(int level)
        {
            return BaseExpToLevel + (level - 1) * ExpToLevelIncreasePerLevel;
        }

        // 여러 레벨을 한 번에 넘기는 경험치도 while로 정확히 처리해야 해서 루프를 쓴다.
        public int AddExp(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));

            CurrentExp += amount;
            int levelsGained = 0;
            while (CurrentExp >= RequiredExpForLevel(Level))
            {
                CurrentExp -= RequiredExpForLevel(Level);
                Level++;
                levelsGained++;
            }
            return levelsGained;
        }

        public void LoadState(int level, int currentExp)
        {
            if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
            if (currentExp < 0) throw new ArgumentOutOfRangeException(nameof(currentExp));

            Level = level;
            CurrentExp = currentExp;
        }
    }
}
```

- [ ] **Step 4: 테스트 통과 확인**

Test Runner에서 `CharacterStatsTests` 재실행.
Expected: PASS (7/7).

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Character Assets/Tests/EditMode/CharacterStatsTests.cs
git commit -m "feat: add CharacterStats level/exp curve"
```

---

### Task 4: BattleMath (처치 시간 계산)

**Files:**
- Create: `Assets/Scripts/Battle/BattleMath.cs`
- Test: `Assets/Tests/EditMode/BattleMathTests.cs`

**Interfaces:**
- Produces: `static class BattleMath { static float TimeToKillSeconds(int attack, float attacksPerSecond, int monsterHp); }`
- Consumes: 없음 (순수 함수).

- [ ] **Step 1: 실패하는 테스트 작성**

`Assets/Tests/EditMode/BattleMathTests.cs`:
```csharp
using System;
using NUnit.Framework;
using IdleGame.Battle;

public class BattleMathTests
{
    [Test]
    public void TimeToKillSeconds_ExactMultiple_ReturnsExpectedSeconds()
    {
        // 공격력 10, 초당 1회 공격, 몬스터 체력 50 -> 5회 타격 필요 -> 5초
        float time = BattleMath.TimeToKillSeconds(attack: 10, attacksPerSecond: 1f, monsterHp: 50);

        Assert.AreEqual(5f, time, 0.0001f);
    }

    [Test]
    public void TimeToKillSeconds_NonExactMultiple_RoundsUpHits()
    {
        // 공격력 10, 초당 2회 공격, 몬스터 체력 45 -> 5회 타격 필요(올림) -> 2.5초
        float time = BattleMath.TimeToKillSeconds(attack: 10, attacksPerSecond: 2f, monsterHp: 45);

        Assert.AreEqual(2.5f, time, 0.0001f);
    }

    [Test]
    public void TimeToKillSeconds_NonPositiveAttack_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BattleMath.TimeToKillSeconds(0, 1f, 10));
    }

    [Test]
    public void TimeToKillSeconds_NonPositiveAttacksPerSecond_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BattleMath.TimeToKillSeconds(10, 0f, 10));
    }

    [Test]
    public void TimeToKillSeconds_NonPositiveMonsterHp_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BattleMath.TimeToKillSeconds(10, 1f, 0));
    }
}
```

- [ ] **Step 2: 테스트 실패 확인**

Expected: FAIL (컴파일 에러 — `BattleMath` 없음).

- [ ] **Step 3: 최소 구현 작성**

`Assets/Scripts/Battle/BattleMath.cs`:
```csharp
using System;

namespace IdleGame.Battle
{
    public static class BattleMath
    {
        // 몬스터 체력을 공격력으로 나눠 필요한 타격 횟수를 올림 계산하고,
        // 공격속도로 나눠 처치까지 걸리는 시간을 구한다. (오프라인 보상 계산에도 재사용)
        public static float TimeToKillSeconds(int attack, float attacksPerSecond, int monsterHp)
        {
            if (attack <= 0) throw new ArgumentOutOfRangeException(nameof(attack));
            if (attacksPerSecond <= 0) throw new ArgumentOutOfRangeException(nameof(attacksPerSecond));
            if (monsterHp <= 0) throw new ArgumentOutOfRangeException(nameof(monsterHp));

            int hitsToKill = (int)Math.Ceiling((double)monsterHp / attack);
            return hitsToKill / attacksPerSecond;
        }
    }
}
```

- [ ] **Step 4: 테스트 통과 확인**

Expected: PASS (5/5).

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Battle/BattleMath.cs Assets/Tests/EditMode/BattleMathTests.cs
git commit -m "feat: add BattleMath time-to-kill calculation"
```

---

### Task 5: StageProgress (스테이지 진행 로직)

**Files:**
- Create: `Assets/Scripts/Battle/StageProgress.cs`
- Test: `Assets/Tests/EditMode/StageProgressTests.cs`

**Interfaces:**
- Produces: `class StageProgress { int CurrentStageIndex; int KillsInStage; bool RegisterKill(int killsRequiredForStage); void LoadState(int stageIndex, int killsInStage); }`

- [ ] **Step 1: 실패하는 테스트 작성**

`Assets/Tests/EditMode/StageProgressTests.cs`:
```csharp
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
```

- [ ] **Step 2: 테스트 실패 확인**

Expected: FAIL (컴파일 에러 — `StageProgress` 없음).

- [ ] **Step 3: 최소 구현 작성**

`Assets/Scripts/Battle/StageProgress.cs`:
```csharp
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
```

- [ ] **Step 4: 테스트 통과 확인**

Expected: PASS (5/5).

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Battle/StageProgress.cs Assets/Tests/EditMode/StageProgressTests.cs
git commit -m "feat: add StageProgress stage advancement logic"
```

---

### Task 6: CurrencyWallet (재화 지갑)

**Files:**
- Create: `Assets/Scripts/Economy/CurrencyWallet.cs`
- Test: `Assets/Tests/EditMode/CurrencyWalletTests.cs`

**Interfaces:**
- Produces: `class CurrencyWallet { int Gold; int EnhanceStones; void AddGold(int amount); void AddEnhanceStones(int amount); bool TrySpendGold(int amount); bool TrySpendEnhanceStones(int amount); void LoadState(int gold, int enhanceStones); }`

- [ ] **Step 1: 실패하는 테스트 작성**

`Assets/Tests/EditMode/CurrencyWalletTests.cs`:
```csharp
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
```

- [ ] **Step 2: 테스트 실패 확인**

Expected: FAIL (컴파일 에러 — `CurrencyWallet` 없음).

- [ ] **Step 3: 최소 구현 작성**

`Assets/Scripts/Economy/CurrencyWallet.cs`:
```csharp
using System;

namespace IdleGame.Economy
{
    public class CurrencyWallet
    {
        public int Gold { get; private set; }
        public int EnhanceStones { get; private set; }

        public void AddGold(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Gold += amount;
        }

        public void AddEnhanceStones(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            EnhanceStones += amount;
        }

        public bool TrySpendGold(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (Gold < amount) return false;
            Gold -= amount;
            return true;
        }

        public bool TrySpendEnhanceStones(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (EnhanceStones < amount) return false;
            EnhanceStones -= amount;
            return true;
        }

        public void LoadState(int gold, int enhanceStones)
        {
            if (gold < 0) throw new ArgumentOutOfRangeException(nameof(gold));
            if (enhanceStones < 0) throw new ArgumentOutOfRangeException(nameof(enhanceStones));
            Gold = gold;
            EnhanceStones = enhanceStones;
        }
    }
}
```

- [ ] **Step 4: 테스트 통과 확인**

Expected: PASS (7/7).

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Economy Assets/Tests/EditMode/CurrencyWalletTests.cs
git commit -m "feat: add CurrencyWallet gold/enhance-stone balances"
```

---

### Task 7: 장비 타입 + EquipmentDropRoller (드롭 확률 로직)

**Files:**
- Create: `Assets/Scripts/Equipment/EquipmentTypes.cs`
- Create: `Assets/Scripts/Equipment/IRandomSource.cs`
- Create: `Assets/Scripts/Equipment/SystemRandomSource.cs`
- Create: `Assets/Scripts/Equipment/EquipmentDropRoller.cs`
- Test: `Assets/Tests/EditMode/EquipmentDropRollerTests.cs`

**Interfaces:**
- Produces:
  - `enum EquipmentSlot { Weapon, Hat, Clothes, Accessory }`
  - `enum EquipmentGrade { Common, Rare, Epic, Legendary }`
  - `interface IRandomSource { double NextDouble(); }`
  - `class SystemRandomSource : IRandomSource`
  - `static class EquipmentDropRoller { static bool TryRollDrop(double dropChance, IRandomSource random, out EquipmentGrade grade); static EquipmentSlot RollSlot(IRandomSource random); }`

- [ ] **Step 1: 실패하는 테스트 작성**

테스트는 결정론적인 가짜 랜덤 소스를 사용해 경계값을 검증한다.

`Assets/Tests/EditMode/EquipmentDropRollerTests.cs`:
```csharp
using System;
using NUnit.Framework;
using IdleGame.Equipment;

public class EquipmentDropRollerTests
{
    private class FixedRandomSource : IRandomSource
    {
        private readonly double _value;
        public FixedRandomSource(double value) => _value = value;
        public double NextDouble() => _value;
    }

    [Test]
    public void TryRollDrop_RollBelowChance_ReturnsTrue()
    {
        var random = new FixedRandomSource(0.1);

        bool dropped = EquipmentDropRoller.TryRollDrop(0.3, random, out _);

        Assert.IsTrue(dropped);
    }

    [Test]
    public void TryRollDrop_RollAtOrAboveChance_ReturnsFalse()
    {
        var random = new FixedRandomSource(0.3);

        bool dropped = EquipmentDropRoller.TryRollDrop(0.3, random, out var grade);

        Assert.IsFalse(dropped);
        Assert.AreEqual(default(EquipmentGrade), grade);
    }

    [Test]
    public void TryRollDrop_InvalidChance_Throws()
    {
        var random = new FixedRandomSource(0.1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => EquipmentDropRoller.TryRollDrop(1.1, random, out _));
    }

    [TestCase(0.0, EquipmentGrade.Common)]
    [TestCase(0.69, EquipmentGrade.Common)]
    [TestCase(0.70, EquipmentGrade.Rare)]
    [TestCase(0.89, EquipmentGrade.Rare)]
    [TestCase(0.90, EquipmentGrade.Epic)]
    [TestCase(0.97, EquipmentGrade.Epic)]
    [TestCase(0.98, EquipmentGrade.Legendary)]
    [TestCase(0.999, EquipmentGrade.Legendary)]
    public void TryRollDrop_GradeWeightBoundaries(double gradeRoll, EquipmentGrade expectedGrade)
    {
        // 첫 NextDouble() 호출은 드롭 여부, 두 번째 호출은 등급 결정에 쓰인다.
        var random = new SequenceRandomSource(0.0, gradeRoll);

        EquipmentDropRoller.TryRollDrop(1.0, random, out var grade);

        Assert.AreEqual(expectedGrade, grade);
    }

    private class SequenceRandomSource : IRandomSource
    {
        private readonly double[] _values;
        private int _index;
        public SequenceRandomSource(params double[] values) => _values = values;
        public double NextDouble() => _values[_index++];
    }

    [Test]
    public void RollSlot_ReturnsValueWithinEnumRange()
    {
        var random = new FixedRandomSource(0.99);

        EquipmentSlot slot = EquipmentDropRoller.RollSlot(random);

        Assert.IsTrue(Enum.IsDefined(typeof(EquipmentSlot), slot));
    }
}
```

- [ ] **Step 2: 테스트 실패 확인**

Expected: FAIL (컴파일 에러 — `IdleGame.Equipment` 타입들 없음).

- [ ] **Step 3: 최소 구현 작성**

`Assets/Scripts/Equipment/EquipmentTypes.cs`:
```csharp
namespace IdleGame.Equipment
{
    public enum EquipmentSlot
    {
        Weapon,
        Hat,
        Clothes,
        Accessory
    }

    public enum EquipmentGrade
    {
        Common,
        Rare,
        Epic,
        Legendary
    }
}
```

`Assets/Scripts/Equipment/IRandomSource.cs`:
```csharp
namespace IdleGame.Equipment
{
    // 강화/드롭 로직에서 랜덤을 주입 가능하게 만들어 결정론적 유닛 테스트를 가능하게 한다.
    public interface IRandomSource
    {
        double NextDouble();
    }
}
```

`Assets/Scripts/Equipment/SystemRandomSource.cs`:
```csharp
using System;

namespace IdleGame.Equipment
{
    public class SystemRandomSource : IRandomSource
    {
        private readonly Random _random = new Random();

        public double NextDouble() => _random.NextDouble();
    }
}
```

`Assets/Scripts/Equipment/EquipmentDropRoller.cs`:
```csharp
using System;

namespace IdleGame.Equipment
{
    public static class EquipmentDropRoller
    {
        // 등급 가중치: 일반 70%, 희귀 20%, 영웅 8%, 전설 2%.
        private const double RareThreshold = 0.70;
        private const double EpicThreshold = 0.90;
        private const double LegendaryThreshold = 0.98;

        public static bool TryRollDrop(double dropChance, IRandomSource random, out EquipmentGrade grade)
        {
            if (dropChance < 0 || dropChance > 1) throw new ArgumentOutOfRangeException(nameof(dropChance));

            grade = default;
            if (random.NextDouble() >= dropChance) return false;

            grade = RollGrade(random);
            return true;
        }

        public static EquipmentSlot RollSlot(IRandomSource random)
        {
            var slots = (EquipmentSlot[])Enum.GetValues(typeof(EquipmentSlot));
            int index = (int)(random.NextDouble() * slots.Length);
            if (index >= slots.Length) index = slots.Length - 1;
            return slots[index];
        }

        private static EquipmentGrade RollGrade(IRandomSource random)
        {
            double roll = random.NextDouble();
            if (roll < RareThreshold) return EquipmentGrade.Common;
            if (roll < EpicThreshold) return EquipmentGrade.Rare;
            if (roll < LegendaryThreshold) return EquipmentGrade.Epic;
            return EquipmentGrade.Legendary;
        }
    }
}
```

- [ ] **Step 4: 테스트 통과 확인**

Expected: PASS (11/11).

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Equipment/EquipmentTypes.cs Assets/Scripts/Equipment/IRandomSource.cs Assets/Scripts/Equipment/SystemRandomSource.cs Assets/Scripts/Equipment/EquipmentDropRoller.cs Assets/Tests/EditMode/EquipmentDropRollerTests.cs
git commit -m "feat: add equipment drop chance and grade roll logic"
```

---

### Task 8: EnhanceCalculator (강화 확률/비용 로직)

**Files:**
- Create: `Assets/Scripts/Enhance/EnhanceCalculator.cs`
- Test: `Assets/Tests/EditMode/EnhanceCalculatorTests.cs`

**Interfaces:**
- Consumes: `IRandomSource` (Task 7).
- Produces: `static class EnhanceCalculator { static double SuccessRate(int currentLevel); static int CostForLevel(int currentLevel); static int TryEnhance(int currentLevel, IRandomSource random); }`

- [ ] **Step 1: 실패하는 테스트 작성**

`Assets/Tests/EditMode/EnhanceCalculatorTests.cs`:
```csharp
using System;
using NUnit.Framework;
using IdleGame.Enhance;
using IdleGame.Equipment;

public class EnhanceCalculatorTests
{
    private class FixedRandomSource : IRandomSource
    {
        private readonly double _value;
        public FixedRandomSource(double value) => _value = value;
        public double NextDouble() => _value;
    }

    [Test]
    public void SuccessRate_DecreasesWithLevelAndClampsToMinimum()
    {
        Assert.AreEqual(0.9, EnhanceCalculator.SuccessRate(0), 0.0001);
        Assert.AreEqual(0.7, EnhanceCalculator.SuccessRate(4), 0.0001);
        Assert.AreEqual(0.05, EnhanceCalculator.SuccessRate(100), 0.0001);
    }

    [Test]
    public void SuccessRate_NegativeLevel_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EnhanceCalculator.SuccessRate(-1));
    }

    [Test]
    public void CostForLevel_IncreasesLinearly()
    {
        Assert.AreEqual(10, EnhanceCalculator.CostForLevel(0));
        Assert.AreEqual(30, EnhanceCalculator.CostForLevel(4));
    }

    [Test]
    public void TryEnhance_RollBelowSuccessRate_ReturnsLevelPlusOne()
    {
        var random = new FixedRandomSource(0.0);

        int newLevel = EnhanceCalculator.TryEnhance(currentLevel: 2, random);

        Assert.AreEqual(3, newLevel);
    }

    [Test]
    public void TryEnhance_RollAtOrAboveSuccessRate_ReturnsLevelMinusOne()
    {
        var random = new FixedRandomSource(0.999);

        int newLevel = EnhanceCalculator.TryEnhance(currentLevel: 2, random);

        Assert.AreEqual(1, newLevel);
    }

    [Test]
    public void TryEnhance_FailAtLevelZero_StaysAtZero()
    {
        var random = new FixedRandomSource(0.999);

        int newLevel = EnhanceCalculator.TryEnhance(currentLevel: 0, random);

        Assert.AreEqual(0, newLevel);
    }
}
```

- [ ] **Step 2: 테스트 실패 확인**

Expected: FAIL (컴파일 에러 — `EnhanceCalculator` 없음).

- [ ] **Step 3: 최소 구현 작성**

`Assets/Scripts/Enhance/EnhanceCalculator.cs`:
```csharp
using System;
using IdleGame.Equipment;

namespace IdleGame.Enhance
{
    public static class EnhanceCalculator
    {
        private const double BaseSuccessRate = 0.9;
        private const double SuccessRateDropPerLevel = 0.05;
        private const double MinSuccessRate = 0.05;
        private const int BaseEnhanceCost = 10;
        private const int CostIncreasePerLevel = 5;

        public static double SuccessRate(int currentLevel)
        {
            if (currentLevel < 0) throw new ArgumentOutOfRangeException(nameof(currentLevel));
            double rate = BaseSuccessRate - currentLevel * SuccessRateDropPerLevel;
            return Math.Max(rate, MinSuccessRate);
        }

        public static int CostForLevel(int currentLevel)
        {
            if (currentLevel < 0) throw new ArgumentOutOfRangeException(nameof(currentLevel));
            return BaseEnhanceCost + currentLevel * CostIncreasePerLevel;
        }

        // 실패해도 장비가 파괴되지 않고 한 단계만 하락한다 (스펙 5절).
        public static int TryEnhance(int currentLevel, IRandomSource random)
        {
            if (currentLevel < 0) throw new ArgumentOutOfRangeException(nameof(currentLevel));

            bool success = random.NextDouble() < SuccessRate(currentLevel);
            if (success) return currentLevel + 1;
            return Math.Max(0, currentLevel - 1);
        }
    }
}
```

- [ ] **Step 4: 테스트 통과 확인**

Expected: PASS (6/6).

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Enhance/EnhanceCalculator.cs Assets/Tests/EditMode/EnhanceCalculatorTests.cs
git commit -m "feat: add EnhanceCalculator success rate and cost logic"
```

---

### Task 9: OfflineRewardCalculator (오프라인 보상 계산)

**Files:**
- Create: `Assets/Scripts/Offline/OfflineRewardCalculator.cs`
- Test: `Assets/Tests/EditMode/OfflineRewardCalculatorTests.cs`

**Interfaces:**
- Consumes: `BattleMath.TimeToKillSeconds(int, float, int) -> float` (Task 4).
- Produces: `static class OfflineRewardCalculator { const int MaxOfflineSeconds; static (int gold, int exp) CalculateReward(int characterAttack, float attacksPerSecond, int monsterHp, int monsterGold, int monsterExp, double elapsedSeconds); }`

- [ ] **Step 1: 실패하는 테스트 작성**

`Assets/Tests/EditMode/OfflineRewardCalculatorTests.cs`:
```csharp
using NUnit.Framework;
using IdleGame.Offline;

public class OfflineRewardCalculatorTests
{
    [Test]
    public void CalculateReward_WithinCap_UsesFullElapsedTime()
    {
        // TimeToKill = ceil(50/10)/1 = 5초. 50초 경과 -> 10킬.
        var (gold, exp) = OfflineRewardCalculator.CalculateReward(
            characterAttack: 10, attacksPerSecond: 1f, monsterHp: 50,
            monsterGold: 3, monsterExp: 2, elapsedSeconds: 50);

        Assert.AreEqual(30, gold);
        Assert.AreEqual(20, exp);
    }

    [Test]
    public void CalculateReward_BeyondCap_ClampsToMaxOfflineSeconds()
    {
        double farBeyondCap = OfflineRewardCalculator.MaxOfflineSeconds + 10_000;

        var (gold, _) = OfflineRewardCalculator.CalculateReward(
            characterAttack: 10, attacksPerSecond: 1f, monsterHp: 50,
            monsterGold: 3, monsterExp: 2, elapsedSeconds: farBeyondCap);

        var (cappedGold, _) = OfflineRewardCalculator.CalculateReward(
            characterAttack: 10, attacksPerSecond: 1f, monsterHp: 50,
            monsterGold: 3, monsterExp: 2, elapsedSeconds: OfflineRewardCalculator.MaxOfflineSeconds);

        Assert.AreEqual(cappedGold, gold);
    }

    [Test]
    public void CalculateReward_ZeroOrNegativeElapsed_ReturnsZero()
    {
        var (gold, exp) = OfflineRewardCalculator.CalculateReward(
            characterAttack: 10, attacksPerSecond: 1f, monsterHp: 50,
            monsterGold: 3, monsterExp: 2, elapsedSeconds: 0);

        Assert.AreEqual(0, gold);
        Assert.AreEqual(0, exp);
    }
}
```

- [ ] **Step 2: 테스트 실패 확인**

Expected: FAIL (컴파일 에러 — `OfflineRewardCalculator` 없음).

- [ ] **Step 3: 최소 구현 작성**

`Assets/Scripts/Offline/OfflineRewardCalculator.cs`:
```csharp
using System;
using IdleGame.Battle;

namespace IdleGame.Offline
{
    public static class OfflineRewardCalculator
    {
        // 스펙 7절: 오프라인 보상은 최대 8시간까지만 인정한다.
        public const int MaxOfflineSeconds = 8 * 60 * 60;

        public static (int gold, int exp) CalculateReward(
            int characterAttack, float attacksPerSecond, int monsterHp,
            int monsterGold, int monsterExp, double elapsedSeconds)
        {
            if (elapsedSeconds <= 0) return (0, 0);

            double cappedSeconds = Math.Min(elapsedSeconds, MaxOfflineSeconds);
            float timeToKill = BattleMath.TimeToKillSeconds(characterAttack, attacksPerSecond, monsterHp);
            int kills = (int)(cappedSeconds / timeToKill);

            return (kills * monsterGold, kills * monsterExp);
        }
    }
}
```

- [ ] **Step 4: 테스트 통과 확인**

Expected: PASS (3/3).

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Offline/OfflineRewardCalculator.cs Assets/Tests/EditMode/OfflineRewardCalculatorTests.cs
git commit -m "feat: add OfflineRewardCalculator capped offline progress"
```

---

### Task 10: SaveData + SaveSystem (JSON 저장/로드)

**Files:**
- Create: `Assets/Scripts/Save/SaveData.cs`
- Create: `Assets/Scripts/Save/SaveSystem.cs`
- Test: `Assets/Tests/EditMode/SaveSystemTests.cs`

**Interfaces:**
- Consumes: `EquipmentSlot`, `EquipmentGrade` (Task 7 — 저장 항목에 사용).
- Produces: `class SaveData { int level; int currentExp; int gold; int enhanceStones; int stageIndex; int killsInStage; string lastSaveTimeUtc; List<EquipmentSaveEntry> equippedItems; }`, `class EquipmentSaveEntry { int slot; int grade; int enhanceLevel; }`, `class SaveSystem { SaveSystem(string filePath); void Save(SaveData data); SaveData Load(); }`

- [ ] **Step 1: 실패하는 테스트 작성**

`Assets/Tests/EditMode/SaveSystemTests.cs`:
```csharp
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
```

- [ ] **Step 2: 테스트 실패 확인**

Expected: FAIL (컴파일 에러 — `SaveData`/`SaveSystem` 없음).

- [ ] **Step 3: 최소 구현 작성**

`Assets/Scripts/Save/SaveData.cs`:
```csharp
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
```

`Assets/Scripts/Save/SaveSystem.cs`:
```csharp
using System.IO;
using UnityEngine;

namespace IdleGame.Save
{
    public class SaveSystem
    {
        private readonly string _filePath;

        public SaveSystem(string filePath)
        {
            _filePath = filePath;
        }

        public void Save(SaveData data)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_filePath, json);
        }

        public SaveData Load()
        {
            if (!File.Exists(_filePath)) return null;
            string json = File.ReadAllText(_filePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
    }
}
```

- [ ] **Step 4: 테스트 통과 확인**

Expected: PASS (2/2).

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Save Assets/Tests/EditMode/SaveSystemTests.cs
git commit -m "feat: add local JSON SaveData/SaveSystem"
```

---

### Task 11: ScriptableObject 데이터 정의 + 샘플 데이터 에셋

**Files:**
- Create: `Assets/Scripts/Data/MonsterData.cs`
- Create: `Assets/Scripts/Data/StageData.cs`
- Create: `Assets/Scripts/Equipment/EquipmentBaseStatsTable.cs`
- Create (Unity 에디터에서 생성, 코드 아님): `Assets/Data/Monsters/Stage1Monster.asset`, `Assets/Data/Stages/Stage1.asset`, `Assets/Data/Equipment/EquipmentBaseStatsTable.asset`

**Interfaces:**
- Consumes: `EquipmentSlot`, `EquipmentGrade` (Task 7).
- Produces: `class MonsterData : ScriptableObject { string monsterName; int maxHealth; int goldReward; int expReward; double dropChance; }`, `class StageData : ScriptableObject { string stageName; MonsterData monster; int killsToAdvance; }`, `class EquipmentBaseStatsTable : ScriptableObject { EquipmentBaseStatsEntry Find(EquipmentSlot, EquipmentGrade); }`

- [ ] **Step 1: MonsterData 작성**

`Assets/Scripts/Data/MonsterData.cs`:
```csharp
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
```

- [ ] **Step 2: StageData 작성**

`Assets/Scripts/Data/StageData.cs`:
```csharp
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
```

- [ ] **Step 3: EquipmentBaseStatsTable 작성**

`Assets/Scripts/Equipment/EquipmentBaseStatsTable.cs`:
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleGame.Equipment
{
    [Serializable]
    public class EquipmentBaseStatsEntry
    {
        public EquipmentSlot slot;
        public EquipmentGrade grade;
        public int bonusAttack;
        public int bonusHealth;
    }

    [CreateAssetMenu(fileName = "EquipmentBaseStatsTable", menuName = "IdleGame/Equipment Base Stats Table")]
    public class EquipmentBaseStatsTable : ScriptableObject
    {
        public List<EquipmentBaseStatsEntry> entries = new List<EquipmentBaseStatsEntry>();

        public EquipmentBaseStatsEntry Find(EquipmentSlot slot, EquipmentGrade grade)
        {
            foreach (var entry in entries)
            {
                if (entry.slot == slot && entry.grade == grade) return entry;
            }
            throw new InvalidOperationException($"No base stats entry for {slot}/{grade}");
        }
    }
}
```

- [ ] **Step 4: Unity 에디터에서 샘플 데이터 에셋 생성 (수동)**

1. `Assets/Data/Monsters` 폴더 생성 → 우클릭 `Create > IdleGame > Monster Data` → 이름 `Stage1Monster` → `monsterName = "Slime"`, `maxHealth = 50`, `goldReward = 3`, `expReward = 10`, `dropChance = 0.3` 설정.
2. `Assets/Data/Stages` 폴더 생성 → `Create > IdleGame > Stage Data` → 이름 `Stage1` → `stageName = "Stage 1"`, `monster = Stage1Monster`, `killsToAdvance = 10` 설정.
3. `Assets/Data/Equipment` 폴더 생성 → `Create > IdleGame > Equipment Base Stats Table` → 이름 `EquipmentBaseStatsTable` → `entries`에 16개 항목(슬롯 4 × 등급 4) 추가, 등급이 높을수록 `bonusAttack`/`bonusHealth`가 커지도록 값 입력 (예: Common +1/+5, Rare +3/+15, Epic +7/+35, Legendary +15/+75, 슬롯별 동일 값 사용).

- [ ] **Step 5: 컴파일 및 에셋 생성 확인**

Unity 콘솔에 에러 없는지 확인, 세 에셋이 Project 창에 보이는지 확인.

- [ ] **Step 6: 커밋**

```bash
git add Assets/Scripts/Data Assets/Scripts/Equipment/EquipmentBaseStatsTable.cs Assets/Scripts/Equipment/EquipmentBaseStatsTable.cs.meta Assets/Data
git commit -m "feat: add monster/stage/equipment ScriptableObject data and sample assets"
```

---

### Task 12: GameEvents + GameManager 스켈레톤

**Files:**
- Create: `Assets/Scripts/Core/GameEvents.cs`
- Create: `Assets/Scripts/Core/GameManager.cs`

**Interfaces:**
- Consumes: `CharacterStats` (Task 3), `StageProgress` (Task 5), `CurrencyWallet` (Task 6), `EquipmentSlot`/`EquipmentGrade` (Task 7).
- Produces: `static class GameEvents { event Action<int> OnLevelUp; event Action<EquipmentSlot, EquipmentGrade> OnEquipmentDropped; event Action<int,int> OnMonsterKilled; event Action<bool,int> OnEnhanceResult; event Action<int> OnStageAdvanced; static void RaiseLevelUp(int); static void RaiseEquipmentDropped(EquipmentSlot, EquipmentGrade); static void RaiseMonsterKilled(int,int); static void RaiseEnhanceResult(bool,int); static void RaiseStageAdvanced(int); }`, `class GameManager : MonoBehaviour { CharacterStats CharacterStats; StageProgress StageProgress; CurrencyWallet Wallet; }`

이 태스크는 MonoBehaviour 골격이라 자동화 테스트 대신 수동 확인으로 검증한다 (스펙 10절).

- [ ] **Step 1: GameEvents 작성**

`Assets/Scripts/Core/GameEvents.cs`:
```csharp
using System;
using IdleGame.Equipment;

namespace IdleGame.Core
{
    // 시스템 간 직접 참조 대신 이벤트로 느슨하게 연결하기 위한 전역 이벤트 버스.
    public static class GameEvents
    {
        public static event Action<int> OnLevelUp;
        public static event Action<EquipmentSlot, EquipmentGrade> OnEquipmentDropped;
        public static event Action<int, int> OnMonsterKilled;
        public static event Action<bool, int> OnEnhanceResult;
        public static event Action<int> OnStageAdvanced;

        public static void RaiseLevelUp(int level) => OnLevelUp?.Invoke(level);
        public static void RaiseEquipmentDropped(EquipmentSlot slot, EquipmentGrade grade) => OnEquipmentDropped?.Invoke(slot, grade);
        public static void RaiseMonsterKilled(int gold, int exp) => OnMonsterKilled?.Invoke(gold, exp);
        public static void RaiseEnhanceResult(bool success, int newLevel) => OnEnhanceResult?.Invoke(success, newLevel);
        public static void RaiseStageAdvanced(int stageIndex) => OnStageAdvanced?.Invoke(stageIndex);
    }
}
```

- [ ] **Step 2: GameManager 작성**

`Assets/Scripts/Core/GameManager.cs`:
```csharp
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
```

- [ ] **Step 3: 씬에 임시 배치 후 수동 확인**

빈 씬에 빈 GameObject `GameManager`를 만들고 `GameManager` 컴포넌트를 추가한다. Play 모드 진입 시 콘솔에 에러가 없는지 확인한다 (아직 아무 이벤트도 발생하지 않으므로 별다른 동작은 없다 — 컴파일/초기화 확인용).

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scripts/Core
git commit -m "feat: add GameEvents bus and GameManager skeleton"
```

---

### Task 13: EquipmentSystem (MonoBehaviour)

**Files:**
- Create: `Assets/Scripts/Equipment/EquippedItem.cs`
- Create: `Assets/Scripts/Equipment/EquipmentSystem.cs`

**Interfaces:**
- Consumes: `EquipmentDropRoller`, `IRandomSource`, `SystemRandomSource`, `EquipmentBaseStatsTable` (Task 7, 11), `GameEvents` (Task 12).
- Produces: `class EquippedItem { EquipmentSlot slot; EquipmentGrade grade; int enhanceLevel; int baseAttack; int baseHealth; int TotalAttack; int TotalHealth; }`, `class EquipmentSystem : MonoBehaviour { int TotalBonusAttack; int TotalBonusHealth; EquippedItem GetEquipped(EquipmentSlot); void RollDropOnKill(double dropChance); void Equip(EquipmentSlot, EquipmentGrade); void LoadState(List<EquipmentSaveEntry>); }`

이 태스크는 MonoBehaviour + ScriptableObject 참조가 얽혀 있어 자동화 테스트 대신 수동 확인으로 검증한다.

- [ ] **Step 1: EquippedItem 작성**

`Assets/Scripts/Equipment/EquippedItem.cs`:
```csharp
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
```

- [ ] **Step 2: EquipmentSystem 작성**

`Assets/Scripts/Equipment/EquipmentSystem.cs`:
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using IdleGame.Core;
using IdleGame.Save;

namespace IdleGame.Equipment
{
    public class EquipmentSystem : MonoBehaviour
    {
        [SerializeField] private EquipmentBaseStatsTable baseStatsTable;

        private readonly Dictionary<EquipmentSlot, EquippedItem> _equipped = new Dictionary<EquipmentSlot, EquippedItem>();
        private readonly IRandomSource _random = new SystemRandomSource();

        public int TotalBonusAttack => Sum(item => item.TotalAttack);
        public int TotalBonusHealth => Sum(item => item.TotalHealth);

        public EquippedItem GetEquipped(EquipmentSlot slot)
        {
            return _equipped.TryGetValue(slot, out var item) ? item : null;
        }

        // 드롭된 장비는 인벤토리 없이 즉시 해당 슬롯에 장착한다 (MVP 단순화, 스펙 4절).
        public void RollDropOnKill(double dropChance)
        {
            if (!EquipmentDropRoller.TryRollDrop(dropChance, _random, out var grade)) return;

            var slot = EquipmentDropRoller.RollSlot(_random);
            Equip(slot, grade);
        }

        public void Equip(EquipmentSlot slot, EquipmentGrade grade)
        {
            var entry = baseStatsTable.Find(slot, grade);
            _equipped[slot] = new EquippedItem
            {
                slot = slot,
                grade = grade,
                enhanceLevel = 0,
                baseAttack = entry.bonusAttack,
                baseHealth = entry.bonusHealth
            };
            GameEvents.RaiseEquipmentDropped(slot, grade);
        }

        public void LoadState(List<EquipmentSaveEntry> savedItems)
        {
            if (savedItems == null) return;

            foreach (var saved in savedItems)
            {
                var slot = (EquipmentSlot)saved.slot;
                var grade = (EquipmentGrade)saved.grade;
                var entry = baseStatsTable.Find(slot, grade);
                _equipped[slot] = new EquippedItem
                {
                    slot = slot,
                    grade = grade,
                    enhanceLevel = saved.enhanceLevel,
                    baseAttack = entry.bonusAttack,
                    baseHealth = entry.bonusHealth
                };
            }
        }

        private int Sum(Func<EquippedItem, int> selector)
        {
            int total = 0;
            foreach (var item in _equipped.Values) total += selector(item);
            return total;
        }
    }
}
```

- [ ] **Step 3: 수동 확인**

씬에 `EquipmentSystem` 컴포넌트를 가진 GameObject를 만들고, 인스펙터에서 `baseStatsTable`에 Task 11에서 만든 `EquipmentBaseStatsTable.asset`을 연결한다. Play 모드에서 `RollDropOnKill(1.0)`을 임시 테스트 코드나 컨텍스트 메뉴로 호출해 `GetEquipped(...)`가 null이 아닌 항목을 반환하는지 확인한다 (확인 후 임시 호출 코드는 제거).

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scripts/Equipment/EquippedItem.cs Assets/Scripts/Equipment/EquipmentSystem.cs
git commit -m "feat: add EquipmentSystem drop/equip/state handling"
```

---

### Task 14: BattleSystem (MonoBehaviour)

**Files:**
- Create: `Assets/Scripts/Battle/BattleSystem.cs`

**Interfaces:**
- Consumes: `CharacterStats` (Task 3), `BattleMath`/`StageProgress` (Task 4, 5), `EquipmentSystem` (Task 13), `MonsterData`/`StageData` (Task 11), `GameEvents` (Task 12).
- Produces: `class BattleSystem : MonoBehaviour { void Initialize(CharacterStats, StageProgress); }`

수동 확인 대상 (MonoBehaviour 틱 루프).

- [ ] **Step 1: BattleSystem 작성**

`Assets/Scripts/Battle/BattleSystem.cs`:
```csharp
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
```

- [ ] **Step 2: 씬 배치 후 수동 확인**

`BattleSystem` 컴포넌트를 가진 GameObject를 만들고 인스펙터에서 `stages`에 `Stage1.asset`을 넣고, `equipmentSystem`에 Task 13의 `EquipmentSystem`을 연결한다. `GameManager.Awake()`가 끝난 뒤 `battleSystem.Initialize(gameManager.CharacterStats, gameManager.StageProgress)`를 호출하도록 임시 부트스트랩 스크립트나 `GameManager.Start()`에서 연결한다 (Task 19에서 정식 배치). Play 모드에서 몇 초 기다린 뒤 콘솔/디버그 로그로 몬스터가 반복 처치되는지 확인한다.

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/Battle/BattleSystem.cs
git commit -m "feat: add BattleSystem auto-attack tick loop"
```

---

### Task 15: EnhanceSystem (MonoBehaviour)

**Files:**
- Create: `Assets/Scripts/Enhance/EnhanceSystem.cs`

**Interfaces:**
- Consumes: `EnhanceCalculator` (Task 8), `EquipmentSystem` (Task 13), `CurrencyWallet` (Task 6), `GameEvents` (Task 12).
- Produces: `class EnhanceSystem : MonoBehaviour { bool TryEnhance(EquipmentSlot slot, CurrencyWallet wallet); }`

- [ ] **Step 1: EnhanceSystem 작성**

`Assets/Scripts/Enhance/EnhanceSystem.cs`:
```csharp
using System;
using UnityEngine;
using IdleGame.Core;
using IdleGame.Economy;
using IdleGame.Equipment;

namespace IdleGame.Enhance
{
    public class EnhanceSystem : MonoBehaviour
    {
        [SerializeField] private EquipmentSystem equipmentSystem;

        private readonly IRandomSource _random = new SystemRandomSource();

        public bool TryEnhance(EquipmentSlot slot, CurrencyWallet wallet)
        {
            var item = equipmentSystem.GetEquipped(slot);
            if (item == null) throw new InvalidOperationException($"No item equipped in slot {slot}");

            int cost = EnhanceCalculator.CostForLevel(item.enhanceLevel);
            if (!wallet.TrySpendEnhanceStones(cost)) return false;

            int previousLevel = item.enhanceLevel;
            item.enhanceLevel = EnhanceCalculator.TryEnhance(previousLevel, _random);
            bool success = item.enhanceLevel > previousLevel;

            GameEvents.RaiseEnhanceResult(success, item.enhanceLevel);
            return success;
        }
    }
}
```

- [ ] **Step 2: 수동 확인**

씬에 `EnhanceSystem` 컴포넌트를 추가하고 `equipmentSystem`을 연결한다. Play 모드에서 Task 13 확인 시 장착된 슬롯에 대해 `TryEnhance`를 임시 호출해 강화석 부족 시 `false`를 반환하고, 강화석을 충분히 준 뒤 재호출 시 `item.enhanceLevel`이 오르내리는지 확인한다.

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/Enhance/EnhanceSystem.cs
git commit -m "feat: add EnhanceSystem enhance attempt handling"
```

---

### Task 16: SaveSystemBehaviour (자동저장 + 오프라인 보상 통합)

**Files:**
- Create: `Assets/Scripts/Save/SaveSystemBehaviour.cs`

**Interfaces:**
- Consumes: `SaveSystem`/`SaveData` (Task 10), `GameManager` (Task 12), `EquipmentSystem` (Task 13), `OfflineRewardCalculator` (Task 9).
- Produces: `class SaveSystemBehaviour : MonoBehaviour { SaveData LoadOrCreate(); void ApplyOfflineReward(SaveData data, MonsterData currentMonster, float attacksPerSecond); }`

- [ ] **Step 1: SaveSystemBehaviour 작성**

`Assets/Scripts/Save/SaveSystemBehaviour.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using IdleGame.Core;
using IdleGame.Data;
using IdleGame.Equipment;
using IdleGame.Offline;

namespace IdleGame.Save
{
    public class SaveSystemBehaviour : MonoBehaviour
    {
        [SerializeField] private float autoSaveIntervalSeconds = 30f;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private EquipmentSystem equipmentSystem;

        private SaveSystem _saveSystem;
        private float _timer;

        private void Awake()
        {
            string path = Path.Combine(Application.persistentDataPath, "save.json");
            _saveSystem = new SaveSystem(path);
        }

        public SaveData LoadOrCreate()
        {
            return _saveSystem.Load() ?? new SaveData { lastSaveTimeUtc = UtcNowString() };
        }

        // 스펙 7절: 마지막 저장 시각과의 경과 시간을 이용해 오프라인 보상을 정산한다.
        public (int gold, int exp) ApplyOfflineReward(SaveData data, MonsterData currentMonster, float attacksPerSecond, int characterAttack)
        {
            if (!DateTime.TryParse(data.lastSaveTimeUtc, null, DateTimeStyles.RoundtripKind, out var lastSave))
            {
                return (0, 0);
            }

            double elapsedSeconds = (DateTime.UtcNow - lastSave).TotalSeconds;
            return OfflineRewardCalculator.CalculateReward(
                characterAttack, attacksPerSecond, currentMonster.maxHealth,
                currentMonster.goldReward, currentMonster.expReward, elapsedSeconds);
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < autoSaveIntervalSeconds) return;

            _timer = 0f;
            SaveNow();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) SaveNow();
        }

        private void OnApplicationQuit()
        {
            SaveNow();
        }

        private void SaveNow()
        {
            _saveSystem.Save(BuildSaveData());
        }

        private SaveData BuildSaveData()
        {
            var stats = gameManager.CharacterStats;
            var stage = gameManager.StageProgress;
            var wallet = gameManager.Wallet;

            var data = new SaveData
            {
                level = stats.Level,
                currentExp = stats.CurrentExp,
                gold = wallet.Gold,
                enhanceStones = wallet.EnhanceStones,
                stageIndex = stage.CurrentStageIndex,
                killsInStage = stage.KillsInStage,
                lastSaveTimeUtc = UtcNowString(),
                equippedItems = new List<EquipmentSaveEntry>()
            };

            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                var item = equipmentSystem.GetEquipped(slot);
                if (item == null) continue;
                data.equippedItems.Add(new EquipmentSaveEntry
                {
                    slot = (int)slot,
                    grade = (int)item.grade,
                    enhanceLevel = item.enhanceLevel
                });
            }

            return data;
        }

        private static string UtcNowString() => DateTime.UtcNow.ToString("o");
    }
}
```

- [ ] **Step 2: 수동 확인**

씬에 `SaveSystemBehaviour`를 추가하고 `gameManager`/`equipmentSystem` 참조를 연결한다. Play 모드로 잠시 진행 후 정지하면 `Application.persistentDataPath`에 `save.json`이 생성되는지 확인한다. 파일 내 `lastSaveTimeUtc`를 과거 시각(예: 3시간 전)으로 직접 수정한 뒤 재실행하여 `ApplyOfflineReward` 호출 결과 골드/경험치가 0보다 큰지 확인한다.

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/Save/SaveSystemBehaviour.cs
git commit -m "feat: add autosave, pause/quit save hooks and offline reward integration"
```

---

### Task 17: UI - TabBarController + BattleScreenUI

**Files:**
- Create: `Assets/Scripts/UI/TabBarController.cs`
- Create: `Assets/Scripts/UI/BattleScreenUI.cs`

**Interfaces:**
- Consumes: `GameManager` (Task 12), `GameEvents` (Task 12).
- Produces: `class TabBarController : MonoBehaviour`, `class BattleScreenUI : MonoBehaviour`

- [ ] **Step 1: TabBarController 작성**

`Assets/Scripts/UI/TabBarController.cs`:
```csharp
using UnityEngine;
using UnityEngine.UI;

namespace IdleGame.UI
{
    public class TabBarController : MonoBehaviour
    {
        [SerializeField] private GameObject battleScreen;
        [SerializeField] private GameObject equipmentScreen;
        [SerializeField] private GameObject enhanceScreen;
        [SerializeField] private Button battleTabButton;
        [SerializeField] private Button equipmentTabButton;
        [SerializeField] private Button enhanceTabButton;

        private void Awake()
        {
            battleTabButton.onClick.AddListener(() => ShowOnly(battleScreen));
            equipmentTabButton.onClick.AddListener(() => ShowOnly(equipmentScreen));
            enhanceTabButton.onClick.AddListener(() => ShowOnly(enhanceScreen));
            ShowOnly(battleScreen);
        }

        private void ShowOnly(GameObject target)
        {
            battleScreen.SetActive(target == battleScreen);
            equipmentScreen.SetActive(target == equipmentScreen);
            enhanceScreen.SetActive(target == enhanceScreen);
        }
    }
}
```

- [ ] **Step 2: BattleScreenUI 작성**

`Assets/Scripts/UI/BattleScreenUI.cs`:
```csharp
using UnityEngine;
using UnityEngine.UI;
using IdleGame.Core;

namespace IdleGame.UI
{
    public class BattleScreenUI : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Text levelText;
        [SerializeField] private Text stageText;
        [SerializeField] private Text goldText;

        private void OnEnable()
        {
            GameEvents.OnLevelUp += HandleChanged;
            GameEvents.OnStageAdvanced += HandleChanged;
            GameEvents.OnMonsterKilled += HandleMonsterKilled;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnLevelUp -= HandleChanged;
            GameEvents.OnStageAdvanced -= HandleChanged;
            GameEvents.OnMonsterKilled -= HandleMonsterKilled;
        }

        private void HandleChanged(int _) => Refresh();
        private void HandleMonsterKilled(int gold, int exp) => Refresh();

        private void Refresh()
        {
            levelText.text = $"Lv. {gameManager.CharacterStats.Level}";
            stageText.text = $"Stage {gameManager.StageProgress.CurrentStageIndex + 1}";
            goldText.text = $"Gold: {gameManager.Wallet.Gold}";
        }
    }
}
```

- [ ] **Step 3: 씬 UI 배치 (수동)**

1. `GameObject > UI > Canvas` 생성 (없다면), `EventSystem`이 자동 생성되는지 확인.
2. Canvas 하위에 `BattleScreen`, `EquipmentScreen`, `EnhanceScreen` 빈 GameObject(각각 RectTransform 꽉 채움) 생성.
3. `BattleScreen` 하위에 `LevelText`, `StageText`, `GoldText` (Legacy `Text`) 배치, `BattleScreenUI` 컴포넌트를 `BattleScreen`에 추가하고 필드 연결.
4. Canvas 하단에 `TabBar` 빈 GameObject + 버튼 3개(`BattleTabButton`, `EquipmentTabButton`, `EnhanceTabButton`, 텍스트는 각각 "전투"/"장비"/"강화") 배치, `TabBarController` 컴포넌트를 추가하고 필드 연결.
5. Play 모드에서 레벨/스테이지/골드 텍스트가 전투 진행에 따라 갱신되는지 확인.

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scripts/UI/TabBarController.cs Assets/Scripts/UI/BattleScreenUI.cs
git commit -m "feat: add tab bar and battle screen UI"
```

---

### Task 18: UI - EquipmentScreenUI

**Files:**
- Create: `Assets/Scripts/UI/EquipmentScreenUI.cs`

**Interfaces:**
- Consumes: `EquipmentSystem` (Task 13), `GameEvents` (Task 12).
- Produces: `class EquipmentScreenUI : MonoBehaviour`

- [ ] **Step 1: EquipmentScreenUI 작성**

`Assets/Scripts/UI/EquipmentScreenUI.cs`:
```csharp
using UnityEngine;
using UnityEngine.UI;
using IdleGame.Core;
using IdleGame.Equipment;

namespace IdleGame.UI
{
    public class EquipmentScreenUI : MonoBehaviour
    {
        [SerializeField] private EquipmentSystem equipmentSystem;
        [SerializeField] private Text weaponText;
        [SerializeField] private Text hatText;
        [SerializeField] private Text clothesText;
        [SerializeField] private Text accessoryText;

        private void OnEnable()
        {
            GameEvents.OnEquipmentDropped += HandleDropped;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnEquipmentDropped -= HandleDropped;
        }

        private void HandleDropped(EquipmentSlot slot, EquipmentGrade grade) => Refresh();

        private void Refresh()
        {
            weaponText.text = Describe(EquipmentSlot.Weapon);
            hatText.text = Describe(EquipmentSlot.Hat);
            clothesText.text = Describe(EquipmentSlot.Clothes);
            accessoryText.text = Describe(EquipmentSlot.Accessory);
        }

        private string Describe(EquipmentSlot slot)
        {
            var item = equipmentSystem.GetEquipped(slot);
            return item == null ? $"{slot}: (empty)" : $"{slot}: {item.grade} +{item.enhanceLevel}";
        }
    }
}
```

- [ ] **Step 2: 씬 UI 배치 (수동)**

`EquipmentScreen` 하위에 `WeaponText`/`HatText`/`ClothesText`/`AccessoryText` (Legacy `Text`) 배치, `EquipmentScreenUI` 컴포넌트를 추가하고 필드 연결. 장비 탭으로 전환했을 때 장착된 슬롯이 텍스트로 표시되는지, 드롭 발생 시 갱신되는지 확인.

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/UI/EquipmentScreenUI.cs
git commit -m "feat: add equipment screen UI"
```

---

### Task 19: UI - EnhanceScreenUI

**Files:**
- Create: `Assets/Scripts/UI/EnhanceScreenUI.cs`

**Interfaces:**
- Consumes: `EquipmentSystem` (Task 13), `EnhanceSystem` (Task 15), `GameManager` (Task 12).
- Produces: `class EnhanceScreenUI : MonoBehaviour`

- [ ] **Step 1: EnhanceScreenUI 작성**

`Assets/Scripts/UI/EnhanceScreenUI.cs`:
```csharp
using UnityEngine;
using UnityEngine.UI;
using IdleGame.Core;
using IdleGame.Enhance;
using IdleGame.Equipment;

namespace IdleGame.UI
{
    public class EnhanceScreenUI : MonoBehaviour
    {
        [SerializeField] private EquipmentSystem equipmentSystem;
        [SerializeField] private EnhanceSystem enhanceSystem;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Dropdown slotDropdown;
        [SerializeField] private Text resultText;
        [SerializeField] private Button enhanceButton;

        private void Awake()
        {
            enhanceButton.onClick.AddListener(OnEnhanceClicked);
        }

        private void OnEnhanceClicked()
        {
            var slot = (EquipmentSlot)slotDropdown.value;
            var item = equipmentSystem.GetEquipped(slot);
            if (item == null)
            {
                resultText.text = "장착된 장비가 없습니다.";
                return;
            }

            bool success = enhanceSystem.TryEnhance(slot, gameManager.Wallet);
            resultText.text = success ? $"강화 성공! +{item.enhanceLevel}" : $"강화 실패. +{item.enhanceLevel}";
        }
    }
}
```

- [ ] **Step 2: 씬 UI 배치 (수동)**

`EnhanceScreen` 하위에 `SlotDropdown` (Legacy `Dropdown`, 옵션에 무기/모자/옷/액세서리 4개 등록), `ResultText`, `EnhanceButton` 배치. `EnhanceScreenUI` 컴포넌트를 추가하고 필드 연결. 강화 탭에서 슬롯 선택 후 버튼을 눌러 결과 텍스트가 성공/실패에 맞게 갱신되는지, 강화석이 부족할 때 `TryEnhance`가 `false`를 반환하며 정상 처리되는지 확인.

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/UI/EnhanceScreenUI.cs
git commit -m "feat: add enhance screen UI"
```

---

### Task 20: 씬 구성 + 부트스트랩 + 수동 플레이테스트 체크리스트

**Files:**
- Modify: `Assets/Scenes/SampleScene.unity` (또는 신규 `Assets/Scenes/Game.unity`)

**Interfaces:** 없음 (씬 배선 작업).

- [ ] **Step 1: 씬에 시스템 GameObject 배치**

`SampleScene`에 다음 GameObject를 배치하고 컴포넌트를 연결한다 (이미 개별 태스크에서 생성한 것들을 한 씬에 모으는 작업):
- `GameManager` (Task 12)
- `EquipmentSystem` (Task 13, `baseStatsTable` 연결)
- `BattleSystem` (Task 14, `stages`/`equipmentSystem` 연결)
- `EnhanceSystem` (Task 15, `equipmentSystem` 연결)
- `SaveSystemBehaviour` (Task 16, `gameManager`/`equipmentSystem` 연결)
- Canvas 하위 UI 전체 (Task 17-19)

- [ ] **Step 2: GameManager에 부트스트랩 연결 추가**

`Assets/Scripts/Core/GameManager.cs`의 `Awake()` 아래에 `Start()`를 추가해 저장 데이터 로드, 오프라인 보상 적용, 각 시스템 상태 복원, `BattleSystem.Initialize` 호출까지 순서대로 실행되게 한다:

```csharp
[SerializeField] private Battle.BattleSystem battleSystem;
[SerializeField] private Equipment.EquipmentSystem equipmentSystem;
[SerializeField] private Save.SaveSystemBehaviour saveSystemBehaviour;
[SerializeField] private Data.MonsterData startingMonster;
[SerializeField] private float attacksPerSecond = 1f;

private void Start()
{
    var saveData = saveSystemBehaviour.LoadOrCreate();

    CharacterStats.LoadState(saveData.level, saveData.currentExp);
    StageProgress.LoadState(saveData.stageIndex, saveData.killsInStage);
    Wallet.LoadState(saveData.gold, saveData.enhanceStones);
    equipmentSystem.LoadState(saveData.equippedItems);

    var (offlineGold, offlineExp) = saveSystemBehaviour.ApplyOfflineReward(
        saveData, startingMonster, attacksPerSecond, CharacterStats.Attack);
    Wallet.AddGold(offlineGold);
    CharacterStats.AddExp(offlineExp);

    battleSystem.Initialize(CharacterStats, StageProgress);
}
```

인스펙터에서 `battleSystem`/`equipmentSystem`/`saveSystemBehaviour`/`startingMonster`(Stage1Monster.asset) 필드를 연결한다.

- [ ] **Step 3: 수동 플레이테스트 체크리스트 실행**

Play 모드에서 아래를 순서대로 확인한다:
1. 캐릭터가 자동으로 몬스터를 공격해 처치한다.
2. 처치 시 골드/경험치가 오르고, 레벨업 시 공격력/체력이 오른다 (BattleScreenUI 텍스트로 확인).
3. 목표 처치 수 달성 시 스테이지가 올라간다.
4. 장비가 확률적으로 드롭되어 EquipmentScreenUI에 표시된다.
5. 강화 탭에서 강화석으로 강화 시도 시 성공/실패에 따라 강화 단계가 오르내린다.
6. Play 모드를 종료했다가 재시작하면 이전 레벨/골드/장비 상태가 그대로 복원된다 (`save.json` 저장 확인).
7. `save.json`의 `lastSaveTimeUtc`를 몇 시간 전으로 수동 수정한 뒤 재시작하면 오프라인 보상(골드/경험치)이 지급된다.
8. 하단 탭바로 전투/장비/강화 화면이 정상 전환된다.

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scenes Assets/Scripts/Core/GameManager.cs
git commit -m "feat: wire scene bootstrap and complete MVP loop"
```
