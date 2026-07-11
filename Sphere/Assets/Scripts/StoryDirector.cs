using System.Collections;
using UnityEngine;

/// <summary>
/// Ejecuta el guion del dia (GDD 8): lee la DayConfig del dia actual y corre la
/// secuencia — lineas del despertar, dialogo del amigo, partida del amigo en el
/// cohete, auto-sabotaje (demolicion) y mecanicas decrementales.
///
/// Todo el contenido se configura por dia en DayManager > Day Configs, sin tocar codigo.
/// </summary>
public class StoryDirector : MonoBehaviour
{
    public static StoryDirector Instance { get; private set; }

    [Header("Referencias (vacio = se buscan solas)")]
    public NPC friend;
    public Transform player;
    public RocketBuildSite site;

    [Header("Partida del amigo")]
    [Tooltip("Segundos entre el despegue del cohete y el fade (corto para que no de " +
             "tiempo a moverse).")]
    public float launchToFadeDelay = 1.2f;

    [TextArea]
    [Tooltip("Linea del player si intenta dormir sin hablar con el amigo el dia de la partida.")]
    public string mustTalkLine = "Deberia hablar con mi amigo antes de dormir...";

    /// <summary>True mientras corre una secuencia guionada (bloquea el input del mundo).</summary>
    public bool IsSequenceRunning { get; private set; }

    /// <summary>True si hoy el amigo se va y aun no ha pasado (bloquea dormir).</summary>
    public bool FriendDeparturePending { get; private set; }

    [Header("Auto-sabotaje (GDD dia 10)")]
    [TextArea]
    [Tooltip("Lineas por defecto del sabotaje (un dia puede traer las suyas).")]
    public string[] defaultSabotageLines =
    {
        "No. Este cohete no va a funcionar.",
        "Tengo que hacerlo mejor. Otra vez."
    };

    /// <summary>True desde el primer dia con sabotageRocket (queda activo: el bucle).</summary>
    public bool SabotageActive { get; private set; }

    string[] activeSabotageLines;

    [Header("Dia 1: primer cohete (junto al amigo)")]
    [Tooltip("Si esta activo, el dia 1 ya hay un cohete en obra en el pad (GDD dia 1).")]
    public bool autoStartFirstRocket = true;
    public RocketData firstRocket = new RocketData
    {
        rocketName = "Nuestro cohete",
        starCost = 8,
        height = 2.5f,
        color = new Color(0.6f, 0.55f, 0.5f)
    };

    [Tooltip("Offset de la burbuja sobre el player para las lineas del despertar.")]
    public Vector3 playerBubbleOffset = new Vector3(0f, 1.8f, 0f);

    DayManager dm;
    bool collectionDisabledSticky; // una vez apagada, queda apagada (GDD 7.4)
    bool caughtUp; // ya se aplicaron las consecuencias de dias anteriores al inicial

    void Awake()
    {
        Instance = this;
        dm = GetComponent<DayManager>();
        if (dm == null) dm = FindFirstObjectByType<DayManager>();
        if (dm != null) dm.OnDayStarted += HandleDayStarted;
    }

    void OnDestroy()
    {
        if (dm != null) dm.OnDayStarted -= HandleDayStarted;
    }

    void Start() { EnsureRefs(); }

    /// <summary>
    /// Resuelve las referencias en el momento de usarlas: el dia 1 arranca desde el
    /// Start() del DayManager, que puede ejecutarse ANTES que el Start() de este
    /// componente (y las referencias aun estarian vacias).
    /// </summary>
    void EnsureRefs()
    {
        if (friend == null) friend = FindFirstObjectByType<NPC>(FindObjectsInactive.Include);
        if (site == null) site = RocketBuildSite.Instance;
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p == null) p = GameObject.Find("Player");
            if (p != null) player = p.transform;
        }
    }

    void HandleDayStarted(int day)
    {
        StopAllCoroutines();
        StartCoroutine(RunDay(day));
    }

    IEnumerator RunDay(int day)
    {
        EnsureRefs();

        // Al empezar la partida en un dia > 1 (Starting Day para testear), aplicar las
        // consecuencias persistentes de los dias saltados (amigo que se fue, etc.).
        if (!caughtUp) { CatchUpTo(day); caughtUp = true; }

        DayConfigSO cfg = dm.GetConfig(day);

        // --- Aplicado en negro (durante el fade) ---

        if (cfg != null && cfg.starCollectionDisabled) collectionDisabledSticky = true;
        if (GameManager.Instance != null)
            GameManager.Instance.CollectionEnabled = !collectionDisabledSticky;

        if (day == 1 && autoStartFirstRocket && site != null &&
            site.CurrentState == RocketBuildSite.State.Empty)
            site.BeginConstruction(firstRocket);

        if (cfg != null && friend != null)
        {
            if (cfg.dialogueLines != null && cfg.dialogueLines.Length > 0)
                friend.dialogue = cfg.dialogueLines;
            // La variante se asigna SIEMPRE (aunque vacia): asi un dia sin variante
            // no hereda la del dia anterior (ej: la despedida del dia 2).
            friend.dialogueRocketComplete = cfg.dialogueLinesRocketComplete;
        }

        // --- Esperar a que abra el fade (y un respiro tras el anuncio de dia) ---

        while (TransitionManager.Instance != null && TransitionManager.Instance.IsRunning)
            yield return null;
        yield return new WaitForSeconds(0.8f);

        if (cfg == null) yield break;

        // --- Lineas del despertar (player) ---

        if (cfg.wakeUpLines != null && cfg.wakeUpLines.Length > 0 &&
            player != null && DialogueManager.Instance != null)
        {
            Transform p = player;
            Vector3 off = playerBubbleOffset;
            DialogueManager.Instance.Say(() => p.position + off, cfg.wakeUpLines);
            while (DialogueManager.Instance.IsOpen) yield return null;
            yield return new WaitForSeconds(0.3f);
        }

        // --- Auto-sabotaje: se ACTIVA este dia (se dispara al faltar 1 estrella) ---

        if (cfg.sabotageRocket)
        {
            SabotageActive = true;
            if (cfg.sabotageLines != null && cfg.sabotageLines.Length > 0)
                activeSabotageLines = cfg.sabotageLines;
        }

        // --- Partida del amigo (GDD dia 2) ---

        if (cfg.friendLeaves && friend != null && !friend.HasLeft)
        {
            // La secuencia NO arranca sola: espera a que el PLAYER clickee al amigo
            // y complete su conversacion de despedida.
            FriendDeparturePending = true;
            if (DialogueManager.Instance != null)
            {
                while (DialogueManager.Instance.CurrentNpc != friend) yield return null;
                while (DialogueManager.Instance.IsOpen) yield return null;
            }

            // Desde aqui el input del mundo queda bloqueado (no da tiempo a moverse).
            IsSequenceRunning = true;

            bool launched = false;
            if (site != null && site.CurrentState == RocketBuildSite.State.Completed)
            {
                site.LaunchRocket();
                launched = true;
            }
            yield return new WaitForSeconds(launched ? launchToFadeDelay : 0.5f);

            NPC f = friend;
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.Play(() =>
                {
                    f.HasLeft = true;
                    f.gameObject.SetActive(false);
                });
                while (TransitionManager.Instance.IsRunning) yield return null;
            }
            else
            {
                f.HasLeft = true;
                f.gameObject.SetActive(false);
            }

            FriendDeparturePending = false;

            // Despedida del player, ya con el amigo lejos.
            if (cfg.afterFriendLeavesLines != null && cfg.afterFriendLeavesLines.Length > 0 &&
                player != null && DialogueManager.Instance != null)
            {
                yield return new WaitForSeconds(0.4f);
                Transform p2 = player;
                Vector3 off2 = playerBubbleOffset;
                DialogueManager.Instance.Say(() => p2.position + off2, cfg.afterFriendLeavesLines);
                while (DialogueManager.Instance.IsOpen) yield return null;
            }

            IsSequenceRunning = false;
        }
    }

    /// <summary>
    /// Aplica los efectos persistentes de todos los dias ANTERIORES a 'day':
    /// partida del amigo, recoleccion apagada y sabotaje activo. Asi Starting Day
    /// funciona como si esos dias ya se hubieran jugado.
    /// </summary>
    void CatchUpTo(int day)
    {
        if (dm == null) return;

        var previous = new System.Collections.Generic.List<DayConfigSO>();
        foreach (DayConfigSO c in dm.dayConfigs)
            if (c != null && c.day < day) previous.Add(c);
        previous.Sort((a, b) => a.day.CompareTo(b.day)); // el mas reciente pisa (lineas)

        foreach (DayConfigSO c in previous)
        {
            if (c.starCollectionDisabled) collectionDisabledSticky = true;

            if (c.sabotageRocket)
            {
                SabotageActive = true;
                if (c.sabotageLines != null && c.sabotageLines.Length > 0)
                    activeSabotageLines = c.sabotageLines;
            }

            if (c.friendLeaves && friend != null)
            {
                friend.HasLeft = true;
                friend.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Llamado por RocketBuildSite cuando al cohete le falta UNA estrella y el
    /// sabotaje esta activo: el player duda y destruye el cohete (GDD dia 10).
    /// </summary>
    public void TriggerSabotage()
    {
        if (!SabotageActive || IsSequenceRunning) return;
        StartCoroutine(SabotageRoutine());
    }

    IEnumerator SabotageRoutine()
    {
        IsSequenceRunning = true;
        EnsureRefs();

        // El cohete tiembla mientras el player duda.
        if (site != null) site.StartRocketShake();
        yield return new WaitForSeconds(0.4f);

        string[] lines = (activeSabotageLines != null && activeSabotageLines.Length > 0)
            ? activeSabotageLines
            : defaultSabotageLines;

        if (player != null && DialogueManager.Instance != null && lines.Length > 0)
        {
            Transform p = player;
            Vector3 off = playerBubbleOffset;
            DialogueManager.Instance.Say(() => p.position + off, lines);
            while (DialogueManager.Instance.IsOpen) yield return null;
        }

        yield return new WaitForSeconds(0.3f);
        if (site != null) site.DemolishRocket(); // detiene el temblor e implosiona

        yield return new WaitForSeconds(Juice.S.demolishDuration + 0.2f);
        IsSequenceRunning = false;
    }
}
