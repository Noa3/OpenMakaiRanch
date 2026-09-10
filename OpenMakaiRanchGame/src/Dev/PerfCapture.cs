using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Character;
using OpenMakaiRanch.World;

/// <summary>
/// PERF-001 baseline: load the composed 3D ranch, run a warm-up + sample
/// window, then report the engine's own performance monitors (FPS, frame
/// time, physics/process averages) — the real-GPU baseline required before
/// any LOD/instancing decision.
///
/// NOTE: the Godot 4.7.2 C# binding does NOT expose per-frame draw-call /
/// vertex / primitive counts (removed in Godot 4); `Performance.GetMonitor`
/// is the authoritative, compiler-verified stats API. Frame-time + FPS are
/// the meaningful signals for a static ranch scene.
///
/// Run: godot --path PROJECT res://scenes/dev/PerfCapture.tscn
/// (isolated-profile env vars recommended so the personal save is untouched).
/// </summary>
[GlobalClass]
public partial class PerfCapture : Node
{
	private const int WarmupFrames = 60;
	private const int SampleFrames = 120;

	private int _frame;
	private double _minFps = double.MaxValue, _maxFps;
	private double _minFrameMs = double.MaxValue, _maxFrameMs;
	private double _sumFps, _sumFrameMs;
	private int _sampled;
	private bool _done;
	private int _avatarCount;

	private static void Sample(Performance.Monitor monitor, string label)
	{
		double v = Performance.GetMonitor(monitor);
		GD.Print($"PERF_STATS {label} = {v:0.3}");
	}

	public override void _Ready()
	{
		var game = GameRoot.Instance;
		if (game is not null && GodotObject.IsInstanceValid(game))
		{
			game.NewGame();
		}

		var scene = GD.Load<PackedScene>("res://scenes/dev/RanchGreybox.tscn");
		if (scene is null)
		{
			GD.PrintErr("PerfCapture: failed to load RanchGreybox.tscn");
			GetTree().Quit(1);
			return;
		}
		var world = scene.Instantiate() as Node3D;
		if (world is null)
		{
			GD.PrintErr("PerfCapture: scene is not a Node3D");
			GetTree().Quit(1);
			return;
		}
		AddChild(world);

		// Place roster avatars (soft-shaded) so the perf baseline reflects the
		// authored composition, not an empty world.
		var rosterRig = world.GetNodeOrNull<RosterRig>("RosterRig");
		if (rosterRig is not null && game is not null)
		{
			rosterRig.Refresh(game);
			foreach (var id in rosterRig.AvatarIds)
			{
				var av = rosterRig.GetAvatar(id);
				if (av is not null)
				{
					av.UseSoftShading = true;
					av.Rebuild();
					_avatarCount++;
				}
			}
		}

		var cam = new Camera3D { Current = true, Position = new Vector3(22f, 14f, 22f), Fov = 55f };
		cam.LookAtFromPosition(cam.Position, new Vector3(0f, 0.5f, 0f), Vector3.Up);
		AddChild(cam);

		GD.Print($"PerfCapture: roster avatars={_avatarCount}; warmup={WarmupFrames}, sample={SampleFrames}");
	}

	public override void _Process(double delta)
	{
		if (_done) return;
		_frame++;

		if (_frame >= WarmupFrames && _frame < WarmupFrames + SampleFrames)
		{
			double fps = Engine.GetFramesPerSecond();
			double frameMs = delta * 1000.0;
			if (fps > 0.0) { if (fps < _minFps) _minFps = fps; if (fps > _maxFps) _maxFps = fps; }
			if (frameMs < _minFrameMs) _minFrameMs = frameMs;
			if (frameMs > _maxFrameMs) _maxFrameMs = frameMs;
			_sumFps += fps;
			_sumFrameMs += frameMs;
			_sampled++;
		}

		if (_frame >= WarmupFrames + SampleFrames)
		{
			_done = true;
			double avgFps = _sampled > 0 ? _sumFps / _sampled : 0.0;
			double avgMs = _sampled > 0 ? _sumFrameMs / _sampled : 0.0;

			GD.Print($"PERF_STATS sampled={_sampled} avatars={_avatarCount}");
			GD.Print($"PERF_STATS fps min={(_minFps == double.MaxValue ? 0 : _minFps):0.1} avg={avgFps:0.1} max={(_maxFps == 0 ? 0 : _maxFps):0.1}");
			GD.Print($"PERF_STATS frame_ms min={_minFrameMs:0.2} avg={avgMs:0.2} max={_maxFrameMs:0.2}");

			// Authoritative engine monitors (compiler-verified enum members).
			Sample(Performance.Monitor.TimeFps, "fps");
			Sample(Performance.Monitor.TimeProcess, "process_ms");
			Sample(Performance.Monitor.TimePhysicsProcess, "physics_ms");
			Sample(Performance.Monitor.RenderTotalDrawCallsInFrame, "draw_calls");
			Sample(Performance.Monitor.RenderTotalPrimitivesInFrame, "primitives");
			Sample(Performance.Monitor.RenderTotalObjectsInFrame, "objects");
			Sample(Performance.Monitor.RenderVideoMemUsed, "video_mem_mb");
			Sample(Performance.Monitor.ObjectNodeCount, "node_count");
			Sample(Performance.Monitor.ObjectCount, "object_count");
			Sample(Performance.Monitor.MemoryStatic, "mem_static_mb");
			Sample(Performance.Monitor.MemoryStaticMax, "mem_static_max_mb");

			GD.Print("PERF_STATS_OK");
			GetTree().Quit(0);
		}
	}
}
