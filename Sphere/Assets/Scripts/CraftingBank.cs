using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// El banco de crafteo (GDD 7.2): click sobre el objeto abre el "panfleto" con el
/// catalogo de cohetes; al elegir uno, spawnea en obra en el RocketBuildSite y se
/// completa depositando estrellas. La UI se construye en runtime (sin assets).
/// </summary>
[RequireComponent(typeof(ClickInteractable))]
public class CraftingBank : MonoBehaviour
{
    public static CraftingBank Instance { get; private set; }

    [Tooltip("Catalogo de cohetes del panfleto (editable).")]
    public List<RocketData> catalog = new List<RocketData>();

    /// <summary>True mientras el panfleto esta abierto (bloquea el input del planeta).</summary>
    public bool IsMenuOpen { get; private set; }

    GameObject panel;
    GameObject canvasRoot;
    TMP_Text starsLabel;

    void Awake()
    {
        Instance = this;
        if (catalog.Count == 0) FillDefaultCatalog();
    }

    void Start()
    {
        GetComponent<ClickInteractable>().onClick.AddListener(OpenMenu);
        BuildUI();
    }

    void FillDefaultCatalog()
    {
        catalog.Add(new RocketData { rocketName = "Cohete de chatarra", starCost = 10, height = 2.2f, color = new Color(0.55f, 0.5f, 0.45f) });
        catalog.Add(new RocketData { rocketName = "Cohete grande", starCost = 25, height = 3.2f, color = new Color(0.75f, 0.75f, 0.8f) });
        catalog.Add(new RocketData { rocketName = "El definitivo", starCost = 50, height = 4.2f, color = new Color(0.9f, 0.55f, 0.25f) });
    }

    public void OpenMenu()
    {
        if (panel == null) return;
        if (starsLabel != null && GameManager.Instance != null)
            starsLabel.text = $"Tienes * {GameManager.Instance.Stars}";
        panel.SetActive(true);
        IsMenuOpen = true;
    }

    public void CloseMenu()
    {
        if (panel != null) panel.SetActive(false);
        IsMenuOpen = false;
    }

    void Choose(RocketData data)
    {
        if (RocketBuildSite.Instance != null)
            RocketBuildSite.Instance.BeginConstruction(data);
        CloseMenu();
    }

    // ---------- UI (panfleto) ----------

    void BuildUI()
    {
        // Canvas como RAIZ de la escena: si cuelga del banco (que rota con el planeta
        // y tiene escala no uniforme) la UI se deforma/inclina.
        GameObject canvasGO = new GameObject("CraftingCanvas");
        canvasRoot = canvasGO;

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 800;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Panel central.
        panel = new GameObject("Pamphlet", typeof(RectTransform));
        panel.transform.SetParent(canvasGO.transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.09f, 0.1f, 0.18f, 0.96f);
        RectTransform prt = (RectTransform)panel.transform;
        prt.sizeDelta = new Vector2(460f, 90f + catalog.Count * 78f + 140f);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 20, 20);
        layout.spacing = 12f;
        layout.childForceExpandHeight = false;
        layout.childControlHeight = true;

        AddText(panel.transform, "Panfleto de cohetes", 30f, FontStyles.Bold, 44f);
        starsLabel = AddText(panel.transform, "Tienes * 0", 22f, FontStyles.Normal, 30f);

        foreach (RocketData data in catalog)
        {
            RocketData captured = data;
            AddButton(panel.transform, $"{data.rocketName}  —  {data.starCost} *",
                      new Color(0.2f, 0.3f, 0.5f, 1f), () => Choose(captured));
        }

        AddButton(panel.transform, "Cerrar", new Color(0.35f, 0.2f, 0.2f, 1f), CloseMenu);

        panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (canvasRoot != null) Destroy(canvasRoot);
    }

    static TMP_Text AddText(Transform parent, string content, float size, FontStyles style, float height)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        go.AddComponent<LayoutElement>().preferredHeight = height;
        return tmp;
    }

    static void AddButton(Transform parent, string labelText, Color color, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject("Button", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(action);
        go.AddComponent<LayoutElement>().preferredHeight = 64f;

        GameObject txt = new GameObject("Label", typeof(RectTransform));
        txt.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = txt.AddComponent<TextMeshProUGUI>();
        tmp.text = labelText;
        tmp.fontSize = 24f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        RectTransform trt = (RectTransform)txt.transform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }
}
