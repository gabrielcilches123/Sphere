using UnityEngine;

/// <summary>
/// Activa la animacion de caminar del pinguino cuando el planeta esta rotando.
/// Pone el bool "Walk" del Animator en true mientras el planeta gira, y (opcional)
/// voltea el sprite segun el sentido de giro.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [Tooltip("Referencia al planeta. Vacio = se busca solo.")]
    public PlanetController planet;

    [Tooltip("Nombre del parametro bool en el Animator.")]
    public string walkParameter = "Walk";

    [Tooltip("Voltear el sprite en X segun el sentido de giro.")]
    public bool flipWithDirection = true;

    Animator anim;
    SpriteRenderer sr;

    void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        if (planet == null) planet = FindFirstObjectByType<PlanetController>();
    }

    void Update()
    {
        if (anim == null) return;

        bool moving = planet != null && planet.IsRotating;
        anim.SetBool(walkParameter, moving);

        if (moving && flipWithDirection && sr != null)
            sr.flipX = planet.LastDir < 0f;
    }
}
