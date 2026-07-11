using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maneja el dialogo activo. Cada linea tiene su propio ancla (la burbuja salta al
/// hablante de esa linea). Click para avanzar. Singleton.
///
/// - Say(anchor, lineas): monologo simple (un solo hablante).
/// - StartConversation(lineas, anclaPlayer, anclaOtro): conversacion con hablantes.
/// - StartDialogue(npc): conversacion del NPC (usa su campo 'dialogue').
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Tooltip("Prefab de la burbuja. Vacio = se carga Resources/SpeechBubble.")]
    public GameObject bubblePrefab;

    struct Entry
    {
        public string text;
        public Func<Vector3> anchor;
    }

    SpeechBubble bubble;
    readonly List<Entry> entries = new List<Entry>();
    int index;

    /// <summary>True mientras haya un dialogo abierto (bloquea giro y recoleccion).</summary>
    public bool IsOpen => entries.Count > 0;

    /// <summary>NPC de la conversacion abierta (null si es un monologo u otro dialogo).</summary>
    public NPC CurrentNpc { get; private set; }

    void Awake()
    {
        Instance = this;
        if (bubblePrefab == null) bubblePrefab = Resources.Load<GameObject>("SpeechBubble");
    }

    /// <summary>Conversacion del NPC (elige su variante activa segun el estado del cohete).</summary>
    public void StartDialogue(NPC npc)
    {
        if (npc == null) return;
        StartConversation(npc.ActiveDialogue, () => npc.PlayerAnchor, () => npc.BubbleAnchor);
        if (IsOpen) CurrentNpc = npc;
    }

    /// <summary>Monologo simple: todas las lineas ancladas al mismo punto.</summary>
    public void Say(Func<Vector3> anchor, params string[] newLines)
    {
        if (newLines == null || anchor == null) return;
        entries.Clear();
        foreach (string line in newLines)
            if (!string.IsNullOrEmpty(line))
                entries.Add(new Entry { text = line, anchor = anchor });
        Open();
    }

    /// <summary>Conversacion: cada linea define su hablante y la burbuja salta a el.</summary>
    public void StartConversation(DialogueLine[] convo, Func<Vector3> playerAnchor, Func<Vector3> otherAnchor)
    {
        if (convo == null || playerAnchor == null || otherAnchor == null) return;
        entries.Clear();
        foreach (DialogueLine line in convo)
        {
            if (line == null || string.IsNullOrEmpty(line.text)) continue;
            entries.Add(new Entry
            {
                text = line.text,
                anchor = line.speaker == DialogueLine.Speaker.Player ? playerAnchor : otherAnchor
            });
        }
        Open();
    }

    void Open()
    {
        CurrentNpc = null; // StartDialogue lo asigna despues si aplica
        if (entries.Count == 0) return;
        index = 0;
        EnsureBubble();
        if (bubble == null) { entries.Clear(); return; }
        bubble.gameObject.SetActive(true);
        ShowLine();
    }

    /// <summary>Avanza a la siguiente linea; cierra si era la ultima.</summary>
    public void Advance()
    {
        if (!IsOpen) return;
        index++;
        if (index >= entries.Count) { Close(); return; }
        ShowLine();
    }

    void ShowLine()
    {
        bubble.SetText(entries[index].text);
        PositionBubble();
    }

    void LateUpdate()
    {
        // Seguir al hablante (por si gira con el planeta) mientras el dialogo esta abierto.
        if (IsOpen && bubble != null) PositionBubble();
    }

    void PositionBubble()
    {
        if (bubble == null || !IsOpen) return;

        bubble.transform.rotation = Quaternion.identity; // siempre derecha
        Vector3 anchor = entries[index].anchor();

        Camera cam = Camera.main;
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

        // La colita se desplaza para seguir apuntando al hablante.
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
        entries.Clear();
        CurrentNpc = null;
        if (bubble != null) bubble.gameObject.SetActive(false);
    }
}
