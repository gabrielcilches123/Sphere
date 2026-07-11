using PrimeTween;
using UnityEngine;

/// <summary>
/// TODOS los parametros de feedback/juice del juego en un solo asset, agrupados por
/// efecto. Cada grupo tiene su toggle para apagarlo. Vive en Resources/JuiceSettings
/// (lo carga la fachada estatica Juice). Ajustar aqui = no tocar codigo.
/// </summary>
[CreateAssetMenu(fileName = "JuiceSettings", menuName = "Sphere/Juice Settings")]
public class JuiceSettings : ScriptableObject
{
    [Header("Recoger estrella")]
    public bool collectEnabled = true;
    [Tooltip("Fuerza del punch de escala al clickearla.")]
    public float collectPunch = 0.18f;
    public float collectPunchDuration = 0.12f;
    [Tooltip("Duracion del vuelo hacia el contador.")]
    public float collectFlyDuration = 0.45f;
    public Ease collectFlyEase = Ease.InCubic;
    [Tooltip("Pop del contador al sumar.")]
    public float counterPunch = 0.3f;
    public float counterPunchDuration = 0.2f;

    [Header("Estrella aterriza (squash & stretch)")]
    public bool landEnabled = true;
    [Tooltip("Deformacion al tocar suelo: +X ensancha, -Y aplasta.")]
    public Vector3 landSquash = new Vector3(0.14f, -0.18f, 0f);
    public float landSquashDuration = 0.22f;

    [Header("Depositar en el cohete")]
    public bool depositEnabled = true;
    [Tooltip("El cohete crece suavemente hasta su nuevo tamano.")]
    public float rocketGrowDuration = 0.35f;
    public Ease rocketGrowEase = Ease.OutBack;
    [Tooltip("Pop de la etiqueta n/costo con cada deposito.")]
    public float labelPunch = 0.25f;
    public float labelPunchDuration = 0.15f;

    [Header("Sabotaje")]
    public bool sabotageEnabled = true;
    [Tooltip("Temblor del cohete mientras el player duda.")]
    public float sabotageShakeStrength = 0.12f;
    public float sabotageShakeFrequency = 9f;
    [Tooltip("Implosion del cohete al demolerse.")]
    public float demolishDuration = 0.35f;
    public Ease demolishEase = Ease.InBack;
    [Tooltip("Sacudida de camara al demoler.")]
    public float cameraShakeStrength = 0.25f;
    public float cameraShakeDuration = 0.35f;

    [Header("Burbuja de dialogo")]
    public bool bubbleEnabled = true;
    [Tooltip("Pop de apertura de la burbuja.")]
    public float bubblePopDuration = 0.22f;
    public Ease bubblePopEase = Ease.OutBack;
    [Tooltip("Micro-punch al pasar de linea.")]
    public float bubbleLinePunch = 0.06f;
    [Tooltip("Encogida al cerrar.")]
    public float bubbleHideDuration = 0.15f;

    [Header("Barra del iglu (mantener click)")]
    public bool holdBarEnabled = true;
    public float barPopDuration = 0.2f;
    public Ease barPopEase = Ease.OutBack;
}
