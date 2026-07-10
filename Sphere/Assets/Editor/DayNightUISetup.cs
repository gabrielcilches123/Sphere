#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.Events;
using TMPro;

/// <summary>
/// Crea un boton "Dia / Noche" en el Canvas y enlaza su onClick a
/// DayNightManager.ToggleDayNight. Tambien asegura un GraphicRaycaster en el Canvas.
///
/// Ejecutar con: Tools > Sphere > Setup Day-Night Button
/// </summary>
public static class DayNightUISetup
{
    [MenuItem("Tools/Sphere/Setup Day-Night Button")]
    public static void Setup()
    {
        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO == null) { Debug.LogError("[DayNightUISetup] No hay 'Canvas' en la escena."); return; }

        if (canvasGO.GetComponent<GraphicRaycaster>() == null)
            canvasGO.AddComponent<GraphicRaycaster>();

        if (GameObject.Find("DayNightButton") != null)
        {
            Debug.Log("[DayNightUISetup] Ya existe DayNightButton.");
            return;
        }

        GameObject btnGO = new GameObject("DayNightButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(canvasGO.transform, false);
        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(20f, -20f);
        rt.sizeDelta = new Vector2(180f, 60f);

        Image img = btnGO.GetComponent<Image>();
        img.color = new Color(0.16f, 0.17f, 0.28f, 0.85f);

        GameObject txtGO = new GameObject("Label", typeof(RectTransform));
        txtGO.transform.SetParent(btnGO.transform, false);
        TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Dia / Noche";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        RectTransform trt = txtGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        GameObject gs = GameObject.Find("GameSystem");
        DayNightManager dn = gs != null ? gs.GetComponent<DayNightManager>() : Object.FindFirstObjectByType<DayNightManager>();
        if (dn != null)
        {
            Button btn = btnGO.GetComponent<Button>();
            UnityAction call = dn.ToggleDayNight;
            UnityEventTools.AddPersistentListener(btn.onClick, call);
            Debug.Log("[DayNightUISetup] Boton creado y enlazado a DayNightManager.ToggleDayNight.");
        }
        else
        {
            Debug.LogWarning("[DayNightUISetup] No encontre DayNightManager; enlaza el boton a mano.");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
#endif
