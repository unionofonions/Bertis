using UnityEngine;
using UnityEngine.Rendering.Universal;
using Clockwork.Collections;
using Clockwork.Pooling;

namespace Clockwork.Simulation
{
    [RequireComponent(typeof(DecalProjector))]
    public class DecalDescriptor : MonoBehaviour, IPoolWorker
    {
        [SerializeField]
        private Vector2Int _atlasSize;
        [SerializeField]
        private bool _globalAtlasRandomization;

        [SerializeField]
        private bool _twist;

        [SerializeField]
        private FadeConfig _fadeConfig;
        private FadeCompositor _fadeCompositor;

        private float _alphaScale = 1f;

        private DecalProjector _projector;

        public float AlphaScale
        {
            set => _alphaScale = Math.Clamp01(value);
        }

        public bool FlipHorizontally
        {
            set
            {
                Vector2 temp = _projector.uvScale;
                temp.x = Math.Abs(temp.x) * (value ? -1f : 1f);
                _projector.uvScale = temp;
            }
        }

        public bool FlipVertically
        {
            set
            {
                Vector2 temp = _projector.uvScale;
                temp.y = Math.Abs(temp.y) * (value ? -1f : 1f);
                _projector.uvScale = temp;
            }
        }

        private void SetFadeFactor(float value)
            => _projector.fadeFactor = value * _alphaScale;

        public void PlaceFree(Vector3 position, Quaternion rotation)
        {
            if (!_fadeCompositor.Start())
            {
                Deactivate();
                return;
            }

            if (_atlasSize is { x: > 0, y: > 0 })
            {
                _projector.uvBias = _globalAtlasRandomization && PrefabRegistry.PrefabOf(this, out var prefab)
                    ? AtlasUvSampler.SampleGlobal(prefab, _atlasSize)
                    : AtlasUvSampler.SampleLocal(_atlasSize);
            }

            transform.SetPositionAndRotation(position, rotation);
            gameObject.SetActive(true);
        }

        public void PlaceNormal(Vector3 position, Vector3 normal)
        {
            Quaternion rotation = Quaternion.LookRotation(-normal);
            if (_twist)
            {
                float twistAngle = Random.Shared.NextSingle() * 360f;
                rotation = Quaternion.AngleAxis(twistAngle, normal) * rotation;
            }
            PlaceFree(position, rotation);
        }

        public void PlaceProjected(Vector3 position, Vector3 direction, Vector3 normal)
        {
            Vector3 forward = -normal;
            Vector3 upward = Vector3.ProjectOnPlane(direction, forward);
            PlaceFree(position, Quaternion.LookRotation(forward, upward));
        }

        protected void Awake()
        {
            _projector = GetComponent<DecalProjector>();
            _fadeCompositor = new FadeCompositor(SetFadeFactor, _fadeConfig);
        }

        protected void Update()
        {
            if (!_fadeCompositor.Update(Time.deltaTime))
            {
                Deactivate();
            }
        }

        protected void OnValidate()
        {
            if (_fadeCompositor != null)
            {
                _fadeCompositor.Config = _fadeConfig;
            }
        }

        private void Deactivate()
        {
            _alphaScale = 1f;
            gameObject.SetActive(false);
            PrefabRegistry.Return(this);
        }

        void IPoolWorker.ScheduleEarlyReturn()
        {
            if (_fadeCompositor.State != FadeState.FadeOut)
            {
                _fadeCompositor.Start(FadeState.FadeOut);
            }
        }

        private static class AtlasUvSampler
        {
            private static readonly HashMap<DecalDescriptor, UniqueSampler<Vector2>> s_globalAtlasSampler = new();

            public static Vector2 SampleGlobal(DecalDescriptor prefab, Vector2Int size)
            {
                if (!s_globalAtlasSampler.TryGetValue(prefab, out var sampler))
                {
                    Vector2[] combinations = GetCombinations(size);
                    sampler = new UniqueSampler<Vector2>(combinations, copyItems: false)
                    {
                        KeepOrder = false,
                        UniqueSamples = int.MaxValue
                    };
                    s_globalAtlasSampler.Add(prefab, sampler);
                }
                return sampler.Sample();
            }

            public static Vector2 SampleLocal(Vector2Int size)
            {
                return new(
                    Random.Shared.NextInt32(size.x) / (float)size.x,
                    Random.Shared.NextInt32(size.y) / (float)size.y);
            }

            private static Vector2[] GetCombinations(Vector2Int size)
            {
                var result = new Vector2[size.x * size.y];
                for (int y = 0; y < size.y; y++)
                {
                    for (int x = 0; x < size.x; x++)
                    {
                        result[x + y * size.x] = new((float)x / size.x, (float)y / size.y);
                    }
                }
                return result;
            }
        }
    }
}