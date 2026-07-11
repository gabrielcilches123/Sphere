using PrimeTween;
using UnityEngine;
using TMPro;

/// <summary>
/// Lleva el recurso "estrellas" del jugador (acumulativo, sin meta). GDD 7.1:
/// las estrellas no se pierden al pasar de dia; son el material para craftear cohetes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    [Tooltip("Texto TMP del contador. Vacio = se busca solo.")]
    public TMP_Text counterText;

    /// <summary>Estrellas acumuladas (recurso).</summary>
    public int Stars { get; private set; }

    /// <summary>
    /// Mecanica decremental (GDD 7.4): cuando es false, el click sobre estrellas no
    /// hace nada y se acumulan en el planeta. La controla el StoryDirector por dia.
    /// </summary>
    public bool CollectionEnabled { get; set; } = true;

    void Awake()
    {
        Instance = this;
        if (counterText == null) counterText = FindFirstObjectByType<TMP_Text>();
        UpdateUI();
    }

    /// <summary>Suma una estrella recogida. Llamado por FallingStar.Collect().</summary>
    public void AddStar()
    {
        Stars++;
        UpdateUI();

        JuiceSettings s = Juice.S;
        if (s.collectEnabled && counterText != null)
        {
            Tween.StopAll(counterText.transform);
            counterText.transform.localScale = Vector3.one;
            Tween.PunchScale(counterText.transform, Vector3.one * s.counterPunch, s.counterPunchDuration);
        }
    }

    /// <summary>Gasta estrellas (crafteo de cohetes). Devuelve false si no alcanzan.</summary>
    public bool SpendStars(int amount)
    {
        if (amount < 0 || Stars < amount) return false;
        Stars -= amount;
        UpdateUI();
        return true;
    }

    void UpdateUI()
    {
        if (counterText != null) counterText.text = $"* {Stars}";
    }
}
