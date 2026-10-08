using Robust.Shared.Map;
using Robust.Shared.Network;

namespace Content.Shared.Civ14.CivResearch;

public sealed partial class CivResearchSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MapCreatedEvent>(OnMapCreated);
    }

    private void OnMapCreated(MapCreatedEvent ev)
    {
        if (_net.IsServer)
            EnsureComp<CivResearchComponent>(ev.Uid);
    }
}
