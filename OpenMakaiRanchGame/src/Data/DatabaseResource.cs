using Godot;
using Godot.Collections;
using OpenMakaiRanch.Core.Resources;

namespace OpenMakaiRanch.Data;

[GlobalClass]
public partial class DatabaseResource : Resource
{
    [Export] public Array<CharacterDefinition> Characters { get; set; } = new();
    [Export] public Array<JobDefinition> Jobs { get; set; } = new();
    [Export] public Array<ItemDefinition> Items { get; set; } = new();
    [Export] public Array<FacilityDefinition> Facilities { get; set; } = new();
    [Export] public Array<MissionDefinition> Missions { get; set; } = new();
    [Export] public Array<EnemyDefinition> Enemies { get; set; } = new();
    [Export] public Array<MilestoneDefinition> Milestones { get; set; } = new();
    [Export] public Array<SkillDefinition> Skills { get; set; } = new();
    [Export] public Array<PetDefinition> Pets { get; set; } = new();
    [Export] public Array<BondEventDefinition> BondEvents { get; set; } = new();
    [Export] public Array<TalentDefinition> Talents { get; set; } = new();
}
