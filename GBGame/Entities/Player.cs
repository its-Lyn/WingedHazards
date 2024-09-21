using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGayme.Entities;
using MonoGayme.Utilities;

namespace GBGame.Entities;

public class Player(Game windowData, int zIndex = 1) : Entity(windowData, zIndex)
{
    private Texture2D _sprite = null!;
    private Vector2 _origin = Vector2.Zero;

    private readonly float TerminalVelocity = 1.5f;
    private readonly float Acceleration = 0.5f;

    public bool IsOnFloor = false;

    public readonly float JumpVelocity = 4f;

    public override void LoadContent()
    {
        Position.X = 40;
        
        _sprite = WindowData.Content.Load<Texture2D>("Sprites/Ground/Ground_4");
        _origin = new Vector2(_sprite.Width / 2, _sprite.Height / 2);
    }

    public override void Update(GameTime time)
    {
        if (InputManager.IsKeyDown(Keys.A))
        {
            Velocity.X = MathUtility.MoveTowards(Velocity.X, -TerminalVelocity, Acceleration);
        } 
        else if (InputManager.IsKeyDown(Keys.D))
        {
            Velocity.X = MathUtility.MoveTowards(Velocity.X, TerminalVelocity, Acceleration);
        }
        else 
        {
            Velocity.X = MathUtility.MoveTowards(Velocity.X, 0, Acceleration);
        }

        if (IsOnFloor && InputManager.IsKeyPressed(Keys.Space))
        {
            Velocity.Y = -JumpVelocity;
            IsOnFloor = false;
        }

        if (!IsOnFloor)
        {
            Velocity.Y = MathUtility.MoveTowards(Velocity.Y, TerminalVelocity, 0.5f);
        }

        Position += Velocity;
    }

    public override void Draw(SpriteBatch batch, GameTime time)
    {
        batch.Draw(_sprite, Position, null, Color.White, 0, _origin, 1, SpriteEffects.None, 0);
    }
}
