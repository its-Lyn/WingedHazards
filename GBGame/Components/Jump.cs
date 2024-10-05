using MonoGayme.Components;

namespace GBGame.Components;

public class Jump(int count) : Component
{
    public int BaseCount { get; set; } = count;

    public int Count { get; set; } = count;
}
