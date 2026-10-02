using System.Numerics;

namespace Content.Client.Humanity.Combat;

[RegisterComponent]
public sealed partial class BattleExplosionParticleComponent : Component
{
    public Vector2 Velocity;
    public float Lifetime;
    public float Age;
    public float Growth;
    public Color Tint = Color.White;
}
