#nullable enable
using Content.Shared.Camera;
using Content.Shared.Input;
using Robust.Shared.Input.Binding;
using Content.Client.Movement.Components;
using Content.Client.Movement.Systems;
using Robust.Shared.Player;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Timing;
using Robust.Client.Player;
using Robust.Shared.GameObjects;
using System.Numerics;
using Robust.Client.Timing;


namespace Content.Client.Civ14.LookZoom;
public sealed partial class LookZoomSystem : EntitySystem
{
    [Dependency] private EyeCursorOffsetSystem _eyeOffset = default!;
    [Dependency] private SharedHandsSystem _handsSystem = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IClientGameTiming _gameTiming = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LookZoomComponent, GetEyeOffsetRelayedEvent>(UpdateLookZoom);

        CommandBinds.Builder
        .Bind(ContentKeyFunctions.LookZoom, InputCmdHandler.FromDelegate(OnLookZoomHandler, handle: false, outsidePrediction: false))
        .Register<LookZoomSystem>();
    }

    public void OnLookZoomHandler(ICommonSession? session)
    {
        // Gets the player character uid
        var uid = session?.AttachedEntity;

        if (!TryComp<LookZoomComponent>(uid, out var comp))
            return;

        // Checks if the cooldown is over by comparing to the current time
        if (_timing.CurTime < comp.DelayedTime)
            return;

        // Sets the cooldown to the current time + 0.1 seconds
        comp.DelayedTime = _timing.CurTime + TimeSpan.FromSeconds(0.1);

        if (comp.State == false)
        {
            _handsSystem.TryGetActiveItem(uid.Value, out var item);
            ResetOffset(uid);
            ResetOffset(item);
            comp.State = true;
            return;
        }
        comp.State = false;
    }
    public void UpdateLookZoom(EntityUid uid, LookZoomComponent comp, ref GetEyeOffsetRelayedEvent args)
    {
        if (comp.State == false)
        {
            return;
        }

        _handsSystem.TryGetActiveItem(comp.Owner, out var item);

        // Sets the offset if there is an item in the active hand with the EyeCurserOffset component
        if (item != null && TryComp<EyeCursorOffsetComponent>(item, out var itemComp))
        {
            SetOffset(item.Value, ref args);
            return;
        }

        //Sets the offset using the EyeCurserOffset on the player entity instead
        SetOffset(uid, ref args);
    }

    private void SetOffset(EntityUid uid, ref GetEyeOffsetRelayedEvent args)
    {
        var offset = _eyeOffset.OffsetAfterMouse(uid, null);
        if (offset == null)
            return;

        args.Offset += offset.Value;
    }

    private void ResetOffset(EntityUid? uid)
    {
        if (TryComp(uid, out EyeCursorOffsetComponent? cursorOffsetComp))
        {
            if (_gameTiming.IsFirstTimePredicted)
                cursorOffsetComp.CurrentPosition = Vector2.Zero;
        }
    }
}
