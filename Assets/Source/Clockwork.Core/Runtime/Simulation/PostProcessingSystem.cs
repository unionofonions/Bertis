using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Clockwork.Collections;

#nullable enable

namespace Clockwork.Simulation;

public static class PostProcessingSystem
{
    private static readonly HashMap<PostProcessingDescriptor, PostProcessingDescriptor> s_prefabToWorker = new();
    private static Transform? s_parent;

#if UNITY_EDITOR
    static PostProcessingSystem()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                s_parent = null;
                s_prefabToWorker.Clear();
            }
        };
    }
#endif

    public static void StartAnimation(PostProcessingDescriptor? descriptor)
    {
        if (descriptor == null)
        {
            return;
        }

        if (!s_prefabToWorker.TryGetValue(descriptor, out PostProcessingDescriptor? worker))
        {
            if (s_parent == null)
            {
                s_parent = ActorHelpers.PersistentTransform($"[{nameof(PostProcessingSystem)}]");
            }
            worker = UnityEngine.Object.Instantiate(descriptor, s_parent);
            s_prefabToWorker.Add(descriptor, worker);
        }

        worker.StartAnimation();
    }
}