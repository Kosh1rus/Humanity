using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.PartStatus.Events;

[Serializable, NetSerializable]
public sealed class GetPartStatusEvent : EntityEventArgs;
