using Content.Shared.Interaction;
using Content.Shared.DoAfter;
using Content.Server.DoAfter;
using Content.Shared.Maps;
using Content.Shared.Popups;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Content.Shared.Gathering;

namespace Content.Server.Gathering;

public sealed partial class StrawCollectorSystem : EntitySystem
{
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private ITileDefinitionManager _tileManager = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IRobustRandom _random = default!;

    // Grass tiles that can be cut
    private readonly HashSet<string> _grassTiles = new()
    {
        "FloorGrass",
        "FloorGrassJungle",
        "FloorGrassDark",
        "FloorGrassLight",
        "FloorPlanetGrass",
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StrawCollectorComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<StrawCollectorComponent, StrawCollectDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(Entity<StrawCollectorComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || args.Target != null || !args.CanReach)
            return;

        var comp = ent.Comp;
        var user = args.User;
        var clickLocation = args.ClickLocation;

        var gridUid = _transform.GetGrid(clickLocation);
        if (!gridUid.HasValue || !TryComp<MapGridComponent>(gridUid.Value, out var grid))
            return;

        var snapPos = _map.TileIndicesFor(gridUid.Value, grid, clickLocation);
        var tileRef = _map.GetTileRef(gridUid.Value, grid, snapPos);
        var tileDef = (ContentTileDefinition)_tileManager[tileRef.Tile.TypeId];

        if (!_grassTiles.Contains(tileDef.ID))
        {
            return;
        }

        var delay = TimeSpan.FromSeconds(comp.CollectTime);
        var netGridUid = GetNetEntity(gridUid.Value);
        var doAfterArgs = new DoAfterArgs(EntityManager, user, delay, new StrawCollectDoAfterEvent(netGridUid, snapPos), ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true
        };

        if (_doAfter.TryStartDoAfter(doAfterArgs))
        {
            _popup.PopupEntity("Вы начинаете срезать траву.", ent, user);
            args.Handled = true;
        }
    }

    private void OnDoAfter(Entity<StrawCollectorComponent> ent, ref StrawCollectDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var gridUid = GetEntity(args.GridUid);
        if (!TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var snapPos = args.SnapPos;
        var tileRef = _map.GetTileRef(gridUid, grid, snapPos);
        var tileDef = (ContentTileDefinition)_tileManager[tileRef.Tile.TypeId];

        if (!_grassTiles.Contains(tileDef.ID))
        {
            return; // Tile has changed, abort
        }

        var dirtTile = _tileManager["FloorDirt"];
        _map.SetTile(gridUid, grid, snapPos, new Tile(dirtTile.TileId));

        var comp = ent.Comp;
        var strawCount = _random.Next(comp.MinAmount, comp.MaxAmount + 1);
        var coordinates = _map.GridTileToLocal(gridUid, grid, snapPos);
        for (int i = 0; i < strawCount; i++)
        {
            Spawn("MaterialStraw1", coordinates);
        }

        _popup.PopupEntity("Вы срезали траву.", ent, args.User);
        args.Handled = true;
    }
}
