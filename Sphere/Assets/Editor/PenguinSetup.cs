#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// Arma un Animator Controller "PenguinPlayer" con dos estados (Idle por defecto y Walk)
/// y un parametro bool "Walk" que alterna entre ambos. Usa las animaciones del asset
/// Nine Pines (penguin_idle / penguin_walk), fuerza loop, y lo asigna al Animator del
/// pinguino de la escena.
///
/// Ejecutar con: Tools > Sphere > Setup Penguin Walk
/// </summary>
public static class PenguinSetup
{
    const string Dir = "Assets/Nine Pines Animation/2D Character Sprite Animation - Penguin/Animations/";
    const string IdlePath = Dir + "penguin_idle.anim";
    const string WalkPath = Dir + "penguin_walk.anim";
    const string OutPath = "Assets/Animations/PenguinPlayer.controller";
    const string PenguinObject = "penguin_idle_01";

    [MenuItem("Tools/Sphere/Setup Penguin Walk")]
    public static void Setup()
    {
        AnimationClip idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(IdlePath);
        AnimationClip walk = AssetDatabase.LoadAssetAtPath<AnimationClip>(WalkPath);
        if (idle == null || walk == null)
        {
            Debug.LogError("[PenguinSetup] No encontre penguin_idle.anim o penguin_walk.anim.");
            return;
        }

        SetLoop(idle);
        SetLoop(walk);

        Directory.CreateDirectory("Assets/Animations");
        AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(OutPath);
        ctrl.AddParameter("Walk", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;

        AnimatorState idleState = sm.AddState("Idle");
        idleState.motion = idle;
        AnimatorState walkState = sm.AddState("Walk");
        walkState.motion = walk;
        sm.defaultState = idleState;

        AnimatorStateTransition toWalk = idleState.AddTransition(walkState);
        toWalk.AddCondition(AnimatorConditionMode.If, 0f, "Walk");
        toWalk.hasExitTime = false;
        toWalk.duration = 0.05f;

        AnimatorStateTransition toIdle = walkState.AddTransition(idleState);
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "Walk");
        toIdle.hasExitTime = false;
        toIdle.duration = 0.05f;

        // Asignar el controller al Animator del pinguino de la escena.
        GameObject go = GameObject.Find("Player/" + PenguinObject);
        if (go == null) go = GameObject.Find(PenguinObject);
        if (go != null)
        {
            Animator animator = go.GetComponent<Animator>();
            if (animator != null)
            {
                animator.runtimeAnimatorController = ctrl;
                EditorUtility.SetDirty(animator);
                Debug.Log("[PenguinSetup] Controller asignado al Animator de " + go.name);
            }
        }
        else
        {
            Debug.LogWarning("[PenguinSetup] No encontre el pinguino en la escena; asigna " + OutPath + " a mano.");
        }

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[PenguinSetup] Listo. Controller en " + OutPath);
    }

    static void SetLoop(AnimationClip clip)
    {
        AnimationClipSettings s = AnimationUtility.GetAnimationClipSettings(clip);
        if (!s.loopTime)
        {
            s.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, s);
            EditorUtility.SetDirty(clip);
        }
    }
}
#endif
