using GBGame.Items;

namespace GBGame.Skills;

public class PlusBomb : Skill
{
    private readonly Bomb _bomb;

    public PlusBomb(Bomb bomb)
    {
        Name = "More Bombs";
        _bomb = bomb;
    }

    public override void OnActivate()
    {
        _bomb.BombCount += 5;
    }
}
