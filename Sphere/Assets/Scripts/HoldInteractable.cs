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
        // La barra siempre horizontal aunque el objeto gire con el planeta.
        if (barRoot != null && barRoot.gameObject.activeSelf)
            barRoot.rotation = Quaternion.identity;
    }

    // ---------- Barra placeholder ----------

    void BuildBar()
    {
        Sprite white = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
            new Vector2(0.5f, 0.5f),
            Texture2D.whiteTexture.width); // sprite de 1x1 unidad

        barRoot = new GameObject("HoldBar").transform;
        barRoot.SetParent(transform, false);
        barRoot.localPosition = barOffset;

        // Compensar la escala del objeto (y sus padres) para que la barra no se deforme.
        Vector3 lossy = transform.lossyScale;
        barRoot.localScale = new Vector3(
            lossy.x != 0f ? 1f / lossy.x : 1f,
            lossy.y != 0f ? 1f / lossy.y : 1f,
            lossy.z != 0f ? 1f / lossy.z : 1f);

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

    void ShowBar(bool visible)
    {
        if (barRoot != null) barRoot.gameObject.SetActive(visible);
    }
}
