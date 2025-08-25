using UnityEngine;
using ImGuiNET;
using Clockwork.Bertis.Components;
using Clockwork.Simulation;

namespace Clockwork.Scripting;

public class LiveConsole
{
    private readonly CameraShakeComponent _cameraShakeComponent;

    public LiveConsole(CameraShakeComponent cameraShakeComponent)
    {
        _cameraShakeComponent = cameraShakeComponent;
    }

    public void Update()
    {
        ImGui.Begin("Live", ImGuiWindowFlags.MenuBar);

        float timeScale = Time.timeScale;
        if (timeScale != 1f)
        {
            if (ImGui.SliderFloat("Time Scale", ref timeScale, 0f, 1.2f))
            {
                Time.timeScale = timeScale;
            }
            if (ImGui.SmallButton("Reset"))
            {
                Time.timeScale = 1f;
            }
        }

        if (TimeDilationSystem.ActiveDilations != 0)
        {
            int activeDilations = TimeDilationSystem.ActiveDilations;
            ImGui.SliderInt(
                "Active Dilations", ref activeDilations,
                0, TimeDilationSystem.MaxSimultaneousDilations,
                "%d", ImGuiSliderFlags.NoInput);
        }

        if (_cameraShakeComponent != null)
        {
            int activeShakes = _cameraShakeComponent.ActiveShakes;
            if (activeShakes != 0)
            {
                ImGui.SliderInt(
                    "Active Shakes", ref activeShakes,
                    0, _cameraShakeComponent.MaxSimultaneousShakes,
                    "%d", ImGuiSliderFlags.NoInput);
            }
        }

        ImGui.End();
    }
}