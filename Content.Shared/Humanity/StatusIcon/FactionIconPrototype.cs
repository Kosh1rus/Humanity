using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
namespace Content.Shared.StatusIcon;
[Prototype]
public sealed partial class FactionIconPrototype : StatusIconData, IPrototype, IInheritingPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<FactionIconPrototype>))]
    public string[]? Parents { get; private set; }
    [NeverPushInheritance, AbstractDataField]
    public bool Abstract { get; private set; }
}
