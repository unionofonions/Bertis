using System;
using UnityEngine;
using Clockwork.Bertis;
using Clockwork.Bertis.Gameplay;
using System.Buffers;

namespace Clockwork.Scripting
{
    public sealed class CommandDefinitions : MonoBehaviour
    {
        private static readonly StringComparison StringComparison = StringComparison.OrdinalIgnoreCase;

        [SerializeField]
        private PlayerPuppet _playerPuppet;

        public void Register(CommandConsole console)
        {
            ThrowHelpers.ThrowIfNull(console);
        }
    }
}