using System.Collections;
using PrimeTween;
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

    [Tooltip("Separacion de la etiqueta sobre la PUNTA del cohete actual (o sobre el pad si esta vacio).")]
    public float labelClearance = 0.9f;

    [Tooltip("Z (mundo) de la etiqueta: delante de los objetos para que siempre se lea.")]
    public float labelDepth = -6.5f;

    [Tooltip("Offset de la burbuja sobre el player (para sus comentarios).")]
    public Vector3 playerBubbleOffset = new Vector3(0f, 1.8f, 0f);

    [TextArea] public string emptyLine = "Deberia elegir un cohete en el banco primero.";
    [TextArea] public string noStarsLine = "No me quedan estrellas...";
    [TextArea] public string completedLine = "Esta listo... creo.";

    public State CurrentState { get; private set; } = State.Empty;
    public RocketData Current { get; private set; }
    public int Deposited { get; private set; }

    Transform rocketRoot;   // raiz del cohete en obra (se destruye al demoler)
    Transform body;         // cuerpo placeholder que crece
    TextMeshPro label;      // progreso "n/costo *"
    Transform player;       // para las burbujas de comentario (habla el pinguino)

    void Awake() { Instance = this; }

    void Start()
    {
        GetComponent<ClickInteractable>().onClick.AddListener(OnClicked);
        BuildLabel();
        UpdateLabel();

        GameObject p = GameObject.FindWithTag("Player");
        if (p == null) p = GameObject.Find("Player");
        if (p != null) player = p.transform;
    }

    void LateUpdate()
    {
        // La etiqueta es un objeto RAIZ (colgarla del pad, con su escala no uniforme
        // y rotando con el planeta, deforma el texto). Flota sobre la punta del cohete.
        if (label != null)
        {
            float top = CurrentRocketHeight() + labelClearance;
            Vector3 pos = transform.position + transform.up * top;
            pos.z = labelDepth; // delante de los objetos de la escena
            label.transform.position = pos;
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
            case State.Empty: SayAsPlayer(emptyLine); break;
            case State.Building: Deposit(); break;
            case State.Completed: SayAsPlayer(completedLine); break;
        }
    }

    void Deposit()
    {
        bool sabotage = StoryDirector.Instance != null && StoryDirector.Instance.SabotageActive;

        int remaining = Current.starCost - Deposited;
        // Con el sabotaje activo el cohete NUNCA se completa: se deposita como mucho
        // hasta dejarlo a UNA estrella (y ahi se dispara el auto-sabotaje).
        int cap = sabotage ? remaining - 1 : remaining;
        int amount = Mathf.Min(starsPerClick, cap);

        int deposited = 0;
        for (int i = 0; i < amount; i++)
            if (GameManager.Instance != null && GameManager.Instance.SpendStars(1)) deposited++;

        if (deposited == 0 && amount > 0) { SayAsPlayer(noStarsLine); return; }

        Deposited += deposited;
        UpdateVisual(animate: true);
        UpdateLabel();
        PunchLabel();

        if (sabotage && Current.starCost - Deposited <= 1)
        {
            // "Cuando esta casi terminado te auto-saboteas" (GDD).
            StoryDirector.Instance.TriggerSabotage();
            return;
        }

        if (Deposited >= Current.starCost)
        {
            CurrentState = State.Completed;
            UpdateLabel();
        }
    }

    /// <summary>Los comentarios los dice el PLAYER (pinguino), no el cohete.</summary>
    void SayAsPlayer(string line)
    {
        if (DialogueManager.Instance == null) return;
        Transform p = player != null ? player : transform;
        Vector3 off = player != null ? playerBubbleOffset : Vector3.up * 2.2f;
        DialogueManager.Instance.Say(() => p.position + off, line);
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
        StopRocketShake();

        JuiceSettings s = Juice.S;
        Transform doomed = rocketRoot;

        // Soltar el estado ya (el sitio queda vacio de inmediato para el gameplay).
        rocketRoot = null;
        body = null;
        Current = null;
        Deposited = 0;
        CurrentState = State.Empty;
        UpdateLabel();

        if (doomed == null) return;

        if (s.sabotageEnabled)
        {
            // Implosion + sacudida de camara; el objeto muere al terminar.
            Tween.StopAll(doomed);
            Juice.ShakeCamera();
            Tween.Scale(doomed, 0f, s.demolishDuration, s.demolishEase)
                 .OnComplete(() => Destroy(doomed.gameObject));
        }
        else
        {
            Destroy(doomed.gameObject);
        }
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

    /// <summary>Altura actual del cohete en obra (0 si el pad esta vacio).</summary>
    float CurrentRocketHeight()
    {
        if (Current == null) return 0f;
        float p = Current.starCost > 0 ? (float)Deposited / Current.starCost : 1f;
        return Current.height * Mathf.Lerp(0.15f, 1f, p);
    }

    void UpdateVisual(bool animate = false)
    {
        if (body == null || Current == null) return;

        // El cohete crece con el progreso: de 15% a 100% de su altura.
        float h = CurrentRocketHeight();

        // Capsula primitiva mide 2 unidades de alto a escala 1.
        Vector3 targetScale = new Vector3(Current.height * 0.28f, h * 0.5f, Current.height * 0.28f);
        Vector3 targetPos = new Vector3(0f, h * 0.5f + 0.15f, 0f);

        JuiceSettings s = Juice.S;
        if (animate && s.depositEnabled)
        {
            Tween.StopAll(body);
            Tween.Scale(body, targetScale, s.rocketGrowDuration, s.rocketGrowEase);
            Tween.LocalPosition(body, targetPos, s.rocketGrowDuration, s.rocketGrowEase);
        }
        else
        {
            body.localScale = targetScale;
            body.localPosition = targetPos;
        }
    }

    void PunchLabel()
    {
        JuiceSettings s = Juice.S;
        if (!s.depositEnabled || label == null) return;
        Tween.StopAll(label.transform);
        label.transform.localScale = Vector3.one;
        Tween.PunchScale(label.transform, Vector3.one * s.labelPunch, s.labelPunchDuration);
    }

    // ---------- Juice del sabotaje (lo dirige StoryDirector) ----------

    Tween shakeTween;

    /// <summary>Empieza el temblor del cohete (la duda del sabotaje).</summary>
    public void StartRocketShake()
    {
        JuiceSettings s = Juice.S;
        if (!s.sabotageEnabled || rocketRoot == null) return;
        shakeTween = Tween.ShakeLocalPosition(rocketRoot,
            Vector3.one * s.sabotageShakeStrength, duration: 60f, frequency: s.sabotageShakeFrequency);
    }

    public void StopRocketShake()
    {
        if (shakeTween.isAlive) shakeTween.Stop();
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
