using PrimeTween;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Interaccion generica de "mantener click sobre el objeto" (GDD 5.7). Al mantener
/// holdSeconds sobre el collider se dispara onComplete. Muestra una barra de progreso
/// placeholder (autoconstruida con sprites blancos, sin assets).
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

    [Header("Barra de progreso (placeholder)")]
    [Tooltip("Offset local de la barra respecto al objeto.")]
    public Vector3 barOffset = new Vector3(0f, 1.8f, -0.2f);
    public float barWidth = 1.6f;
    public float barHeight = 0.2f;

    [Tooltip("Z (mundo) de la barra: delante de los objetos para que siempre se vea.")]
    public float barDepth = -6.5f;

    public bool IsHolding { get; private set; }

    float progress;
    Transform barRoot;
    Transform barFill;

    void Awake()
    {
        BuildBar();
        ShowBar(false);
    }

    /// <summary>Empieza el hold (llamado por PlanetController al presionar sobre este objeto).</summary>
    public void BeginHold()
    {
        IsHolding = true;
        progress = 0f;
        ShowBar(true);
        UpdateBar();
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
        // La barra es un objeto RAIZ (colgada de un padre con escala no uniforme que
        // rota, el texto/sprites sufren shear). Sigue al objeto y queda horizontal.
        if (barRoot != null && barRoot.gameObject.activeSelf)
        {
            Vector3 pos = transform.position
                + transform.up * barOffset.y
                + new Vector3(barOffset.x, 0f, 0f);
            pos.z = barDepth; // delante de los objetos de la escena
            barRoot.position = pos;
            barRoot.rotation = Quaternion.identity;
        }
    }

    void OnDestroy()
    {
        if (barTween.isAlive) barTween.Stop();
        if (barRoot != null) Destroy(barRoot.gameObject);
    }

    // ---------- Barra placeholder ----------

    void BuildBar()
    {
        Sprite white = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
            new Vector2(0.5f, 0.5f),
            Texture2D.whiteTexture.width); // sprite de 1x1 unidad

        // Objeto RAIZ (sin padre): posicion y rotacion se manejan en LateUpdate.
        barRoot = new GameObject("HoldBar_" + name).transform;

        SpriteRenderer bg = new GameObject("BG").AddComponent<SpriteRenderer>();
        bg.transform.SetParent(barRoot, false);
        bg.sprite = white;
        bg.color = new Color(0f, 0f, 0f, 0.6f);
        bg.transform.localScale = new Vector3(barWidth, barHeight, 1f);
        bg.sortingOrder = 30;

        barFill = new GameObject("Fill").transform;
        barFill.SetParent(barRoot, false);
        SpriteRenderer fill = barFill.gameObject.AddComponent<SpriteRenderer>();
        fill.sprite = white;
        fill.color = new Color(1f, 0.9f, 0.3f, 1f);
        fill.sortingOrder = 31;
    }

    void UpdateBar()
    {
        if (barFill == null) return;
        float p = Mathf.Clamp01(progress / Mathf.Max(0.01f, holdSeconds));
        float w = barWidth * p;
        barFill.localScale = new Vector3(w, barHeight * 0.7f, 1f);
        barFill.localPosition = new Vector3(-barWidth * 0.5f + w * 0.5f, 0f, -0.01f);
    }

    Tween barTween;

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
                barTween = Tween.Scale(barRoot, 1f, s.barPopDuration, s.barPopEase);
            }
            else barRoot.localScale = Vector3.one;
        }
        else if (s.holdBarEnabled && barRoot.gameObject.activeSelf)
        {
            Transform bar = barRoot;
            barTween = Tween.Scale(bar, 0f, s.barPopDuration * 0.6f, Ease.InBack)
                            .OnComplete(() => bar.gameObject.SetActive(false));
        }
        else
        {
            barRoot.gameObject.SetActive(false);
        }
    }
}
