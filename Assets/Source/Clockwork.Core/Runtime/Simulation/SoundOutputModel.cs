using System.Threading;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Clockwork.Collections;
using Clockwork.Pooling;

#nullable enable

namespace Clockwork.Simulation
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundOutputModel : MonoBehaviour
    {
        private bool _isSpatial;
        private float _timer;
        private CancellationToken _cancellationToken;

        private AudioSource? _audioSource;

        public static void Play(SoundDescriptor? descriptor, Vector3 position, CancellationToken cancellationToken = default)
        {
            if (descriptor != null)
            {
                PlayImpl(descriptor, descriptor.Samples.Sample(), position, cancellationToken);
            }
        }

        public static void Play(SoundDescriptor? descriptor, CancellationToken cancellationToken = default)
            => Play(descriptor, position: Vector3.zero, cancellationToken);

        public static void PlayAtIndex(SoundDescriptor? descriptor, int index, Vector3 position, CancellationToken cancellationToken = default)
        {
            if (descriptor != null)
            {
                if ((uint)index >= (uint)descriptor.Samples.Count)
                {
                    Debug.LogError(
                        $"SoundOutputModel.PlayAtIndex failed: index out of range ({index}/{descriptor.Samples.Count}).",
                        context: descriptor);
                    return;
                }
                PlayImpl(descriptor, descriptor.Samples[index], position, cancellationToken);
            }
        }

        public static void PlayAtIndex(SoundDescriptor? descriptor, int index, CancellationToken cancellationToken = default)
            => PlayAtIndex(descriptor, index, position: Vector3.zero, cancellationToken);

        private static void PlayImpl(SoundDescriptor descriptor, AudioClip? sample, Vector3 position, CancellationToken cancellationToken)
        {
            if (sample == null || descriptor.OutputModel == null)
            {
                return;
            }

            if (!descriptor.OutputModel._isSpatial && !cancellationToken.CanBeCanceled)
            {
                SoundOutputModel outputModel = OneShotModelCache.Get(descriptor.OutputModel);
                outputModel._audioSource!.PlayOneShot(sample, descriptor.VolumeScale);
            }
            else
            {
                PrefabRegistry.Rent(descriptor.OutputModel, out SoundOutputModel? outputModel);
                outputModel.transform.position = position;
                outputModel.enabled = true;
                outputModel._audioSource!.volume = descriptor.VolumeScale;
                outputModel._audioSource!.clip = sample;
                outputModel._audioSource!.Play();
                outputModel._timer = sample.length;
                outputModel._cancellationToken = cancellationToken;
            }
        }

        protected void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _isSpatial = _audioSource.spatialBlend > 0f;
        }

        protected void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f || _cancellationToken.IsCancellationRequested)
            {
                _audioSource!.Stop();
                enabled = false;
                PrefabRegistry.Return(this);
            }
        }

        private static class OneShotModelCache
        {
            private static readonly HashMap<SoundOutputModel, SoundOutputModel> s_prefabToInstance = new();

            private static Transform? s_rootParent;

#if UNITY_EDITOR
            static OneShotModelCache()
            {
                EditorApplication.playModeStateChanged += state =>
                {
                    if (state == PlayModeStateChange.EnteredEditMode)
                    {
                        s_rootParent = null;
                        s_prefabToInstance.Clear();
                    }
                };
            }
#endif

            public static SoundOutputModel Get(SoundOutputModel prefab)
            {
                if (!s_prefabToInstance.TryGetValue(prefab, out SoundOutputModel? instance))
                {
                    instance = Create($"[OneShotSoundOutputModel:{prefab.name}]");
                    s_prefabToInstance.Add(prefab, instance);
                }
                return instance;
            }

            private static SoundOutputModel Create(string name)
            {
                if (s_rootParent == null)
                {
                    s_rootParent = ActorHelpers.PersistentTransform("[OneShotSoundOutputModelCache]");
                }

                var actor = new GameObject(name);
                var result = actor.AddComponent<SoundOutputModel>();
                result._audioSource = actor.GetComponent<AudioSource>();
                actor.transform.SetParent(s_rootParent, worldPositionStays: false);
                result.enabled = false;
                return result;
            }
        }
    }
}