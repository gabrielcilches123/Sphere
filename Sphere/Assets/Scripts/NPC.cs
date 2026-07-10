using UnityEngine;

/// <summary>
/// Personaje con el que se puede hablar. Al clickearlo (raycast desde PlanetController)
/// inicia un dialogo de varias lineas via DialogueManager. Necesita un Collider para el click.
/// </summary>
[RequireComponent(typeof(Collider))]
public class NPC : MonoBehaviour
{
    [TextArea]
    [Tooltip("Lineas del dialogo. Se avanzan con cada click.")]
    public string[] lines = new string[]
    {
        "Hola, viajero!",
        "Este planeta esta lleno de estrellas caidas.",
        "Recogelas todas... si no te aburres!"
    };

    [Tooltip("Desplazamiento (en mundo) de la burbuja respecto al NPC.")]
    public Vector3 bubbleOffset = new Vector3(0f, 2.2f, 0f);

    [Tooltip("Marca si el sprite mira a la derecha por defecto (sin voltear).")]
    public bool defaultFacesRight = true;

    /// <summary>Si el NPC ya se fue a otro planeta (no reaparece con el dia).</summary>
    [System.NonSerialized] public bool HasLeft;

    Transform player;
    SpriteRenderer sr;

    /// <summary>Punto del mundo donde se coloca la burbuja (sobre el NPC).</summary>
    public Vector3 BubbleAnchor => transform.position + bubbleOffset;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        GameObject p = GameObject.FindWithTag("Player");
        if (p == null) p = GameObject.Find("Player");
        if (p != null) player = p.transform;
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
