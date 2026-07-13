using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reproductor de cinematicas del telescopio (GDD 7.3).
/// - CinematicSO (asset): identidad + mensaje + duracion de la cinematica.
/// - CinematicPanel (escena): su arte, vinculado arrastrando el SO al panel.
/// - CinematicAnimation (script en el panel, opcional): la animacion especifica.
///
/// Flujo: el dia entrega su CinematicSO -> se busca el panel vinculado a ese asset ->
/// fade in -> Animate() (o espera estatica) -> fade out. Bloquea el input (IsPlaying).
/// </summary>
public class TelescopeCinematic : MonoBehaviour
{
    public static TelescopeCinematic Instance { get; private set; }

    [Tooltip("Cinematica por defecto (dias con evento que no arrastran una propia).")]
    public CinematicSO defaultCinematic;

    public bool IsPlaying { get; private set; }

    readonly List<CinematicPanel> panels = new List<CinematicPanel>();

    void Awake()
    {
        Instance = this;

        // Registrar todos los paneles de la escena y apagarlos desde el frame 0.
        panels.Clear();
        panels.AddRange(FindObjectsByType<CinematicPanel>(
            FindObjectsInactive.Include, FindObjectsSortMode.None));

        foreach (CinematicPanel p in panels)
        {
            if (p == null) continue;
            p.Group.alpha = 0f;
            p.Group.blocksRaycasts = false;
            p.gameObject.SetActive(false);
        }
    }

    /// <summary>Reproduce una cinematica (null = la default).</summary>
    public void Play(CinematicSO cinematic = null)
    {
        if (IsPlaying) return;

        CinematicSO so = cinematic != null ? cinematic : defaultCinematic;
        if (so == null)
        {
            Debug.LogWarning("[Cinematica] El dia no define cinematica y no hay default asignada.");
            return;
        }

        CinematicPanel panel = panels.Find(p => p != null && p.cinematic == so);
        if (panel == null)
        {
            Debug.LogWarning($"[Cinematica] Ningun panel de la escena tiene arrastrada la cinematica '{so.name}'.");
            return;
        }

        CinematicContext ctx = new CinematicContext
        {
            panel = panel,
            host = this,
            message = so.message,
            duration = so.duration,
            animationTime = Mathf.Max(0.5f, so.duration - so.fadeTime * 2f)
        };
        StartCoroutine(Run(so, ctx));
    }

    IEnumerator Run(CinematicSO so, CinematicContext ctx)
    {
        IsPlaying = true;

        CinematicPanel panel = ctx.panel;
        panel.gameObject.SetActive(true);
        panel.Group.alpha = 0f;

        if (panel.message != null && !string.IsNullOrEmpty(ctx.message))
            panel.message.text = ctx.message;

        yield return Fade(panel.Group, 0f, 1f, so.fadeTime);

        // Animacion especifica del panel (script propio); sin script = estatico.
        CinematicAnimation anim = panel.GetComponent<CinematicAnimation>();
        if (anim != null) yield return anim.Animate(ctx);
        else yield return new WaitForSeconds(ctx.animationTime);

        yield return Fade(panel.Group, 1f, 0f, so.fadeTime);

        panel.gameObject.SetActive(false);
        IsPlaying = false;
    }

    IEnumerator Fade(CanvasGroup group, float from, float to, float time)
    {
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, t / time);
            yield return null;
        }
        group.alpha = to;
    }
}
