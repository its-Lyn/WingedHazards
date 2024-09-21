using System;
using GBGame.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Components;

namespace GBGame.Items;
public class Sword(Game windowData, AnimatedSpriteSheet sheet, Player player) : Item(windowData)
{
    public override void LoadContent()
    {
        InventorySprite = WindowData.Content.Load<Texture2D>("Sprites/UI/Sword");

        Name = "Sword";
        Description = "Ol' reliable.";
    }

    public override void Use() 
    {
        if (sheet.Done)
        {
            Console.WriteLine("Using sword.");
            sheet.Done = false;

            if (player.IsOnFloor)
            {
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
    }
}
