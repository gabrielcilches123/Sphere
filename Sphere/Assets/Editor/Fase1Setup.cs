#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;

/// <summary>
/// FASE 1 del plan (docs/PLAN-implementacion.md): sistema de Dias.
/// - Anade DayManager al GameSystem y lo conecta (spawner, dayText).
/// - Crea el IGLU placeholder (esfera achatada) sobre el planeta, con
///   HoldInteractable + Igloo (mantener click para dormir).
/// - Crea el texto "Dia N" centrado en el Canvas (anuncio al despertar).
/// - Apaga el goteo de prueba del StarSpawner (las tandas ahora las manda el dia).
///
/// Ejecutar con: Tools > Sphere > Run Fase 1 Setup  (idempotente: no duplica)
/// </summary>
public static class Fase1Setup
{
    [MenuItem("Tools/Sphere/Run Fase 1 Setup")]
    public static void Run()
    {
        GameObject gs = GameObject.Find("GameSystem");
        if (gs == null) { Debug.LogError("[Fase1] No encontre GameSystem."); return; }

        // 1. DayManager en GameSystem.
        DayManager dm = gs.GetComponent<DayManager>();
        if (dm == null) { dm = gs.AddComponent<DayManager>(); Debug.Log("[Fase1] DayManager anadido."); }
        dm.spawner = gs.GetComponent<StarSpawner>();

        // 2. Apagar goteo de prueba: ahora el dia manda las tandas.
        StarSpawner spawner = gs.GetComponent<StarSpawner>();
        if (spawner != null && spawner.spawnInterval > 0f)
        {
            spawner.spawnInterval = 0f;
            Debug.Log("[Fase1] Goteo de prueba del StarSpawner apagado (spawnInterval = 0).");
        }

        // 3. Iglu placeholder sobre el planeta.
        GameObject planet = GameObject.Find("Planet");
        if (GameObject.Find("Igloo") == null && planet != null)
        {
            GameObject igloo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            igloo.name = "Igloo";

            // Posicion en la superficie: angulo 135 (arriba-izquierda), plano frontal z=-5.
            Vector3 center = planet.transform.position;
            float ang = 135f * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f);
            igloo.transform.position = new Vector3(
                center.x + dir.x * 7.5f,
                center.y + dir.y * 7.5f,
                -5f);
            igloo.transform.rotation = Quaternion.Euler(0f, 0f, 135f - 90f); // de pie, radial
            igloo.transform.localScale = new Vector3(2.4f, 1.6f, 1.6f);      // domo achatado
            igloo.transform.SetParent(planet.transform, true);

            HoldInteractable hold = igloo.AddComponent<HoldInteractable>();
            hold.holdSeconds = 1.5f;
            igloo.AddComponent<Igloo>();

            Debug.Log("[Fase1] Iglu creado y colgado del planeta.");
        }

        // 4. Texto "Dia N" en el Canvas.
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            Transform existing = canvas.transform.Find("DayText");
            GameObject dtGo = existing != null ? existing.gameObject : null;
            if (dtGo == null)
            {
                dtGo = new GameObject("DayText", typeof(RectTransform));
                dtGo.transform.SetParent(canvas.transform, false);
                TextMeshProUGUI tmp = dtGo.AddComponent<TextMeshProUGUI>();
                tmp.text = "Dia 1";
                tmp.fontSize = 56f;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                RectTransform rt = dtGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.75f);
                rt.anchorMax = new Vector2(0.5f, 0.75f);
                rt.sizeDelta = new Vector2(600f, 100f);
                dtGo.SetActive(false); // lo activa DayManager al anunciar
                Debug.Log("[Fase1] DayText creado en el Canvas.");
            }
            dm.dayText = dtGo.GetComponent<TMP_Text>();
        }

        EditorUtility.SetDirty(dm);
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("[Fase1] Setup completo y escena guardada.");
    }
}
#endif
