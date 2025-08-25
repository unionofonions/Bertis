using UnityEngine;

namespace Clockwork.Pooling
{
    public sealed class PrefabConfigProvider : MonoBehaviour
    {
        [field: SerializeField]
        public int MaxSize { get; private set; }
    }
}