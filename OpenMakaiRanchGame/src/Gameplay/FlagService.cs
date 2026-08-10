using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;

namespace OpenMakaiRanch.Gameplay;

/// <summary>
/// Manages all flag systems: global flags (Flag.csv), temporary flags (Tflag.csv),
/// and character-specific flags (Cflag.csv).
/// Mirrors the original eraMakaiRanch flag architecture.
/// </summary>
public sealed class FlagService
{
    // Global flags (Flag.csv) - 537 flags for game state, events, unlocks
    private readonly Dictionary<int, bool> _globalFlags = new();
    
    // Global int flags (Flag.csv has numeric values too)
    private readonly Dictionary<int, int> _globalIntFlags = new();
    
    // Temporary flags (Tflag.csv) - 104 flags for temporary state
    private readonly Dictionary<int, bool> _tempFlags = new();
    
    // Temporary int flags
    private readonly Dictionary<int, int> _tempIntFlags = new();
    
    // Per-character flags (Cflag.csv) - 125 flags per character
    private readonly Dictionary<string, Dictionary<int, bool>> _charBoolFlags = new();
    private readonly Dictionary<string, Dictionary<int, int>> _charIntFlags = new();
    
    // Flag definitions from CSV
    private List<FlagDefinition> _globalFlagDefs = new();
    private List<FlagDefinition> _tempFlagDefs = new();
    private List<FlagDefinition> _charFlagDefs = new();
    
    public int GlobalFlagCount => _globalFlags.Count + _globalIntFlags.Count;
    public int TempFlagCount => _tempFlags.Count + _tempIntFlags.Count;
    public int TotalCharFlagCount => _charBoolFlags.Values.Sum(d => d.Count) + _charIntFlags.Values.Sum(d => d.Count);
    
    public FlagService()
    {
        SeedFlagDefinitions();
    }
    
    // === SYNC WITH FLAGSTORAGE (SaveState persistence) ===
    
    public void SyncFromStorage(FlagStorage storage)
    {
        _globalFlags.Clear();
        _globalIntFlags.Clear();
        _tempFlags.Clear();
        _tempIntFlags.Clear();
        _charBoolFlags.Clear();
        _charIntFlags.Clear();
        
        foreach (var kvp in storage.GlobalBoolFlags)
            _globalFlags[kvp.Key] = kvp.Value;
        foreach (var kvp in storage.GlobalIntFlags)
            _globalIntFlags[kvp.Key] = kvp.Value;
        foreach (var kvp in storage.TempBoolFlags)
            _tempFlags[kvp.Key] = kvp.Value;
        foreach (var kvp in storage.TempIntFlags)
            _tempIntFlags[kvp.Key] = kvp.Value;
        foreach (var kvp in storage.CharBoolFlags)
            _charBoolFlags[kvp.Key] = new Dictionary<int, bool>(kvp.Value);
        foreach (var kvp in storage.CharIntFlags)
            _charIntFlags[kvp.Key] = new Dictionary<int, int>(kvp.Value);
    }
    
    public void SyncToStorage(FlagStorage storage)
    {
        storage.GlobalBoolFlags.Clear();
        storage.GlobalIntFlags.Clear();
        storage.TempBoolFlags.Clear();
        storage.TempIntFlags.Clear();
        storage.CharBoolFlags.Clear();
        storage.CharIntFlags.Clear();
        
        foreach (var kvp in _globalFlags)
            storage.GlobalBoolFlags[kvp.Key] = kvp.Value;
        foreach (var kvp in _globalIntFlags)
            storage.GlobalIntFlags[kvp.Key] = kvp.Value;
        foreach (var kvp in _tempFlags)
            storage.TempBoolFlags[kvp.Key] = kvp.Value;
        foreach (var kvp in _tempIntFlags)
            storage.TempIntFlags[kvp.Key] = kvp.Value;
        foreach (var kvp in _charBoolFlags)
            storage.CharBoolFlags[kvp.Key] = new Dictionary<int, bool>(kvp.Value);
        foreach (var kvp in _charIntFlags)
            storage.CharIntFlags[kvp.Key] = new Dictionary<int, int>(kvp.Value);
    }
    
    // === GLOBAL FLAGS ===
    
    public bool GetGlobalFlag(int id)
    {
        return _globalFlags.TryGetValue(id, out var value) && value;
    }
    
    public int GetGlobalIntFlag(int id, int defaultValue = 0)
    {
        return _globalIntFlags.TryGetValue(id, out var value) ? value : defaultValue;
    }
    
    public void SetGlobalFlag(int id, bool value)
    {
        _globalFlags[id] = value;
    }
    
    public void SetGlobalIntFlag(int id, int value)
    {
        _globalIntFlags[id] = value;
    }
    
    public void ToggleGlobalFlag(int id)
    {
        if (_globalFlags.TryGetValue(id, out var current))
            _globalFlags[id] = !current;
        else
            _globalFlags[id] = true;
    }
    
    public void IncrementGlobalIntFlag(int id, int delta = 1)
    {
        var current = _globalIntFlags.TryGetValue(id, out var value) ? value : 0;
        _globalIntFlags[id] = current + delta;
    }
    
    // === TEMPORARY FLAGS ===
    
    public bool GetTempFlag(int id)
    {
        return _tempFlags.TryGetValue(id, out var value) && value;
    }
    
    public int GetTempIntFlag(int id, int defaultValue = 0)
    {
        return _tempIntFlags.TryGetValue(id, out var value) ? value : defaultValue;
    }
    
    public void SetTempFlag(int id, bool value)
    {
        _tempFlags[id] = value;
    }
    
    public void SetTempIntFlag(int id, int value)
    {
        _tempIntFlags[id] = value;
    }
    
    public void ClearTempFlags()
    {
        _tempFlags.Clear();
        _tempIntFlags.Clear();
    }
    
    // === CHARACTER FLAGS ===
    
    public bool GetCharFlag(string characterId, int id)
    {
        if (!_charBoolFlags.TryGetValue(characterId, out var flags))
            return false;
        return flags.TryGetValue(id, out var value) && value;
    }
    
    public int GetCharIntFlag(string characterId, int id, int defaultValue = 0)
    {
        if (!_charIntFlags.TryGetValue(characterId, out var flags))
            return defaultValue;
        return flags.TryGetValue(id, out var value) ? value : defaultValue;
    }
    
    public void SetCharFlag(string characterId, int id, bool value)
    {
        if (!_charBoolFlags.ContainsKey(characterId))
            _charBoolFlags[characterId] = new Dictionary<int, bool>();
        _charBoolFlags[characterId][id] = value;
    }
    
    public void SetCharIntFlag(string characterId, int id, int value)
    {
        if (!_charIntFlags.ContainsKey(characterId))
            _charIntFlags[characterId] = new Dictionary<int, int>();
        _charIntFlags[characterId][id] = value;
    }
    
    public void ClearCharFlags(string characterId)
    {
        _charBoolFlags.Remove(characterId);
        _charIntFlags.Remove(characterId);
    }
    
    public void CopyCharFlags(string fromCharacter, string toCharacter)
    {
        if (_charBoolFlags.TryGetValue(fromCharacter, out var boolFlags))
        {
            if (!_charBoolFlags.ContainsKey(toCharacter))
                _charBoolFlags[toCharacter] = new Dictionary<int, bool>();
            foreach (var kvp in boolFlags)
                _charBoolFlags[toCharacter][kvp.Key] = kvp.Value;
        }
        
        if (_charIntFlags.TryGetValue(fromCharacter, out var intFlags))
        {
            if (!_charIntFlags.ContainsKey(toCharacter))
                _charIntFlags[toCharacter] = new Dictionary<int, int>();
            foreach (var kvp in intFlags)
                _charIntFlags[toCharacter][kvp.Key] = kvp.Value;
        }
    }
    
    // === HELPER METHODS FOR EVENT TRIGGERS ===
    
    public bool IsUnlocked(int unlockBitId)
    {
        return GetGlobalIntFlag(unlockBitId, 0) != 0;
    }
    
    public void Unlock(int unlockBitId)
    {
        SetGlobalIntFlag(unlockBitId, 1);
    }
    
    public bool IsEventTriggered(int eventId)
    {
        return GetTempFlag(eventId);
    }
    
    public void TriggerEvent(int eventId)
    {
        SetTempFlag(eventId, true);
    }
    
    public void ClearEvent(int eventId)
    {
        SetTempFlag(eventId, false);
    }
    
    public void ClearAllEvents()
    {
        ClearTempFlags();
    }
    
    public bool HasCharacterFlag(string characterId, int flagId)
    {
        return GetCharFlag(characterId, flagId);
    }
    
    public void SetCharacterFlag(string characterId, int flagId, bool value)
    {
        SetCharFlag(characterId, flagId, value);
    }
    
    public int GetCharacterIntFlag(string characterId, int flagId, int defaultValue = 0)
    {
        return GetCharIntFlag(characterId, flagId, defaultValue);
    }
    
    public void SetCharacterIntFlag(string characterId, int flagId, int value)
    {
        SetCharIntFlag(characterId, flagId, value);
    }
    
    // === GAME STATE HELPERS ===
    
    public int GetSlaveCount() => GetGlobalIntFlag(5, 0);
    public int GetWorkload() => GetGlobalIntFlag(1, 0);
    public int GetMaxWorkload() => GetGlobalIntFlag(2, 0);
    public int GetCattleHealth() => GetGlobalIntFlag(0, 80);
    public int GetPrivateRoomCount() => GetGlobalIntFlag(20, 0);
    public int GetEmptyPrivateRoomCount() => GetGlobalIntFlag(21, 0);
    public int GetOccupiedPrivateRoomCount() => GetGlobalIntFlag(22, 0);
    public int GetEmptyPetKennelCount() => GetGlobalIntFlag(23, 0);
    public int GetMagicCuffCount() => GetGlobalIntFlag(24, 0);
    public int GetSlaveDormOccupancy() => GetGlobalIntFlag(25, 0);
    public int GetTentacleModificationCount() => GetGlobalIntFlag(40, 0);
    public int GetVisitCount() => GetGlobalIntFlag(41, 0);
    
    public bool IsMainMenuUnlocked() => IsUnlocked(90);
    public bool IsScheduleUnlocked() => IsUnlocked(91);
    public bool IsOkachiStreetUnlocked() => IsUnlocked(92);
    public bool IsApolloStreetUnlocked() => IsUnlocked(93);
    
    public bool IsTrainingActive() => GetTempFlag(0);
    public bool IsOffScheduleH() => GetTempFlag(1);
    public bool IsInternalHumiliationActive() => GetTempFlag(2);
    public bool IsGoblinGangActive() => GetTempFlag(3);
    public bool IsTimeCompressionActive() => GetTempFlag(8);
    public bool IsInBathtub() => GetTempFlag(9);
    
    public bool HasMilkConstitutionBeforeTraining() => GetTempFlag(4);
    public int GetPreTrainingStamina() => GetTempIntFlag(6, 0);
    public string GetBathroomLocation()
    {
        var bathId = GetTempIntFlag(7, 0);
        return bathId switch
        {
            0 => "none",
            1 => "main",
            2 => "private",
            3 => "hot_spring",
            _ => "unknown"
        };
    }
    
    // === FLAG DEFINITIONS (from CSV) ===
    
    private void SeedFlagDefinitions()
    {
        // Global flag definitions (Flag.csv)
        _globalFlagDefs = new List<FlagDefinition>
        {
            new FlagDefinition(0, "\u4e73\u725b\u5065\u5eb7\u5ea6", "Livestock health status", FlagCategory.GameState),
            new FlagDefinition(1, "\u4e8b\u52d9\u4f5c\u696d\u91cf", "Office work amount", FlagCategory.GameState),
            new FlagDefinition(2, "\u4e8b\u52d9\u4f5c\u696d\u91cfMAX", "Max office work amount", FlagCategory.GameState),
            new FlagDefinition(3, "\u5927\u6b32\u5834\u626b\u9664\u5b8c\u4e86\u5ea6", "Big bath cleaning progress", FlagCategory.GameState),
            new FlagDefinition(4, "\u3042\u306a\u305f\u7528\u3075\u308d\u626b\u9664\u5b8c\u4e86", "Your bath cleaning complete", FlagCategory.GameState),
            new FlagDefinition(5, "\u5974\u6575\u4eba\u6570", "Number of slaves", FlagCategory.GameState),
            new FlagDefinition(6, "\u8ffd\u8e2a\u4e2d\u4eba\u6570", "Number being pursued", FlagCategory.GameState),
            new FlagDefinition(20, "\u5ba4\u5ba4\u6570\uff0f\u5408\u8a08", "Total private rooms", FlagCategory.GameState),
            new FlagDefinition(21, "\u5ba4\u5ba4\u6570\uff0f\u7a7a\u5ba4", "Empty private rooms", FlagCategory.GameState),
            new FlagDefinition(22, "\u5ba4\u5ba4\u6570\uff0f\u4f7f\u7528\u4e2d", "Occupied private rooms", FlagCategory.GameState),
            new FlagDefinition(23, "\u7a7a\u304d\u30da\u30c3\u30c8\u5c0f\u5c4b\u6570", "Empty pet kennels", FlagCategory.GameState),
            new FlagDefinition(24, "\u9b54\u529b\u9396\u4f7f\u7528\u53ef\u80fd\u6570", "Magic cuffs usable count", FlagCategory.GameState),
            new FlagDefinition(25, "\u5974\u6575\u5bdd\uff0f\u4f7f\u7528\u4eba\u6570", "Slave dormitory occupancy", FlagCategory.GameState),
            new FlagDefinition(40, "\u8951\u624b\u8eab\u4f53\u6539\u9020\u53ef\u80fd\u56de\u6570", "Tentacle body modification count", FlagCategory.GameState),
            new FlagDefinition(41, "\u73fe\u6642\u523b\u8a2a\u554f\u56de\u6570", "Current time visit count", FlagCategory.GameState),
            new FlagDefinition(42, "\u6642\u523b\u5236\u9650\uff0f\u90e8\u5c4b\u63fa\u5909\u66f4", "Time limit room change", FlagCategory.GameState),
            new FlagDefinition(90, "\u30a2\u30f3\u30ed\u30c3\u30afBIT\uff0f\u30e1\u30a4\u30f3\u30e1\u30cb\u30e5\u30fc", "Main menu unlock bit", FlagCategory.Unlock),
            new FlagDefinition(91, "\u30a2\u30f3\u30ed\u30c3\u30afBIT\uff0f\u30b9\u30b1\u30b8\u30e5\u30fc\u30eb", "Schedule unlock bit", FlagCategory.Unlock),
            new FlagDefinition(92, "\u30a2\u30f3\u30ed\u30c3\u30afBIT\uff0f\u30aa\u30ab\u30c1\u8857", "Okachi Street unlock bit", FlagCategory.Unlock),
            new FlagDefinition(93, "\u30a2\u30f3\u30ed\u30c3\u30afBIT\uff0f\u30a2\u30c3\u30dd\u30ed\u8857", "Apollo Street unlock bit", FlagCategory.Unlock),
        };
        
        // Temporary flag definitions (Tflag.csv)
        _tempFlagDefs = new List<FlagDefinition>
        {
            new FlagDefinition(0, "\u8abf\u6559\u4e2d", "Currently training", FlagCategory.Temporary),
            new FlagDefinition(1, "\u30b9\u30b1\u30b8\u30e5\u30fc\u30eb\u5916\uff28", "Off-schedule H", FlagCategory.Temporary),
            new FlagDefinition(2, "\u4f53\u5185\u51cc\u8fb1\u4e2d", "Internal humiliation active", FlagCategory.Temporary),
            new FlagDefinition(3, "\u30b4\u30d6\u30ea\u30f3\u8f2a\u59ec", "Goblin gang active", FlagCategory.Temporary),
            new FlagDefinition(4, "\u8abf\u6559\u524d\u6bcd\u4e73\u4f53\u8cea", "Pre-training milk constitution", FlagCategory.Temporary),
            new FlagDefinition(5, "\u30eb\u30fc\u30d7\u4e2d\u30e1\u30c3\u30bb\u30fc\u30b8\u8868\u793a", "Loop message display", FlagCategory.Temporary),
            new FlagDefinition(6, "\u8abf\u6559\u524d\u4f53\u529b", "Pre-training stamina", FlagCategory.Temporary),
            new FlagDefinition(7, "\u6b32\u5834", "Bathroom", FlagCategory.Temporary),
            new FlagDefinition(8, "\u6642\u9593\u5726\u7f29\u4e2d", "Time compression active", FlagCategory.Temporary),
            new FlagDefinition(9, "\u6d74\u69fd\u5185", "In bathtub", FlagCategory.Temporary),
        };
        
        // Character flag definitions (Cflag.csv)
        _charFlagDefs = new List<FlagDefinition>
        {
            new FlagDefinition(0, "\u8eab\u9577", "Height", FlagCategory.Character),
            new FlagDefinition(1, "\u53e3\u4e0a\u8868\u793a", "Comment display", FlagCategory.Character),
            new FlagDefinition(2, "\u53e3\u4e0a\u30d1\u30bf\u30fc\u30f3", "Comment pattern", FlagCategory.Character),
            new FlagDefinition(3, "\u5c02\u7528\u8a2d\u5b9a\uff0f\u53e3\u4e0a", "Special comment setting", FlagCategory.Character),
            new FlagDefinition(4, "\u5c02\u7528\u8a2d\u5b9a\uff0f\u30a4\u30d9\u30f3\u30c8", "Special event setting", FlagCategory.Character),
            new FlagDefinition(5, "\u5c02\u7528\u8a2d\u5b9a\uff0f\u9854\u30b0\u30e9\u8868\u793a\u95a2\u6570", "Face graphic display function", FlagCategory.Character),
            new FlagDefinition(6, "\u8abf\u6559\u753b\u50cf\u3042\u308a", "Has training image", FlagCategory.Character),
            new FlagDefinition(7, "\u4e00\u4eba\u79f0\u304c\u540d\u524d", "First person is name", FlagCategory.Character),
            new FlagDefinition(8, "\u6027\u77e5\u8b58", "Sexual knowledge", FlagCategory.Character),
            new FlagDefinition(10, "\u30ab\u30e9\u30fc\u30b3\u30fc\u30c9\uff0f\u9aed", "Hair color code", FlagCategory.Character),
        };
    }
    
    public FlagDefinition? GetGlobalFlagDefinition(int id)
    {
        return _globalFlagDefs.FirstOrDefault(f => f.Id == id);
    }
    
    public FlagDefinition? GetTempFlagDefinition(int id)
    {
        return _tempFlagDefs.FirstOrDefault(f => f.Id == id);
    }
    
    public FlagDefinition? GetCharFlagDefinition(int id)
    {
        return _charFlagDefs.FirstOrDefault(f => f.Id == id);
    }
    
    public IReadOnlyList<FlagDefinition> GetAllGlobalFlags() => _globalFlagDefs.AsReadOnly();
    public IReadOnlyList<FlagDefinition> GetAllTempFlags() => _tempFlagDefs.AsReadOnly();
    public IReadOnlyList<FlagDefinition> GetAllCharFlags() => _charFlagDefs.AsReadOnly();
}

/// <summary>
/// Definition of a flag from CSV data.
/// </summary>
public readonly struct FlagDefinition
{
    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    public FlagCategory Category { get; }
    
    public FlagDefinition(int id, string name, string description, FlagCategory category)
    {
        Id = id;
        Name = name;
        Description = description;
        Category = category;
    }
}

public enum FlagCategory
{
    GameState,
    Unlock,
    Temporary,
    Character,
}
