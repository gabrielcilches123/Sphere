using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla la rotacion del planeta y arbitra el input:
///
/// - Al presionar, si el click cae SOBRE una estrella (raycast) -> la recoge y ese
///   gesto NO gira el planeta.
/// - Si el click cae en VACIO -> gira mientras se mantiene: mitad izquierda = positivo,
///   mitad derecha = negativo.
///
/// Usa el nuevo Input System (Pointer.current): mouse, touch y web.
/// </summary>
public class PlanetController : MonoBehaviour
{
    [Tooltip("Velocidad de rotacion en grados por segundo mientras se mantiene presionado.")]
    public float degreesPerSecond = 60f;

    [Tooltip("Eje de rotacion. Vista lateral 2D (rueda) = (0,0,1).")]
    public Vector3 axis = new Vector3(0f, 0f, 1f);

    [Tooltip("Camara usada para el raycast de recoleccion. Vacio = Camera.main.")]
    public Camera cam;

    bool suppressRotation; // el gesto actual empezo sobre una estrella
    HoldInteractable activeHold; // hold en curso (iglu, etc.)

    /// <summary>True en los frames en que el planeta esta girando por input.</summary>
    public bool IsRotating { get; private set; }

    /// <summary>Signo del ultimo giro (+1 / -1). Util para orientar al personaje.</summary>
    public float LastDir { get; private set; }

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        IsRotating = false; // por defecto no gira este frame

        // Durante una transicion (fade), cinematica o menu abierto no se gira ni se recoge.
        if (TransitionManager.Instance != null && TransitionManager.Instance.IsRunning) return;
        if (TelescopeCinematic.Instance != null && TelescopeCinematic.Instance.IsPlaying) return;
        if (CraftingBank.Instance != null && CraftingBank.Instance.IsMenuOpen) return;

        Pointer pointer = Pointer.current;
        if (pointer == null) return;

        if (pointer.press.wasPressedThisFrame)
            suppressRotation = IsPointerOverUI() || HandlePress(pointer.position.ReadValue());

        // Hold en curso: avanzar mientras se mantiene sobre el objeto.
        if (activeHold != null)
        {
            if (!pointer.press.isPressed)
            {
                activeHold.CancelHold();
                activeHold = null;
            }
            else
            {
                bool stillOver = IsPointerOver(activeHold, pointer.position.ReadValue());
                bool completed = activeHold.TickHold(Time.deltaTime, stillOver);
                if (completed || !activeHold.IsHolding) activeHold = null;
            }
        }

        if (pointer.press.wasReleasedThisFrame)
            suppressRotation = false;

        bool dialogueOpen = DialogueManager.Instance != null && DialogueManager.Instance.IsOpen;
        if (pointer.press.isPressed && !suppressRotation && !dialogueOpen && axis != Vector3.zero)
        {
            float x = pointer.position.ReadValue().x;
            float dir = (x < Screen.width * 0.5f) ? -1f : 1f; // invertido: izquierda -, derecha +
            transform.Rotate(axis.normalized, dir * degreesPerSecond * Time.deltaTime, Space.Self);
            IsRotating = true;
            LastDir = dir;
        }
    }

    /// <summary>True si el cursor esta sobre un elemento de UI (para no girar al usar botones).</summary>
    bool IsPointerOverUI()
    {
        UnityEngine.EventSystems.EventSystem es = UnityEngine.EventSystems.EventSystem.current;
        return es != null && es.IsPointerOverGameObject();
    }

    /// <summary>
    /// Decide que hace un click. Prioridad:
    /// 1) Si hay dialogo abierto -> avanza el dialogo.
    /// 2) Raycast: NPC -> habla; Estrella -> recoge.
    /// 3) Vacio -> no consume (deja girar).
    /// Devuelve true si el gesto se consumio (no debe girar el planeta).
    /// </summary>
    bool HandlePress(Vector2 screenPos)
    {
        // 1. Dialogo abierto: cualquier click avanza.
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
        {
            DialogueManager.Instance.Advance();
            return true;
        }

        // 2. Raycast a NPC o estrella.
        if (cam == null) cam = Camera.main;
        if (cam == null) return false;

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            NPC npc = hit.collider.GetComponentInParent<NPC>();
            if (npc != null)
            {
                npc.Interact();
                return true;
            }

            FallingStar star = hit.collider.GetComponentInParent<FallingStar>();
            if (star != null)
            {
                // Mecanica decremental: si la recoleccion esta apagada, el click sobre
                // la estrella se ignora (cae al giro, como si fuera vacio).
                if (GameManager.Instance == null || GameManager.Instance.CollectionEnabled)
                {
                    star.Collect();
                    return true;
                }
            }

            HoldInteractable hold = hit.collider.GetComponentInParent<HoldInteractable>();
            if (hold != null)
            {
                hold.BeginHold();
                activeHold = hold;
                return true;
            }

            ClickInteractable clickable = hit.collider.GetComponentInParent<ClickInteractable>();
            if (clickable != null)
            {
                clickable.Click();
                return true;
            }
        }
        return false;
    }

    /// <summary>True si el cursor sigue sobre el mismo HoldInteractable.</summary>
    bool IsPointerOver(HoldInteractable hold, Vector2 screenPos)
    {
        if (cam == null) return false;
        Ray ray = cam.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, out RaycastHit hit, 1000f)
            && hit.collider.GetComponentInParent<HoldInteractable>() == hold;
    }
}
