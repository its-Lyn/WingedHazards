namespace GBGame.Skills;

public abstract class Skill
{
    public string Name { get; set; }

    public abstract void OnActivate();
}
