# Unity 6000.5.6f1용 2D 6레인 리듬게임 프로토타입

Unity **6000.5.6f1**과 Unity Input System **1.20.0**을 기준으로 만든 2D 최소기능 프로토타입입니다. 프로젝트 세카이의 세로 원근 레인 연출에서 아이디어만 참고했으며, 원작의 이미지·음원·캐릭터·UI는 사용하지 않았습니다.

## 구현 기능

- 먼 쪽이 좁고 가까운 쪽이 넓은 사다리꼴 플레이 필드
- Orthographic 카메라와 XY 평면만 사용하는 순수 2D 렌더링
- 노트 위치와 크기 보간으로 표현하는 가상 원근감
- 일정 속도 선형 이동으로 판정선 근처 감속 현상 제거
- 6개 레인
- Input Action `Lane1`~`Lane6`
- 키보드 `A S D J K L` 바인딩
- 키를 누르고 있는 동안 해당 레인 전체와 키 표시가 네온으로 점등
- 1개 또는 왼손·오른손에 하나씩 배치되는 2개 노트 무작위 생성
- 0.52~0.78초 간격으로 빠르게 다가오는 직선형 노트
- PERFECT / GOOD / MISS 판정
- 점수, 현재 콤보, 최고 콤보 표시
- 플레이 필드를 침범하지 않는 상단 사이버 시티 배경
- 외부 이미지나 유료 에셋 불필요

## 프로젝트 실행 방법

1. ZIP 파일을 원하는 위치에 압축 해제합니다.
2. Unity Hub를 실행합니다.
3. 왼쪽의 `Projects`를 누릅니다.
4. 오른쪽 위 `Add` 또는 `Open`을 누릅니다.
5. 압축을 푼 폴더 안의 `ProjectSekaiStylePrototype` 폴더를 선택합니다. 이 폴더를 열었을 때 `Assets`, `Packages`, `ProjectSettings`가 보여야 합니다.
6. Editor Version은 `6000.5.6f1`을 선택합니다.
7. 처음 열 때 Package Manager가 Input System 1.20.0을 설치하고 스크립트를 컴파일하므로 잠시 기다립니다.
8. Input System 활성화와 에디터 재시작을 묻는 창이 나오면 **Yes**를 누릅니다.
9. 다시 열린 뒤 `Assets/Main.unity`가 자동 생성됩니다.
10. 상단 중앙의 삼각형 `Play` 버튼을 누릅니다.
11. `Game` 창을 한 번 클릭한 뒤 `A S D J K L` 키로 플레이합니다.

씬이 자동 생성되지 않으면 상단 메뉴에서 `Tools > Rhythm Prototype > Create or Reset Main Scene`을 누르세요.

## Unity 6000.5.6f1 설치부터 시작하기

### 1. Unity Hub 설치

Unity 공식 사이트에서 Unity Hub를 내려받아 설치합니다. Hub는 Unity Editor 버전과 프로젝트를 관리하는 프로그램입니다.

### 2. Editor 설치

1. Unity Hub 왼쪽에서 `Installs`를 선택합니다.
2. `Install Editor`를 누릅니다.
3. `6000.5.6f1`을 선택합니다.
4. Windows에서 PC 실행 파일만 만들 예정이라면 기본 Editor와 Windows Build Support만 있으면 됩니다.
5. 설치가 끝날 때까지 기다립니다.

### 3. 프로젝트 열기

Unity 프로젝트는 실행 파일을 더블클릭하는 방식이 아니라 Hub에서 프로젝트 폴더를 선택해 엽니다. 반드시 `Assets`, `Packages`, `ProjectSettings` 세 폴더의 바로 위 폴더를 선택하세요.

## Input System 구성

입력 에셋은 `Assets/Input/RhythmControls.inputactions`입니다.

| Action Map | Action | Binding |
|---|---|---|
| Gameplay | Lane1 | `<Keyboard>/a` |
| Gameplay | Lane2 | `<Keyboard>/s` |
| Gameplay | Lane3 | `<Keyboard>/d` |
| Gameplay | Lane4 | `<Keyboard>/j` |
| Gameplay | Lane5 | `<Keyboard>/k` |
| Gameplay | Lane6 | `<Keyboard>/l` |

코드는 각 액션에 대해 `WasPressedThisFrame()`을 확인합니다. 예전 `Input.GetKeyDown`은 사용하지 않습니다.

바인딩을 바꾸려면 다음과 같이 합니다.

1. Project 창에서 `Assets > Input > RhythmControls`를 더블클릭합니다.
2. 왼쪽에서 `Gameplay`를 선택합니다.
3. 바꾸려는 `Lane` 액션 아래의 키 바인딩을 선택합니다.
4. 오른쪽 `Path`에서 새 키를 지정합니다.
5. `Save Asset`을 누릅니다.

## 에디터의 주요 창

- **Hierarchy**: 현재 씬의 오브젝트 목록
- **Scene**: 오브젝트를 배치하고 편집하는 화면
- **Game**: 실제 플레이 화면
- **Inspector**: 선택한 오브젝트의 설정을 변경하는 화면
- **Project**: 코드, 입력 에셋, 씬 등의 파일 목록
- **Console**: 컴파일 오류와 실행 로그가 표시되는 화면

## 게임 수치 조절하기

Hierarchy의 `Rhythm Game Prototype`을 선택한 다음 Inspector에서 조절합니다.

- `Note Travel Time`: 노트가 위쪽에서 아래쪽까지 이동하는 시간, 기본값 1.38초
- `Min Spawn Interval`: 노트 묶음의 최소 생성 간격, 기본값 0.52초
- `Max Spawn Interval`: 노트 묶음의 최대 생성 간격, 기본값 0.78초
- `Double Note Chance`: 2개 동시 노트 확률, 기본값 0.32
- `Near Half Width`: 플레이 필드 가까운 쪽 절반 폭
- `Far Half Width`: 플레이 필드 먼 쪽 절반 폭
- `Near Y`, `Far Y`: 가까운 쪽과 먼 쪽의 화면상 Y 위치
- `Near Half Width`: 가까운 쪽 폭, 기본값 7.00으로 넓게 설정
- `Judge Y`: 판정선의 화면상 Y 위치, 기본값 -2.78
- `Input Actions`: `RhythmControls` 입력 에셋 참조

Play 중에 변경한 값은 Play를 종료하면 원래대로 돌아갑니다. 계속 사용할 값은 Play를 끈 상태에서 변경하세요.

## 코드 구조

- `BuildInputActions()`: 입력 에셋을 불러오고 6개의 액션 준비
- `Build2DResources()`: 공용 2D 스프라이트와 재질 생성
- `BuildOrthographicCamera()`: 정사영 2D 카메라 생성
- `BuildCyberpunkBackground()`: 2D 도시와 사이버펑크 배경 생성
- `Build2DStage()`: XY 평면의 사다리꼴 판, 레인, 판정선 생성
- `UpdateSpawner()`: 1~2개 무작위 노트 생성
- `UpdateNoteVisual()`: 노트의 2D 위치와 크기를 보간하여 가상 원근감 표현
- `HandleLaneActions()`: Input Action 입력 검사
- `UpdateLaneGlows()`: 누르고 있는 레인을 네온으로 점등
- `TryHitLane()`: 같은 레인에서 판정선과 가장 가까운 노트 판정
- `OnGUI()`: 점수, 콤보, 키 안내 표시

## 오류가 발생할 때

### Input System 패키지 안에서 `GetInstanceID` 또는 `CreateAssetWithContent` 오류가 발생할 때

Input System 1.17.0이 Unity 6000.5의 새 Editor API와 맞지 않아 생기는 오류입니다. 수정된 프로젝트는 1.20.0을 사용합니다.

기존에 압축을 풀어 사용하던 프로젝트를 직접 복구하려면 다음 순서대로 진행합니다.

1. `Enter Safe Mode` 창에서는 `Enter Safe Mode`를 누릅니다.
2. Unity Editor를 완전히 종료합니다.
3. 프로젝트 폴더의 `Packages/manifest.json`을 메모장으로 엽니다.
4. `"com.unity.inputsystem": "1.17.0"`을 `"com.unity.inputsystem": "1.20.0"`으로 변경하고 저장합니다.
5. 프로젝트 폴더의 `Library/PackageCache` 안에서 이름이 `com.unity.inputsystem@`으로 시작하는 폴더를 삭제합니다. 이 폴더는 Unity가 다시 생성하는 캐시입니다.
6. Unity Hub에서 프로젝트를 다시 엽니다.
7. Package Manager가 1.20.0을 다시 받은 뒤 컴파일이 끝날 때까지 기다립니다.

새 ZIP을 새 폴더에 압축 해제해서 여는 경우에는 위의 수동 작업이 필요하지 않습니다.

### `The type or namespace name 'InputSystem' could not be found`

1. `Window > Package Manager`를 엽니다.
2. 왼쪽에서 `Unity Registry`를 선택합니다.
3. `Input System`을 검색하고 설치합니다.
4. 설치 후 에디터 재시작을 묻는다면 `Yes`를 누릅니다.

### 키 입력이 되지 않을 때

1. `Edit > Project Settings > Player`를 엽니다.
2. `Other Settings > Configuration`을 찾습니다.
3. `Active Input Handling`을 `Input System Package (New)` 또는 `Both`로 설정합니다.
4. Unity Editor를 재시작합니다.
5. Play 후 Game 창 내부를 클릭하고 다시 입력합니다.

### 플레이 화면이 생성되지 않을 때

1. Console의 빨간 오류가 없는지 확인합니다.
2. 오류가 없다면 `Tools > Rhythm Prototype > Create or Reset Main Scene`을 실행합니다.
3. `Assets/Main.unity`를 더블클릭합니다.

## 다음 단계

- AudioSource와 실제 BPM 기반 타이밍 추가
- 랜덤 생성 대신 JSON 또는 ScriptableObject 채보 사용
- 롱 노트와 플릭 노트 추가
- 판정 이펙트 및 오브젝트 풀링 추가
- Canvas와 TextMeshPro 기반 정식 UI로 교체
