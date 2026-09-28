using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Unity 6000.5 2D rhythm game prototype.
// All gameplay objects live on the XY plane. Perspective is simulated by
// interpolating each note's position and size as it approaches the judgement line.
public class RhythmGamePrototype : MonoBehaviour
{
    private const int LaneCount = 6;

    [Header("2D Play Field")]
    public float nearY = -3.05f;
    public float farY = 3.15f;
    public float nearHalfWidth = 7.00f;
    public float farHalfWidth = 2.15f;
    public float judgeY = -2.78f;

    [Header("Notes")]
    public float noteTravelTime = 1.38f;
    public float minSpawnInterval = 0.52f;
    public float maxSpawnInterval = 0.78f;
    [Range(0.0f, 1.0f)]
    public float doubleNoteChance = 0.32f;

    [Header("Input System")]
    public InputActionAsset inputActions;

    private readonly string[] laneLabels = { "A", "S", "D", "J", "K", "L" };
    private readonly string[] fallbackBindings =
    {
        "<Keyboard>/a", "<Keyboard>/s", "<Keyboard>/d",
        "<Keyboard>/j", "<Keyboard>/k", "<Keyboard>/l"
    };

    private readonly List<FallingNote> notes = new List<FallingNote>();
    private readonly List<Mesh> runtimeMeshes = new List<Mesh>();

    private InputActionMap gameplayActionMap;
    private InputAction[] laneActions;
    private bool ownsRuntimeActionMap;

    private Camera gameCamera;
    private Texture2D whiteTexture;
    private Sprite whiteSprite;

    private Material boardMaterial;
    private Material railMaterial;
    private Material railGlowMaterial;
    private Material[] laneGlowMaterials;
    private MeshRenderer[] laneGlowRenderers;

    private SpriteRenderer horizonRenderer;
    private SpriteRenderer judgementHaloRenderer;
    private SpriteRenderer judgementLineRenderer;
    private float spawnTimer;
    private float judgementTimer;
    private string judgementText = "READY";
    private int score;
    private int combo;
    private int bestCombo;

    private class FallingNote
    {
        public int lane;
        public float progress;
        public float currentY;
        public GameObject gameObject;
        public SpriteRenderer renderer;
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        BuildInputActions();
        Build2DResources();
        BuildOrthographicCamera();
        BuildCyberpunkBackground();
        Build2DStage();
        spawnTimer = 0.75f;
    }

    private void OnEnable()
    {
        if (gameplayActionMap != null)
        {
            gameplayActionMap.Enable();
        }
    }

    private void OnDisable()
    {
        if (gameplayActionMap != null)
        {
            gameplayActionMap.Disable();
        }
    }

    private void BuildInputActions()
    {
        laneActions = new InputAction[LaneCount];

        if (inputActions != null)
        {
            gameplayActionMap = inputActions.FindActionMap("Gameplay", true);
            for (int i = 0; i < LaneCount; i++)
            {
                laneActions[i] = gameplayActionMap.FindAction("Lane" + (i + 1), true);
            }
            return;
        }

        ownsRuntimeActionMap = true;
        gameplayActionMap = new InputActionMap("Gameplay");
        for (int i = 0; i < LaneCount; i++)
        {
            laneActions[i] = gameplayActionMap.AddAction(
                "Lane" + (i + 1),
                InputActionType.Button,
                fallbackBindings[i]);
        }
    }

    private void Build2DResources()
    {
        whiteTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        whiteTexture.name = "Runtime White Pixel";
        whiteTexture.filterMode = FilterMode.Point;
        whiteTexture.wrapMode = TextureWrapMode.Clamp;
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();
        whiteSprite = Sprite.Create(
            whiteTexture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1.0f);
        whiteSprite.name = "Runtime White Sprite";

        Shader shader = Shader.Find("Sprites/Default");
        boardMaterial = NewMaterial(shader, new Color(0.012f, 0.018f, 0.060f, 1.0f));
        railMaterial = NewMaterial(shader, new Color(0.08f, 0.92f, 1.00f, 1.0f));
        railGlowMaterial = NewMaterial(shader, new Color(0.06f, 0.82f, 1.00f, 0.23f));

        laneGlowMaterials = new Material[LaneCount];
        laneGlowRenderers = new MeshRenderer[LaneCount];
        for (int lane = 0; lane < LaneCount; lane++)
        {
            Color color = lane < 3
                ? new Color(0.05f, 0.95f, 1.00f, 0.30f)
                : new Color(1.00f, 0.08f, 0.78f, 0.30f);
            laneGlowMaterials[lane] = NewMaterial(shader, color);
        }
    }

    private Material NewMaterial(Shader shader, Color color)
    {
        Material material = new Material(shader);
        material.color = color;
        return material;
    }

    private void BuildOrthographicCamera()
    {
        GameObject cameraObject = new GameObject("2D Rhythm Camera");
        cameraObject.transform.parent = transform;
        cameraObject.tag = "MainCamera";
        gameCamera = cameraObject.AddComponent<Camera>();
        gameCamera.orthographic = true;
        gameCamera.orthographicSize = 5.25f;
        gameCamera.clearFlags = CameraClearFlags.SolidColor;
        gameCamera.backgroundColor = new Color(0.004f, 0.006f, 0.025f, 1.0f);
        gameCamera.nearClipPlane = 0.1f;
        gameCamera.farClipPlane = 50.0f;
        cameraObject.transform.position = new Vector3(0.0f, 0.0f, -10.0f);
    }

    private void Update()
    {
        HandleLaneActions();
        UpdateLaneGlows();
        UpdateCyberpunkPulse();
        UpdateNotes();
        UpdateSpawner();

        if (judgementTimer > 0.0f)
        {
            judgementTimer -= Time.deltaTime;
        }
    }

    private void BuildCyberpunkBackground()
    {
        Color[] bands =
        {
            new Color(0.010f, 0.008f, 0.040f, 1.0f),
            new Color(0.018f, 0.009f, 0.060f, 1.0f),
            new Color(0.028f, 0.010f, 0.078f, 1.0f),
            new Color(0.022f, 0.020f, 0.082f, 1.0f),
            new Color(0.008f, 0.030f, 0.070f, 1.0f)
        };

        for (int i = 0; i < bands.Length; i++)
        {
            float y = -4.2f + i * 2.1f;
            CreateSpriteRect("Background Band", new Vector2(0.0f, y), new Vector2(19.0f, 2.15f), bands[i], -50);
        }

        for (int i = 0; i < 24; i++)
        {
            float x = -8.8f + i * 0.76f;
            float y = 2.1f + ((i * 13) % 7) * 0.31f;
            float size = 0.018f + (i % 3) * 0.012f;
            Color starColor = i % 2 == 0
                ? new Color(0.30f, 0.90f, 1.00f, 0.85f)
                : new Color(1.00f, 0.25f, 0.82f, 0.80f);
            CreateSpriteRect("Data Star", new Vector2(x, y), new Vector2(size, size), starColor, -45);
        }

        for (int i = 0; i < 18; i++)
        {
            float side = i < 9 ? -1.0f : 1.0f;
            int index = i % 9;
            float width = 0.52f + (index % 3) * 0.20f;
            float height = 1.0f + ((index * 5) % 6) * 0.42f;
            float x = side * (farHalfWidth + 1.10f + index * 0.58f);
            float y = farY + 0.05f + height * 0.5f;
            Color buildingColor = index % 3 == 0
                ? new Color(0.020f, 0.018f, 0.085f, 1.0f)
                : (index % 3 == 1
                    ? new Color(0.075f, 0.012f, 0.100f, 1.0f)
                    : new Color(0.018f, 0.055f, 0.085f, 1.0f));
            CreateSpriteRect("2D Cyber Tower", new Vector2(x, y), new Vector2(width, height), buildingColor, -35);

            // Keep city lights, but omit the nearest pair marked in red.
            if (index > 0)
            {
                Color windowColor = (index + i / 9) % 3 == 0
                    ? new Color(0.05f, 0.82f, 1.00f, 0.92f)
                    : ((index + i / 9) % 3 == 1
                        ? new Color(1.00f, 0.08f, 0.72f, 0.92f)
                        : new Color(1.00f, 0.62f, 0.10f, 0.92f));
                CreateSpriteRect(
                    "Tower Neon Strip",
                    new Vector2(x - side * width * 0.22f, y + height * 0.05f),
                    new Vector2(0.045f, height * 0.58f),
                    windowColor,
                    -34);
            }

        }

        horizonRenderer = CreateSpriteRect(
            "Digital Horizon",
            new Vector2(0.0f, farY - 0.02f),
            new Vector2(8.4f, 0.055f),
            new Color(0.30f, 0.95f, 1.00f, 0.85f),
            -25);
    }

    private void Build2DStage()
    {
        CreateQuadMesh2D(
            "2D Trapezoid Play Field",
            new Vector2(-nearHalfWidth, nearY),
            new Vector2(nearHalfWidth, nearY),
            new Vector2(-farHalfWidth, farY),
            new Vector2(farHalfWidth, farY),
            boardMaterial,
            -10);

        for (int lane = 0; lane < LaneCount; lane++)
        {
            CreateLaneGlow(lane);
        }

        for (int boundary = 0; boundary <= LaneCount; boundary++)
        {
            float ratio = (float)boundary / LaneCount;
            float nearX = Mathf.Lerp(-nearHalfWidth, nearHalfWidth, ratio);
            float farX = Mathf.Lerp(-farHalfWidth, farHalfWidth, ratio);
            CreateRail("Lane Halo " + boundary, nearX, farX, 0.070f, railGlowMaterial, -3);
            CreateRail("Lane Line " + boundary, nearX, farX, 0.018f, railMaterial, 1);
        }

        for (int i = 1; i <= 9; i++)
        {
            float t = i / 10.0f;
            float perspectiveT = t * t;
            float y = Mathf.Lerp(farY, nearY, perspectiveT);
            float halfWidth = Mathf.Lerp(farHalfWidth, nearHalfWidth, perspectiveT);
            CreateSpriteRect(
                "Perspective Grid",
                new Vector2(0.0f, y),
                new Vector2(halfWidth * 2.0f, 0.025f + perspectiveT * 0.018f),
                new Color(0.36f, 0.18f, 0.82f, 0.46f),
                -2);
        }

        float judgeHalfWidth = HalfWidthAtY(judgeY);
        judgementHaloRenderer = CreateSpriteRect(
            "2D Judgement Halo",
            new Vector2(0.0f, judgeY),
            new Vector2(judgeHalfWidth * 2.0f, 0.24f),
            new Color(1.00f, 0.08f, 0.72f, 0.18f),
            7);
        judgementLineRenderer = CreateSpriteRect(
            "2D Judgement Line",
            new Vector2(0.0f, judgeY),
            new Vector2(judgeHalfWidth * 2.0f, 0.065f),
            new Color(1.00f, 0.18f, 0.82f, 1.0f),
            8);

    }

    private void CreateLaneGlow(int lane)
    {
        float nearLaneWidth = nearHalfWidth * 2.0f / LaneCount;
        float farLaneWidth = farHalfWidth * 2.0f / LaneCount;
        float nearLeft = -nearHalfWidth + nearLaneWidth * lane + 0.04f;
        float nearRight = nearLeft + nearLaneWidth - 0.08f;
        float farLeft = -farHalfWidth + farLaneWidth * lane + 0.02f;
        float farRight = farLeft + farLaneWidth - 0.04f;

        GameObject glow = CreateQuadMesh2D(
            "Held Lane Glow " + (lane + 1),
            new Vector2(nearLeft, nearY),
            new Vector2(nearRight, nearY),
            new Vector2(farLeft, farY),
            new Vector2(farRight, farY),
            laneGlowMaterials[lane],
            -1);

        laneGlowRenderers[lane] = glow.GetComponent<MeshRenderer>();
        laneGlowRenderers[lane].enabled = false;
    }

    private void CreateRail(string objectName, float nearX, float farX, float halfThickness, Material material, int sortingOrder)
    {
        CreateQuadMesh2D(
            objectName,
            new Vector2(nearX - halfThickness, nearY),
            new Vector2(nearX + halfThickness, nearY),
            new Vector2(farX - halfThickness * 0.45f, farY),
            new Vector2(farX + halfThickness * 0.45f, farY),
            material,
            sortingOrder);
    }

    private GameObject CreateQuadMesh2D(
        string objectName,
        Vector2 bottomLeft,
        Vector2 bottomRight,
        Vector2 topLeft,
        Vector2 topRight,
        Material material,
        int sortingOrder)
    {
        GameObject meshObject = new GameObject(objectName);
        meshObject.transform.parent = transform;
        MeshFilter filter = meshObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = meshObject.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        mesh.name = objectName + " Mesh";
        mesh.vertices = new Vector3[]
        {
            new Vector3(bottomLeft.x, bottomLeft.y, 0.0f),
            new Vector3(bottomRight.x, bottomRight.y, 0.0f),
            new Vector3(topLeft.x, topLeft.y, 0.0f),
            new Vector3(topRight.x, topRight.y, 0.0f)
        };
        mesh.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
        mesh.RecalculateBounds();
        runtimeMeshes.Add(mesh);
        filter.sharedMesh = mesh;
        renderer.sharedMaterial = material;
        renderer.sortingOrder = sortingOrder;
        return meshObject;
    }

    private SpriteRenderer CreateSpriteRect(string objectName, Vector2 position, Vector2 size, Color color, int sortingOrder)
    {
        GameObject spriteObject = new GameObject(objectName);
        spriteObject.transform.parent = transform;
        spriteObject.transform.position = new Vector3(position.x, position.y, 0.0f);
        spriteObject.transform.localScale = new Vector3(size.x, size.y, 1.0f);
        SpriteRenderer renderer = spriteObject.AddComponent<SpriteRenderer>();
        renderer.sprite = whiteSprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private void UpdateSpawner()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0.0f)
        {
            return;
        }

        if (Random.value < doubleNoteChance)
        {
            int leftLane = Random.Range(0, 3);
            int rightLane = Random.Range(3, 6);
            SpawnNote(leftLane);
            SpawnNote(rightLane);
        }
        else
        {
            SpawnNote(Random.Range(0, LaneCount));
        }

        spawnTimer = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    private void SpawnNote(int lane)
    {
        Color noteColor = lane < 3
            ? new Color(0.12f, 1.00f, 0.92f, 1.0f)
            : new Color(1.00f, 0.18f, 0.82f, 1.0f);
        SpriteRenderer renderer = CreateSpriteRect(
            "2D Note Lane " + (lane + 1),
            new Vector2(0.0f, farY),
            new Vector2(0.40f, 0.08f),
            noteColor,
            25);

        FallingNote note = new FallingNote();
        note.lane = lane;
        note.progress = 0.0f;
        note.currentY = farY;
        note.gameObject = renderer.gameObject;
        note.renderer = renderer;
        notes.Add(note);
        UpdateNoteVisual(note);
    }

    private void UpdateNotes()
    {
        float safeTravelTime = Mathf.Max(0.15f, noteTravelTime);
        for (int i = notes.Count - 1; i >= 0; i--)
        {
            FallingNote note = notes[i];
            note.progress += Time.deltaTime / safeTravelTime;

            if (note.progress > 1.06f)
            {
                combo = 0;
                ShowJudgement("MISS");
                RemoveNoteAt(i);
                continue;
            }

            UpdateNoteVisual(note);
        }
    }

    private void UpdateNoteVisual(FallingNote note)
    {
        float t = Mathf.Clamp01(note.progress);
        // Linear interpolation keeps the falling speed constant all the way to
        // the bottom. SmoothStep caused an unwanted slowdown near the judgement line.
        float halfWidth = Mathf.Lerp(farHalfWidth, nearHalfWidth, t);
        float laneWidth = halfWidth * 2.0f / LaneCount;
        float x = -halfWidth + laneWidth * (note.lane + 0.5f);
        float y = Mathf.Lerp(farY, nearY, t);

        note.currentY = y;
        note.gameObject.transform.position = new Vector3(x, y, 0.0f);
        note.gameObject.transform.localScale = new Vector3(
            laneWidth * 0.78f,
            Mathf.Lerp(0.075f, 0.19f, t),
            1.0f);

        float pulse = 0.90f + Mathf.Sin(Time.time * 18.0f + note.lane) * 0.10f;
        Color baseColor = note.lane < 3
            ? new Color(0.12f, 1.00f, 0.92f, 1.0f)
            : new Color(1.00f, 0.18f, 0.82f, 1.0f);
        note.renderer.color = new Color(baseColor.r * pulse, baseColor.g * pulse, baseColor.b * pulse, 1.0f);
    }

    private void HandleLaneActions()
    {
        for (int lane = 0; lane < LaneCount; lane++)
        {
            if (laneActions[lane].WasPressedThisFrame())
            {
                TryHitLane(lane);
            }
        }
    }

    private bool IsLaneHeld(int lane)
    {
        return laneActions != null && laneActions[lane] != null && laneActions[lane].IsPressed();
    }

    private void UpdateLaneGlows()
    {
        float alpha = 0.25f + Mathf.Sin(Time.time * 12.0f) * 0.08f;
        for (int lane = 0; lane < LaneCount; lane++)
        {
            bool held = IsLaneHeld(lane);
            laneGlowRenderers[lane].enabled = held;
            if (held)
            {
                laneGlowMaterials[lane].color = lane < 3
                    ? new Color(0.05f, 0.95f, 1.00f, alpha)
                    : new Color(1.00f, 0.08f, 0.78f, alpha);
            }
        }
    }

    private void UpdateCyberpunkPulse()
    {
        float pulse = 0.72f + Mathf.Sin(Time.time * 6.0f) * 0.28f;
        if (judgementLineRenderer != null)
        {
            judgementLineRenderer.color = new Color(1.0f, 0.10f + pulse * 0.10f, 0.62f + pulse * 0.25f, 1.0f);
        }
        if (judgementHaloRenderer != null)
        {
            judgementHaloRenderer.color = new Color(1.0f, 0.05f, 0.72f, 0.10f + pulse * 0.14f);
        }
        if (horizonRenderer != null)
        {
            horizonRenderer.color = new Color(0.20f + pulse * 0.12f, 0.72f + pulse * 0.25f, 1.0f, 0.75f + pulse * 0.20f);
        }
    }

    private void TryHitLane(int lane)
    {
        int bestIndex = -1;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i].lane != lane)
            {
                continue;
            }

            float distance = Mathf.Abs(notes[i].currentY - judgeY);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;
            }
        }

        if (bestIndex < 0 || bestDistance > 0.52f)
        {
            return;
        }

        if (bestDistance <= 0.22f)
        {
            score += 1000;
            ShowJudgement("PERFECT");
        }
        else
        {
            score += 500;
            ShowJudgement("GOOD");
        }

        combo++;
        bestCombo = Mathf.Max(bestCombo, combo);
        RemoveNoteAt(bestIndex);
    }

    private void RemoveNoteAt(int index)
    {
        FallingNote note = notes[index];
        notes.RemoveAt(index);
        Destroy(note.gameObject);
    }

    private void ShowJudgement(string text)
    {
        judgementText = text;
        judgementTimer = 0.45f;
    }

    private float HalfWidthAtY(float y)
    {
        float t = Mathf.InverseLerp(farY, nearY, y);
        return Mathf.Lerp(farHalfWidth, nearHalfWidth, t);
    }

    private void OnGUI()
    {
        DrawScreenRect(new Rect(14, 12, 365, 112), new Color(0.01f, 0.02f, 0.06f, 0.82f));
        DrawScreenRect(new Rect(14, 12, 5, 112), new Color(0.05f, 0.95f, 1.0f, 0.95f));
        DrawScreenRect(new Rect(Screen.width - 230, 18, 210, 34), new Color(0.05f, 0.01f, 0.08f, 0.78f));

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = Mathf.Max(20, Screen.height / 32);
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = new Color(0.30f, 1.0f, 0.95f);

        GUIStyle infoStyle = new GUIStyle(GUI.skin.label);
        infoStyle.fontSize = Mathf.Max(15, Screen.height / 48);
        infoStyle.normal.textColor = new Color(0.60f, 0.95f, 1.0f);

        GUI.Label(new Rect(28, 18, 500, 45), "CYBER//RHYTHM 2D", titleStyle);
        GUI.Label(new Rect(24, 58, 450, 28), "SCORE  " + score, infoStyle);
        GUI.Label(new Rect(24, 84, 450, 28), "COMBO  " + combo + "    BEST  " + bestCombo, infoStyle);

        GUIStyle statusStyle = new GUIStyle(infoStyle);
        statusStyle.alignment = TextAnchor.MiddleCenter;
        statusStyle.normal.textColor = new Color(1.0f, 0.25f, 0.80f);
        GUI.Label(new Rect(Screen.width - 230, 20, 210, 28), "2D SYSTEM // ONLINE", statusStyle);

        if (judgementTimer > 0.0f || judgementText == "READY")
        {
            GUIStyle judgeStyle = new GUIStyle(titleStyle);
            judgeStyle.alignment = TextAnchor.MiddleCenter;
            judgeStyle.fontSize = Mathf.Max(28, Screen.height / 20);
            judgeStyle.normal.textColor = judgementText == "MISS"
                ? new Color(1.0f, 0.35f, 0.45f)
                : new Color(0.40f, 1.0f, 0.90f);
            GUI.Label(new Rect(0, Screen.height * 0.45f, Screen.width, 70), judgementText, judgeStyle);
        }

        DrawKeyHints();
    }

    private void DrawScreenRect(Rect rect, Color color)
    {
        Color oldColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = oldColor;
    }

    private void DrawKeyHints()
    {
        GUIStyle keyStyle = new GUIStyle(GUI.skin.box);
        keyStyle.fontSize = Mathf.Max(16, Screen.height / 42);
        keyStyle.fontStyle = FontStyle.Bold;
        keyStyle.alignment = TextAnchor.MiddleCenter;
        keyStyle.normal.textColor = Color.white;

        float totalWidth = Mathf.Min(Screen.width * 0.72f, 720.0f);
        float keyWidth = totalWidth / LaneCount;
        float startX = (Screen.width - totalWidth) * 0.5f;
        float y = Screen.height - 62.0f;

        for (int lane = 0; lane < LaneCount; lane++)
        {
            GUI.backgroundColor = IsLaneHeld(lane)
                ? (lane < 3 ? new Color(0.05f, 1.0f, 1.0f) : new Color(1.0f, 0.08f, 0.78f))
                : new Color(0.16f, 0.12f, 0.28f);
            GUI.Box(
                new Rect(startX + keyWidth * lane + 3.0f, y, keyWidth - 6.0f, 42.0f),
                laneLabels[lane],
                keyStyle);
        }
        GUI.backgroundColor = Color.white;
    }

    private void OnDestroy()
    {
        if (ownsRuntimeActionMap && gameplayActionMap != null)
        {
            gameplayActionMap.Dispose();
        }

        for (int i = 0; i < runtimeMeshes.Count; i++)
        {
            Destroy(runtimeMeshes[i]);
        }

        Destroy(boardMaterial);
        Destroy(railMaterial);
        Destroy(railGlowMaterial);

        if (laneGlowMaterials != null)
        {
            for (int i = 0; i < laneGlowMaterials.Length; i++)
            {
                Destroy(laneGlowMaterials[i]);
            }
        }

        Destroy(whiteSprite);
        Destroy(whiteTexture);
    }
}
