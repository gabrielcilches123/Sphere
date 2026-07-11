using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Columna vertebral del juego (GDD seccion 6): los dias funcionan como rondas.
/// - Al comenzar cada dia cae una tanda de estrellas (starsPerDay, con override por dia).
/// - Se avanza de dia durmiendo en el iglu (Sleep()), con fade a negro.
/// - Expone OnDayStarted(dia) para que otros sistemas (telescopio, eventos, guion)
///   reaccionen al dia actual.
/// </summary>
public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Dias")]
    [Tooltip("Dia en el que empieza la partida.")]
    public int startingDay = 1;

    [Tooltip("Cuantas estrellas caen por defecto al comenzar cada dia.")]
    public int starsPerDay = 8;

    [Serializable]
    public class DayConfig
    {
        [Tooltip("Numero de dia al que aplica esta configuracion.")]
        public int day = 1;

        [Tooltip("Estrellas que caen ese dia. -1 = usar el starsPerDay por defecto.")]
        public int starsOverride = -1;

        [Header("Evento de telescopio")]
        [Tooltip("Si este dia tiene evento de telescopio (cinematica al mirar).")]
        public bool telescopeEvent = false;

        [TextArea]
        [Tooltip("Texto de la cinematica de ESTE dia (ej: 'Tu amigo aterriza en otro planeta...'). " +
                 "Vacio = usar el mensaje por defecto del TelescopeCinematic.")]
        public string telescopeMessage = "";

        [Tooltip("Duracion de la cinematica de este dia en segundos. -1 = duracion por defecto.")]
        public float telescopeDuration = -1f;
    }

    [Tooltip("Configuracion opcional por dia (deja vacio para usar siempre el default).")]
    public List<DayConfig> dayConfigs = new List<DayConfig>();

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

    /// <summary>Configuracion del dia (o null si no tiene entrada).</summary>
    public DayConfig GetConfig(int day) => dayConfigs.Find(c => c.day == day);

    /// <summary>True si ese dia tiene evento de telescopio (GDD 7.3).</summary>
    public bool IsTelescopeEventDay(int day)
    {
        DayConfig cfg = GetConfig(day);
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
        int stars = starsPerDay;
        DayConfig cfg = dayConfigs.Find(c => c.day == CurrentDay);
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
