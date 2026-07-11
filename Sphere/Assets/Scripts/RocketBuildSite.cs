using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// El sitio (posicion predefinida) donde se construye el cohete elegido (GDD 7.2).
/// - BeginConstruction(data): spawnea el cohete en obra.
/// - Click sobre el sitio: deposita estrellas acumuladas (starsPerClick por click);
///   el cohete crece con el progreso hasta completarse.
/// - DemolishRocket() / LaunchRocket(): API para el guion (auto-sabotaje, partida).
/// </summary>
[RequireComponent(typeof(ClickInteractable))]
public class RocketBuildSite : MonoBehaviour
{
    public static RocketBuildSite Instance { get; private set; }

    public enum State { Empty, Building, Completed }

    [Tooltip("Estrellas que se depositan por cada click.")]
    public int starsPerClick = 1;

    [Tooltip("Altura de la etiqueta de progreso sobre el pad (unidades de mundo).")]
    public float labelHeight = 2.2f;

    [TextArea] public string emptyLine = "Deberia elegir un cohete en el banco primero.";
    [TextArea] public string noStarsLine = "No me quedan estrellas...";
    [TextArea] public string completedLine = "Esta listo... creo.";

    public State CurrentState { get; private set; } = State.Empty;
    public RocketData Current { get; private set; }
    public int Deposited { get; private set; }

    Transform rocketRoot;   // raiz del cohete en obra (se destruye al demoler)
    Transform body;         // cuerpo placeholder que crece
    TextMeshPro label;      // progreso "n/costo *"

    void Awake() { Instance = this; }

    void Start()
    {
        GetComponent<ClickInteractable>().onClick.AddListener(OnClicked);
        BuildLabel();
        UpdateLabel();
    }

    void LateUpdate()
    {
        // La etiqueta es un objeto RAIZ (colgarla del pad, con su escala no uniforme
        // y rotando con el planeta, deforma el texto). Sigue al sitio cada frame.
        if (label != null)
        {
            label.transform.position = transform.position + transform.up * labelHeight + Vector3.back * 0.3f;
            label.transform.rotation = Quaternion.identity;
        }
    }

    void OnDestroy()
    {
        if (label != null) Destroy(label.gameObject);
        if (rocketRoot != null) Destroy(rocketRoot.gameObject);
    }

    // ---------- Interaccion ----------

    void OnClicked()
    {
        switch (CurrentState)
        {
            case State.Empty: SayOverSite(emptyLine); break;
            case State.Building: Deposit(); break;
            case State.Completed: SayOverSite(completedLine); break;
        }
    }

    void Deposit()
    {
        int remaining = Current.starCost - Deposited;
        int amount = Mathf.Min(starsPerClick, remaining);

        int deposited = 0;
        for (int i = 0; i < amount; i++)
            if (GameManager.Instance != null && GameManager.Instance.SpendStars(1)) deposited++;

        if (deposited == 0) { SayOverSite(noStarsLine); return; }

        Deposited += deposited;
        UpdateVisual();
        UpdateLabel();

        if (Deposited >= Current.starCost)
        {
            CurrentState = State.Completed;
            UpdateLabel();
        }
    }

    void SayOverSite(string line)
    {
        if (DialogueManager.Instance == null) return;
        Transform t = transform;
        DialogueManager.Instance.Say(() => t.position + t.up * 2.2f, line);
    }

    // ---------- API para el guion (Fase 4) ----------

    /// <summary>Empieza la construccion del cohete elegido (reemplaza el actual si habia).</summary>
    public void BeginConstruction(RocketData data)
    {
        if (data == null) return;
        ClearRocket();

        Current = data;
        Deposited = 0;
        CurrentState = State.Building;

        // El cohete se cuelga del PLANETA (escala uniforme), no del pad (escala no
        // uniforme que lo aplastaria). Se coloca en la posicion/orientacion del pad.
        rocketRoot = new GameObject("Rocket_" + data.rocketName).transform;
        rocketRoot.SetParent(transform.parent, false);
        rocketRoot.position = transform.position;
        rocketRoot.rotation = transform.rotation;

        // Compensar la escala heredada para trabajar en unidades de mundo.
        Vector3 lossy = rocketRoot.lossyScale;
        rocketRoot.localScale = new Vector3(
            lossy.x != 0f ? rocketRoot.localScale.x / lossy.x : 1f,
            lossy.y != 0f ? rocketRoot.localScale.y / lossy.y : 1f,
            lossy.z != 0f ? rocketRoot.localScale.z / lossy.z : 1f);

        GameObject bodyGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bodyGo.name = "Body";
        body = bodyGo.transform;
        body.SetParent(rocketRoot, false);

        Renderer r = bodyGo.GetComponent<Renderer>();
        if (r != null)
        {
            r.material.color = data.color;
            r.material.SetColor("_BaseColor", data.color); // URP
        }

        UpdateVisual();
        UpdateLabel();
    }

    /// <summary>Auto-sabotaje: destruye el cohete actual (las estrellas gastadas se pierden).</summary>
    public void DemolishRocket()
    {
        ClearRocket();
        UpdateLabel();
    }

    /// <summary>Despega el cohete (solo si esta completo) y deja el sitio vacio.</summary>
    public void LaunchRocket()
    {
        if (CurrentState != State.Completed || rocketRoot == null) return;
        StartCoroutine(LaunchRoutine(rocketRoot));
        rocketRoot = null;
        body = null;
        Current = null;
        Deposited = 0;
        CurrentState = State.Empty;
        UpdateLabel();
    }

    IEnumerator LaunchRoutine(Transform rocket)
    {
        rocket.SetParent(null, true); // que no gire con el planeta mientras vuela
        Vector3 dir = transform.up;   // radial hacia afuera
        float t = 0f;
        const float flightTime = 2.5f;
        while (t < flightTime && rocket != null)
        {
            t += Time.deltaTime;
            rocket.position += dir * (10f * Time.deltaTime) * (1f + t); // acelera
            yield return null;
        }
        if (rocket != null) Destroy(rocket.gameObject);
    }

    // ---------- Visual ----------

    void ClearRocket()
    {
        if (rocketRoot != null) Destroy(rocketRoot.gameObject);
        rocketRoot = null;
        body = null;
        Current = null;
        Deposited = 0;
        CurrentState = State.Empty;
    }

    void UpdateVisual()
    {
        if (body == null || Current == null) return;

        // El cohete crece con el progreso: de 15% a 100% de su altura.
        float p = Current.starCost > 0 ? (float)Deposited / Current.starCost : 1f;
        float h = Current.height * Mathf.Lerp(0.15f, 1f, p);

        // Capsula primitiva mide 2 unidades de alto a escala 1.
        body.localScale = new Vector3(Current.height * 0.28f, h * 0.5f, Current.height * 0.28f);
        body.localPosition = new Vector3(0f, h * 0.5f + 0.15f, 0f);
    }

    void UpdateLabel()
    {
        if (label == null) return;
        switch (CurrentState)
        {
            case State.Empty: label.text = ""; break;
            case State.Building: label.text = $"{Deposited}/{Current.starCost} *"; break;
            case State.Completed: label.text = "Completado"; break;
        }
    }

    void BuildLabel()
    {
        // Objeto RAIZ (sin padre): evita el shear de la escala no uniforme del pad.
        GameObject go = new GameObject("RocketProgressLabel");

        label = go.AddComponent<TextMeshPro>();
        label.fontSize = 5f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.rectTransform.sizeDelta = new Vector2(4f, 1f);
    }
}
