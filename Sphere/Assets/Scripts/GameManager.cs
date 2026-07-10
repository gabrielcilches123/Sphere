using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Cuenta las estrellas recogidas y la meta. Al completar la mision NO reinicia la escena:
/// muestra "Mision completa!", hace un fade a negro (durante el cual el NPC se va y el
/// contador se resetea) y el juego continua.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Meta")]
    public int target = 20;

    [Header("UI")]
    [Tooltip("Texto TMP del contador. Vacio = se busca solo.")]
    public TMP_Text counterText;

    [Tooltip("Segundos que se muestra 'Mision completa!' antes del fade.")]
    public float messageTime = 1.5f;

    public int Score { get; private set; }

    /// <summary>True mientras se procesa el fin de mision (bloquea input).</summary>
    public bool IsBusy { get; private set; }

    void Awake()
    {
        Instance = this;
        if (counterText == null) counterText = FindFirstObjectByType<TMP_Text>();
        UpdateUI();
    }

    public void AddStar()
    {
        if (IsBusy) return;
        Score++;
        UpdateUI();
        if (Score >= target) StartCoroutine(CompleteMission());
    }

    IEnumerator CompleteMission()
    {
        IsBusy = true;

        if (counterText != null) counterText.text = "Mision completa!";
        yield return new WaitForSeconds(messageTime);

        bool done = false;
        System.Action atBlack = () =>
        {
            if (DayNightManager.Instance != null) DayNightManager.Instance.MakeNpcsLeave();
            Score = 0;
            UpdateUI();
        };

        if (TransitionManager.Instance != null)
            TransitionManager.Instance.Play(atBlack, () => done = true);
        else { atBlack(); done = true; }

        while (!done) yield return null;
        IsBusy = false;
    }

    void UpdateUI()
    {
        if (counterText != null) counterText.text = $"* {Score} / {target}";
    }
}
