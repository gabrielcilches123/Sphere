using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Cinematica placeholder del telescopio (GDD 7.3): overlay fullscreen que se funde,
/// muestra un mensaje y una "estrella fugaz" (el amigo) cruzando la vista.
///
/// La UI vive EN LA ESCENA (canvas apagado, asignado por inspector): el arte, la fuente
/// y el layout se editan ahi. El codigo solo la enciende, anima y apaga.
/// Bloquea el input del planeta mientras dura (IsPlaying).
/// </summary>
public class TelescopeCinematic : MonoBehaviour
{
    public static TelescopeCinematic Instance { get; private set; }

    [Header("UI (objetos de la escena)")]
    [Tooltip("CanvasGroup del canvas de la cinematica (objeto APAGADO en la escena).")]
    public CanvasGroup group;

    [Tooltip("Texto del mensaje de la cinematica.")]
    public TMP_Text text;

    [Tooltip("La 'estrella fugaz' que cruza la vista (RectTransform).")]
    public RectTransform comet;

    [Header("Contenido por defecto")]
    [TextArea]
    [Tooltip("Mensaje si el dia no define uno propio.")]
    public string message = "Ves a tu amigo surcando la galaxia en su cohete...";

    [Tooltip("Duracion total por defecto (segundos).")]
    public float duration = 5f;

    [Tooltip("Velocidad del fundido de entrada/salida.")]
    public float fadeTime = 0.5f;

    [Header("Trayectoria del cometa (anclas de viewport 0..1)")]
    public Vector2 cometFrom = new Vector2(-0.15f, 0.75f);
    public Vector2 cometTo = new Vector2(1.15f, 0.45f);

    public bool IsPlaying { get; private set; }

    void Awake()
    {
        Instance = this;
        if (group != null)
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.gameObject.SetActive(false); // apagada desde el frame 0
        }
    }

    /// <summary>
    /// Reproduce la cinematica. Cada evento puede traer su propio mensaje y duracion
    /// (los define la config del dia en DayManager); con null/-1 usa los defaults.
    /// </summary>
    public void Play(string customMessage = null, float customDuration = -1f)
    {
        if (IsPlaying || group == null) return;
        if (text != null) text.text = string.IsNullOrEmpty(customMessage) ? message : customMessage;
        StartCoroutine(Routine(customDuration > 0f ? customDuration : duration));
    }

    IEnumerator Routine(float totalTime)
    {
        IsPlaying = true;
        group.gameObject.SetActive(true);

        yield return FadeGroup(0f, 1f);

        // El "cometa" cruza el cielo en diagonal durante toda la cinematica.
        float travel = Mathf.Max(0.5f, totalTime - fadeTime * 2f);
        float t = 0f;
        while (t < travel)
        {
            t += Time.deltaTime;
            if (comet != null)
            {
                Vector2 p = Vector2.Lerp(cometFrom, cometTo, t / travel);
                comet.anchorMin = p;
                comet.anchorMax = p;
            }
            yield return null;
        }

        yield return FadeGroup(1f, 0f);

        group.gameObject.SetActive(false);
        IsPlaying = false;
    }

    IEnumerator FadeGroup(float from, float to)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        group.alpha = to;
    }
}
