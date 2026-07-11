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
    [Header("Referencias (vacio = se buscan solas)")]
    public NPC friend;
    public Transform player;
    public RocketBuildSite site;

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

    void Awake()
    {
        dm = GetComponent<DayManager>();
        if (dm == null) dm = FindFirstObjectByType<DayManager>();
        if (dm != null) dm.OnDayStarted += HandleDayStarted;
    }

    void OnDestroy()
    {
        if (dm != null) dm.OnDayStarted -= HandleDayStarted;
    }

    void Start()
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
        DayConfigSO cfg = dm.GetConfig(day);

        // --- Aplicado en negro (durante el fade) ---

        if (cfg != null && cfg.starCollectionDisabled) collectionDisabledSticky = true;
        if (GameManager.Instance != null)
            GameManager.Instance.CollectionEnabled = !collectionDisabledSticky;

        if (day == 1 && autoStartFirstRocket && site != null &&
            site.CurrentState == RocketBuildSite.State.Empty)
            site.BeginConstruction(firstRocket);

        if (cfg != null && cfg.friendLines != null && cfg.friendLines.Length > 0 && friend != null)
            friend.lines = cfg.friendLines;

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

        // --- Auto-sabotaje: demoler el cohete en obra ---

        if (cfg.demolishRocket && site != null &&
            site.CurrentState != RocketBuildSite.State.Empty)
        {
            site.DemolishRocket();
        }

        // --- Partida del amigo (GDD dia 2) ---

        if (cfg.friendLeaves && friend != null && !friend.HasLeft)
        {
            // El amigo dice su despedida/invitacion.
            if (DialogueManager.Instance != null)
            {
                friend.Interact(); // voltea al player y abre su dialogo
                while (DialogueManager.Instance.IsOpen) yield return null;
            }

            // Si el cohete esta completo, despega; el amigo desaparece en el fade.
            bool launched = false;
            if (site != null && site.CurrentState == RocketBuildSite.State.Completed)
            {
                site.LaunchRocket();
                launched = true;
            }
            if (launched) yield return new WaitForSeconds(2.6f);

            NPC f = friend;
            if (TransitionManager.Instance != null)
                TransitionManager.Instance.Play(() =>
                {
                    f.HasLeft = true;
                    f.gameObject.SetActive(false);
                });
            else
            {
                f.HasLeft = true;
                f.gameObject.SetActive(false);
            }
        }
    }
}
