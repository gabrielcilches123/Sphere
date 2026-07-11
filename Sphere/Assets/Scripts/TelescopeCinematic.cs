using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Cinematica placeholder del telescopio (GDD 7.3): overlay fullscreen que se funde,
/// muestra un mensaje y una "estrella fugaz" (el amigo en su cohete) cruzando la vista.
/// Se cierra sola tras 'duration' segundos. Bloquea el input del planeta mientras dura.
/// Construye su propia UI en runtime (sin assets).
/// </summary>
public class TelescopeCinematic : MonoBehaviour
{
    public static TelescopeCinematic Instance { get; private set; }

    [TextArea]
    [Tooltip("Mensaje de la cinematica.")]
    public string message = "Ves a tu amigo surcando la galaxia en su cohete...";

    [Tooltip("Duracion total de la cinematica (segundos).")]
    public float duration = 5f;

    [Tooltip("Velocidad del fundido de entrada/salida.")]
    public float fadeTime = 0.5f;

    public bool IsPlaying { get; private set; }

    CanvasGroup group;
    TMP_Text text;
    RectTransform comet;

    void Awake()
    {
        Instance = this;
        BuildOverlay();
    }

    void BuildOverlay()
    {
        GameObject canvasGO = new GameObject("TelescopeCanvas");
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900; // debajo del TransitionCanvas (1000)
        canvasGO.AddComponent<CanvasScaler>();

        // Fondo: azul espacial muy oscuro (vista por el telescopio).
        Image bg = NewImage(canvasGO.transform, "BG", new Color(0.01f, 0.015f, 0.06f, 1f));
        Stretch(bg.rectTransform);

        // "Cometa": el amigo cruzando a lo lejos.
        Image cometImg = NewImage(canvasGO.transform, "Comet", new Color(1f, 0.95f, 0.7f, 1f));
        comet = cometImg.rectTransform;
        comet.sizeDelta = new Vector2(26f, 26f);

        // Mensaje.
        GameObject txtGO = new GameObject("Message", typeof(RectTransform));
        txtGO.transform.SetParent(canvasGO.transform, false);
        text = txtGO.AddComponent<TextMeshProUGUI>();
        text.text = message;
        text.fontSize = 30f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.9f, 0.92f, 1f, 1f);
        RectTransform trt = (RectTransform)txtGO.transform;
        trt.anchorMin = new Vector2(0.5f, 0.18f);
        trt.anchorMax = new Vector2(0.5f, 0.18f);
        trt.sizeDelta = new Vector2(900f, 120f);

        group = canvasGO.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        canvasGO.SetActive(false);
    }

    static Image NewImage(Transform parent, string name, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Reproduce la cinematica. Cada evento puede traer su propio mensaje y duracion
    /// (los define la config del dia en DayManager); con null/-1 usa los defaults.
    /// </summary>
    public void Play(string customMessage = null, float customDuration = -1f)
    {
        if (IsPlaying) return;
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
        Vector2 from = new Vector2(-0.15f, 0.75f);
        Vector2 to = new Vector2(1.15f, 0.45f);
        while (t < travel)
        {
            t += Time.deltaTime;
            Vector2 p = Vector2.Lerp(from, to, t / travel);
            comet.anchorMin = p;
            comet.anchorMax = p;
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
