# Unity 6000.5.6f1용 2.5D 6레인 리듬게임 프로토타입

Unity **6000.5.6f1**과 Unity Input System **1.20.0**을 기준으로 만든 최소기능 프로토타입입니다. 프로젝트 세카이의 세로 원근 레인 연출에서 아이디어만 참고했으며, 원작의 이미지·음원·캐릭터·UI는 사용하지 않았습니다.

## 구현 기능

- 먼 쪽이 좁고 가까운 쪽이 넓은 사다리꼴 플레이 필드
- Perspective 카메라를 이용한 2.5D 원근감
- 6개 레인
- Input Action `Lane1`~`Lane6`
- 키보드 `A S D J K L` 바인딩
- 키를 누르고 있는 동안 해당 레인 전체와 키 표시가 네온으로 점등
- 1개 또는 왼손·오른손에 하나씩 배치되는 2개 노트 무작위 생성
- 0.52~0.78초 간격으로 빠르게 다가오는 직선형 노트
- PERFECT / GREAT / GOOD / MISS 판정
- 점수, 현재 콤보, 최고 콤보 표시
- 음악 시간과 박자를 공통 기준으로 사용하는 `RhythmClock`
- 실제 채보가 없을 때는 랜덤 노트, `ChartData`를 연결하면 고정 채보 사용
- 네온 레일, 원근 그리드, 사이버 시티 타워, 홀로그램 게이트로 구성된 사이버펑크 무대
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

- `Note Speed`: 노트 속도, 기본값 15
- `Min Spawn Interval`: 노트 묶음의 최소 생성 간격, 기본값 0.52초
- `Max Spawn Interval`: 노트 묶음의 최대 생성 간격, 기본값 0.78초
- `Double Note Chance`: 2개 동시 노트 확률, 기본값 0.32
- `Near Half Width`: 플레이 필드 가까운 쪽 절반 폭
- `Far Half Width`: 플레이 필드 먼 쪽 절반 폭
- `Judge Z`: 판정선의 깊이 위치
- `Input Actions`: `RhythmControls` 입력 에셋 참조

Play 중에 변경한 값은 Play를 종료하면 원래대로 돌아갑니다. 계속 사용할 값은 Play를 끈 상태에서 변경하세요.

## 외부 머티리얼 연결하기

이 프로젝트는 아트 에셋을 자유롭게 교체할 수 있도록 코드 내부에서 머티리얼을 생성하지 않습니다. `Main` 씬을 새로 만들면 `Rhythm Game Controller`의 모든 Material 슬롯은 `None (Material)` 상태입니다.

1. Project 창의 `Assets`에서 우클릭하고 `Create > Folder`를 선택합니다.
2. 폴더 이름을 `Materials`로 지정합니다.
3. `Assets/Materials`에서 우클릭하고 `Create > Material`로 필요한 `.mat` 에셋을 만듭니다.
4. 각 머티리얼에 원하는 Shader, 색상, 텍스처와 발광 설정을 지정합니다.
5. `Main` 씬에서 `Rhythm Game Prototype` 오브젝트를 선택합니다.
6. Inspector의 `Rhythm Game Controller`에서 아래 슬롯에 `.mat` 에셋을 드래그합니다.
7. `Ctrl + S`로 씬을 저장합니다.

| Inspector 그룹 | 슬롯 | 적용 대상 |
|---|---|---|
| Materials - Play Field | Board Material | 메인 사다리꼴 플레이필드 |
| Materials - Play Field | Side Floor Material | 플레이필드 양옆 바닥 |
| Materials - Play Field | Rail Material | 레인 경계선과 뒤쪽 게이트 |
| Materials - Play Field | Rail Glow Material | 레인 경계선의 넓은 발광 영역 |
| Materials - Play Field | Grid Material | 플레이필드 가로 격자선 |
| Materials - Play Field | Judge Material | 판정선 |
| Materials - Notes and Lane Input | Left Note Material | A, S, D 레인의 노트 |
| Materials - Notes and Lane Input | Right Note Material | J, K, L 레인의 노트 |
| Materials - Notes and Lane Input | Left Lane Glow Material | A, S, D 키를 누를 때 표시되는 레인 |
| Materials - Notes and Lane Input | Right Lane Glow Material | J, K, L 키를 누를 때 표시되는 레인 |
| Materials - Cyber City | City Material A/B | 사이버 도시 건물 본체 |
| Materials - Cyber City | City Cyan/Magenta Material | 건물 네온과 뒤쪽 포털 |

머티리얼을 연결하지 않은 부분은 렌더링되지 않거나 사용 중인 렌더 파이프라인의 기본 오류 색상으로 보일 수 있습니다. 이는 슬롯을 공백으로 둔 의도된 상태이며 원하는 `.mat` 파일을 연결하면 해결됩니다.

Inspector에서 연결한 머티리얼은 프로젝트 에셋이므로 스크립트가 색상을 덮어쓰거나 종료 시 `Destroy()`하지 않습니다. 레인 입력 효과는 지정한 머티리얼의 외형을 유지한 채 Renderer의 활성화 여부만 변경합니다.

`Tools > Rhythm Prototype > Create or Reset Main Scene`을 실행하면 씬이 다시 생성되면서 연결한 머티리얼도 초기화됩니다. 실행하기 전에 사용 중인 머티리얼 목록을 확인하고, 실행 후 다시 연결해 주세요.

## 코드 구조

기존의 큰 `RhythmGamePrototype.cs`는 다음 5개 게임플레이 스크립트로 분리했습니다.

| 파일 | 역할 |
|---|---|
| `Assets/Scripts/RhythmGameController.cs` | 게임 시작과 종료, Input System 입력, 외부 머티리얼 슬롯, 카메라·플레이필드·사이버 도시·UI 및 다른 시스템 연결 |
| `Assets/Scripts/RhythmClock.cs` | 음악 재생, DSP 기반 곡 시간, BPM·박자, 일시정지와 전역 재생 배율 관리 |
| `Assets/Scripts/ChartData.cs` | `NoteData`, `BpmEventData`, 곡 정보와 채보 목록을 보관하는 ScriptableObject |
| `Assets/Scripts/NoteManager.cs` | 채보 또는 랜덤 노트 생성, 음악 시간 기반 이동, 원근 메시 갱신, 노트 검색과 자동 MISS 처리 |
| `Assets/Scripts/JudgementScoreManager.cs` | PERFECT·GREAT·GOOD·MISS 판정, 점수·콤보·최대 콤보와 최종 `PlayResult` 계산 |

`Assets/Editor/RhythmPrototypeSetup.cs`는 게임플레이 스크립트가 아니라 Unity 에디터 보조 도구입니다. 프로젝트를 처음 열 때 `Main.unity`를 만들고 위 컴포넌트들을 루트 오브젝트에 연결합니다.

스크립트 사이의 기본 실행 흐름은 다음과 같습니다.

1. `RhythmGameController`가 세 시스템을 초기화하고 `RhythmClock`을 시작합니다.
2. `NoteManager`가 `RhythmClock.SongTime`에 맞춰 노트를 생성하고 이동합니다.
3. 키 입력이 발생하면 컨트롤러가 레인 번호와 곡 시간을 `JudgementScoreManager`에 전달합니다.
4. 판정 시스템이 `NoteManager`에서 가장 가까운 노트를 찾아 판정과 점수를 계산합니다.
5. 판정 이벤트를 받은 컨트롤러가 점수, 콤보와 판정 문구를 화면에 표시합니다.

### 실제 채보 연결하기

1. Project 창에서 우클릭합니다.
2. `Create > Rhythm Game > Chart Data`를 선택합니다.
3. 생성된 에셋에 곡 제목, Audio Clip, BPM과 노트 목록을 입력합니다.
4. `Main` 씬의 `Rhythm Game Prototype` 오브젝트를 선택합니다.
5. `Rhythm Game Controller > Chart Data` 칸에 만든 에셋을 연결합니다.

`Chart Data`를 비워 두면 기존 프로토타입과 동일하게 1개 또는 왼손·오른손 동시 노트가 무작위로 생성됩니다.

## 프로젝트 안의 파일과 폴더 역할

| 경로 | 역할 |
|---|---|
| `Assets/Editor/RhythmPrototypeSetup.cs` | `Main.unity` 자동 생성, Input Actions 연결, 빌드 씬 등록을 수행하는 에디터 전용 도구 |
| `Assets/Input/RhythmControls.inputactions` | A, S, D, J, K, L 키와 `Lane1`~`Lane6` 액션의 바인딩 정보 |
| `Assets/Scripts/` | 위에서 설명한 5개의 리듬게임 런타임 스크립트가 들어 있는 폴더 |
| `*.meta` | Unity가 에셋 참조를 유지하기 위해 자동으로 사용하는 GUID 파일이므로 삭제하거나 Git에서 제외하면 안 됨 |
| `Packages/manifest.json` | Unity Input System 1.20.0과 내장 Audio 모듈 사용 설정 |
| `ProjectSettings/ProjectVersion.txt` | 이 프로젝트를 열 Unity Editor 버전 지정 |
| `README_KO.md` | 설치, 실행, 입력 설정, 코드 구조와 문제 해결 방법을 설명하는 문서 |
| `Assets/Main.unity` | 프로젝트를 처음 열거나 메뉴 명령을 실행하면 자동 생성되는 실제 플레이 씬 |

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

### `AudioSource` 또는 `AudioClip`을 찾을 수 없을 때

이 오류는 Unity 내장 Audio 모듈이 비활성화되어 있을 때 발생합니다. 수정된 프로젝트의 `Packages/manifest.json`에는 `"com.unity.modules.audio": "1.0.0"`이 포함되어 있습니다.

기존 프로젝트를 직접 수정하려면 다음 순서로 진행합니다.

1. Unity Editor를 종료합니다.
2. `Packages/manifest.json`을 메모장으로 엽니다.
3. `dependencies` 안에 `"com.unity.modules.audio": "1.0.0"`을 추가합니다.
4. 바로 위 패키지 항목 끝에 쉼표가 있는지 확인하고 저장합니다.
5. Unity Hub에서 프로젝트를 다시 엽니다.
6. 패키지 설치와 스크립트 컴파일이 끝날 때까지 기다립니다.

### 플레이 화면이 생성되지 않을 때

1. Console의 빨간 오류가 없는지 확인합니다.
2. 오류가 없다면 `Tools > Rhythm Prototype > Create or Reset Main Scene`을 실행합니다.
3. `Assets/Main.unity`를 더블클릭합니다.

## 다음 단계

- JSON 채보를 `ChartData`로 변환하는 에디터 임포터 추가
- 롱 노트와 플릭 노트 추가
- 판정 이펙트 및 오브젝트 풀링 추가
- Canvas와 TextMeshPro 기반 정식 UI로 교체
