using System;

#nullable enable

namespace Clockwork.Simulation;

[Serializable]
public struct FadeConfig
{
    public float FadeInDuration;
    public float FreezeDuration;
    public float FadeOutDuration;
}

public enum FadeState
{
    None,
    FadeIn,
    Freeze,
    FadeOut
}

[Serializable]
public class FadeCompositor
{
    private readonly Action<float> _setFactor;
    private FadeConfig _config;
    private FadeState _state;
    private float _progress;
    private float _deltaScale;

    public FadeCompositor(Action<float> setFactor, FadeConfig config)
    {
        ThrowHelpers.ThrowIfNull(setFactor);
        _setFactor = setFactor;
        _config = config;
    }

    public FadeConfig Config
    {
        set => _config = value;
    }

    public FadeState State => _state;

    public bool Start(FadeState fromState)
    {
        FadeState nextState;
        float nextDuration;
        float nextFactor;

        switch (fromState)
        {
            case FadeState.None:
                _state = FadeState.None;
                _setFactor(0f);
                return false;

            case FadeState.FadeIn:
                if (_config.FadeInDuration > 0f)
                {
                    nextState = FadeState.FadeIn;
                    nextDuration = _config.FadeInDuration;
                    nextFactor = 0f;
                    break;
                }
                goto case FadeState.Freeze;

            case FadeState.Freeze:
                if (_config.FreezeDuration > 0f)
                {
                    nextState = FadeState.Freeze;
                    nextDuration = _config.FreezeDuration;
                    nextFactor = 1f;
                    break;
                }
                goto case FadeState.FadeOut;

            case FadeState.FadeOut:
                if (_config.FadeOutDuration > 0f)
                {
                    nextState = FadeState.FadeOut;
                    nextDuration = _config.FadeOutDuration;
                    nextFactor = 1f;
                    break;
                }
                goto case FadeState.None;

            default:
                ThrowHelpers.ThrowUndefinedEnumIndex(fromState);
                return false;
        }

        _state = nextState;
        _progress = 0f;
        _deltaScale = 1f / nextDuration;
        _setFactor(nextFactor);
        return true;
    }

    public bool Start()
        => Start(FadeState.FadeIn);

    public bool Update(float deltaTime)
    {
        float progress = _progress + deltaTime * _deltaScale;

        if (progress >= 1f)
        {
            if (_state == FadeState.None)
            {
                return false;
            }
            return Start(_state switch
            {
                FadeState.FadeIn => FadeState.Freeze,
                FadeState.Freeze => FadeState.FadeOut,
                _ => FadeState.None
            });
        }

        switch (_state)
        {
            case FadeState.None:
                return false;
            case FadeState.FadeIn:
                _setFactor(progress);
                break;
            case FadeState.FadeOut:
                _setFactor(1f - progress);
                break;
        }

        _progress = progress;
        return true;
    }
}