#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Elimina TODOS los componentes "Missing (Mono Script)" de la escena abierta
/// (restos de scripts borrados/movidos). No toca componentes validos.
///
/// Ejecutar: Tools > Sphere > Cleanup Missing Scripts
/// </summary>
public static class MissingScriptsCleanup
{
    [MenuItem("Tools/Sphere/Cleanup Missing Scripts")]
    public static void Run()
    {
        int removed = 0;

        // Recorre TODOS los GameObjects de la escena (incluidos inactivos).
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (count > 0)
            {
                Undo.RegisterCompleteObjectUndo(go, "Remove missing scripts");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                removed += count;
                Debug.Log($"[Cleanup] {count} script(s) missing eliminados de '{go.name}'.");
            }
        }

        if (removed > 0)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        }
        Debug.Log($"[Cleanup] Listo: {removed} componente(s) missing eliminados en total.");
    }
}
#endif
