namespace Content.Shared.Humanity.Visuals;

public sealed class NomadGatherEffectEvent(NomadWorkEffect effect) : EntityEventArgs
{
    public readonly NomadWorkEffect Effect = effect;
}
