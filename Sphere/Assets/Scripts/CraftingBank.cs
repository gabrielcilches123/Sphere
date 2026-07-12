using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// El banco de crafteo (GDD 7.2): click sobre el objeto abre el "panfleto" con el
/// catalogo de cohetes; al elegir uno, spawnea en obra en el RocketBuildSite.
///
/// La UI vive EN LA ESCENA (panel apagado, asignado por inspector). El codigo solo
/// la prende/apaga y clona el boton-plantilla una vez por cohete del catalogo, para
/// que el menu siga siendo data-driven pero el arte/layout se edite en la escena.
/// </summary>
[RequireComponent(typeof(ClickInteractable))]
public class CraftingBank : MonoBehaviour
{
    public static CraftingBank Instance { get; private set; }

    [Tooltip("Catalogo de cohetes del panfleto (editable).")]
    public List<RocketData> catalog = new List<RocketData>();

    [Header("UI (objetos de la escena)")]
    [Tooltip("Panel del panfleto. Debe estar APAGADO en la escena; el codigo lo abre/cierra.")]
    public GameObject panel;

    [Tooltip("Texto 'Tienes * N' (se refresca al abrir).")]
    public TMP_Text starsLabel;

    [Tooltip("Boton PLANTILLA: puede quedar VISIBLE en la escena para editar su estilo; " +
             "al iniciar Play se clona uno por cohete y la plantilla se oculta sola. " +
             "Hijos esperados: 'Icon' (Image) y 'Label' (TMP).")]
    public Button buttonTemplate;

    [Tooltip("Contenedor de los botones (con LayoutGroup). Vacio = el padre de la plantilla.")]
    public Transform buttonContainer;

    [Tooltip("Boton de cerrar el panfleto.")]
    public Button closeButton;

    /// <summary>True mientras el panfleto esta abierto (bloquea el input del planeta).</summary>
    public bool IsMenuOpen { get; private set; }

    void Awake()
    {
        Instance = this;
        if (catalog.Count == 0) FillDefaultCatalog();
    }

    void Start()
    {
        GetComponent<ClickInteractable>().onClick.AddListener(OpenMenu);
        if (closeButton != null) closeButton.onClick.AddListener(CloseMenu);

        BuildButtons();

        if (panel != null) panel.SetActive(false); // cerrado al iniciar, pase lo que pase
        IsMenuOpen = false;
    }

    void FillDefaultCatalog()
    {
        catalog.Add(new RocketData { rocketName = "Cohete de chatarra", starCost = 10, height = 2.2f, color = new Color(0.55f, 0.5f, 0.45f) });
        catalog.Add(new RocketData { rocketName = "Cohete grande", starCost = 25, height = 3.2f, color = new Color(0.75f, 0.75f, 0.8f) });
        catalog.Add(new RocketData { rocketName = "El definitivo", starCost = 50, height = 4.2f, color = new Color(0.9f, 0.55f, 0.25f) });
    }

    /// <summary>
    /// Clona el boton-plantilla una vez por cohete del catalogo y oculta la plantilla
    /// (que queda visible en la escena solo para editar su estilo).
    /// </summary>
    void BuildButtons()
    {
        if (buttonTemplate == null) return;
        Transform parent = buttonContainer != null ? buttonContainer : buttonTemplate.transform.parent;

        foreach (RocketData data in catalog)
        {
            RocketData captured = data;
            Button btn = Instantiate(buttonTemplate, parent);
            btn.gameObject.SetActive(true);
            btn.name = "Rocket_" + data.rocketName;

            TMP_Text label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = $"{data.rocketName}\n{data.starCost} *";

            // Icono del cohete (hijo "Icon"): usa el sprite del catalogo si existe.
            Transform iconT = btn.transform.Find("Icon");
            if (iconT != null && data.icon != null)
            {
                Image icon = iconT.GetComponent<Image>();
                if (icon != null) icon.sprite = data.icon;
            }

            btn.onClick.AddListener(() => Choose(captured));
        }

        buttonTemplate.gameObject.SetActive(false); // la plantilla no participa en el juego
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
}
