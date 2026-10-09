// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Gameplay;
using Content.Client._Shitmed.UserInterface.Systems.PartStatus.Widgets;
using Content.Shared._Shitmed.Targeting;
using Content.Client._Shitmed.Targeting;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Utility;
using Robust.Client.Graphics;
using Content.Shared._Shitmed.PartStatus.Events;


namespace Content.Client._Shitmed.UserInterface.Systems.PartStatus;

public sealed partial class PartStatusUIController : UIController, IOnStateEntered<GameplayState>, IOnSystemChanged<TargetingSystem>
{
    [Dependency] private IEntityManager _entManager = default!;
    [Dependency] private IEntityNetworkManager _net = default!;
    private TargetingComponent? _targetingComponent;
    private PartStatusControl? PartStatusControl => UIManager.GetActiveUIWidgetOrNull<PartStatusControl>();

    public void OnSystemLoaded(TargetingSystem system)
    {
        system.PartStatusStartup += UpdatePartStatusControl;
        system.PartStatusShutdown += RemovePartStatusControl;
        system.PartStatusUpdate += UpdatePartStatusControl;
    }

    public void OnSystemUnloaded(TargetingSystem system)
    {
        system.PartStatusStartup -= UpdatePartStatusControl;
        system.PartStatusShutdown -= RemovePartStatusControl;
        system.PartStatusUpdate -= UpdatePartStatusControl;
    }

    public void OnStateEntered(GameplayState state) => Refresh();

    public void RemovePartStatusControl()
    {
        _targetingComponent = null;
        Refresh();
    }

    public void UpdatePartStatusControl(TargetingComponent component)
    {
        _targetingComponent = component;
        Refresh();
    }

    private void Refresh()
    {
        if (PartStatusControl is not { } control)
            return;

        control.Visible = _targetingComponent != null;
        if (_targetingComponent != null)
            control.SetTextures(_targetingComponent.BodyStatus);
    }

    public Texture GetTexture(SpriteSpecifier specifier) => _entManager.System<SpriteSystem>().Frame0(specifier);

    public void GetPartStatusMessage()
    {
        if (_targetingComponent == null)
            return;

        _net.SendSystemNetworkMessage(new GetPartStatusEvent());
    }
}
