namespace GBGame.Skills;

public abstract class Skill
{
    public string Name { get; protected init; }

    public abstract void OnActivate();
}
