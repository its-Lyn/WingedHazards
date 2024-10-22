using System;
using GBGame.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Components;

namespace GBGame.Items;
public class Sword(GameWindow windowData, AnimatedSpriteSheet sheet, AnimatedSpriteSheet slash, Player player) : Item
{
    public override void LoadContent()
    {
        InventorySprite = windowData.ContentData.Get("Sword");

        Name = "Sword";
        Description = "Ol' reliable.";
    }

    public override void Use()
    {
        if (!sheet.Finished) return;
        
        sheet.Finished = false;
        if (!slash.Finished)
            slash.Reset();
        slash.Finished = false;

        if (!player.IsOnFloor) return;
            
        if (player.FacingRight)
        {
            player.Position.X += 1f;
        }
        else
        {
            player.Position.X -= 1f;
        }
    }
}
