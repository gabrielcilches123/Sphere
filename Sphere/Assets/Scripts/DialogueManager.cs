using UnityEngine;

/// <summary>
/// Maneja el dialogo activo: instancia/posiciona la burbuja sobre el NPC, muestra las
/// lineas y avanza al siguiente click. Singleton (DialogueManager.Instance).
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Tooltip("Prefab de la burbuja. Vacio = se carga Resources/SpeechBubble.")]
    public GameObject bubblePrefab;

    SpeechBubble bubble;
    NPC current;
    int line;
    Camera cam;

    /// <summary>True mientras haya un dialogo abierto (bloquea giro y recoleccion).</summary>
    public bool IsOpen => current != null;

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
        if (bubblePrefab == null) bubblePrefab = Resources.Load<GameObject>("SpeechBubble");
    }

    public void StartDialogue(NPC npc)
    {
        if (npc == null || npc.lines == null || npc.lines.Length == 0) return;
        current = npc;
        line = 0;
        EnsureBubble();
        if (bubble == null) return;
        bubble.gameObject.SetActive(true);
        ShowLine();
    }

    /// <summary>Avanza a la siguiente linea; cierra si era la ultima.</summary>
    public void Advance()
    {
        if (current == null) return;
        line++;
        if (line >= current.lines.Length) { Close(); return; }
        ShowLine();
    }

    void ShowLine()
    {
        bubble.SetText(current.lines[line]);
        PositionBubble();
    }

    void LateUpdate()
    {
        // Seguir al NPC (por si se mueve/gira) mientras el dialogo esta abierto.
        if (current != null && bubble != null) PositionBubble();
    }

    void PositionBubble()
    {
        if (bubble == null || current == null) return;

        bubble.transform.rotation = Quaternion.identity; // siempre derecha
        Vector3 anchor = current.BubbleAnchor;

        if (cam == null) cam = Camera.main;
        if (cam == null || !cam.orthographic || bubble.body == null)
        {
            bubble.transform.position = anchor;
            return;
        }

        // Clamp para que la burbuja no se salga de la pantalla (en viewport 0..1).
        float viewW = 2f * cam.orthographicSize * cam.aspect;
        float viewH = 2f * cam.orthographicSize;
        float halfWvp = (bubble.body.size.x * 0.5f) / viewW;
        float halfHvp = (bubble.body.size.y * 0.5f) / viewH;
        const float m = 0.01f;

        Vector3 vp = cam.WorldToViewportPoint(anchor);
        vp.x = (halfWvp * 2f >= 1f) ? 0.5f : Mathf.Clamp(vp.x, halfWvp + m, 1f - halfWvp - m);
        vp.y = (halfHvp * 2f >= 1f) ? 0.5f : Mathf.Clamp(vp.y, halfHvp + m, 1f - halfHvp - m);

        Vector3 world = cam.ViewportToWorldPoint(vp);
        world.z = anchor.z;
        bubble.transform.position = world;

        // La colita se desplaza para seguir apuntando al NPC (que esta bajo el anchor).
        if (bubble.tail != null)
        {
            float limit = Mathf.Max(0f, bubble.body.size.x * 0.5f - 0.3f);
            float tailX = Mathf.Clamp(anchor.x - world.x, -limit, limit);
            Vector3 tl = bubble.tail.localPosition;
            bubble.tail.localPosition = new Vector3(tailX, tl.y, tl.z);
        }
    }

    void EnsureBubble()
    {
        if (bubble != null) return;
        if (bubblePrefab == null) bubblePrefab = Resources.Load<GameObject>("SpeechBubble");
        if (bubblePrefab != null)
            bubble = Instantiate(bubblePrefab).GetComponent<SpeechBubble>();
    }

    void Close()
    {
        current = null;
        if (bubble != null) bubble.gameObject.SetActive(false);
    }
}
