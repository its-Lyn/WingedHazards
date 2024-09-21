using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GBGame.Items;

public class Sword(Game windowData) : Item(windowData)
{
    public override void LoadContent()
    {
        InventorySprite = WindowData.Content.Load<Texture2D>("Sprites/UI/Sword");

        Name = "Sword";
        Description = "Ol' reliable.";
    }

    public override void Use() 
    {
        Console.WriteLine("Using sword.");
    }
}
