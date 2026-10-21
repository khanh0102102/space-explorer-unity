using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Tự động dựng project Space Explorer: 3 scene, 4 prefab, UI tiếng Việt,
/// import sprite Kenney và tạo hình dự phòng nếu asset chưa được chép vào.
/// Chạy từ menu Tools > Space Explorer > Tạo toàn bộ dự án.
/// </summary>
public static class SpaceExplorerProjectSetup
{
    private const string ArtRoot = "Assets/Art/KenneySpaceShooter";
    private const string GeneratedRoot = "Assets/Art/Generated";
    private const string PrefabRoot = "Assets/Prefabs";
    private const string SceneRoot = "Assets/Scenes";
    private const string MenuScenePath = SceneRoot + "/MainMenu.unity";
    private const string GameplayScenePath = SceneRoot + "/Gameplay.unity";
    private const string EndScenePath = SceneRoot + "/EndGame.unity";

    private const string LaserPrefabPath = PrefabRoot + "/Laser.prefab";
    private const string AsteroidPrefabPath = PrefabRoot + "/Asteroid.prefab";
    private const string StarPrefabPath = PrefabRoot + "/Star.prefab";
    private const string PlayerPrefabPath = PrefabRoot + "/PlayerShip.prefab";

    private static string AbsoluteProjectPath(string relativePath)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    [MenuItem("Tools/Space Explorer/Tạo toàn bộ dự án")]
    public static void SetupAll()
    {
        if (!ConfirmRegenerationIfNeeded())
            return;

        try
        {
            EnsureFolders();
            CopyKenneyAssetsFromLocalDownload();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ImportKenneySprites();

            Sprite background = LoadKenneyOrFallback("Backgrounds/darkPurple.png", "FallbackBackground");
            Sprite ship = LoadKenneyOrFallback("PNG/playerShip1_blue.png", "FallbackShip");
            Sprite asteroid = LoadKenneyOrFallback("PNG/Meteors/meteorBrown_big1.png", "FallbackAsteroid");
            Sprite star = LoadKenneyOrFallback("PNG/Power-ups/star_gold.png", "FallbackStar");
            Sprite laser = LoadKenneyOrFallback("PNG/Lasers/laserBlue01.png", "FallbackLaser");

            AudioClip laserSound = AssetDatabase.LoadAssetAtPath<AudioClip>(ArtRoot + "/Bonus/sfx_laser1.ogg");
            AudioClip loseSound = AssetDatabase.LoadAssetAtPath<AudioClip>(ArtRoot + "/Bonus/sfx_lose.ogg");
            AudioClip collectSound = AssetDatabase.LoadAssetAtPath<AudioClip>(ArtRoot + "/Bonus/sfx_twoTone.ogg");

            GameObject laserPrefab = BuildLaserPrefab(laser);
            GameObject asteroidPrefab = BuildAsteroidPrefab(asteroid);
            GameObject starPrefab = BuildStarPrefab(star, collectSound);
            GameObject playerPrefab = BuildPlayerPrefab(ship, laserPrefab, laserSound);

            BuildMenuScene(background);
            BuildGameplayScene(background, playerPrefab, asteroidPrefab, starPrefab, loseSound);
            BuildEndScene(background);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MenuScenePath, true),
                new EditorBuildSettingsScene(GameplayScenePath, true),
                new EditorBuildSettingsScene(EndScenePath, true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);

            EditorUtility.DisplayDialog(
                "Space Explorer đã sẵn sàng",
                "Đã tạo 3 scene, prefab và UI tiếng Việt.\n\n" +
                "Mở Assets/Scenes/MainMenu.unity và nhấn Play để thử game.\n" +
                "Nếu chưa chép asset Kenney, game vẫn chạy với hình dự phòng.",
                "Đóng");
        }
        catch (Exception exception)
        {
            Debug.LogError("Không thể tạo Space Explorer: " + exception);
            EditorUtility.DisplayDialog("Không thể tạo dự án", exception.Message + "\nXem Console để biết chi tiết.", "Đóng");
        }
    }

    [MenuItem("Tools/Space Explorer/Mở scene Menu")]
    public static void OpenMenuScene()
    {
        OpenSceneOrWarn(MenuScenePath);
    }

    [MenuItem("Tools/Space Explorer/Mở scene Gameplay")]
    public static void OpenGameplayScene()
    {
        OpenSceneOrWarn(GameplayScenePath);
    }

    private static bool ConfirmRegenerationIfNeeded()
    {
        string[] generatedPaths =
        {
            MenuScenePath,
            GameplayScenePath,
            EndScenePath,
            LaserPrefabPath,
            AsteroidPrefabPath,
            StarPrefabPath,
            PlayerPrefabPath
        };

        bool hasGeneratedContent = false;
        foreach (string path in generatedPaths)
        {
            if (File.Exists(AbsoluteProjectPath(path)))
            {
                hasGeneratedContent = true;
                break;
            }
        }

        if (!hasGeneratedContent)
            return true;

        return EditorUtility.DisplayDialog(
            "Tạo lại Space Explorer?",
            "Scene/prefab đã tồn tại. Tạo lại sẽ ghi đè các scene và prefab do công cụ tạo. " +
            "Hãy hủy nếu bạn đã chỉnh sửa thủ công và chưa sao lưu.",
            "Tạo lại",
            "Hủy");
    }

    private static void OpenSceneOrWarn(string path)
    {
        if (!File.Exists(AbsoluteProjectPath(path)))
        {
            EditorUtility.DisplayDialog("Chưa có scene", "Hãy chạy Tools > Space Explorer > Tạo toàn bộ dự án trước.", "Đóng");
            return;
        }

        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory(AbsoluteProjectPath(ArtRoot));
        Directory.CreateDirectory(AbsoluteProjectPath(GeneratedRoot));
        Directory.CreateDirectory(AbsoluteProjectPath(PrefabRoot));
        Directory.CreateDirectory(AbsoluteProjectPath(SceneRoot));
        Directory.CreateDirectory(AbsoluteProjectPath("Assets/Scripts"));
        AssetDatabase.Refresh();
    }

    private static void CopyKenneyAssetsFromLocalDownload()
    {
        // Đường dẫn đúng theo máy của người làm bài. Nếu chạy trên máy khác,
        // tool sẽ bỏ qua bước này và dùng asset đã chép thủ công hoặc hình dự phòng.
        const string downloadedAssetRoot = @"C:\Users\hodac\Downloads\kenney_space-shooter-remastered";
        if (!Directory.Exists(downloadedAssetRoot))
        {
            Debug.Log("Không tìm thấy thư mục Kenney tại " + downloadedAssetRoot +
                      ". Nếu cần, chép Backgrounds, PNG, Bonus và license.txt vào " + ArtRoot + ".");
            return;
        }

        string[] folders = { "Backgrounds", "PNG", "Bonus" };
        foreach (string folder in folders)
        {
            string source = Path.Combine(downloadedAssetRoot, folder);
            string destination = AbsoluteProjectPath(Path.Combine(ArtRoot, folder));
            if (Directory.Exists(source))
                CopyDirectory(source, destination);
        }

        string sourceLicense = Path.Combine(downloadedAssetRoot, "license.txt");
        string destinationLicense = AbsoluteProjectPath(Path.Combine(ArtRoot, "license.txt"));
        if (File.Exists(sourceLicense))
            File.Copy(sourceLicense, destinationLicense, true);

        Debug.Log("Đã sao chép asset Kenney từ thư mục Downloads vào " + ArtRoot + ".");
    }

    private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);

        foreach (string sourceFile in Directory.GetFiles(sourceDirectory))
        {
            string destinationFile = Path.Combine(destinationDirectory, Path.GetFileName(sourceFile));
            File.Copy(sourceFile, destinationFile, true);
        }

        foreach (string sourceSubdirectory in Directory.GetDirectories(sourceDirectory))
        {
            string destinationSubdirectory = Path.Combine(destinationDirectory, Path.GetFileName(sourceSubdirectory));
            CopyDirectory(sourceSubdirectory, destinationSubdirectory);
        }
    }

    private static void ImportKenneySprites()
    {
        if (!AssetDatabase.IsValidFolder(ArtRoot))
            return;

        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { ArtRoot });
        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || !path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                continue;

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (Math.Abs(importer.spritePixelsPerUnit - 100f) > 0.01f)
            {
                importer.spritePixelsPerUnit = 100f;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }

            if (changed)
                importer.SaveAndReimport();
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    private static Sprite LoadKenneyOrFallback(string relativeAssetPath, string fallbackName)
    {
        string assetPath = ArtRoot + "/" + relativeAssetPath;
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite != null)
            return sprite;

        Debug.LogWarning("Chưa tìm thấy asset Kenney: " + assetPath + ". Đang dùng hình dự phòng.");
        return EnsureFallbackSprite(fallbackName);
    }

    private static Sprite EnsureFallbackSprite(string name)
    {
        string path = GeneratedRoot + "/" + name + ".png";
        string absolutePath = AbsoluteProjectPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));

        if (!File.Exists(absolutePath))
            File.WriteAllBytes(absolutePath, CreateFallbackPng(name));

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static byte[] CreateFallbackPng(string name)
    {
        bool isBackground = name == "FallbackBackground";
        int width = isBackground ? 512 : 64;
        int height = isBackground ? 512 : 64;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color32 clear = new Color32(0, 0, 0, 0);
        Color32[] pixels = new Color32[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = clear;

        if (isBackground)
        {
            Color32 baseColor = new Color32(7, 10, 31, 255);
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = baseColor;

            System.Random random = new System.Random(417);
            for (int i = 0; i < 520; i++)
            {
                int x = random.Next(width);
                int y = random.Next(height);
                byte brightness = (byte)random.Next(100, 240);
                pixels[y * width + x] = new Color32((byte)(brightness - 10), brightness, 255, 255);
                if (i % 7 == 0 && x + 1 < width)
                    pixels[y * width + x + 1] = new Color32(65, 93, 173, 255);
            }
        }
        else
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = x - 31.5f;
                    float dy = y - 31.5f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    Color32 color = clear;

                    if (name == "FallbackShip")
                    {
                        float halfWidth = Mathf.Lerp(13f, 1.5f, Mathf.Clamp01((y - 4f) / 56f));
                        if (y >= 4 && y <= 60 && Mathf.Abs(dx) <= halfWidth)
                        {
                            color = Mathf.Abs(dx) < 3f
                                ? new Color32(205, 251, 255, 255)
                                : new Color32(45, 177, 255, 255);
                        }
                        if (y <= 10 && Mathf.Abs(dx) <= 5f)
                            color = new Color32(255, (byte)(125 + y * 8), 55, 255);
                    }
                    else if (name == "FallbackAsteroid")
                    {
                        float angle = Mathf.Atan2(dy, dx);
                        float radius = 22f + Mathf.Sin(angle * 5f) * 2.4f + Mathf.Cos(angle * 7f) * 1.6f;
                        if (distance <= radius)
                        {
                            int shade = Mathf.Clamp((int)(155 - distance * 1.4f + Mathf.Sin(x * 0.8f + y) * 17f), 65, 190);
                            color = new Color32((byte)shade, (byte)(shade * 0.78f), (byte)(shade * 0.68f), 255);
                        }
                    }
                    else if (name == "FallbackStar")
                    {
                        float angle = Mathf.Atan2(dy, dx);
                        float radius = 25f * (0.60f + 0.40f * Mathf.Cos(angle * 5f));
                        if (distance <= radius)
                            color = distance < 8f
                                ? new Color32(255, 255, 220, 255)
                                : new Color32(255, (byte)Mathf.Clamp(207 - distance * 1.1f, 120, 207), 45, 255);
                    }
                    else if (name == "FallbackLaser")
                    {
                        if (y >= 5 && y <= 59 && Mathf.Abs(dx) <= 4f)
                            color = Mathf.Abs(dx) <= 1f
                                ? new Color32(230, 255, 255, 255)
                                : new Color32(35, 205, 255, 255);
                    }

                    pixels[y * width + x] = color;
                }
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        byte[] bytes = texture.EncodeToPNG();
        UnityEngine.Object.DestroyImmediate(texture);
        return bytes;
    }

    private static GameObject BuildLaserPrefab(Sprite sprite)
    {
        GameObject go = new GameObject("Laser - Tia xanh");
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 20;
        go.transform.localScale = Vector3.one * 1.35f;

        BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.7f, 0.9f);
        Rigidbody2D body = go.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.bodyType = RigidbodyType2D.Kinematic;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        go.AddComponent<LaserProjectile>();

        GameObject saved = SavePrefab(go, LaserPrefabPath);
        UnityEngine.Object.DestroyImmediate(go);
        return saved;
    }

    private static GameObject BuildAsteroidPrefab(Sprite sprite)
    {
        GameObject go = new GameObject("Asteroid - Thien thach");
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 5;

        CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.42f;
        go.AddComponent<AsteroidMotion>();

        GameObject saved = SavePrefab(go, AsteroidPrefabPath);
        UnityEngine.Object.DestroyImmediate(go);
        return saved;
    }

    private static GameObject BuildStarPrefab(Sprite sprite, AudioClip collectSound)
    {
        GameObject go = new GameObject("Star - Sao thuong");
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 4;

        CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.36f;

        StarCollectible collectible = go.AddComponent<StarCollectible>();
        SetObjectReference(collectible, "collectSound", collectSound);

        GameObject saved = SavePrefab(go, StarPrefabPath);
        UnityEngine.Object.DestroyImmediate(go);
        return saved;
    }

    private static GameObject BuildPlayerPrefab(Sprite sprite, GameObject laserPrefab, AudioClip laserSound)
    {
        GameObject go = new GameObject("Player Ship - Tau vu tru");
        go.transform.localScale = Vector3.one * 0.82f;

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 10;

        Rigidbody2D body = go.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.42f;

        AudioSource audioSource = go.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(go.transform, false);
        firePoint.transform.localPosition = new Vector3(0f, 0.58f, 0f);

        PlayerController playerController = go.AddComponent<PlayerController>();
        SetObjectReference(playerController, "laserPrefab", laserPrefab);
        SetObjectReference(playerController, "firePoint", firePoint.transform);
        SetObjectReference(playerController, "laserSound", laserSound);
        SetFloat(playerController, "moveSpeed", 6f);
        SetFloat(playerController, "fireCooldown", 0.24f);

        GameObject saved = SavePrefab(go, PlayerPrefabPath);
        UnityEngine.Object.DestroyImmediate(go);
        return saved;
    }

    private static GameObject SavePrefab(GameObject gameObject, string path)
    {
        string directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(AbsoluteProjectPath(directory));
        AssetDatabase.DeleteAsset(path);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, path);
        return prefab;
    }

    private static void BuildMenuScene(Sprite background)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCameraAndBackground(background, "Main Menu Background");
        CreateTransitionSystem();
        Canvas canvas = CreateCanvas("Canvas - Menu chinh");
        Transform root = canvas.transform;

        CreatePanel(root, "Main Card", Vector2.zero, new Vector2(900f, 850f), new Color(0.025f, 0.045f, 0.12f, 0.86f));
        CreateText(root, "Game Title", "SPACE EXPLORER", 64, new Vector2(0f, 260f), new Vector2(1050f, 100f), TextAnchor.MiddleCenter, new Color(0.40f, 0.88f, 1f), FontStyle.Bold);
        CreateText(root, "Subtitle", "PHIÊU LƯU GIỮA DẢI NGÂN HÀ", 27, new Vector2(0f, 185f), new Vector2(1000f, 55f), TextAnchor.MiddleCenter, new Color(0.87f, 0.93f, 1f), FontStyle.Bold);
        CreateText(root, "Menu Intro", "Điều khiển phi thuyền • Né thiên thạch • Thu thập sao", 22, new Vector2(0f, 130f), new Vector2(1120f, 45f), TextAnchor.MiddleCenter, new Color(0.68f, 0.78f, 0.93f), FontStyle.Normal);

        Button play = CreateButton(root, "Button - Play", "BẮT ĐẦU PHIÊU LƯU", new Vector2(0f, 35f), new Vector2(430f, 76f), new Color(0.06f, 0.48f, 0.77f));
        Button instructions = CreateButton(root, "Button - Instructions", "HƯỚNG DẪN", new Vector2(0f, -65f), new Vector2(430f, 76f), new Color(0.16f, 0.25f, 0.47f));
        Button quit = CreateButton(root, "Button - Quit", "THOÁT TRÒ CHƠI", new Vector2(0f, -165f), new Vector2(430f, 76f), new Color(0.49f, 0.16f, 0.28f));
        CreateText(root, "Menu Footer", "PHIÊN BẢN HỌC TẬP  •  KENNEY SPACE SHOOTER", 17, new Vector2(0f, -305f), new Vector2(1000f, 40f), TextAnchor.MiddleCenter, new Color(0.55f, 0.66f, 0.83f), FontStyle.Normal);

        GameObject instructionsPanel = CreatePanel(root, "Instructions Panel", Vector2.zero, new Vector2(920f, 700f), new Color(0.018f, 0.03f, 0.09f, 0.97f));
        CreateText(instructionsPanel.transform, "Instructions Title", "CÁCH CHƠI", 46, new Vector2(0f, 255f), new Vector2(800f, 75f), TextAnchor.MiddleCenter, new Color(0.4f, 0.88f, 1f), FontStyle.Bold);
        string instructionsText =
            "DI CHUYỂN\nDùng phím mũi tên hoặc WASD để lái tàu.\n\n" +
            "BẮN LASER\nGiữ phím SPACE hoặc nhấn chuột trái.\n\n" +
            "MỤC TIÊU\nThu thập sao để nhận +10 điểm.\nBắn trúng thiên thạch nhận +5 điểm.\n\n" +
            "CẢNH BÁO\nChạm thiên thạch sẽ bị trừ 10 điểm và kết thúc lượt chơi.\nCố gắng đạt kỷ lục cao nhất!";
        CreateText(instructionsPanel.transform, "Instructions Content", instructionsText, 25, new Vector2(0f, 15f), new Vector2(780f, 420f), TextAnchor.MiddleCenter, Color.white, FontStyle.Normal);
        Button closeInstructions = CreateButton(instructionsPanel.transform, "Button - Close Instructions", "ĐÃ HIỂU", new Vector2(0f, -260f), new Vector2(330f, 65f), new Color(0.06f, 0.48f, 0.77f));
        instructionsPanel.SetActive(false);

        GameObject controller = new GameObject("MainMenuUI");
        MainMenuUI menu = controller.AddComponent<MainMenuUI>();
        SetObjectReference(menu, "playButton", play);
        SetObjectReference(menu, "instructionsButton", instructions);
        SetObjectReference(menu, "closeInstructionsButton", closeInstructions);
        SetObjectReference(menu, "quitButton", quit);
        SetObjectReference(menu, "instructionsPanel", instructionsPanel);

        EditorSceneManager.SaveScene(scene, MenuScenePath);
    }

    private static void BuildGameplayScene(Sprite background, GameObject playerPrefab, GameObject asteroidPrefab, GameObject starPrefab, AudioClip loseSound)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCameraAndBackground(background, "Gameplay Background");
        CreateTransitionSystem();

        GameObject player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
        if (player != null)
            player.transform.position = new Vector3(0f, -3.3f, 0f);

        Canvas canvas = CreateCanvas("Canvas - HUD");
        Transform root = canvas.transform;
        CreateStretchedPanel(root, "HUD - Top Bar", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 104f), new Color(0.015f, 0.025f, 0.08f, 0.86f));
        CreateText(root, "Score Text", "ĐIỂM: 0", 30, new Vector2(-735f, 478f), new Vector2(500f, 58f), TextAnchor.MiddleLeft, new Color(1f, 0.85f, 0.35f), FontStyle.Bold);
        CreateText(root, "Control Guide", "MŨI TÊN / WASD: DI CHUYỂN     •     SPACE: BẮN", 20, new Vector2(420f, 478f), new Vector2(1000f, 58f), TextAnchor.MiddleRight, new Color(0.78f, 0.87f, 1f), FontStyle.Bold);
        CreateText(root, "Gameplay Hint", "THU THẬP SAO  •  NÉ THIÊN THẠCH", 19, new Vector2(0f, -495f), new Vector2(820f, 42f), TextAnchor.MiddleCenter, new Color(0.65f, 0.77f, 0.93f), FontStyle.Bold);

        HUDController hud = new GameObject("HUDController").AddComponent<HUDController>();
        Text scoreText = root.Find("Score Text").GetComponent<Text>();
        SetObjectReference(hud, "scoreText", scoreText);

        GameObject managerObject = new GameObject("GameManager");
        GameManager manager = managerObject.AddComponent<GameManager>();
        SetObjectReference(manager, "hud", hud);
        SetObjectReference(manager, "gameOverClip", loseSound);

        GameObject spawnerObject = new GameObject("SpawnManager");
        SpawnManager spawner = spawnerObject.AddComponent<SpawnManager>();
        SetObjectReference(spawner, "asteroidPrefab", asteroidPrefab);
        SetObjectReference(spawner, "starPrefab", starPrefab);
        SetInt(spawner, "initialAsteroids", 9);
        SetInt(spawner, "maxAsteroids", 13);
        SetInt(spawner, "starCount", 14);
        SetFloat(spawner, "asteroidSpawnInterval", 4.5f);

        EditorSceneManager.SaveScene(scene, GameplayScenePath);
    }

    private static void BuildEndScene(Sprite background)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCameraAndBackground(background, "End Game Background");
        CreateTransitionSystem();
        Canvas canvas = CreateCanvas("Canvas - End Game");
        Transform root = canvas.transform;

        CreatePanel(root, "End Game Card", Vector2.zero, new Vector2(900f, 820f), new Color(0.025f, 0.045f, 0.12f, 0.91f));
        CreateText(root, "End Title", "KẾT THÚC PHIÊU LƯU", 50, new Vector2(0f, 245f), new Vector2(1100f, 90f), TextAnchor.MiddleCenter, new Color(1f, 0.55f, 0.47f), FontStyle.Bold);
        CreateText(root, "End Subtitle", "Phi công, hãy sẵn sàng cho chuyến bay tiếp theo!", 23, new Vector2(0f, 165f), new Vector2(1100f, 45f), TextAnchor.MiddleCenter, new Color(0.77f, 0.85f, 0.98f), FontStyle.Normal);
        Text finalScoreText = CreateText(root, "Final Score", "ĐIỂM CỦA BẠN: 0", 38, new Vector2(0f, 70f), new Vector2(800f, 65f), TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.35f), FontStyle.Bold);
        Text bestScoreText = CreateText(root, "Best Score", "KỶ LỤC: 0", 29, new Vector2(0f, 5f), new Vector2(800f, 55f), TextAnchor.MiddleCenter, new Color(0.45f, 0.89f, 1f), FontStyle.Bold);
        Button retry = CreateButton(root, "Button - Retry", "CHƠI LẠI", new Vector2(0f, -95f), new Vector2(410f, 70f), new Color(0.06f, 0.48f, 0.77f));
        Button menu = CreateButton(root, "Button - Main Menu", "VỀ MENU CHÍNH", new Vector2(0f, -185f), new Vector2(410f, 70f), new Color(0.16f, 0.25f, 0.47f));
        Button quit = CreateButton(root, "Button - Quit", "THOÁT TRÒ CHƠI", new Vector2(0f, -275f), new Vector2(410f, 70f), new Color(0.49f, 0.16f, 0.28f));

        EndGameUI endUI = new GameObject("EndGameUI").AddComponent<EndGameUI>();
        SetObjectReference(endUI, "finalScoreText", finalScoreText);
        SetObjectReference(endUI, "bestScoreText", bestScoreText);
        SetObjectReference(endUI, "retryButton", retry);
        SetObjectReference(endUI, "menuButton", menu);
        SetObjectReference(endUI, "quitButton", quit);

        EditorSceneManager.SaveScene(scene, EndScenePath);
    }

    private static void CreateCameraAndBackground(Sprite background, string backgroundName)
    {
        // Dùng tên riêng cho từng scene để Hierarchy rõ ràng khi trình bày bài thực hành.
        GameObject cameraObject = new GameObject("Main Camera - " + backgroundName);
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.02f, 0.025f, 0.08f);
        cameraObject.AddComponent<AudioListener>();

        GameObject backgroundObject = new GameObject(backgroundName);
        backgroundObject.transform.position = new Vector3(0f, 0f, 5f);
        SpriteRenderer renderer = backgroundObject.AddComponent<SpriteRenderer>();
        renderer.sprite = background;
        renderer.sortingOrder = -100;

        if (background != null)
        {
            Vector2 size = background.bounds.size;
            if (size.x > 0f && size.y > 0f)
            {
                backgroundObject.transform.localScale = new Vector3(18.5f / size.x, 10.15f / size.y, 1f);
            }
        }
    }

    private static void CreateTransitionSystem()
    {
        GameObject transitionRoot = new GameObject("SceneTransition");
        SceneTransition transition = transitionRoot.AddComponent<SceneTransition>();

        GameObject canvasObject = new GameObject(
            "Transition Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transitionRoot.transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject overlay = new GameObject("Fade Overlay", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        overlay.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);

        Image image = overlay.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = true;
        CanvasGroup group = overlay.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        SetObjectReference(transition, "fadeGroup", group);
        SetFloat(transition, "fadeDuration", 0.4f);
    }

    private static Canvas CreateCanvas(string name)
    {
        GameObject canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        return canvas;
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = panel.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = true;
        return panel;
    }

    private static GameObject CreateStretchedPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private static Text CreateText(Transform parent, string name, string value, int fontSize, Vector2 position, Vector2 size, TextAnchor alignment, Color color, FontStyle style)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = Mathf.Max(12, fontSize - 8);
        text.resizeTextMaxSize = fontSize;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size, Color backgroundColor)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = backgroundColor;
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = backgroundColor;
        colors.highlightedColor = Color.Lerp(backgroundColor, Color.white, 0.22f);
        colors.pressedColor = Color.Lerp(backgroundColor, Color.black, 0.2f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0.45f);
        button.colors = colors;
        button.transition = Selectable.Transition.ColorTint;

        Text text = CreateText(buttonObject.transform, "Label", label, 25, Vector2.zero, size - new Vector2(20f, 12f), TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
        text.resizeTextMinSize = 16;
        return button;
    }

    private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
    {
        if (target == null)
            return;

        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning("Không tìm thấy thuộc tính '" + propertyName + "' trên " + target.name);
            return;
        }

        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetInt(UnityEngine.Object target, string propertyName, int value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
            return;
        property.intValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
            return;
        property.floatValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }
}
