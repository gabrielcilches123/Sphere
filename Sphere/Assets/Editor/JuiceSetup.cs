#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Crea el asset de configuracion de juice en Resources/JuiceSettings.asset
/// (donde lo carga la fachada Juice). Todos los parametros de feel del juego
/// se ajustan en ese asset, sin tocar codigo.
///
/// Ejecutar con: Tools > Sphere > Create Juice Settings
/// </summary>
public static class JuiceSetup
{
    const string Path = "Assets/Resources/JuiceSettings.asset";

    [MenuItem("Tools/Sphere/Create Juice Settings")]
    public static void Run()
    {
        JuiceSettings existing = AssetDatabase.LoadAssetAtPath<JuiceSettings>(Path);
        if (existing != null)
        {
            Debug.Log("[Juice] Ya existe " + Path);
            Selection.activeObject = existing;
            return;
        }

        Directory.CreateDirectory("Assets/Resources");
        JuiceSettings so = ScriptableObject.CreateInstance<JuiceSettings>();
        AssetDatabase.CreateAsset(so, Path);
        AssetDatabase.SaveAssets();
        Selection.activeObject = so;
        Debug.Log("[Juice] Creado " + Path + " — ajusta ahi todo el feel del juego.");
    }

    /// <summary>Sobrescribe el asset existente con los defaults actuales del codigo.</summary>
    [MenuItem("Tools/Sphere/Reset Juice Settings (defaults)")]
    public static void ResetToDefaults()
    {
        JuiceSettings existing = AssetDatabase.LoadAssetAtPath<JuiceSettings>(Path);
        if (existing == null) { Run(); return; }

        JuiceSettings fresh = ScriptableObject.CreateInstance<JuiceSettings>();
        EditorUtility.CopySerialized(fresh, existing);
        Object.DestroyImmediate(fresh);
        EditorUtility.SetDirty(existing);
        AssetDatabase.SaveAssets();
        Selection.activeObject = existing;
        Debug.Log("[Juice] JuiceSettings reseteado a los defaults del codigo.");
    }
}
#endif
