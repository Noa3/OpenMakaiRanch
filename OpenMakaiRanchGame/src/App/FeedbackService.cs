using System;
using Godot;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.App;

public partial class FeedbackService : Node
{
    private AudioStreamPlayer _player = null!;
    private AudioStreamWav _navigateCue = null!;
    private AudioStreamWav _confirmCue = null!;
    private AudioStreamWav _errorCue = null!;

    public bool AudioEnabled { get; private set; } = true;
    public bool HapticsEnabled { get; private set; } = true;

    public bool SupportsHaptics => OS.HasFeature("mobile") || OS.HasFeature("android") || OS.HasFeature("ios") || OS.HasFeature("web_android");

    public override void _Ready()
    {
        _player = new AudioStreamPlayer
        {
            Bus = "Master",
            VolumeDb = -12f,
            MaxPolyphony = 2
        };
        AddChild(_player);

        _navigateCue = CreateTone(660f, 0.05f, 0.18f);
        _confirmCue = CreateTone(880f, 0.08f, 0.22f);
        _errorCue = CreateTone(220f, 0.12f, 0.24f);
    }

    public void ToggleAudioEnabled()
    {
        AudioEnabled = !AudioEnabled;
    }

    public void ToggleHapticsEnabled()
    {
        HapticsEnabled = !HapticsEnabled;
    }

    public void ApplySettings(SettingsState settings)
    {
        AudioEnabled = settings.AudioEnabled;
        HapticsEnabled = settings.HapticsEnabled;
    }

    public void PlayNavigate()
    {
        PlayCue(_navigateCue, 14, 0.25f);
    }

    public void PlayConfirm()
    {
        PlayCue(_confirmCue, 28, 0.4f);
    }

    public void PlayError()
    {
        PlayCue(_errorCue, 48, 0.7f);
    }

    public void PulseHaptics(int durationMs = 24, float amplitude = 0.35f)
    {
        if (!HapticsEnabled || !SupportsHaptics)
        {
            return;
        }

        Input.VibrateHandheld(durationMs, amplitude);
    }

    private void PlayCue(AudioStreamWav cue, int vibrationMs, float vibrationAmplitude)
    {
        if (AudioEnabled)
        {
            _player.Stream = cue;
            _player.Play();
        }

        PulseHaptics(vibrationMs, vibrationAmplitude);
    }

    private static AudioStreamWav CreateTone(float frequency, float durationSeconds, float volume)
    {
        const int sampleRate = 22050;
        var totalSamples = Math.Max(1, (int)(sampleRate * durationSeconds));
        var pcm = new byte[totalSamples * sizeof(short)];
        for (var index = 0; index < totalSamples; index++)
        {
            var progress = index / (float)totalSamples;
            var envelope = MathF.Min(MathF.Min(progress * 8f, (1f - progress) * 6f), 1f);
            var sample = MathF.Sin(2f * MathF.PI * frequency * index / sampleRate) * envelope * volume;
            var sampleValue = (short)(sample * short.MaxValue);
            pcm[index * 2] = (byte)(sampleValue & 0xff);
            pcm[index * 2 + 1] = (byte)((sampleValue >> 8) & 0xff);
        }

        return new AudioStreamWav
        {
            Format = AudioStreamWav.FormatEnum.Format16Bits,
            MixRate = sampleRate,
            Stereo = false,
            Data = pcm
        };
    }
}