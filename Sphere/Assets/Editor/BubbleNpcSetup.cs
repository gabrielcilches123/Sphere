#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
using TMPro;

/// <summary>
/// Genera los sprites de la burbuja (fondo redondeado 9-slice + colita), arma el prefab
/// Resources/SpeechBubble, y crea un NPC de ejemplo en la escena (sprite de pinguino +
/// collider + NPC), colgado del planeta y en el plano frontal para que sea clickeable.
///
/// Ejecutar con: Tools > Sphere > Setup NPC + Dialogue
/// </summary>
public static class BubbleNpcSetup
{
    const string ArtDir = "Assets/Art";
    const string BodyPath = "Assets/Art/bubble_body.png";
    const string TailPath = "Assets/Art/bubble_tail.png";
    const string PrefabPath = "Assets/Resources/SpeechBubble.prefab";
    const string PenguinSprite =
        "Assets/Nine Pines Animation/2D Character Sprite Animation - Penguin/sprites/penguin_idle_01.png";

    static readonly Color Outline = new Color(0.16f, 0.17f, 0.28f, 1f);
    static readonly Color TextCol = new Color(0.16f, 0.17f, 0.28f, 1f);

    // Fase 1: genera e importa los sprites. Se debe correr ANTES de la fase 2
    // (la importacion termina entre invocaciones separadas).
    [MenuItem("Tools/Sphere/Setup NPC + Dialogue (1 Sprites)")]
    public static void GenerateSprites()
    {
        CreateRoundedBody();
        CreateTail();
        AssetDatabase.SaveAssets();
        Debug.Log("[BubbleNpcSetup] Fase 1: sprites generados. Ahora corre la fase 2.");
    }

    // Fase 2: carga los sprites ya importados, arma el prefab de la burbuja y crea el NPC.
    [MenuItem("Tools/Sphere/Setup NPC + Dialogue (2 Build)")]
    public static void BuildBubbleAndNpc()
    {
        Sprite bodySprite = AssetDatabase.LoadAssetAtPath<Sprite>(BodyPath);
        Sprite tailSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TailPath);
        if (bodySprite == null || tailSprite == null)
        {
            Debug.LogError("[BubbleNpcSetup] Sprites aun no importados. Corre la fase 1 primero.");
            return;
        }

        BuildBubblePrefab(bodySprite, tailSprite);
        CreateNpc();

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[BubbleNpcSetup] Fase 2: prefab burbuja + NPC listos.");
    }

    // ---------- Sprites ----------

    static Sprite CreateRoundedBody()
    {
        const int size = 64;
        const int radius = 18;
        const int border = 3;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] px = new Color[size * size];
        Color clear = new Color(1f, 1f, 1f, 0f);

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                Color c = clear;
                if (InsideRounded(x, y, size, radius - border, border))
                    c = Color.white;
                else if (InsideRounded(x, y, size, radius, 0))
                    c = Outline;
                px[y * size + x] = c;
            }

        tex.SetPixels(px);
        tex.Apply();

        Directory.CreateDirectory(ArtDir);
        File.WriteAllBytes(BodyPath, tex.EncodeToPNG());
        return ConfigureSprite(BodyPath, size, new Vector4(radius, radius, radius, radius));
    }

    static Sprite CreateTail()
    {
        const int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] px = new Color[size * size];
        Color clear = new Color(1f, 1f, 1f, 0f);

        // Triangulo apuntando hacia abajo: base arriba, punta abajo.
        Vector2 a = new Vector2(3, size - 3);
        Vector2 b = new Vector2(size - 3, size - 3);
        Vector2 c = new Vector2(size / 2f, 3);

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                px[y * size + x] = PointInTriangle(new Vector2(x + 0.5f, y + 0.5f), a, b, c)
                    ? Color.white : clear;

        tex.SetPixels(px);
        tex.Apply();

        File.WriteAllBytes(TailPath, tex.EncodeToPNG());
        return ConfigureSprite(TailPath, size, Vector4.zero);
    }

    // ---------- Prefab de la burbuja ----------

    static void BuildBubblePrefab(Sprite bodySprite, Sprite tailSprite)
    {
        GameObject root = new GameObject("SpeechBubble");
        SpeechBubble sb = root.AddComponent<SpeechBubble>();

        GameObject bodyGo = new GameObject("Body");
        bodyGo.transform.SetParent(root.transform, false);
        SpriteRenderer bodySR = bodyGo.AddComponent<SpriteRenderer>();
        bodySR.sprite = bodySprite;
        bodySR.drawMode = SpriteDrawMode.Sliced;
        bodySR.size = new Vector2(2f, 1f);
        bodySR.color = Color.white;
        bodySR.sortingOrder = 21;

        GameObject tailGo = new GameObject("Tail");
        tailGo.transform.SetParent(root.transform, false);
        tailGo.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
        SpriteRenderer tailSR = tailGo.AddComponent<SpriteRenderer>();
        tailSR.sprite = tailSprite;
        tailSR.color = Color.white;
        tailSR.sortingOrder = 20;

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(root.transform, false);
        textGo.transform.localPosition = new Vector3(0f, 0f, -0.05f);
        TextMeshPro tmp = textGo.AddComponent<TextMeshPro>();
        tmp.font = TMP_Settings.defaultFontAsset;
        tmp.text = "...";
        tmp.fontSize = 4f;
        tmp.color = TextCol;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        MeshRenderer tmpMr = textGo.GetComponent<MeshRenderer>();
        if (tmpMr != null) tmpMr.sortingOrder = 22;

        sb.text = tmp;
        sb.body = bodySR;
        sb.tail = tailGo.transform;

        Directory.CreateDirectory("Assets/Resources");
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        Debug.Log("[BubbleNpcSetup] Prefab burbuja en " + PrefabPath);
    }

    // ---------- NPC de ejemplo ----------

    static void CreateNpc()
    {
        if (GameObject.Find("NPC_Penguin") != null)
        {
            Debug.Log("[BubbleNpcSetup] Ya existe NPC_Penguin, no lo duplico.");
            return;
        }

        Sprite npcSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PenguinSprite);

        GameObject npc = new GameObject("NPC_Penguin");
        SpriteRenderer sr = npc.AddComponent<SpriteRenderer>();
        sr.sprite = npcSprite;
        sr.color = new Color(0.8f, 0.85f, 1f); // tinte para distinguirlo del player
        sr.sortingOrder = 6;

        BoxCollider col = npc.AddComponent<BoxCollider>();
        if (npcSprite != null)
        {
            Vector3 b = npcSprite.bounds.size;
            col.size = new Vector3(Mathf.Max(b.x, 0.2f), Mathf.Max(b.y, 0.2f), 0.5f);
        }

        npc.AddComponent<NPC>();

        GameObject planet = GameObject.Find("Planet");
        if (planet != null) npc.transform.SetParent(planet.transform, true);

        // En la superficie (radio ~7.9) hacia arriba-derecha, en el plano frontal (-Z).
        npc.transform.position = new Vector3(4.5f, 7.06f, -5f);
        npc.transform.localScale = Vector3.one * 0.22f;

        Debug.Log("[BubbleNpcSetup] NPC_Penguin creado y colgado del planeta.");
    }

    // Importa un PNG como Sprite (Full Rect para 9-slice) de forma sincrona y devuelve el Sprite.
    static Sprite ConfigureSprite(string path, int ppu, Vector4 border)
    {
        AssetDatabase.ImportAsset(path);
        TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType = TextureImporterType.Sprite;
        ti.filterMode = FilterMode.Bilinear;

        TextureImporterSettings s = new TextureImporterSettings();
        ti.ReadTextureSettings(s);
        s.spriteMode = (int)SpriteImportMode.Single;  // un solo sprite (no Multiple)
        s.spriteMeshType = SpriteMeshType.FullRect;   // necesario para 9-slice
        s.spritePixelsPerUnit = ppu;
        s.spriteBorder = border;
        s.spriteAlignment = (int)SpriteAlignment.Center;
        ti.SetTextureSettings(s);

        ti.SaveAndReimport();
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ---------- Helpers de geometria ----------

    // Rectangulo redondeado con margen (inset) opcional para el borde.
    static bool InsideRounded(int x, int y, int size, float radius, int inset)
    {
        float min = radius + inset;
        float max = size - radius - inset;
        float cx = Mathf.Clamp(x, min, max);
        float cy = Mathf.Clamp(y, min, max);
        if (x < inset || x > size - inset || y < inset || y > size - inset) return false;
        float dx = x - cx, dy = y - cy;
        return dx * dx + dy * dy <= radius * radius;
    }

    static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Sign(p, a, b);
        float d2 = Sign(p, b, c);
        float d3 = Sign(p, c, a);
        bool neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool pos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(neg && pos);
    }

    static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }
}
#endif
