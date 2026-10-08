using Content.Client.Humanity.Mortar;
using Content.Shared._RMC14.Mortar;
using Content.Shared.Humanity.Mortar;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._RMC14.Mortar;

[UsedImplicitly]
public sealed class MortarBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private MortarWindow? _window;

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<MortarWindow>();
        _window.AimChanged += (heading, range) => SendPredictedMessage(new MortarAimMessage(heading, range));
        _window.FireButton.OnPressed += _ => SendPredictedMessage(new MortarFireMessage());
        EntMan.System<MortarAimPreviewSystem>().Aiming.Add(Owner);
        Refresh();
    }

    public void Refresh()
    {
        if (_window is { IsOpen: true } && EntMan.TryGetComponent<MortarComponent>(Owner, out var mortar))
            _window.Refresh(mortar);
    }

    protected override void Dispose(bool disposing)
    {
        EntMan.System<MortarAimPreviewSystem>().Aiming.Remove(Owner);
        base.Dispose(disposing);
    }
}
