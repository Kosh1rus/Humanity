using Content.Client.Humanity.Mortar;
using Content.Shared._RMC14.Mortar;
using Content.Shared.Humanity.Mortar;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Timing;

namespace Content.Client._RMC14.Mortar;

[UsedImplicitly]
public sealed partial class MortarBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private IGameTiming _timing = default!;
    private static readonly TimeSpan AimTimeout = TimeSpan.FromSeconds(1);
    private MortarWindow? _window;
    private (float Heading, int Range, TimeSpan ExpiresAt)? _pendingAim;

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<MortarWindow>();
        _window.AimChanged += (heading, range) =>
        {
            _pendingAim = (heading, range, _timing.RealTime + AimTimeout);
            EntMan.System<MortarAimPreviewSystem>().SetPreview(Owner, heading);
            SendPredictedMessage(new MortarAimMessage(heading, range));
        };
        _window.FireButton.OnPressed += _ => SendPredictedMessage(new MortarFireMessage());
        EntMan.System<MortarAimPreviewSystem>().SetPreview(Owner);
        Refresh();
    }

    public override void Update()
    {
        base.Update();
        if (_pendingAim is { } pending && _timing.RealTime >= pending.ExpiresAt)
            Refresh();
    }

    public void Refresh()
    {
        if (_window is { IsOpen: true } && EntMan.TryGetComponent<MortarComponent>(Owner, out var mortar))
        {
            if (_pendingAim is { } pending &&
                ((mortar.Heading == pending.Heading && mortar.Range == pending.Range) ||
                 _timing.RealTime >= pending.ExpiresAt))
            {
                _pendingAim = null;
                EntMan.System<MortarAimPreviewSystem>().SetPreview(Owner);
            }
            _window.Refresh(mortar, _pendingAim == null);
        }
    }

    protected override void Dispose(bool disposing)
    {
        _pendingAim = null;
        EntMan.System<MortarAimPreviewSystem>().RemovePreview(Owner);
        base.Dispose(disposing);
    }
}
