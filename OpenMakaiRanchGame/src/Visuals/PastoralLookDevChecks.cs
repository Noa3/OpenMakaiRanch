using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;

namespace OpenMakaiRanch.Visuals;

/// <summary>Rendered checks for the isolated pastoral study; never launched on a normal player profile.</summary>
public partial class PastoralLookDevChecks : Node
{
    private PastoralLookDev _study = null!;
    private readonly List<object> _checks = new(), _captures = new();
    private int _failures;
    private string _output = "";
    public override async void _Ready()
    {
        var canWrite = false;
        try {
            _output = OS.GetEnvironment("OMR_LOOKDEV_OUTPUT");
            var id = OS.GetEnvironment("OMR_LOOKDEV_RUN_ID");
            if (!Path.IsPathFullyQualified(_output) || string.IsNullOrWhiteSpace(id)
                || !File.Exists(Path.Combine(_output,"owner.txt")) || File.ReadAllText(Path.Combine(_output,"owner.txt")).Trim()!=id)
                throw new InvalidOperationException("Use the isolated lookdev launcher.");
            canWrite = true;
            if (DisplayServer.GetName()=="headless") throw new InvalidOperationException("A real renderer/display is required.");
            Check("requested renderer active",RenderingServer.GetCurrentRenderingMethod().ToString()==OS.GetEnvironment("OMR_LOOKDEV_RENDERER"));
            _study = new PastoralLookDev(); AddChild(_study); await Frames(5);
            Check("isolated world",_study.View.OwnWorld3D);
            Check("sky bound",_study.SkyView.Bound);
            Check("familiar green grass",_study.Grass.AlbedoColor.G > _study.Grass.AlbedoColor.R && _study.Grass.AlbedoColor.G > _study.Grass.AlbedoColor.B);
            Check("native resolution",Mathf.IsEqualApprox(_study.View.Scaling3DScale,1));
            Check("no depth of field",_study.Camera.Attributes is null);
            Check("no bloom needed",!_study.EnvironmentNode.Environment.GlowEnabled);
            Check("sky has no uncontrolled clock",!PastoralSky.ShaderCode.Contains("TIME") && !PastoralSky.ShaderCode.Contains("POSITION"));
            var updates=_study.SkyView.ParameterUpdates;
            for(var i=0;i<100;i++) _study.SetView(0,.24f,true,"High");
            Check("unchanged context never dirties sky",_study.SkyView.ParameterUpdates==updates);
            var day=await Capture("pastoral-day");
            _study.SetView(0,.24f,false,"High"); await Frames(4);
            Check("planet not visible by day",Difference(day,ImageNow())<.001);
            Check("daytime lamp is not a point light",_study.Lantern.LampLight.LightEnergy==0);
            _study.SetView(1,0,true,"High");
            var night=await Capture("pastoral-night");
            Check("day and night visibly differ",Difference(day,night)>.05);
            var planetPixel=ProjectDirection(PastoralSky.PlanetDirection);
            var moonPixel=ProjectDirection(PastoralSky.MoonDirection);
            Check("companion world is within view",_study.View.GetVisibleRect().HasPoint(planetPixel));
            Check("moon is within view",_study.View.GetVisibleRect().HasPoint(moonPixel));
            _study.SetView(1,0,false,"High");
            var ordinaryNight=await Capture("pastoral-moon-only");
            Check("second body visibly toggles",Difference(night,ordinaryNight)>.00001);
            Check("second body changes its own sky region",ColorDifference(Probe(night,planetPixel),Probe(ordinaryNight,planetPixel))>.015);
            Check("ordinary moon stays visible when companion is off",ColorDifference(Probe(night,moonPixel),Probe(ordinaryNight,moonPixel))<.015);
            _study.SetView(1,1,true,"High"); var storm=await Capture("pastoral-overcast");
            _study.SetView(1,1,false,"High"); await Frames(4);
            Check("overcast occludes second body",Difference(storm,ImageNow())<.001);
            Check("overcast is visibly different",Difference(night,storm)>.01);
            _study.SetView(1,0,true,"Low"); await Capture("pastoral-low");
            Check("low retains blue companion sky",_study.SkyView.Bound && (bool)_study.SkyView.Material.GetShaderParameter("second_body"));
            Check("low removes local point light",_study.Lantern.LampLight.LightEnergy==0);
            Check("low retains stone emission",_study.Lantern.StoneMaterial.EmissionEnergyMultiplier>1);
            _study.Lantern.Configure(1,"High",false,true);
            Check("shelter suppresses exterior light",_study.Lantern.LampLight.LightEnergy==0);
            _study.Lantern.Configure(1,"Medium",true,false);
            Check("medium avoids shadow passes",!_study.Lantern.LampLight.ShadowEnabled);
            Check("lamp distance fading active",_study.Lantern.LampLight.DistanceFadeEnabled);
            Check("lamp radius bounded",_study.Lantern.LampLight.OmniRange<=4.5f);
            Check("stone stays opaque",_study.Lantern.StoneMaterial.Transparency==BaseMaterial3D.TransparencyEnum.Disabled);
            _study.SetView(1,0,true,"High"); _study.SetLanternCamera(true);
            var lit=await Capture("pastoral-lantern-on");
            _study.Lantern.Configure(1,"High",false,false,false);
            var unlit=await Capture("pastoral-lantern-off");
            Check("mana stone and local light visibly change",Difference(lit,unlit)>.003);
            var seconds=_study.VisualSeconds;
            _study.AdvanceWater(1,true); Check("water pause respected",_study.VisualSeconds==seconds);
            _study.ReducedMotion=true; _study.AdvanceWater(1,false); Check("water reduced motion respected",_study.VisualSeconds==seconds);
            _study.ReducedMotion=false; _study.AdvanceWater(double.NaN,false); Check("water invalid delta ignored",_study.VisualSeconds==seconds);
            _study.AdvanceWater(1,false); Check("water catchup bounded",_study.VisualSeconds>seconds && _study.VisualSeconds-seconds<=.100001);
            Contracts();
        } catch(Exception e) { Check(e.Message,false); GD.PushError(e.ToString()); }
        if(canWrite) File.WriteAllText(Path.Combine(_output,"results.json"),JsonSerializer.Serialize(new {
            schema=1, source_commit=OS.GetEnvironment("OMR_LOOKDEV_SOURCE_COMMIT"), run_id=OS.GetEnvironment("OMR_LOOKDEV_RUN_ID"),
            renderer=RenderingServer.GetCurrentRenderingMethod().ToString(), adapter=RenderingServer.GetVideoAdapterName(),
            engine=Engine.GetVersionInfo()["string"].ToString(), scope="Pastoral atmosphere prototype, not production landscape/performance certification",
            passed=_failures==0, failures=_failures, checks=_checks,captures=_captures
        },new JsonSerializerOptions { WriteIndented=true }));
        GD.Print(_failures==0?"PASTORAL LOOKDEV PASS":"PASTORAL LOOKDEV FAIL");
        if(_study is not null) { _study.QueueFree(); await Frames(2); }
        GetTree().Quit(_failures==0?0:1);
    }
    private void Contracts()
    {
        var node=new WorldEnvironment { Environment=new Godot.Environment() };
        using var a=new PastoralSky(); using var b=new PastoralSky();
        var originalMode=node.Environment.BackgroundMode;
        Check("empty background may be bound",a.TryBind(node));
        Check("competing sky owner refused",!b.TryBind(node,true));
        a.Configure(float.NaN,float.PositiveInfinity,Vector3.Zero,true);
        Check("invalid night becomes finite",(float)a.Material.GetShaderParameter("night_amount")==0);
        Check("invalid clouds become finite",(float)a.Material.GetShaderParameter("cloud_cover")==0);
        Check("invalid sun becomes normalized",Mathf.IsEqualApprox(((Vector3)a.Material.GetShaderParameter("sun_direction")).Length(),1));
        a.Dispose(); Check("release restores background",node.Environment.Sky is null && node.Environment.BackgroundMode==originalMode);
        var authored=new Sky { SkyMaterial=new ProceduralSkyMaterial() }; node.Environment.Sky=authored;
        Check("authored sky never overwritten implicitly",!a.TryBind(node) && node.Environment.Sky==authored);
        Check("explicit lab replacement allowed",a.TryBind(node,true)); a.Dispose(); Check("authored sky restored",node.Environment.Sky==authored);
        a.TryBind(node,true); var replacement=new Sky { SkyMaterial=new ProceduralSkyMaterial() };
        node.Environment.Sky=replacement; a.Configure(1,0,Vector3.Up,true);
        Check("external sky takeover preserved",node.Environment.Sky==replacement && node.Environment.BackgroundMode==Godot.Environment.BGMode.Sky);
        node.Free();
    }
    private void Check(string name,bool value) { _checks.Add(new { name,passed=value }); if(!value)_failures++; GD.Print($"PASTORAL {(value?"OK":"FAIL")}: {name}"); }
    private async Task Frames(int count) { for(var i=0;i<count;i++) await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw); }
    private Image ImageNow()=>_study.View.GetTexture().GetImage();
    private async Task<Image> Capture(string name) {
        await Frames(8); var image=ImageNow();
        if(image.IsEmpty() || image.SavePng(Path.Combine(_output,name+".png"))!=Error.Ok) throw new IOException("Capture failed");
        _captures.Add(new { file=name+".png",width=image.GetWidth(),height=image.GetHeight() }); return image;
    }
    private Vector2 ProjectDirection(Vector3 direction)=>_study.Camera.UnprojectPosition(_study.Camera.GlobalPosition+direction*100);
    private static Color Probe(Image image,Vector2 p) {
        var x=Mathf.RoundToInt(p.X);var y=Mathf.RoundToInt(p.Y);
        if(x<1||y<1||x>=image.GetWidth()-1||y>=image.GetHeight()-1) throw new InvalidOperationException("Sky probe outside image");
        return image.GetPixel(x,y);
    }
    private static float ColorDifference(Color a,Color b)=>(Math.Abs(a.R-b.R)+Math.Abs(a.G-b.G)+Math.Abs(a.B-b.B))/3;
    private static double Difference(Image a,Image b) {
        if(a.GetSize()!=b.GetSize()) throw new InvalidOperationException("Mismatched captures");
        double total=0;var count=0;
        for(var y=0;y<a.GetHeight();y+=4) for(var x=0;x<a.GetWidth();x+=4) { total+=ColorDifference(a.GetPixel(x,y),b.GetPixel(x,y));count++; }
        return total/Math.Max(1,count);
    }
}
