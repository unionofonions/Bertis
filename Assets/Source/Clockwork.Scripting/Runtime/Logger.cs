using System;
using System.Text;
using UnityEngine;
using ImGuiNET;
using Clockwork.Collections;
using static Clockwork.Scripting.StringInterpolationHelper;

#nullable enable

namespace Clockwork.Scripting;

public enum LogLevel
{
    Trace = 0,
    Information = 1,
    Warning = 2,
    Error = 3
}

[Serializable]
public readonly struct LogEntry
{
    public readonly LogLevel Level;
    public readonly string Message;
    public readonly object? Context;
    public readonly float Timestamp;
    public readonly int FrameCount;

    public LogEntry(
        LogLevel level,
        string message,
        object? context,
        float timestamp,
        int frameCount)
    {
        Level = level;
        Message = message;
        Context = context;
        Timestamp = timestamp;
        FrameCount = frameCount;
    }
}

public class Logger
{
    private const string NullMessageString = "<null>";
    private const StringComparison FilterStringComparison = StringComparison.OrdinalIgnoreCase;

    private int _maxEntries = 100;
    private bool _showTimestamp = false;
    private bool _showFrameCount = false;
    private bool _autoScroll = true;

    private readonly Deque<LogEntry> _allEntries = new();
    private readonly Vector<LogEntry> _filteredEntries = new();

    private readonly bool[] _levelFilters = { true, true, true, true };
    private string _searchFilter = string.Empty;
    private string _tagFilter = string.Empty;

    private bool _isWindowOpen = true;
    private bool _showWindow = true;
    private int _selectedEntry = -1;
    private bool _followTail = true;
    private Vector2 _scrollPosition = Vector2.zero;

    private readonly int[] _levelCounts = new int[4];
    private readonly byte[] _searchBuffer = new byte[256];
    private readonly byte[] _tagBuffer = new byte[256];

    private static readonly Color[] LogColors = new Color[]
    {
        new(0.75f, 0.75f, 0.75f, 1f),
        new(0.25f, 0.75f, 0.35f, 1f),
        new(0.95f, 0.55f, 0.20f, 1f),
        new(0.95f, 0.25f, 0.25f, 1f)
    };

    public void Log(LogLevel level, object? message, object? context = null)
    {
        var entry = new LogEntry(
            level,
            message?.ToString() ?? NullMessageString,
            context,
            Time.unscaledTime,
            Time.frameCount);

        _allEntries.PushBack(entry);
        _levelCounts[(int)level]++;

        while (_allEntries.Count > _maxEntries)
        {
            LogEntry front = _allEntries.PopFront();
            _levelCounts[(int)front.Level]--;
        }

        UpdateFilteredLogs();

        if (_followTail && _autoScroll)
        {
            _scrollPosition.y = float.MaxValue;
        }
    }

    public void Clear()
    {
        _allEntries.Clear();
        _filteredEntries.Clear();
        _selectedEntry = -1;
        Array.Clear(_levelCounts, 0, _levelCounts.Length);
    }

    private void UpdateFilteredLogs()
    {
        _filteredEntries.Clear();

        for (int i = 0; i < _allEntries.Count; i++)
        {
            LogEntry entry = _allEntries[i];
            if (!_levelFilters[(int)entry.Level])
            {
                continue;
            }
            if (!string.IsNullOrEmpty(_searchFilter) &&
                entry.Message.IndexOf(_searchFilter, FilterStringComparison).Equals(-1))
            {
                continue;
            }
            if (!string.IsNullOrEmpty(_tagFilter) &&
                entry.Message.IndexOf(_tagFilter, FilterStringComparison).Equals(-1))
            {
                continue;
            }
            _filteredEntries.Push(entry);
        }
    }

    internal void Update()
    {
        ImGui.SetNextWindowSize(new Vector2(800f, 400f), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Log", ref _isWindowOpen, ImGuiWindowFlags.MenuBar))
        {
            DrawMenuBar();
            DrawFilterControls();
            ImGui.Separator();
            DrawLogList();
            ImGui.Separator();
            DrawLogDetails();
        }
        ImGui.End();
    }

    private void DrawMenuBar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("Options"))
            {
                ImGui.Checkbox("Show Timestamps", ref _showTimestamp);
                ImGui.Checkbox("Show Frame Count", ref _showFrameCount);
                ImGui.Checkbox("Auto Scroll", ref _autoScroll);
                ImGui.Separator();
                ImGui.SliderInt("Max Entries", ref _maxEntries, 100, 1000);
                ImGui.EndMenu();
            }
            if (_allEntries.Count != _filteredEntries.Count)
            {
                ImGui.Text(F($"Total: {_allEntries.Count} | Filtered: {_filteredEntries.Count}"));
            }
            ImGui.EndMenuBar();
        }
    }

    private void DrawFilterControls()
    {
        ImGui.Text("Levels:");
        ImGui.SameLine();

        for (int i = 0; i < _levelFilters.Length; i++)
        {
            LogLevel level = (LogLevel)i;
            Color color = LogColors[i];

            ImGui.PushStyleColor(ImGuiCol.Button, color);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, color * 1.2f);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, color * 0.8f);

            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, _levelFilters[i] ? 1f : 0.3f);

            if (ImGui.Button(F($"{ToString(level)} ({_levelCounts[i]})")))
            {
                _levelFilters[i] = !_levelFilters[i];
                UpdateFilteredLogs();
            }

            ImGui.PopStyleVar();

            ImGui.PopStyleColor(3);

            if (i < _levelFilters.Length - 1)
            {
                ImGui.SameLine();
            }
        }

        ImGui.Text("Search:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200f);
        if (ImGui.InputText("##search", _searchBuffer, (uint)_searchBuffer.Length))
        {
            _searchFilter = Encoding.UTF8.GetString(_searchBuffer).TrimEnd('\0');
            UpdateFilteredLogs();
        }

        ImGui.SameLine();
        ImGui.Text("Tag:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        if (ImGui.InputText("##tag", _tagBuffer, (uint)_tagBuffer.Length))
        {
            _tagFilter = Encoding.UTF8.GetString(_tagBuffer).TrimEnd('\0');
            UpdateFilteredLogs();
        }
    }

    private void DrawLogList()
    {
        Vector2 avail = ImGui.GetContentRegionAvail();
        float listHeight = _selectedEntry >= 0 ? avail.y * 0.7f : avail.y - 60f;

        if (ImGui.BeginChild("Log List", new Vector2(0f, listHeight))) // TODO
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 1f));

            for (int i = 0; i < _filteredEntries.Count; i++)
            {
                DrawLogEntry(_filteredEntries[i], i);
            }

            if (_followTail && _autoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
            {
                ImGui.SetScrollHereY(1f);
            }

            ImGui.PopStyleVar();
        }

        ImGui.EndChild();
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, _allEntries.Count > 0 ? 1f : 0.3f);
        if (ImGui.Button("Clear"))
        {
            Clear();
        }
        ImGui.PopStyleVar();
        ImGui.SameLine();
        ImGui.Checkbox("Follow Tail", ref _followTail);
    }

    private void DrawLogEntry(LogEntry entry, int index)
    {
        Color color = LogColors[(int)entry.Level];
        ImGui.PushStyleColor(ImGuiCol.Text, color);

        ReadOnlySpan<char> message;
        if (_showTimestamp)
        {
            if (_showFrameCount)
            {
                message = F($"[{entry.Timestamp:000.00}] {entry.Message} - {entry.FrameCount}");
            }
            else
            {
                message = F($"[{entry.Timestamp:000.00}] {entry.Message}");
            }
        }
        else
        {
            message = entry.Message;
        }

        if (ImGui.Selectable(message, _selectedEntry == index))
        {
            _selectedEntry = index;
        }

        if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup(F($"Log Context {index}"));
        }

        if (ImGui.BeginPopup(F($"Log Context {index}")))
        {
            if (ImGui.MenuItem("Copy Message"))
            {
                GUIUtility.systemCopyBuffer = entry.Message;
            }
#if UNITY_EDITOR
            if (ImGui.MenuItem("Ping object"))
            {
                UnityEditor.EditorGUIUtility.PingObject(entry.Context as UnityEngine.Object);
            }
#endif
            ImGui.EndPopup();
        }

        ImGui.PopStyleColor();
    }

    private void DrawLogDetails()
    {
    }

    private static string ToString(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => "Trace",
            LogLevel.Information => "Info",
            LogLevel.Warning => "Warning",
            LogLevel.Error => "Error",
            _ => throw new ArgumentOutOfRangeException(nameof(level))
        };
    }
}