using UnityEngine;

#nullable enable

namespace Clockwork;

public static class ActorHelpers
{
    public static GameObject PersistentActor(string name)
    {
        var actor = new GameObject(name);
        Object.DontDestroyOnLoad(actor);
        return actor;
    }

    public static Transform PersistentTransform(string name)
        => PersistentActor(name).transform;

    public static T PersistentComponent<T>(string name) where T : Component
        => PersistentActor(name).AddComponent<T>();

    public static void ResetLocalPositionAndRotation(this Transform transform)
        => transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
}