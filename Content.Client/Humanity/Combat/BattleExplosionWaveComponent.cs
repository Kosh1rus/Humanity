namespace Content.Client.Humanity.Combat;

[RegisterComponent]
public sealed partial class BattleExplosionWaveComponent : Component
{
    public float Age;
    public float Lifetime = 1.2f;
    public float MaxRadius;
}
