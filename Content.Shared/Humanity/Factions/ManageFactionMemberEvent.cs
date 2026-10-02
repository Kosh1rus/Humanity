using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.Humanity.Factions;

[Serializable, NetSerializable]
public sealed class ManageFactionMemberEvent : EntityEventArgs
{
    public NetUserId Target { get; }
    public bool TransferLeadership { get; }

    public ManageFactionMemberEvent(NetUserId target, bool transferLeadership)
    {
        Target = target;
        TransferLeadership = transferLeadership;
    }
}
