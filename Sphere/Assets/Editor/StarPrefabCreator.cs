#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Utilidad de editor: genera un sprite de estrella de 5 puntas y arma el prefab
/// "Star" (SpriteRenderer 2D + BoxCollider para el click + FallingStar) en Resources,
/// para que StarSpawner lo instancie. Se puede reemplazar la imagen luego en Assets/Art/star.png.
///
/// Ejecutar con: Tools > Sphere > Create Star Prefab
/// </summary>
public static class StarPrefabCreator
{
    const string ArtDir = "Assets/Art";
    const string TexPath = "Assets/Art/star.png";
    const string ResDir = "Assets/Resources";
    const string PrefabPath = "Assets/Resources/Star.prefab";

    [MenuItem("Tools/Sphere/Create Star Prefab")]
    public static void Create()
    {
        Sprite sprite = CreateStarSprite();

        GameObject go = new GameObject("Star");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        BoxCollider col = go.AddComponent<BoxCollider>();
        col.size = new Vector3(1f, 1f, 0.3f);

        go.AddComponent<FallingStar>();

        Directory.CreateDirectory(ResDir);
        PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
        Object.DestroyImmediate(go);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[StarPrefabCreator] Prefab creado en " + PrefabPath);
    }

    static Sprite CreateStarSprite()
    {
        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        Color[] px = new Color[size * size];
        Color clear = new Color(1f, 1f, 1f, 0f);
        for (int i = 0; i < px.Length; i++) px[i] = clear;

        Vector2 c = new Vector2(size / 2f, size / 2f);
        float outer = size * 0.48f;
        float inner = outer * 0.42f;
        Vector2[] pts = new Vector2[10];
        for (int i = 0; i < 10; i++)
        {
            float ang = Mathf.PI / 2f + i * Mathf.PI / 5f; // primera punta hacia arriba
            float r = (i % 2 == 0) ? outer : inner;
            pts[i] = c + new Vector2(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r);
        }

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if (PointInPoly(new Vector2(x + 0.5f, y + 0.5f), pts))
                    px[y * size + x] = Color.white;

        tex.SetPixels(px);
        tex.Apply();

        Directory.CreateDirectory(ArtDir);
        File.WriteAllBytes(TexPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(TexPath);

        TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(TexPath);
        ti.textureType = TextureImporterType.Sprite;
        ti.spritePixelsPerUnit = size; // 1 unidad de mundo = sprite completo
        ti.filterMode = FilterMode.Bilinear;
        ti.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(TexPath);
    }

    // Test punto-en-poligono (ray casting).
    static bool PointInPoly(Vector2 p, Vector2[] poly)
    {
        bool inside = false;
        int n = poly.Length;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
                inside = !inside;
        }
        return inside;
    }
}
#endif
