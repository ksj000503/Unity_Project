# 트러블슈팅 로그

프로젝트 진행 중 발생한 문제와 해결 과정을 기록합니다. (포트폴리오 작성용)

---

## 1. Claude Code에서 mcp-unity MCP 서버가 연결되지 않던 문제

**상황**

Unity 프로젝트에 `mcp-unity` 패키지(`com.gamelovers.mcp-unity`)를 설치하고 `.mcp.json`에 서버를 등록했지만, Claude Code 세션에서 `/mcp` 명령으로 확인해도 `mcp-unity` 서버 자체가 목록에 뜨지 않았다. Unity 에디터를 재시작하고 Claude Code 세션도 여러 번 재시작해봤지만 승인(trust) 프롬프트조차 뜨지 않는 상태가 반복됐다.

**원인 분석**

1. Claude Code는 프로젝트 스코프 MCP 서버(`.mcp.json`)를 세션 "시작 시점"에만 로드한다 — `.mcp.json`을 수정해도 이미 떠 있는 세션에는 반영되지 않는다.
2. Unity 쪽은 실제로 정상이었다 — `netstat`으로 확인한 결과 mcp-unity의 웹소켓 브리지 포트(8090)가 정상적으로 리스닝 중이었다.
3. 진짜 원인은 `.claude/settings.local.json`에 있었다:
   ```json
   {
     "disabledMcpjsonServers": ["mcp-unity"]
   }
   ```
   과거에 실수로(혹은 세션 초기 설정 과정에서) mcp-unity가 명시적으로 "거부됨" 상태로 기록되어 있었다. 한 번 거부 목록에 들어간 서버는 승인 프롬프트 자체가 다시 뜨지 않고 조용히 무시된다.

**해결**

`.claude/settings.local.json`의 `disabledMcpjsonServers`에서 `mcp-unity`를 제거하고, 대신 `enabledMcpjsonServers`에 추가:
```json
{
  "enabledMcpjsonServers": ["mcp-unity"]
}
```
세션을 한 번 더 재시작하니 별도 승인 프롬프트 없이 바로 연결되었고, `get_scene_info` 같은 mcp-unity 도구 호출이 정상 동작함을 확인했다.

**배운 점**

MCP 서버가 "안 뜬다"고 해서 항상 서버(여기서는 Unity) 쪽 문제인 것은 아니다. 클라이언트(Claude Code) 쪽의 승인/거부 상태 캐시부터 확인하는 게 먼저다. `~/.claude.json`(프로젝트별 `enabledMcpjsonServers`/`disabledMcpjsonServers`)과 `.claude/settings.local.json`을 함께 봐야 원인이 보인다.

---

## 2. Unity에서 코드로 새로 만든 파일의 `.meta`가 생성되지 않던 문제

**상황**

구현 계획(TDD 플랜)에 따라 subagent가 `Assets/Scripts/IdleGame.asmdef`, `Assets/Tests/EditMode/IdleGame.Tests.asmdef` 두 파일을 파일시스템에 직접 작성(Write 툴)했다. 이후 `mcp-unity`의 `recompile_scripts`를 호출해 "컴파일 에러 0건"을 확인했지만, 리뷰 단계에서 두 파일의 `.meta` 컴패니언 파일이 전혀 생성되지 않았다는 게 발견됐다. `.meta`가 없으면 Unity의 AssetDatabase가 해당 파일을 자산으로 인식하지 못한 상태라는 뜻이다.

**원인 분석**

- `recompile_scripts`는 이미 Unity가 알고 있는 C# 스크립트를 다시 컴파일하는 동작이지, `Assets/` 하위에 새로 생긴 파일을 감지해서 임포트(`AssetDatabase.Refresh()`)하는 동작이 아니다.
- 파일이 Unity 에디터의 API가 아니라 외부 텍스트 툴로 디스크에 직접 쓰여졌기 때문에, Unity가 그 존재 자체를 아직 모르는 상태였다.
- "컴파일 에러 0건"이라는 검증 결과만으로는 "파일이 실제로 임포트됐다"는 것을 증명하지 못했다 — 리뷰어가 이 부분을 잘못된 검증(false positive)으로 정확히 짚어냈다.

**해결**

`mcp__mcp-unity__execute_menu_item("Assets/Refresh")`를 호출해 Unity에게 강제로 전체 에셋 리프레시를 시켰다. 그 결과 두 `.asmdef` 파일과 상위 폴더들(`Assets/Scripts.meta`, `Assets/Tests.meta`, `Assets/Tests/EditMode.meta`)까지 포함해 총 5개의 `.meta` 파일이 정상 생성됐고, 이후 태스크부터는 항상 "파일 쓰기 → Assets/Refresh → 콘솔 에러 확인 → `.meta` 존재 확인" 순서를 표준 절차로 삼았다.

**배운 점**

Unity 프로젝트에서 파일을 에디터 밖(텍스트 툴, 스크립트)에서 직접 생성할 때는, 컴파일 성공 여부와 별개로 "에셋 임포트가 실제로 일어났는가"를 항상 별도로 검증해야 한다. 자동화된 리뷰 단계에서 "리포트의 주장을 그대로 믿지 말고 근거(파일 존재, 콘솔 로그)로 검증하라"는 원칙이 실제로 버그를 잡아낸 사례.

---

## 3. 방치형 게임 특성을 반영한 CharacterStats 코드 리뷰 피드백

**상황**

`CharacterStats`(레벨/경험치/스탯 공식) 구현이 자동 리뷰를 통과하고 커밋 대기 상태였는데, 실제 코드를 직접 읽어본 프로젝트 담당자가 세 가지 개선을 요청했다.

**피드백 내용과 반영**

1. **자료형: `int` → `long`.** 방치형 게임은 시간이 지날수록 경험치 같은 누적 수치가 매우 커진다. `int`(약 21억 상한)로는 장기 플레이 시 오버플로우 위험이 있어, 경험치 관련 값(`CurrentExp`, `RequiredExpForLevel` 반환값, `AddExp`/`LoadState`의 경험치 매개변수)을 `long`으로 변경. 전투력 스탯(`Attack`, `MaxHealth`)처럼 경험치·재화가 아닌 값은 그대로 `int` 유지 — 필요한 범위에만 정확히 적용.
2. **알고리즘은 그대로, 주석만 추가.** `AddExp`의 다중 레벨업 처리는 `while` 루프로 구현되어 있었다. 경험치 총합을 등차수열 합 공식(이차방정식)으로 풀면 반복문 없이 O(1)에 레벨을 계산할 수도 있지만, 지금 규모에서는 루프가 더 단순하고 검증하기 쉽다고 판단해 로직은 유지하고, "필요 시 이차방정식으로 O(1) 최적화 가능"이라는 주석 한 줄만 남겼다 — 불필요한 최적화를 지금 하지 않는다는 YAGNI 원칙의 실제 적용 사례.
3. **`LoadState`의 레벨업 정규화 누락.** 기존 `LoadState`는 저장된 레벨/경험치를 그대로 대입할 뿐, 만약 저장된 경험치가 해당 레벨의 요구치를 넘는 상태(세이브 파일 수동 편집, 과거 버전 데이터 등)로 들어와도 레벨업을 다시 계산하지 않았다. `AddExp`가 쓰는 레벨업 정규화 로직을 공용 private 메서드로 뽑아 `LoadState`에서도 재사용하도록 수정 — 저장된 데이터가 어떤 상태로 들어오든 항상 정합성 있는 레벨/경험치로 정규화되게 함.

**배운 점**

자동화된 리뷰(스펙 준수/코드 품질 체크)가 통과해도, 도메인 지식(방치형 게임에서는 숫자가 커진다, 세이브 데이터는 오염될 수 있다)에 기반한 사람의 리뷰가 여전히 중요하다. 자동 리뷰는 "브리핑에 명시된 요구사항 충족 여부"를 보지만, "장르 특성상 앞으로 생길 문제"는 도메인 이해가 있는 사람만 짚어낼 수 있었다.
