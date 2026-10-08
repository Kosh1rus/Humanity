using Content.Shared.CCVar;
using Content.Shared.Weather;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Utility;

namespace Content.Server.Humanity.Persistence;

public sealed partial class NomadWorldSaveSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _config = default!;
    [Dependency] private IResourceManager _resources = default!;
    [Dependency] private MapLoaderSystem _loader = default!;
    private float _elapsed;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (!_config.GetCVar(CCVars.UsePersistence))
            return;
        _elapsed += frameTime;
        if (_elapsed < 900f)
            return;
        _elapsed = 0;
        var query = EntityQueryEnumerator<WeatherNomadsComponent>();
        while (query.MoveNext(out var map, out _))
        {
            if (!Paused(map))
            {
                Save(map);
                break;
            }
        }
    }

    public bool Save(EntityUid map)
    {
        var pathString = _config.GetCVar(CCVars.GameMap);
        if (!_config.GetCVar(CCVars.UsePersistence) || string.IsNullOrWhiteSpace(pathString) || !pathString.EndsWith(".yml"))
            return false;
        var path = new ResPath(pathString);
        var temporary = new ResPath(pathString + ".tmp");
        var backup = new ResPath(pathString + ".bak");
        try
        {
            if (!_loader.TrySaveMap(map, temporary))
                return false;
            if (_resources.UserData.Exists(path))
            {
                _resources.UserData.Delete(backup);
                _resources.UserData.Rename(path, backup);
            }
            _resources.UserData.Rename(temporary, path);
            Log.Info($"Saved Humanity world to {path}");
            return true;
        }
        catch (Exception exception)
        {
            Log.Error($"Failed to save Humanity world: {exception}");
            if (!_resources.UserData.Exists(path) && _resources.UserData.Exists(backup))
                _resources.UserData.Rename(backup, path);
            return false;
        }
    }
}
