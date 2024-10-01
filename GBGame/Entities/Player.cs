using GBGame.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Components;
using MonoGayme.Components.Colliders;
using MonoGayme.Entities;
using MonoGayme.Utilities;
using System;

namespace GBGame.Entities;

public class Player(Game windowData, int zIndex = 1) : Entity(windowData, zIndex)
{
    private Texture2D _sprite = null!;
    private Vector2 _origin = Vector2.Zero;

    private readonly float TerminalVelocity = 2f;
    private readonly float Acceleration = 0.5f;

    public bool IsOnFloor = false;
    public bool FacingRight = true;

    public readonly float JumpVelocity = 6f;

    public RectCollider Collider = null!;
    private Timer _immunityTimer = null!;

    private Health _health = null!;

    public void ApplyKnockBack(RectCollider other)
    {
        Vector2 dir = Vector2.Normalize(Collider.GetCentre() - other.GetCentre());
        Velocity += 5 * dir;

        _health.HealthPoints--;
        if (_health.HealthPoints <= 0)
        {
            // :trollface:
            WindowData.Exit();
        }

        Collider.Enabled = false;
        _immunityTimer.Start();
    }

    public override void LoadContent()
    {
        Position.X = 40;
        
        _sprite = WindowData.Content.Load<Texture2D>("Sprites/Ground/Ground_4");
        _origin = new Vector2(_sprite.Width / 2, _sprite.Height / 2);

        Components.AddComponent(new RectCollider());
        Collider = Components.GetComponent<RectCollider>()!;

        Components.AddComponent(new Timer(1, false, true, "ImmunityTimer"));
        _immunityTimer = Components.GetComponent<Timer>()!;
        _immunityTimer.OnTimeOut = () => {
            Collider.Enabled = true;
        };

        Components.AddComponent(new Health(2));
        _health = Components.GetComponent<Health>()!;
    }

    public override void Update(GameTime time)
    {
        _immunityTimer.Cycle(time);

        if (InputManager.IsKeyDown(GBGame.KeyboardLeft) || InputManager.IsGamePadDown(GBGame.ControllerLeft))
        {
            Velocity.X = MathUtility.MoveTowards(Velocity.X, -TerminalVelocity, Acceleration);
            FacingRight = false;
        } 
        else if (InputManager.IsKeyDown(GBGame.KeyboardRight) || InputManager.IsGamePadDown(GBGame.ControllerRight))
        {
            Velocity.X = MathUtility.MoveTowards(Velocity.X, TerminalVelocity, Acceleration);
            FacingRight = true;
        }
        else 
        {
            Velocity.X = MathUtility.MoveTowards(Velocity.X, 0, Acceleration);
        }

        if (IsOnFloor && (InputManager.IsKeyPressed(GBGame.KeyboardJump) || InputManager.IsGamePadPressed(GBGame.ControllerJump)))
        {
            Velocity.Y = -JumpVelocity;
            IsOnFloor = false;
        }

        if (!IsOnFloor)
        {
            Velocity.Y = MathUtility.MoveTowards(Velocity.Y, TerminalVelocity, 0.8f);
        }

        Position += Velocity;
        Position.X = float.Round(Position.X);

        Collider.Bounds = new Rectangle((int)Position.X - 2, (int)Position.Y - 2, 4, 4);
    }

    public override void Draw(SpriteBatch batch, GameTime time)
    {
        batch.Draw(_sprite, Position, null, Color.White, 0, _origin, 1, SpriteEffects.None, 0);
    }
}
