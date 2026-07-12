using PrimeTween;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Interaccion generica de "mantener click sobre el objeto" (GDD 5.7). Al mantener
/// holdSeconds sobre el collider se dispara onComplete.
///
/// La barra de progreso vive EN LA ESCENA (canvas world-space) y se asigna por
/// inspector: el codigo solo la muestra/oculta y anima el fillAmount del Image.
/// El arte, proporciones y posicion se editan directamente en la escena.
///
/// El ciclo lo maneja PlanetController: BeginHold() al presionar sobre el objeto,
/// TickHold() cada frame mientras se mantiene, CancelHold() al soltar o salirse.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HoldInteractable : MonoBehaviour
{
    [Tooltip("Segundos de click mantenido para completar la accion.")]
    public float holdSeconds = 1.5f;

    [Tooltip("Accion al completar el hold (ej: dormir).")]
    public UnityEvent onComplete;

    [Header("Barra de progreso (objetos de la escena)")]
    [Tooltip("Raiz de la barra en la escena (se activa/desactiva). Vacio = sin barra.")]
    public Transform barRoot;

    [Tooltip("Image del relleno (tipo Filled): el progreso anima su Fill Amount.")]
    public Image barFill;

    [Header("Seguimiento (opcional)")]
    [Tooltip("Si esta activo, la barra se recoloca cada frame sobre este objeto y se " +
             "mantiene horizontal. Apagado = la barra se queda donde la pusiste en la escena.")]
    public bool followTarget = false;

    [Tooltip("Solo con Follow Target: offset respecto al objeto.")]
    public Vector3 barOffset = new Vector3(0f, 1.8f, 0f);

    [Tooltip("Solo con Follow Target: Z (mundo) de la barra.")]
    public float barDepth = -6.5f;

    public bool IsHolding { get; private set; }

    float progress;
    Vector3 barRootScale = Vector3.one; // escala "natural" de la barra en la escena
    Tween barTween;

    void Awake()
    {
        if (barRoot != null)
        {
            barRootScale = barRoot.localScale;
            barRoot.gameObject.SetActive(false); // oculta instantanea desde el frame 0
        }
    }

    /// <summary>Empieza el hold (llamado por PlanetController al presionar sobre este objeto).</summary>
    public void BeginHold()
    {
        IsHolding = true;
        progress = 0f;
        UpdateBar();
        ShowBar(true);
    }

    /// <summary>
    /// Avanza el hold. 'stillOver' indica si el cursor sigue sobre el objeto.
    /// Devuelve true si la accion se completo este frame.
    /// </summary>
    public bool TickHold(float dt, bool stillOver)
    {
        if (!IsHolding) return false;
        if (!stillOver) { CancelHold(); return false; }

        progress += dt;
        UpdateBar();

        if (progress >= holdSeconds)
        {
            CancelHold();
            onComplete?.Invoke();
            return true;
        }
        return false;
    }

    public void CancelHold()
    {
        IsHolding = false;
        progress = 0f;
        ShowBar(false);
    }

    void LateUpdate()
    {
        if (!followTarget) return;
        if (barRoot != null && barRoot.gameObject.activeSelf)
        {
            Vector3 pos = transform.position
                + transform.up * barOffset.y
                + new Vector3(barOffset.x, 0f, 0f);
            pos.z = barDepth;
            barRoot.position = pos;
            barRoot.rotation = Quaternion.identity;
        }
    }

    void UpdateBar()
    {
        if (barFill == null) return;
        barFill.fillAmount = Mathf.Clamp01(progress / Mathf.Max(0.01f, holdSeconds));
    }

    void ShowBar(bool visible)
    {
        if (barRoot == null) return;

        JuiceSettings s = Juice.S;
        if (barTween.isAlive) barTween.Stop();

        if (visible)
        {
            barRoot.gameObject.SetActive(true);
            if (s.holdBarEnabled)
            {
                barRoot.localScale = Vector3.zero;
                barTween = Tween.Scale(barRoot, barRootScale, s.barPopDuration, s.barPopEase);
            }
            else barRoot.localScale = barRootScale;
        }
        else if (s.holdBarEnabled && barRoot.gameObject.activeSelf)
        {
            Transform bar = barRoot;
            Vector3 natural = barRootScale;
            barTween = Tween.Scale(bar, Vector3.zero, s.barPopDuration * 0.6f, Ease.InBack)
                            .OnComplete(() =>
                            {
                                bar.gameObject.SetActive(false);
                                bar.localScale = natural;
                            });
        }
        else
        {
            barRoot.gameObject.SetActive(false);
            barRoot.localScale = barRootScale;
        }
    }

    void OnDestroy()
    {
        if (barTween.isAlive) barTween.Stop();
    }
}
