using GBGame.Items;
using GBGame.States;

namespace GBGame.Skills;

public class BombRadius : Skill
{
    private InGame _game;

    public BombRadius(InGame game)
    {
        _game = game;

        Name = "bigger bombs";
    }

    public override void OnActivate()
    {
        _game.Bomb.RadiusData.X += 8;
        _game.Bomb.RadiusData.Y += 8;

        _game.Bomb.RadiusData.Width += 16;
        _game.Bomb.RadiusData.Height += 16;

        _game.Bomb.BombCount += 5;
    }
}
