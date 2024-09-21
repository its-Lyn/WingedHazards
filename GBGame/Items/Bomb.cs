using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GBGame.Items;

public class Bomb(Game windowData) : Item(windowData)
{
    public int BombCount = 5;

    public override void LoadContent()
    {
        InventorySprite = WindowData.Content.Load<Texture2D>("Sprites/UI/Bomb");

        Name = "Bomb";
        Description = $"{BombCount} bombs left.";
    }

    public override void Use()
    {
        if (BombCount - 1 < 0) 
        {
            Console.WriteLine("Cannot use Bomb!");
            return;
        }

        BombCount--;
        Description = $"{BombCount} bombs left.";
        Console.WriteLine("Used Bomb.");
    }
}
