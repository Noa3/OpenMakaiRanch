using System;
using Godot;
using GEnvironment = Godot.Environment;

namespace OpenMakaiRanch.Visuals;

/// <summary>
/// Familiar blue sky, ordinary clouds/sun and an optional fictional Earth-like companion to the moon.
/// Caller-supplied presentation state only. Static between changes: no TIME, camera-position uniform,
/// ephemeris, weather rolls or per-frame radiance invalidation. Never replaces an authored sky implicitly.
/// </summary>
public sealed class PastoralSky : IDisposable
{
    private const string Lease = "_omr_pastoral_sky_owner";
    private WorldEnvironment? _target;
    private GEnvironment? _environment;
    private Sky? _previousSky;
    private GEnvironment.BGMode _previousBackground;
    private GEnvironment.AmbientSource _previousAmbient;
    private (float Night, float Clouds, Vector3 Sun, bool Planet)? _last;
    public ShaderMaterial Material { get; } = new() { Shader = new Shader { Code = ShaderCode } };
    public Sky Sky { get; }
    public int ParameterUpdates { get; private set; }
    public bool Bound => _environment is not null && GodotObject.IsInstanceValid(_environment)
        && _target is not null && GodotObject.IsInstanceValid(_target) && _target.Environment == _environment
        && OwnsLease && _environment.Sky == Sky && _environment.BackgroundMode == GEnvironment.BGMode.Sky;
    private bool OwnsLease => _environment is not null && GodotObject.IsInstanceValid(_environment)
        && _environment.HasMeta(Lease) && unchecked((ulong)_environment.GetMeta(Lease).AsInt64()) == Material.GetInstanceId();

    public PastoralSky()
    {
        Sky = new Sky { SkyMaterial = Material };
        Sky.Set("process_mode", 2); // Incremental: state changes slowly, never every frame.
        Sky.Set("radiance_size", 2); // 128x128; visible celestial edges are evaluated at native resolution.
    }
    public bool TryBind(WorldEnvironment target, bool replaceAuthoredSky = false)
    {
        Dispose();
        if (!GodotObject.IsInstanceValid(target) || target.Environment is not { } environment
            || environment.HasMeta(Lease) || (!replaceAuthoredSky && environment.Sky is not null)) return false;
        _target = target; _environment = environment; _previousSky = environment.Sky;
        _previousBackground = environment.BackgroundMode; _previousAmbient = environment.AmbientLightSource;
        environment.SetMeta(Lease, Material.GetInstanceId());
        environment.Sky = Sky; environment.BackgroundMode = GEnvironment.BGMode.Sky;
        // Preserve the existing DaylightRig's ambient-color/energy authority, not implicit sky ambient.
        environment.AmbientLightSource = GEnvironment.AmbientSource.Color;
        return true;
    }
    public bool Configure(float night, float cloudCover, Vector3 sunDirection, bool secondBody)
    {
        if (!Bound) { Dispose(); return false; }
        night = PresenceTemperament.Unit(night); cloudCover = PresenceTemperament.Unit(cloudCover);
        if (!float.IsFinite(sunDirection.X) || !float.IsFinite(sunDirection.Y) || !float.IsFinite(sunDirection.Z)
            || sunDirection.LengthSquared() < .001f) sunDirection = new Vector3(.2f, .7f, -.6f);
        sunDirection = sunDirection.Normalized();
        var current = (night, cloudCover, sunDirection, secondBody);
        if (_last == current) return true;
        Material.SetShaderParameter("night_amount", night); Material.SetShaderParameter("cloud_cover", cloudCover);
        Material.SetShaderParameter("sun_direction", sunDirection); Material.SetShaderParameter("second_body", secondBody);
        _last = current; ParameterUpdates++; return true;
    }
    public void Dispose()
    {
        if (OwnsLease)
        {
            // Sky and background form one transaction. A replacement sky keeps its chosen background.
            if (_environment!.Sky == Sky && _environment.BackgroundMode == GEnvironment.BGMode.Sky) {
                _environment.Sky = _previousSky; _environment.BackgroundMode = _previousBackground;
                if (_environment.AmbientLightSource == GEnvironment.AmbientSource.Color) _environment.AmbientLightSource = _previousAmbient;
            }
            _environment.RemoveMeta(Lease);
        }
        _environment = null; _target = null; _last = null;
    }
    public static readonly Vector3 MoonDirection = new Vector3(.29f, .28f, -.91f).Normalized();
    public static readonly Vector3 PlanetDirection = new Vector3(-.23f, .38f, -.90f).Normalized();
    public const string ShaderCode = """
        shader_type sky;
        uniform float night_amount = 0.0;
        uniform float cloud_cover = 0.24;
        uniform vec3 sun_direction = vec3(0.2, 0.7, -0.6);
        uniform bool second_body = true;
        float hash21(vec2 p) { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }
        float noise2(vec2 p) {
            vec2 i = floor(p), f = fract(p); f = f * f * (3.0 - 2.0 * f);
            return mix(mix(hash21(i), hash21(i + vec2(1,0)), f.x),
                mix(hash21(i + vec2(0,1)), hash21(i + vec2(1,1)), f.x), f.y);
        }
        float clouds(vec2 p) {
            return noise2(p) * 0.57 + noise2(p * 2.13 + 7.4) * 0.29 + noise2(p * 4.31 + 15.2) * 0.14;
        }
        vec4 body(vec3 ray, vec3 center, float radius, bool planet) {
            if (dot(ray, center) < cos(radius * 1.1)) return vec4(0.0);
            vec3 right = normalize(cross(vec3(0,1,0), center));
            vec3 up = normalize(cross(center, right));
            vec2 p = vec2(dot(ray, right), dot(ray, up)) / sin(radius);
            float r2 = dot(p,p); if (r2 > 1.0) return vec4(0.0);
            vec3 n = vec3(p, sqrt(max(0.0, 1.0 - r2)));
            float light = 0.10 + 0.90 * max(0.0, dot(n, normalize(vec3(-0.7,0.45,0.72))));
            vec3 color;
            if (planet) {
                float land = smoothstep(0.48, 0.59, clouds(p * 3.4 + n.z * vec2(1.2,0.4)));
                color = mix(vec3(0.025,0.16,0.39), vec3(0.15,0.29,0.13), land);
                float white_cloud = smoothstep(0.59,0.76, clouds(p * 7.0 + vec2(n.z * 3.0, 8.0)));
                color = mix(color, vec3(0.77,0.82,0.83), white_cloud * 0.85) * light;
                color += vec3(0.02,0.06,0.10) * pow(1.0 - n.z, 3.0);
            } else {
                float maria = clouds(p * 5.2 + 21.0);
                color = mix(vec3(0.32,0.35,0.39), vec3(0.79,0.80,0.76), maria) * light;
            }
            return vec4(color, 1.0 - smoothstep(0.96,1.0,r2));
        }
        void sky() {
            vec3 d = normalize(EYEDIR);
            float height = pow(max(d.y,0.0), 0.42);
            vec3 day = mix(vec3(0.66,0.80,0.91), vec3(0.13,0.39,0.76), height);
            vec3 night = mix(vec3(0.050,0.080,0.13), vec3(0.008,0.018,0.050), height);
            vec3 color = mix(day, night, night_amount);
            float overcast = smoothstep(0.60,1.0,cloud_cover);
            float twilight = sin(night_amount * PI);
            color += vec3(0.13,0.045,0.008) * twilight * pow(1.0 - max(d.y,0.0), 5.0);
            float sun_angle = acos(clamp(dot(d, normalize(sun_direction)), -1.0,1.0));
            color += vec3(1.0,0.90,0.65) * (1.0-smoothstep(0.006,0.009,sun_angle)) * 2.0 * (1.0-night_amount);
            color += vec3(0.13,0.09,0.03) * exp(-sun_angle * 10.0) * (1.0-night_amount);
            if (!AT_CUBEMAP_PASS && d.y > 0.03) {
                float visibility = smoothstep(0.28,0.85,night_amount);
                vec4 moon = body(d, normalize(vec3(0.29,0.28,-0.91)), 0.013, false);
                color = mix(color, moon.rgb, moon.a * visibility);
                if (second_body) {
                    vec4 planet = body(d, normalize(vec3(-0.23,0.38,-0.90)), 0.034, true);
                    color = mix(color, planet.rgb, planet.a * visibility);
                }
            }
            // Clouds are composited AFTER celestial bodies: no moon/planet drawn on top of bad weather.
            vec2 cloud_uv = d.xz / (max(d.y,0.04) + 0.25) * 2.2;
            float density = clouds(cloud_uv);
            float coverage = smoothstep(0.70-cloud_cover*0.42, 0.85-cloud_cover*0.42, density);
            coverage *= smoothstep(0.025,0.16,d.y);
            coverage = mix(coverage,1.0,overcast);
            vec3 cloud_color = mix(vec3(0.92,0.94,0.95),vec3(0.055,0.072,0.10),night_amount);
            cloud_color *= mix(1.0,0.61,overcast);
            color = mix(color,cloud_color,coverage);
            vec3 ground = mix(vec3(0.16,0.23,0.105),vec3(0.012,0.021,0.014),night_amount);
            COLOR = mix(ground,color,smoothstep(-0.05,0.015,d.y));
        }
        """;
}
