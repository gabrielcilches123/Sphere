using UnityEngine;

/// <summary>
/// El iglu: mantener click sobre el para dormir y pasar al dia siguiente (GDD 6).
/// Se apoya en HoldInteractable y conecta su onComplete a DayManager.Sleep().
/// </summary>
[RequireComponent(typeof(HoldInteractable))]
public class Igloo : MonoBehaviour
{
    void Start()
    {
        GetComponent<HoldInteractable>().onComplete.AddListener(() =>
        {
            if (DayManager.Instance != null) DayManager.Instance.Sleep();
        });
    }
}
