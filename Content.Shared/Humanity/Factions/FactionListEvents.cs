using Robust.Shared.Serialization;

namespace Content.Shared.Humanity.Factions;

[Serializable, NetSerializable]
public sealed class FactionListRequestEvent : EntityEventArgs
{
}

[Serializable, NetSerializable]
public sealed class FactionListResponseEvent : EntityEventArgs
{
    public Dictionary<string, List<string>> Factions { get; }

    public FactionListResponseEvent(Dictionary<string, List<string>> factions)
    {
        Factions = factions;
    }
}
