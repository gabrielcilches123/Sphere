using UnityEngine;

/// <summary>
/// Personaje con el que se puede hablar. Al clickearlo inicia su conversacion
/// (lista de DialogueLine con hablante por linea: el NPC o el Player).
/// Necesita un Collider para el click.
/// </summary>
[RequireComponent(typeof(Collider))]
public class NPC : MonoBehaviour
{
    [Tooltip("Conversacion al clickear: cada linea define quien habla y que dice.")]
    public DialogueLine[] dialogue = new DialogueLine[]
    {
        new DialogueLine { speaker = DialogueLine.Speaker.Amigo, text = "Hola, viajero!" }
    };

    [Tooltip("Variante cuando el cohete en obra esta COMPLETO (vacio = usar la normal). " +
             "Ej dia 1: 'Genial, el cohete esta terminado, ve a dormir...'")]
    public DialogueLine[] dialogueRocketComplete;

    [Tooltip("Desplazamiento (en mundo) de la burbuja respecto al NPC.")]
    public Vector3 bubbleOffset = new Vector3(0f, 2.2f, 0f);

    [Tooltip("Desplazamiento de la burbuja sobre el player (sus lineas de la conversacion).")]
    public Vector3 playerBubbleOffset = new Vector3(0f, 1.8f, 0f);

    [Tooltip("Marca si el sprite mira a la derecha por defecto (sin voltear).")]
    public bool defaultFacesRight = true;

    /// <summary>Si el NPC ya se fue a otro planeta (no reaparece con el dia).</summary>
    [System.NonSerialized] public bool HasLeft;

    Transform player;
    SpriteRenderer sr;

    /// <summary>Punto del mundo donde se coloca la burbuja (sobre el NPC).</summary>
    public Vector3 BubbleAnchor => transform.position + bubbleOffset;

    /// <summary>Punto de la burbuja para las lineas del PLAYER en la conversacion.</summary>
    public Vector3 PlayerAnchor =>
        player != null ? player.position + playerBubbleOffset : BubbleAnchor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        GameObject p = GameObject.FindWithTag("Player");
        if (p == null) p = GameObject.Find("Player");
        if (p != null) player = p.transform;
    }

    /// <summary>Conversacion activa: la variante 'cohete completo' si aplica, si no la normal.</summary>
    public DialogueLine[] ActiveDialogue
    {
        get
        {
            bool rocketComplete = RocketBuildSite.Instance != null &&
                RocketBuildSite.Instance.CurrentState == RocketBuildSite.State.Completed;
            if (rocketComplete && dialogueRocketComplete != null && dialogueRocketComplete.Length > 0)
                return dialogueRocketComplete;
            return dialogue;
        }
    }

    public void Interact()
    {
        if (player != null) FaceTowards(player.position);
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(this);
    }

    /// <summary>Voltea el sprite del NPC para mirar hacia un punto.</summary>
    void FaceTowards(Vector3 target)
    {
        if (sr == null) return;
        bool targetIsLeft = target.x < transform.position.x;
        sr.flipX = defaultFacesRight ? targetIsLeft : !targetIsLeft;
    }
}
