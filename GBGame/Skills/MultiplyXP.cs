namespace GBGame.Skills;

public class MultiplyXP : Skill
{
    private readonly GameWindow _window;

    public MultiplyXP(GameWindow window)
    {
        Name = "More XP";
        _window = window;
    }

    public override void OnActivate()
    {
        _window.XPMultiplier *= 2;
    }
}
