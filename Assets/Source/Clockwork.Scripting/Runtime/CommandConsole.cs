using System;
using System.Linq;
using ImGuiNET;
using UnityEngine;
using Clockwork.Collections;

#nullable enable

namespace Clockwork.Scripting;

public class CommandConsole
{
    private const int MaxInputLength = 64;

    private string _input = string.Empty;

    private readonly Vector<string> _history = new();
    private int _historyIndex = -1;

    private bool _isFocused;

    private readonly HashMap<string, Action<string[]>> _commands = new(StringComparer.OrdinalIgnoreCase);

    public void ToggleFocus(bool enable)
        => _isFocused = enable;

    public void RegisterCommand(string command, Action<string[]> invoke)
    {
        ThrowHelpers.ThrowIfNull(command);
        ThrowHelpers.ThrowIfNull(invoke);

        if (!_commands.TryAdd(command, invoke))
        {
            Debug.LogError($"RegisterCommand failed: command '{command}' already registered.");
            return;
        }
    }

    public unsafe void Update()
    {
        if (ImGui.Begin("Command"))
        {
            ImGui.PushItemWidth(-1f);

            if (_isFocused)
            {
                _isFocused = false;
                ImGui.SetKeyboardFocusHere();
            }

            var flags =
                ImGuiInputTextFlags.EnterReturnsTrue |
                ImGuiInputTextFlags.AllowTabInput |
                ImGuiInputTextFlags.EscapeClearsAll |
                ImGuiInputTextFlags.CallbackHistory;

            ImGui.PushStyleColor(ImGuiCol.TextDisabled, new Vector4(1f, 1f, 1f, 0.85f));

            bool pressed = ImGui.InputTextWithHint(
                "##Input",
                GetPlaceholder(),
                ref _input,
                MaxInputLength,
                flags,
                OnInputFieldChanged);

            ImGui.PopStyleColor();

            if (pressed)
            {
                OnInputFieldSubmitted();
            }

            if (_isFocused)
            {
                _isFocused = false;
                ImGui.SetKeyboardFocusHere(-1);
            }

            ImGui.PopItemWidth();
        }

        ImGui.End();

        static string GetPlaceholder()
        {
            float time = (float)ImGui.GetTime() / 3f;
            float phase = time % 1f;
            return (int)(phase * 12f) switch
            {
                0 => "",
                1 => ".",
                2 => "..",
                3 => "...",
                4 => " ..",
                5 => "  .",
                6 => "",
                7 => "  .",
                8 => " ..",
                9 => "...",
                10 => "..",
                _ => ".",
            };
        }
    }

    private unsafe int OnInputFieldChanged(ImGuiInputTextCallbackData* data)
    {
        if (_history.IsEmpty)
        {
            return 0;
        }
        if (data->EventKey == ImGuiKey.UpArrow)
        {
            if (_historyIndex == -1)
            {
                _historyIndex = _history.Count - 1;
            }
            else if (_historyIndex > 0)
            {
                _historyIndex--;
            }
        }
        else if (data->EventKey == ImGuiKey.DownArrow)
        {
            if (_historyIndex == -1)
            {
                return 0;
            }
            if (++_historyIndex >= _history.Count)
            {
                _historyIndex = -1;
                return ReplaceInput(data, string.Empty);
            }
        }

        if ((uint)_historyIndex < (uint)_history.Count)
        {
            ReplaceInput(data, _history[_historyIndex]);
        }

        return 0;
    }

    private static unsafe int ReplaceInput(ImGuiInputTextCallbackData* data, string text)
    {
        if (data->EventFlag == ImGuiInputTextFlags.CallbackHistory)
        {
            ImGuiInputTextCallbackDataPtr ptr = data;
            ptr.DeleteChars(0, data->BufTextLen);
            ptr.InsertChars(0, text);
            return 1;
        }
        return 0;
    }

    private void OnInputFieldSubmitted()
    {
        Execute(_input);

        if (_history.IsEmpty || _history[^1] != _input)
        {
            _history.Push(_input);
        }

        _historyIndex = -1;
        _input = string.Empty;
        _isFocused = true;
    }

    private void Execute(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogWarning("> Invalid command");
            return;
        }

        string[] parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!_commands.TryGetValue(parts[0], out var invoke))
        {
            Debug.LogWarning($"> Undefined command: '{text}'");
            return;
        }

        try
        {
            if (parts.Length == 0)
            {
                invoke.Invoke(Array.Empty<string>());
            }
            else if (parts.Length == 1)
            {
                invoke.Invoke(new string[] { parts[0] });
            }
            else
            {
                invoke.Invoke(parts.Skip(1).ToArray());
            }
            Debug.LogTrace($"> {parts[0]}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"> Exception while executing command: {ex.Message}");
        }
    }
}