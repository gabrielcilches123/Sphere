using UnityEngine;

/// <summary>
/// El iglu: mantener click sobre el para dormir y pasar al dia siguiente (GDD 6).
/// Si la config del dia exige completar el cohete (requireRocketCompleteToSleep) y
/// no esta completo, no se duerme: el player dice la linea de bloqueo.
/// </summary>
[RequireComponent(typeof(HoldInteractable))]
public class Igloo : MonoBehaviour
{
    [Tooltip("Offset de la burbuja sobre el player para la linea de bloqueo.")]
    public Vector3 playerBubbleOffset = new Vector3(0f, 1.8f, 0f);

    Transform player;

    void Start()
    {
        GetComponent<HoldInteractable>().onComplete.AddListener(TrySleep);

        GameObject p = GameObject.FindWithTag("Player");
        if (p == null) p = GameObject.Find("Player");
        if (p != null) player = p.transform;
    }

    void TrySleep()
    {
        if (DayManager.Instance == null) return;

        DayConfigSO cfg = DayManager.Instance.GetConfig(DayManager.Instance.CurrentDay);
        bool blocked = cfg != null && cfg.requireRocketCompleteToSleep &&
                       (RocketBuildSite.Instance == null ||
                        RocketBuildSite.Instance.CurrentState != RocketBuildSite.State.Completed);

        if (blocked)
        {
            SayAsPlayer(cfg.cantSleepLine);
            return;
        }

        // Dia de la partida del amigo: hay que despedirse antes de dormir.
        if (StoryDirector.Instance != null && StoryDirector.Instance.FriendDeparturePending)
        {
            SayAsPlayer(StoryDirector.Instance.mustTalkLine);
            return;
        }

        DayManager.Instance.Sleep();
    }

    void SayAsPlayer(string line)
    {
        if (player == null || DialogueManager.Instance == null || string.IsNullOrEmpty(line)) return;
        Transform p = player;
        Vector3 off = playerBubbleOffset;
        DialogueManager.Instance.Say(() => p.position + off, line);
    }
}
