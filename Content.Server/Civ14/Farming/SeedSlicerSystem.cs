using Content.Shared.Botany.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Botany;

namespace Content.Server.Botany.Systems;

public sealed partial class SeedSlicerSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private BotanySystem _botanySystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SeedSlicerComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(EntityUid uid, SeedSlicerComponent slicer, AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach)
            return;
        var target = args.Target;
        if (target == null)
            return;

        var user = args.User;


        if (!TryComp<ProduceComponent>(target.Value, out var produce))
            return;


        if (produce.PlantProtoId is not { } plantId ||
            !_botanySystem.TryGetPlantComponent<PlantDataComponent>(produce.PlantData, plantId, out var seed))
        {
            return;
        }

        // Obtém o nome da entidade do MetaDataComponent
        string entityName = "неизвестный предмет";
        if (TryComp(target.Value, out MetaDataComponent? metaData))
        {
            entityName = metaData.EntityName;
        }

        _popup.PopupCursor($"Вы извлекли семя. Источник: {entityName}.", user, PopupType.Medium);

        var coords = Transform(uid).Coordinates;
        _botanySystem.SpawnSeedPacket(seed, plantId, produce.PlantData, coords, user);
        QueueDel(target.Value);

        args.Handled = true;
    }
}
