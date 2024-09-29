using System;
using GBGame.Entities;
using GBGame.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Components;

namespace GBGame.Items;

public class Bomb(Game windowData, Player player) : Item(windowData)
{
    public int BombCount = 5;
    public bool CanPlace = true;
    public AnimatedSpriteSheet Sheet = null!;
    
    private Vector2 _pos;

    public bool Exploded = false;
    public Rectangle KillRadius;

    public override void LoadContent()
    {
        InventorySprite = WindowData.Content.Load<Texture2D>("Sprites/UI/Bomb");

        Name = "Bomb";
        Description = $"{BombCount} bombs left.";

        Sheet = new AnimatedSpriteSheet(WindowData.Content.Load<Texture2D>("Sprites/SpriteSheets/BombPlaced"), new Vector2(5, 1), 0.5f);
    }

    public override void Use()
    {
        if (BombCount - 1 < 0 || !CanPlace) 
        {
            Console.WriteLine("Cannot use Bomb!");
            return;
        }

        BombCount--;
        Description = BombCount == 1 ? "1 bomb left." : $"{(BombCount == 0 ? "No" : BombCount)} bombs left."; 

        CanPlace = false;
        Sheet.Done = false;
        _pos = Vector2.Floor(player.Position / 8) * 8;
        KillRadius = new Rectangle((int)_pos.X - 16, (int)_pos.Y - 16, 40, 40);
        
        Console.WriteLine("Used Bomb.");
    }

    public void Draw(SpriteBatch batch)
        => Sheet.Draw(batch, _pos, false);
}
