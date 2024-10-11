using GBGame.Components;
using GBGame.Entities;

namespace GBGame.Skills;

public class MoreHP : Skill
{
    private readonly Player _player;

    public MoreHP(Player player)
    { 
        _player = player;

        Name = "one more life";
    }
    
    public override void OnActivate()
        => _player.AddHealth();
}
