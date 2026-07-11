using UnityEngine;

/// <summary>
/// El telescopio (GDD 7.3). Click para mirar:
/// - Dia CON evento de telescopio -> cinematica placeholder (TelescopeCinematic).
/// - Dia SIN evento -> el player dice "No parece que haya algo interesante."
/// </summary>
[RequireComponent(typeof(ClickInteractable))]
public class Telescope : MonoBehaviour
{
    [TextArea]
    [Tooltip("Linea del player en dias sin evento.")]
    public string noEventLine = "No parece que haya algo interesante.";

    [Tooltip("El player (para la burbuja). Vacio = se busca por nombre/tag.")]
    public Transform player;

    [Tooltip("Offset de la burbuja sobre el player.")]
    public Vector3 bubbleOffset = new Vector3(0f, 1.8f, 0f);

    void Start()
    {
        GetComponent<ClickInteractable>().onClick.AddListener(Look);

        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p == null) p = GameObject.Find("Player");
            if (p != null) player = p.transform;
        }
    }

    public void Look()
    {
        DayManager.DayConfig cfg = DayManager.Instance != null
            ? DayManager.Instance.GetConfig(DayManager.Instance.CurrentDay)
            : null;
        bool eventDay = cfg != null && cfg.telescopeEvent;

        if (eventDay && TelescopeCinematic.Instance != null)
        {
            // Cada dia trae su propio mensaje/duracion (modular desde el inspector).
            TelescopeCinematic.Instance.Play(cfg.telescopeMessage, cfg.telescopeDuration);
        }
        else if (player != null && DialogueManager.Instance != null)
        {
            Transform p = player;
            Vector3 off = bubbleOffset;
            DialogueManager.Instance.Say(() => p.position + off, noEventLine);
        }
    }
}
