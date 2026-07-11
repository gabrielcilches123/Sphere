using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Transicion de pantalla con fade a negro reutilizable. Crea su propio overlay
/// (Canvas + Image negra) en runtime. Play() hace: fundir a negro -> ejecutar una
/// accion (con la pantalla en negro) -> fundir de vuelta.
/// </summary>
public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [Tooltip("Duracion de cada fundido (segundos).")]
    public float fadeTime = 0.4f;

    [Tooltip("Tiempo en negro antes de volver (segundos).")]
    public float holdTime = 0.1f;

    CanvasGroup group;

    /// <summary>True mientras corre un fade (para bloquear input durante transiciones).</summary>
    public bool IsRunning { get; private set; }

    void Awake()
    {
        Instance = this;
        BuildOverlay();
    }

    void BuildOverlay()
    {
        GameObject canvasGO = new GameObject("TransitionCanvas");
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // por encima de todo
        canvasGO.AddComponent<CanvasScaler>();

        GameObject imgGO = new GameObject("Black");
        imgGO.transform.SetParent(canvasGO.transform, false);
        Image img = imgGO.AddComponent<Image>();
        img.color = Color.black;
        img.raycastTarget = false;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        group = canvasGO.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    /// <summary>Funde a negro, ejecuta 'atBlack', y funde de vuelta (luego 'onDone').</summary>
    public void Play(Action atBlack, Action onDone = null)
    {
        StopAllCoroutines();
        StartCoroutine(Routine(atBlack, onDone));
    }

    IEnumerator Routine(Action atBlack, Action onDone)
    {
        IsRunning = true;
        yield return Fade(0f, 1f);
        atBlack?.Invoke();
        if (holdTime > 0f) yield return new WaitForSeconds(holdTime);
        yield return Fade(1f, 0f);
        IsRunning = false;
        onDone?.Invoke();
    }

    IEnumerator Fade(float from, float to)
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
