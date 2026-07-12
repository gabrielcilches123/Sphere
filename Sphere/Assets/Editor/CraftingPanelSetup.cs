#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// (Re)construye el panel del panfleto de crafteo como objeto de la escena y lo
/// conecta al CraftingBank. Layout:
///   Canvas/CraftingPanel (apagado por defecto)
///     ├ Title          arriba al centro
///     ├ StarsLabel     debajo del titulo
///     ├ Buttons        HORIZONTAL (cohetes lado a lado)
///     │   └ ButtonTemplate (VISIBLE para editar; con Icon + Label; se oculta en Play)
///     └ CloseButton    arriba-derecha, SOBRESALIENDO de la esquina (boton X)
///
/// OJO: si ya existe CraftingPanel, lo BORRA y lo rehace (se pierden retoques manuales).
/// Ejecutar: Tools > Sphere > Setup Crafting Panel (escena)
/// </summary>
public static class CraftingPanelSetup
{
    [MenuItem("Tools/Sphere/Setup Crafting Panel (escena)")]
    public static void Run()
    {
        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO == null) { Debug.LogError("[CraftingPanel] No encontre el Canvas principal."); return; }
        if (canvasGO.GetComponent<GraphicRaycaster>() == null) canvasGO.AddComponent<GraphicRaycaster>();

        CraftingBank bank = Object.FindFirstObjectByType<CraftingBank>(FindObjectsInactive.Include);
        if (bank == null) { Debug.LogError("[CraftingPanel] No encontre el CraftingBank."); return; }

        // Rehacer desde cero.
        Transform old = canvasGO.transform.Find("CraftingPanel");
        if (old != null) { Object.DestroyImmediate(old.gameObject); Debug.Log("[CraftingPanel] Panel anterior borrado."); }

        // ----- Panel -----
        GameObject panel = new GameObject("CraftingPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);
        panel.GetComponent<Image>().color = new Color(0.09f, 0.1f, 0.18f, 0.96f);
        RectTransform prt = (RectTransform)panel.transform;
        prt.sizeDelta = new Vector2(720f, 420f);

        // ----- Titulo: arriba al centro -----
        TMP_Text title = AddText(panel.transform, "Title", "Panfleto de cohetes", 30f, FontStyles.Bold);
        RectTransform trt = title.rectTransform;
        trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = new Vector2(0f, -18f);
        trt.sizeDelta = new Vector2(500f, 42f);

        // ----- Contador de estrellas: bajo el titulo -----
        TMP_Text stars = AddText(panel.transform, "StarsLabel", "Tienes * 0", 22f, FontStyles.Normal);
        RectTransform srt = stars.rectTransform;
        srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 1f);
        srt.pivot = new Vector2(0.5f, 1f);
        srt.anchoredPosition = new Vector2(0f, -62f);
        srt.sizeDelta = new Vector2(400f, 28f);

        // ----- Contenedor HORIZONTAL de botones -----
        GameObject buttonsGo = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        buttonsGo.transform.SetParent(panel.transform, false);
        RectTransform brt = (RectTransform)buttonsGo.transform;
        brt.anchorMin = new Vector2(0.04f, 0.06f);
        brt.anchorMax = new Vector2(0.96f, 0.72f);
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;

        HorizontalLayoutGroup hl = buttonsGo.GetComponent<HorizontalLayoutGroup>();
        hl.spacing = 18f;
        hl.childControlWidth = true;
        hl.childControlHeight = true;
        hl.childForceExpandWidth = true;
        hl.childForceExpandHeight = true;

        // ----- Boton plantilla (VISIBLE, con Icon + Label) -----
        GameObject tGo = new GameObject("ButtonTemplate", typeof(RectTransform), typeof(Image), typeof(Button));
        tGo.transform.SetParent(buttonsGo.transform, false);
        tGo.GetComponent<Image>().color = new Color(0.2f, 0.3f, 0.5f, 1f);

        // Icon: zona superior del boton (placeholder blanco translucido).
        GameObject iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(tGo.transform, false);
        Image icon = iconGo.GetComponent<Image>();
        icon.color = new Color(1f, 1f, 1f, 0.35f);
        icon.preserveAspect = true;
        RectTransform irt = (RectTransform)iconGo.transform;
        irt.anchorMin = new Vector2(0.12f, 0.38f);
        irt.anchorMax = new Vector2(0.88f, 0.94f);
        irt.offsetMin = Vector2.zero;
        irt.offsetMax = Vector2.zero;

        // Label: zona inferior.
        TMP_Text label = AddText(tGo.transform, "Label", "Cohete\n0 *", 22f, FontStyles.Normal);
        RectTransform lrt = label.rectTransform;
        lrt.anchorMin = new Vector2(0.05f, 0.04f);
        lrt.anchorMax = new Vector2(0.95f, 0.36f);
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;

        // ----- Boton X: arriba-derecha, sobresaliendo de la esquina -----
        GameObject closeGo = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeGo.transform.SetParent(panel.transform, false);
        closeGo.GetComponent<Image>().color = new Color(0.8f, 0.25f, 0.25f, 1f);
        RectTransform crt = (RectTransform)closeGo.transform;
        crt.anchorMin = crt.anchorMax = new Vector2(1f, 1f); // esquina superior derecha
        crt.pivot = new Vector2(0.5f, 0.5f);
        crt.anchoredPosition = Vector2.zero;                 // centrado EN la esquina (sobresale)
        crt.sizeDelta = new Vector2(56f, 56f);

        TMP_Text x = AddText(closeGo.transform, "Label", "X", 28f, FontStyles.Bold);
        RectTransform xrt = x.rectTransform;
        xrt.anchorMin = Vector2.zero;
        xrt.anchorMax = Vector2.one;
        xrt.offsetMin = Vector2.zero;
        xrt.offsetMax = Vector2.zero;

        panel.SetActive(false); // apagado por defecto (activalo en escena para editar)

        // ----- Conectar al CraftingBank -----
        bank.panel = panel;
        bank.starsLabel = stars;
        bank.buttonTemplate = tGo.GetComponent<Button>();
        bank.buttonContainer = buttonsGo.transform;
        bank.closeButton = closeGo.GetComponent<Button>();
        EditorUtility.SetDirty(bank);

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("[CraftingPanel] Panel reconstruido (horizontal, X en esquina) y conectado.");
    }

    static TMP_Text AddText(Transform parent, string name, string content, float size, FontStyles style)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return tmp;
    }
}
#endif
