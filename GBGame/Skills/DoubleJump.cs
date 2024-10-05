using GBGame.Components;
using GBGame.Entities;

namespace GBGame.Skills;

public class DoubleJump : Skill
{
    private readonly Player _player;

    public DoubleJump(Player player)
    {
        Name = "Double Jump";
        _player = player;
    }

    public override void OnActivate()
    {
        Jump? jump = _player.Components.GetComponent<Jump>();
        if (jump is null) return;

        jump.BaseCount = 2;
        jump.Count = 2;
    }
}
