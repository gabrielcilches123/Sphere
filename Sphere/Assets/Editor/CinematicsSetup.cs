#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Configura el sistema de cinematicas por REFERENCIA DIRECTA (sin ids):
/// - Crea/actualiza el asset Cinematic_Viaje (nombre, mensaje, duracion).
/// - Lo arrastra al CinematicPanel 'TelescopeCinematica_Panel' de la escena.
/// - Lo deja como default del reproductor (TelescopeCinematic).
///
/// Ejecutar: Tools > Sphere > Setup Cinematics (SO)   (idempotente)
/// </summary>
public static class CinematicsSetup
{
    const string Dir = "Assets/GameData/Cinematics";
    const string ViajePath = Dir + "/Cinematic_Viaje.asset";

    [MenuItem("Tools/Sphere/Setup Cinematics (SO)")]
    public static void Run()
    {
        TelescopeCinematic player = Object.FindFirstObjectByType<TelescopeCinematic>(FindObjectsInactive.Include);
        if (player == null) { Debug.LogError("[Cinematics] No encontre TelescopeCinematic (GameSystem)."); return; }

        Directory.CreateDirectory(Dir);

        // Asset de la cinematica 'viaje'.
        CinematicSO viaje = AssetDatabase.LoadAssetAtPath<CinematicSO>(ViajePath);
        if (viaje == null && File.Exists(ViajePath)) AssetDatabase.DeleteAsset(ViajePath);
        if (viaje == null)
        {
            viaje = ScriptableObject.CreateInstance<CinematicSO>();
            AssetDatabase.CreateAsset(viaje, ViajePath);
            Debug.Log("[Cinematics] Creado " + ViajePath);
        }
        if (string.IsNullOrEmpty(viaje.nombre)) viaje.nombre = "El amigo viaja por la galaxia";
        if (string.IsNullOrEmpty(viaje.message)) viaje.message = "Ves a tu amigo surcando la galaxia en su cohete...";
        EditorUtility.SetDirty(viaje);

        // Vincular el panel de la escena arrastrando el asset (sin ids).
        GameObject panelGo = GameObject.Find("Canvas/TelescopeCinematica_Panel");
        if (panelGo == null)
        {
            // GameObject.Find no encuentra inactivos: buscar entre todos los paneles.
            foreach (CinematicPanel p in Object.FindObjectsByType<CinematicPanel>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (p.gameObject.name == "TelescopeCinematica_Panel") { panelGo = p.gameObject; break; }
        }

        if (panelGo != null)
        {
            CinematicPanel panel = panelGo.GetComponent<CinematicPanel>();
            if (panel != null)
            {
                panel.cinematic = viaje;
                if (panel.message == null)
                {
                    Transform m = panelGo.transform.Find("Message");
                    if (m != null) panel.message = m.GetComponent<TMPro.TMP_Text>();
                }
                EditorUtility.SetDirty(panel);
                Debug.Log("[Cinematics] Cinematic_Viaje vinculada al panel TelescopeCinematica_Panel.");
            }
        }
        else Debug.LogWarning("[Cinematics] No encontre TelescopeCinematica_Panel en la escena.");

        // Default del reproductor.
        player.defaultCinematic = viaje;
        EditorUtility.SetDirty(player);

        AssetDatabase.SaveAssets();
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("[Cinematics] Setup completo y escena guardada.");
    }
}
#endif
