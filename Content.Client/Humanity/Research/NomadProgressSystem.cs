using Content.Shared.Civ14.CivResearch;
using Content.Shared.Humanity.Research;
using Robust.Client.Player;

namespace Content.Client.Humanity.Research;

public sealed class NomadProgressSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _players = default!;
    private float _elapsed;
    public event Action<string, int>? ProgressChanged;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _elapsed += frameTime;
        if (_elapsed < 1f)
            return;
        _elapsed = 0;
        if (_players.LocalEntity is not { } player ||
            !TryComp<CivResearchComponent>(Transform(player).MapUid, out var research) || research.IsTDM)
        {
            ProgressChanged?.Invoke("", -1);
            return;
        }
        ProgressChanged?.Invoke(NomadResearch.Status(research), research.GetCurrentAge());
    }
}
