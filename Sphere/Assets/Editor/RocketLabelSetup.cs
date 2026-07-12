#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;

/// <summary>
/// Crea el texto de progreso del cohete (RocketProgressLabel) como OBJETO DE LA
/// ESCENA (raiz, TMP 3D) y lo conecta al RocketBuildSite. La fuente/estilo se editan
/// en la escena; el codigo solo actualiza el texto y lo recoloca sobre el cohete.
///
/// Ejecutar: Tools > Sphere > Setup Rocket Label (escena)   (idempotente)
/// </summary>
public static class RocketLabelSetup
{
    [MenuItem("Tools/Sphere/Setup Rocket Label (escena)")]
    public static void Run()
    {
        RocketBuildSite site = Object.FindFirstObjectByType<RocketBuildSite>(FindObjectsInactive.Include);
        if (site == null) { Debug.LogError("[RocketLabel] No encontre el RocketBuildSite."); return; }

        GameObject go = GameObject.Find("RocketProgressLabel");
        TextMeshPro label;

        if (go == null)
        {
            go = new GameObject("RocketProgressLabel");
            // Posicion de preview (en Play la recoloca LateUpdate sobre el cohete).
            go.transform.position = site.transform.position + site.transform.up * 2.2f + Vector3.back * 1.5f;

            label = go.AddComponent<TextMeshPro>();
            label.text = "0/10 *";
            label.fontSize = 5f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.rectTransform.sizeDelta = new Vector2(4f, 1f);
            Debug.Log("[RocketLabel] RocketProgressLabel creado en la escena (editable).");
        }
        else
        {
            label = go.GetComponent<TextMeshPro>();
        }

        site.label = label;
        EditorUtility.SetDirty(site);

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("[RocketLabel] Conectado al RocketBuildSite y escena guardada.");
    }
}
#endif
