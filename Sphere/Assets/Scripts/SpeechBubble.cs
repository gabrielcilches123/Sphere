using PrimeTween;
using UnityEngine;
using TMPro;

/// <summary>
/// Burbuja de dialogo world-space estilo "A Short Hike": un fondo redondeado (9-slice)
/// que se adapta al tamano del texto, con una colita que apunta al NPC.
/// </summary>
public class SpeechBubble : MonoBehaviour
{
    [Tooltip("Texto (TextMeshPro 3D) dentro de la burbuja.")]
    public TextMeshPro text;

    [Tooltip("Fondo de la burbuja (SpriteRenderer en modo Sliced).")]
    public SpriteRenderer body;

    [Tooltip("Colita que apunta al NPC.")]
    public Transform tail;

    [Tooltip("Margen alrededor del texto (x = horizontal, y = vertical).")]
    public Vector2 padding = new Vector2(0.6f, 0.4f);

    [Tooltip("Ancho maximo antes de partir el texto en varias lineas.")]
    public float maxWidth = 8f;

    public void SetText(string s)
    {
        if (text == null) return;

        text.text = s;

        // Tamano preferido del texto, limitado a maxWidth.
        Vector2 pref = text.GetPreferredValues(s, maxWidth, 0f);
        float w = Mathf.Min(pref.x, maxWidth);
        pref = text.GetPreferredValues(s, w, 0f); // alto real a ese ancho
        float h = pref.y;

        text.rectTransform.sizeDelta = new Vector2(w, h);

        Vector2 size = new Vector2(w, h) + padding * 2f;
        if (body != null) body.size = size;

        if (tail != null)
        {
            float z = tail.localPosition.z;
            tail.localPosition = new Vector3(0f, -size.y * 0.5f + 0.05f, z);
        }
    }

    [ContextMenu("Preview Sample")]
    public void PreviewSample()
    {
        SetText("I GUESS I'M BORED!!!");
    }

    // ---------- Juice (config en Resources/JuiceSettings) ----------

    Tween scaleTween; // pop de apertura / punch / cierre (uno a la vez)

    void OnEnable()
    {
        if (!Application.isPlaying) return;
        JuiceSettings s = Juice.S;
        if (!s.bubbleEnabled) return;

        // Pop de apertura.
        if (scaleTween.isAlive) scaleTween.Stop();
        transform.localScale = Vector3.one * 0.6f;
        scaleTween = Tween.Scale(transform, 1f, s.bubblePopDuration, s.bubblePopEase);
    }

    /// <summary>Micro-punch al cambiar de linea (no interrumpe el pop de apertura).</summary>
    public void PunchLine()
    {
        JuiceSettings s = Juice.S;
        if (!s.bubbleEnabled) return;
        if (scaleTween.isAlive) return; // el pop de apertura sigue corriendo

        transform.localScale = Vector3.one;
        scaleTween = Tween.PunchScale(transform, Vector3.one * s.bubbleLinePunch, 0.12f);
    }

    /// <summary>Encoge la burbuja y la desactiva al terminar.</summary>
    public void HideAnimated()
    {
        JuiceSettings s = Juice.S;
        if (!s.bubbleEnabled || !gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            return;
        }

        if (scaleTween.isAlive) scaleTween.Stop();
        scaleTween = Tween.Scale(transform, 0f, s.bubbleHideDuration, Ease.InBack)
             .OnComplete(() =>
             {
                 gameObject.SetActive(false);
                 transform.localScale = Vector3.one;
             });
    }
}
