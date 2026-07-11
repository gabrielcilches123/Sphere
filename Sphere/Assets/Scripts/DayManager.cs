using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Columna vertebral del juego (GDD seccion 6): los dias funcionan como rondas.
/// - Al comenzar cada dia cae una tanda de estrellas (starsPerDay, con override por dia).
/// - Se avanza de dia durmiendo en el iglu (Sleep()), con fade a negro.
/// - La configuracion de cada dia vive en assets DayConfigSO (Create > Sphere > Day Config)
///   anadidos a la lista dayConfigs. Dias sin asset = dia normal.
/// - Expone OnDayStarted(dia) para el StoryDirector, telescopio, etc.
/// </summary>
public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Dias")]
    [Tooltip("Dia en el que empieza la partida.")]
    public int startingDay = 1;

    [Tooltip("Cuantas estrellas caen por defecto al comenzar cada dia.")]
    public int starsPerDay = 8;

    [Tooltip("Assets de configuracion por dia (Create > Sphere > Day Config).")]
    public List<DayConfigSO> dayConfigs = new List<DayConfigSO>();

    [Header("Referencias")]
    [Tooltip("Spawner de estrellas. Vacio = se busca solo.")]
    public StarSpawner spawner;

    [Tooltip("Texto TMP para anunciar 'Dia N'. Vacio = sin anuncio.")]
    public TMP_Text dayText;

    [Tooltip("Segundos que se muestra el anuncio 'Dia N'.")]
    public float announceTime = 2.5f;

    public int CurrentDay { get; private set; }

    /// <summary>Se dispara al comenzar cada dia (incluido el primero).</summary>
    public event Action<int> OnDayStarted;

    void Awake() { Instance = this; }

    void Start()
    {
        if (spawner == null) spawner = FindFirstObjectByType<StarSpawner>();
        CurrentDay = Mathf.Max(1, startingDay);
        BeginDay();
    }

    /// <summary>Configuracion del dia (o null si no tiene asset asignado).</summary>
    public DayConfigSO GetConfig(int day) => dayConfigs.Find(c => c != null && c.day == day);

    /// <summary>True si ese dia tiene evento de telescopio (GDD 7.3).</summary>
    public bool IsTelescopeEventDay(int day)
    {
        DayConfigSO cfg = GetConfig(day);
        return cfg != null && cfg.telescopeEvent;
    }

    /// <summary>Dormir en el iglu: avanza al dia siguiente con fade a negro.</summary>
    public void Sleep()
    {
        if (TransitionManager.Instance != null)
            TransitionManager.Instance.Play(() => { CurrentDay++; BeginDay(); });
        else { CurrentDay++; BeginDay(); }
    }

    void BeginDay()
    {
        DayConfigSO cfg = GetConfig(CurrentDay);

        int stars = starsPerDay;
        if (cfg != null && cfg.starsOverride >= 0) stars = cfg.starsOverride;
        if (spawner != null) spawner.SpawnBatch(stars);

        OnDayStarted?.Invoke(CurrentDay);

        if (dayText != null)
        {
            StopAllCoroutines();
            StartCoroutine(AnnounceDay());
        }
    }

    IEnumerator AnnounceDay()
    {
        dayText.text = $"Dia {CurrentDay}";
        dayText.gameObject.SetActive(true);
        yield return new WaitForSeconds(announceTime);
        dayText.gameObject.SetActive(false);
    }
}
