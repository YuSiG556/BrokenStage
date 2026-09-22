using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Unity 6000.5 compatible, asset-free rhythm game prototype.
// Add this component to one GameObject and press Play.
public class RhythmGamePrototype : MonoBehaviour
{
    private const int LaneCount = 6;

    [Header("Play field")]
    public float nearZ = 0.0f;
    public float farZ = 20.0f;
    public float nearHalfWidth = 5.8f;
    public float farHalfWidth = 2.35f;
    public float judgeZ = 1.15f;

    [Header("Notes")]
    public float noteSpeed = 15.0f;
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

    private InputActionMap gameplayActionMap;
    private InputAction[] laneActions;
    private bool ownsRuntimeActionMap;

    private Material boardMaterial;
    private Material sideFloorMaterial;
    private Material railMaterial;
    private Material railGlowMaterial;
    private Material gridMaterial;
    private Material judgeMaterial;
    private Material noteMaterialA;
    private Material noteMaterialB;
    private Material cityMaterialA;
    private Material cityMaterialB;
    private Material cityCyanMaterial;
    private Material cityMagentaMaterial;
    private Material[] laneGlowMaterials;
    private MeshRenderer[] laneGlowRenderers;
    private Camera gameCamera;
    private float spawnTimer;
    private float judgementTimer;
    private string judgementText = "READY";
    private int score;
    private int combo;
    private int bestCombo;

    private class FallingNote
    {
        public int lane;
        public float z;
        public GameObject gameObject;
        public Mesh mesh;
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        BuildInputActions();
        BuildMaterials();
        BuildCamera();
        BuildStage();
        spawnTimer = 0.6f;
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

        // Fallback for a manually created scene without the inputactions asset.
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

    private void BuildMaterials()
    {
        Shader shader = Shader.Find("Unlit/Color");
        Shader transparentShader = Shader.Find("Sprites/Default");
        if (transparentShader == null)
        {
            transparentShader = shader;
        }

        boardMaterial = NewMaterial(shader, new Color(0.012f, 0.018f, 0.055f, 1.0f));
        sideFloorMaterial = NewMaterial(shader, new Color(0.018f, 0.008f, 0.040f, 1.0f));
        railMaterial = NewMaterial(shader, new Color(0.10f, 0.92f, 1.00f, 1.0f));
        railGlowMaterial = NewMaterial(transparentShader, new Color(0.10f, 0.80f, 1.00f, 0.22f));
        gridMaterial = NewMaterial(transparentShader, new Color(0.25f, 0.18f, 0.70f, 0.38f));
        judgeMaterial = NewMaterial(shader, new Color(1.00f, 0.18f, 0.78f, 1.0f));
        noteMaterialA = NewMaterial(shader, new Color(0.16f, 1.00f, 0.92f, 1.0f));
        noteMaterialB = NewMaterial(shader, new Color(1.00f, 0.20f, 0.82f, 1.0f));
        cityMaterialA = NewMaterial(shader, new Color(0.025f, 0.012f, 0.075f, 1.0f));
        cityMaterialB = NewMaterial(shader, new Color(0.055f, 0.015f, 0.105f, 1.0f));
        cityCyanMaterial = NewMaterial(shader, new Color(0.05f, 0.75f, 0.92f, 1.0f));
        cityMagentaMaterial = NewMaterial(shader, new Color(0.95f, 0.08f, 0.72f, 1.0f));

        laneGlowMaterials = new Material[LaneCount];
        laneGlowRenderers = new MeshRenderer[LaneCount];
        for (int i = 0; i < LaneCount; i++)
        {
            Color glowColor = i < 3
                ? new Color(0.05f, 0.95f, 1.00f, 0.32f)
                : new Color(1.00f, 0.08f, 0.78f, 0.32f);
            laneGlowMaterials[i] = NewMaterial(transparentShader, glowColor);
        }
    }

    private Material NewMaterial(Shader shader, Color color)
    {
        Material material = new Material(shader);
        material.color = color;
        return material;
    }

    private void BuildCamera()
    {
        GameObject cameraObject = new GameObject("Rhythm Camera");
        cameraObject.tag = "MainCamera";
        gameCamera = cameraObject.AddComponent<Camera>();
        gameCamera.clearFlags = CameraClearFlags.SolidColor;
        gameCamera.backgroundColor = new Color(0.008f, 0.012f, 0.035f, 1.0f);
        gameCamera.fieldOfView = 55.0f;
        gameCamera.nearClipPlane = 0.1f;
        gameCamera.farClipPlane = 100.0f;
        cameraObject.transform.position = new Vector3(0.0f, 8.0f, -9.2f);
        cameraObject.transform.rotation = Quaternion.LookRotation(
            new Vector3(0.0f, 0.0f, 8.2f) - cameraObject.transform.position,
            Vector3.up);
    }

    private void BuildStage()
    {
        CreateQuadMesh(
            "Left Cyber Deck",
            new Vector3(-13.0f, -0.03f, nearZ),
            new Vector3(-nearHalfWidth, -0.03f, nearZ),
            new Vector3(-13.0f, -0.03f, farZ + 5.0f),
            new Vector3(-farHalfWidth, -0.03f, farZ + 5.0f),
            sideFloorMaterial);

        CreateQuadMesh(
            "Right Cyber Deck",
            new Vector3(nearHalfWidth, -0.03f, nearZ),
            new Vector3(13.0f, -0.03f, nearZ),
            new Vector3(farHalfWidth, -0.03f, farZ + 5.0f),
            new Vector3(13.0f, -0.03f, farZ + 5.0f),
            sideFloorMaterial);

        CreateQuadMesh(
            "Play Field",
            new Vector3(-nearHalfWidth, 0.0f, nearZ),
            new Vector3(nearHalfWidth, 0.0f, nearZ),
            new Vector3(-farHalfWidth, 0.0f, farZ),
            new Vector3(farHalfWidth, 0.0f, farZ),
            boardMaterial);

        for (int i = 0; i <= LaneCount; i++)
        {
            float ratio = (float)i / LaneCount;
            float nearX = Mathf.Lerp(-nearHalfWidth, nearHalfWidth, ratio);
            float farX = Mathf.Lerp(-farHalfWidth, farHalfWidth, ratio);
            CreateRail("Lane Glow Rail " + i, nearX, farX, 0.085f, railGlowMaterial);
            CreateRail("Lane Rail " + i, nearX, farX, 0.025f, railMaterial);
        }

        for (int lane = 0; lane < LaneCount; lane++)
        {
            CreateLaneGlow(lane);
        }

        for (float z = 2.6f; z < farZ - 0.5f; z += 2.45f)
        {
            CreateGridBar(z);
        }

        CreateHorizontalBar("Judgement Line", judgeZ, 0.0f, judgeMaterial);
        CreateHorizontalBar("Far Gate", farZ - 0.15f, -0.01f, railMaterial);
        BuildCyberCity();
        BuildHorizonPortal();
    }

    private void CreateLaneGlow(int lane)
    {
        float nearLaneWidth = nearHalfWidth * 2.0f / LaneCount;
        float farLaneWidth = farHalfWidth * 2.0f / LaneCount;
        float nearLeft = -nearHalfWidth + nearLaneWidth * lane + 0.05f;
        float nearRight = nearLeft + nearLaneWidth - 0.10f;
        float farLeft = -farHalfWidth + farLaneWidth * lane + 0.025f;
        float farRight = farLeft + farLaneWidth - 0.05f;

        GameObject glow = CreateQuadMesh(
            "Held Lane Glow " + (lane + 1),
            new Vector3(nearLeft, 0.018f, nearZ),
            new Vector3(nearRight, 0.018f, nearZ),
            new Vector3(farLeft, 0.018f, farZ),
            new Vector3(farRight, 0.018f, farZ),
            laneGlowMaterials[lane]);

        laneGlowRenderers[lane] = glow.GetComponent<MeshRenderer>();
        laneGlowRenderers[lane].enabled = false;
    }

    private void CreateGridBar(float z)
    {
        float halfWidth = HalfWidthAtZ(z);
        float depth = 0.025f;
        CreateQuadMesh(
            "Cyber Grid " + z,
            new Vector3(-halfWidth, 0.012f, z - depth),
            new Vector3(halfWidth, 0.012f, z - depth),
            new Vector3(-halfWidth, 0.012f, z + depth),
            new Vector3(halfWidth, 0.012f, z + depth),
            gridMaterial);
    }

    private void BuildCyberCity()
    {
        for (int sideIndex = 0; sideIndex < 2; sideIndex++)
        {
            float side = sideIndex == 0 ? -1.0f : 1.0f;
            for (int i = 0; i < 9; i++)
            {
                float z = 1.8f + i * 2.65f;
                float height = 1.4f + ((i * 7) % 5) * 0.72f;
                float width = 0.72f + (i % 3) * 0.23f;
                float x = side * (HalfWidthAtZ(Mathf.Min(z, farZ)) + 1.55f + (i % 2) * 0.48f);
                Material buildingMaterial = i % 2 == 0 ? cityMaterialA : cityMaterialB;
                CreateBox("Cyber Tower", new Vector3(x, height * 0.5f, z), new Vector3(width, height, 0.85f), buildingMaterial);

                Material accent = (i + sideIndex) % 2 == 0 ? cityCyanMaterial : cityMagentaMaterial;
                float innerFaceX = x - side * (width * 0.52f);
                CreateBox(
                    "Tower Neon",
                    new Vector3(innerFaceX, height * 0.58f, z - 0.44f),
                    new Vector3(0.055f, height * 0.55f, 0.035f),
                    accent);
            }
        }
    }

    private void BuildHorizonPortal()
    {
        float z = farZ + 0.35f;
        CreateBox("Horizon Left", new Vector3(-farHalfWidth - 0.35f, 1.7f, z), new Vector3(0.10f, 3.4f, 0.10f), cityCyanMaterial);
        CreateBox("Horizon Right", new Vector3(farHalfWidth + 0.35f, 1.7f, z), new Vector3(0.10f, 3.4f, 0.10f), cityMagentaMaterial);
        CreateBox("Horizon Top", new Vector3(0.0f, 3.4f, z), new Vector3(farHalfWidth * 2.0f + 0.8f, 0.10f, 0.10f), railMaterial);
    }

    private GameObject CreateBox(string objectName, Vector3 position, Vector3 scale, Material material)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = objectName;
        box.transform.parent = transform;
        box.transform.position = position;
        box.transform.localScale = scale;
        box.GetComponent<MeshRenderer>().sharedMaterial = material;
        Collider boxCollider = box.GetComponent<Collider>();
        if (boxCollider != null)
        {
            Destroy(boxCollider);
        }
        return box;
    }

    private void CreateRail(string objectName, float nearX, float farX, float halfThickness, Material material)
    {
        CreateQuadMesh(
            objectName,
            new Vector3(nearX - halfThickness, 0.025f, nearZ),
            new Vector3(nearX + halfThickness, 0.025f, nearZ),
            new Vector3(farX - halfThickness * 0.45f, 0.025f, farZ),
            new Vector3(farX + halfThickness * 0.45f, 0.025f, farZ),
            material);
    }

    private void CreateHorizontalBar(string objectName, float z, float y, Material material)
    {
        float halfWidth = HalfWidthAtZ(z);
        float depth = 0.13f;
        CreateQuadMesh(
            objectName,
            new Vector3(-halfWidth, y + 0.045f, z - depth),
            new Vector3(halfWidth, y + 0.045f, z - depth),
            new Vector3(-halfWidth, y + 0.045f, z + depth),
            new Vector3(halfWidth, y + 0.045f, z + depth),
            material);
    }

    private GameObject CreateQuadMesh(
        string objectName,
        Vector3 nearLeft,
        Vector3 nearRight,
        Vector3 farLeft,
        Vector3 farRight,
        Material material)
    {
        GameObject meshObject = new GameObject(objectName);
        meshObject.transform.parent = transform;
        MeshFilter filter = meshObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = meshObject.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        mesh.name = objectName + " Mesh";
        mesh.vertices = new Vector3[] { nearLeft, nearRight, farLeft, farRight };
        mesh.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
        mesh.RecalculateBounds();
        filter.sharedMesh = mesh;
        renderer.sharedMaterial = material;
        return meshObject;
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
            // Double notes always use one left-hand lane and one right-hand lane.
            int leftLane = Random.Range(0, 3);
            int rightLane = Random.Range(3, 6);
            SpawnNote(leftLane, noteMaterialA);
            SpawnNote(rightLane, noteMaterialB);
        }
        else
        {
            int lane = Random.Range(0, LaneCount);
            SpawnNote(lane, lane < 3 ? noteMaterialA : noteMaterialB);
        }

        spawnTimer = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    private void SpawnNote(int lane, Material material)
    {
        GameObject noteObject = new GameObject("Note Lane " + (lane + 1));
        noteObject.transform.parent = transform;
        MeshFilter filter = noteObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = noteObject.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        mesh.name = "Falling Note Mesh";
        filter.sharedMesh = mesh;
        renderer.sharedMaterial = material;

        FallingNote note = new FallingNote();
        note.lane = lane;
        note.z = farZ - 0.35f;
        note.gameObject = noteObject;
        note.mesh = mesh;
        notes.Add(note);
        UpdateNoteMesh(note);
    }

    private void UpdateNotes()
    {
        for (int i = notes.Count - 1; i >= 0; i--)
        {
            FallingNote note = notes[i];
            note.z -= noteSpeed * Time.deltaTime;

            if (note.z < nearZ - 0.75f)
            {
                combo = 0;
                ShowJudgement("MISS");
                RemoveNoteAt(i);
                continue;
            }

            UpdateNoteMesh(note);
        }
    }

    private void UpdateNoteMesh(FallingNote note)
    {
        float halfWidth = HalfWidthAtZ(note.z);
        float laneWidth = halfWidth * 2.0f / LaneCount;
        float centerX = -halfWidth + laneWidth * (note.lane + 0.5f);
        float noteHalfWidth = laneWidth * 0.39f;
        float halfDepth = Mathf.Lerp(0.10f, 0.24f, 1.0f - NormalizedZ(note.z));
        float y = 0.10f;

        note.mesh.Clear();
        note.mesh.vertices = new Vector3[]
        {
            new Vector3(centerX - noteHalfWidth, y, note.z - halfDepth),
            new Vector3(centerX + noteHalfWidth, y, note.z - halfDepth),
            new Vector3(centerX - noteHalfWidth, y, note.z + halfDepth),
            new Vector3(centerX + noteHalfWidth, y, note.z + halfDepth)
        };
        note.mesh.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
        note.mesh.RecalculateBounds();
    }

    private void HandleLaneActions()
    {
        for (int lane = 0; lane < LaneCount; lane++)
        {
            if (ReadLaneActionDown(lane))
            {
                TryHitLane(lane);
            }
        }
    }

    private bool ReadLaneActionDown(int lane)
    {
        return laneActions[lane].WasPressedThisFrame();
    }

    private bool IsLaneHeld(int lane)
    {
        return laneActions != null && laneActions[lane] != null && laneActions[lane].IsPressed();
    }

    private void UpdateLaneGlows()
    {
        if (laneGlowRenderers == null)
        {
            return;
        }

        float pulse = 0.28f + Mathf.Sin(Time.time * 12.0f) * 0.08f;
        for (int lane = 0; lane < LaneCount; lane++)
        {
            bool held = IsLaneHeld(lane);
            laneGlowRenderers[lane].enabled = held;
            if (held)
            {
                Color glowColor = lane < 3
                    ? new Color(0.05f, 0.95f, 1.00f, pulse)
                    : new Color(1.00f, 0.08f, 0.78f, pulse);
                laneGlowMaterials[lane].color = glowColor;
            }
        }
    }

    private void UpdateCyberpunkPulse()
    {
        float pulse = 0.78f + Mathf.Sin(Time.time * 7.0f) * 0.22f;
        judgeMaterial.color = new Color(1.0f, 0.10f + pulse * 0.12f, 0.62f + pulse * 0.20f, 1.0f);
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

            float distance = Mathf.Abs(notes[i].z - judgeZ);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;
            }
        }

        if (bestIndex < 0 || bestDistance > 1.15f)
        {
            return;
        }

        if (bestDistance <= 0.45f)
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

    private float HalfWidthAtZ(float z)
    {
        return Mathf.Lerp(nearHalfWidth, farHalfWidth, NormalizedZ(z));
    }

    private float NormalizedZ(float z)
    {
        return Mathf.Clamp01((z - nearZ) / (farZ - nearZ));
    }

    private void OnGUI()
    {
        DrawScreenRect(new Rect(14, 12, 365, 112), new Color(0.01f, 0.02f, 0.06f, 0.78f));
        DrawScreenRect(new Rect(14, 12, 5, 112), new Color(0.05f, 0.95f, 1.0f, 0.95f));
        DrawScreenRect(new Rect(Screen.width - 230, 18, 210, 34), new Color(0.05f, 0.01f, 0.08f, 0.72f));

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = Mathf.Max(20, Screen.height / 32);
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;

        GUIStyle infoStyle = new GUIStyle(GUI.skin.label);
        infoStyle.fontSize = Mathf.Max(15, Screen.height / 48);
        infoStyle.normal.textColor = new Color(0.60f, 0.95f, 1.0f);

        titleStyle.normal.textColor = new Color(0.30f, 1.0f, 0.95f);
        GUI.Label(new Rect(28, 18, 500, 45), "CYBER//RHYTHM", titleStyle);
        GUI.Label(new Rect(24, 58, 450, 28), "SCORE  " + score, infoStyle);
        GUI.Label(new Rect(24, 84, 450, 28), "COMBO  " + combo + "    BEST  " + bestCombo, infoStyle);

        GUIStyle statusStyle = new GUIStyle(infoStyle);
        statusStyle.alignment = TextAnchor.MiddleCenter;
        statusStyle.normal.textColor = new Color(1.0f, 0.25f, 0.80f);
        GUI.Label(new Rect(Screen.width - 230, 20, 210, 28), "SYSTEM // ONLINE", statusStyle);

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

        for (int i = 0; i < LaneCount; i++)
        {
            GUI.backgroundColor = IsLaneHeld(i)
                ? (i < 3 ? new Color(0.05f, 1.0f, 1.0f) : new Color(1.0f, 0.08f, 0.78f))
                : new Color(0.16f, 0.12f, 0.28f);
            GUI.Box(new Rect(startX + keyWidth * i + 3.0f, y, keyWidth - 6.0f, 42.0f), laneLabels[i], keyStyle);
        }
        GUI.backgroundColor = Color.white;
    }

    private void OnDestroy()
    {
        if (ownsRuntimeActionMap && gameplayActionMap != null)
        {
            gameplayActionMap.Dispose();
        }

        Destroy(boardMaterial);
        Destroy(sideFloorMaterial);
        Destroy(railMaterial);
        Destroy(railGlowMaterial);
        Destroy(gridMaterial);
        Destroy(judgeMaterial);
        Destroy(noteMaterialA);
        Destroy(noteMaterialB);
        Destroy(cityMaterialA);
        Destroy(cityMaterialB);
        Destroy(cityCyanMaterial);
        Destroy(cityMagentaMaterial);

        if (laneGlowMaterials != null)
        {
            for (int i = 0; i < laneGlowMaterials.Length; i++)
            {
                Destroy(laneGlowMaterials[i]);
            }
        }
    }
}
