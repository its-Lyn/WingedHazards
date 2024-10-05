using MonoGayme.Components;

namespace GBGame.Components;

public class Health(int hp) : Component
{
    public int OriginalHealthPoints { get; } = hp;
    public int HealthPoints { get; set; } = hp;
}
