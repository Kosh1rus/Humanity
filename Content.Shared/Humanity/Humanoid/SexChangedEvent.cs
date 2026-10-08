namespace Content.Shared.Humanoid;

public sealed class SexChangedEvent(Sex oldSex, Sex newSex) : EntityEventArgs
{
    public readonly Sex OldSex = oldSex;
    public readonly Sex NewSex = newSex;
}
