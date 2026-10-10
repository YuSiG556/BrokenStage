// 파일의 존재 여부를 확인하기 위해 System.IO 네임스페이스를 불러온다.
using System.IO;
// 메뉴 추가, 에셋 검색 등 Unity 에디터 전용 기능을 사용한다.
using UnityEditor;
// 에디터에서 새 씬을 만들고 저장하기 위한 기능을 사용한다.
using UnityEditor.SceneManagement;
// GameObject, Debug 등 Unity의 기본 런타임 형식을 사용한다.
using UnityEngine;
// InputActionAsset 형식을 사용하기 위해 새 Input System 네임스페이스를 불러온다.
using UnityEngine.InputSystem;
// Scene 형식을 사용하기 위한 씬 관리 네임스페이스를 불러온다.
using UnityEngine.SceneManagement;

// Unity 에디터가 스크립트를 불러오자마자 이 정적 클래스를 초기화한다.
[InitializeOnLoad]
public static class RhythmPrototypeSetup
{
    // 자동으로 만들거나 초기화할 메인 씬의 프로젝트 내부 경로다.
    private const string ScenePath = "Assets/Main.unity";

    // 에디터가 이 클래스를 처음 불러올 때 한 번 실행되는 정적 생성자다.
    static RhythmPrototypeSetup()
    {
        // 에디터 초기화가 끝난 다음 프레임에 씬 존재 여부를 검사하도록 예약한다.
        EditorApplication.delayCall += CreateSceneOnFirstOpen;
    }

    // 프로젝트를 처음 열었을 때 Main 씬이 없으면 프로토타입 씬을 자동 생성한다.
    private static void CreateSceneOnFirstOpen()
    {
        // 지정한 경로에 Main 씬 파일이 아직 없는 경우만 새 씬을 만든다.
        if (!File.Exists(ScenePath))
        {
            // 아래의 공용 씬 생성 함수를 호출한다.
            CreatePrototypeScene();
        }
    }

    // Unity 상단 메뉴에 수동으로 씬을 다시 만드는 명령을 추가한다.
    [MenuItem("Tools/Rhythm Prototype/Create or Reset Main Scene")]
    public static void CreatePrototypeScene()
    {
        // 프로젝트에서 사용할 Input Actions 에셋 경로를 상수로 보관한다.
        const string inputAssetPath = "Assets/Input/RhythmControls.inputactions";
        // Input Actions 에셋이 최신 상태로 인식되도록 강제로 다시 임포트한다.
        AssetDatabase.ImportAsset(inputAssetPath, ImportAssetOptions.ForceUpdate);

        // 현재 씬을 비우고 완전히 새로운 단일 씬을 만든다.
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        // 모든 리듬게임 오브젝트의 부모가 될 루트 GameObject를 만든다.
        GameObject root = new GameObject("Rhythm Game Prototype");
        // 루트 오브젝트에 새 리듬게임 컨트롤러를 추가한다.
        // RequireComponent 속성에 의해 RhythmClock, NoteManager,
        // JudgementScoreManager와 ChartAutoLoader도 같은 오브젝트에 자동으로 추가된다.
        RhythmGameController prototype = root.AddComponent<RhythmGameController>();
        // 프로젝트의 InputActionAsset을 불러와 게임 컴포넌트의 공개 필드에 연결한다.
        prototype.inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
            inputAssetPath);
        // 완성된 씬을 Assets/Main.unity 경로에 저장한다.
        EditorSceneManager.SaveScene(scene, ScenePath);

        // 빌드 설정의 씬 목록을 Main 씬 하나로 지정한다.
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            // true는 이 씬이 실제 빌드에 포함된다는 뜻이다.
            new EditorBuildSettingsScene(ScenePath, true)
        };

        // Hierarchy에서 방금 만든 루트 오브젝트가 선택된 상태가 되도록 한다.
        Selection.activeGameObject = root;
        // 씬 생성이 끝났음을 Console 창에 알린다.
        Debug.Log("Rhythm prototype scene created. Press the Play button.");
    }
}
