using Robust.Shared.Timing;

namespace Content.Shared.Inventory;

public partial class InventorySystem
{
    [Dependency] private IGameTiming _timing = default!;
}
