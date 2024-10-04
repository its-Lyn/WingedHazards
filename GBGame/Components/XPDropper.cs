using MonoGayme.Components;

namespace GBGame.Components;

public class XPDropper(int xp) : Component
{
    public int XP { get; set; } = xp;
}
