using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Entities;
using MonoGayme.Entities.Colliders;
using MonoGayme.Utilities;

namespace GBGame.Entities.Enemies;

public class Bat(Game windowData, Vector2 pos, int zIndex = 0) : Entity(windowData, zIndex), IRectCollider
{
    private Texture2D _sprite = null!;
    
    private bool _locked = false;
    private Entity? _lockedEntity;

    private float _speed = 0.6f;

    public Rectangle Collider { get; set; }

    private Shapes _shapes = null!;

    public void Lock(Entity entity) {
        _lockedEntity = entity;
        _locked = true;
    }

    public override void LoadContent()
    {
        _sprite = WindowData.Content.Load<Texture2D>("Sprites/Ground/Ground_4");
        Position = pos;

        _shapes = new Shapes(WindowData.GraphicsDevice);
    }

    public override void Update(GameTime time)
    {
        if (_locked) 
        {
            if (_lockedEntity is null) 
            {
                Console.Error.WriteLine("How does this even happen?");
                return;
            }
            
            Vector2 dir = _lockedEntity.Position - Position;
            dir.Normalize();

            Vector2 target = dir * _speed;
            Velocity = MathUtility.MoveTowards(Velocity, target, 0.05f);
        }

        Position += Velocity;
        Collider = new Rectangle((int)Position.X, (int)Position.Y, 8, 8);
    }

    public override void Draw(SpriteBatch batch, GameTime time)
    {
        batch.Draw(_sprite, Position, Color.White);
        _shapes.DrawRectangleLines(Collider, Color.Red, batch);
    }
}
