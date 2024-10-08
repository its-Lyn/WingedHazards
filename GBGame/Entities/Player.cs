using GBGame.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Components;
using MonoGayme.Components.Colliders;
using MonoGayme.Entities;
using MonoGayme.Utilities;
using System.Collections.Generic;

namespace GBGame.Entities;

public class Player(Game windowData, Camera2D camera, int zIndex = 1) : Entity(windowData, zIndex)
{
    private AnimatedSpriteSheet _sprite = null!;
    private AnimatedSpriteSheet _walkSprite = null!;
    private AnimatedSpriteSheet _idleSprite = null!;
    private AnimatedSpriteSheet _jumpSprite = null!;

    private Vector2 _origin = Vector2.Zero;

    private readonly float TerminalVelocity = 2f;
    private readonly float Acceleration = 0.5f;
    public float FallDecrease = 0;
    public float GravityMultiplier = 0;

    public bool IsOnFloor = false;
    public bool IsJumping = false;
    public bool FacingRight = true;

    public readonly float JumpVelocity = 7f;

    public RectCollider Collider = null!;
    private Timer _immunityTimer = null!;

    public Health Health = null!;

    private List<SpriteSheet> _health = [];

    private Jump _jump = null!;

    public int Level = 1;
    public int XP = 0;

    private Texture2D _healthSheet = null!;
    private int _basePosition = 1;

    private Shapes _shapes = null!;

    private void CycleWalk(GameTime time)
    {
        if (IsOnFloor)
        {
            if (_sprite != _walkSprite) _sprite = _walkSprite;
            _sprite.CycleAnimation(time);
        }
    }

    private void HandleJump()
    {
        Velocity.Y = -JumpVelocity;
        IsJumping = true;
    }

    public void ApplyKnockBack(RectCollider other)
    {
        Vector2 dir = Vector2.Normalize(Collider.GetCentre() - other.GetCentre());
        Velocity += 5 * dir;

        _health[Health.HealthPoints - 1].DecrementY();

        Health.HealthPoints--;
        if (Health.HealthPoints <= 0)
        {
            // :trollface:
            WindowData.Exit();
        }

        Collider.Enabled = false;
        _immunityTimer.Start();
    }

    public void AddHealth()
    { 
        Health.HealthPoints++;

        SpriteSheet sheet = new SpriteSheet(_healthSheet, new Vector2(1, 2), new Vector2(_basePosition, 20));
        sheet.IncrementY();

        _health.Add(sheet);
        if (Health.HealthPoints <= Health.OriginalHealthPoints)
        {
            sheet.DecrementY();
            foreach (SpriteSheet health in _health)
            {
                if (health.Y == 0)
                {
                    health.IncrementY();
                    break;
                }
            }
        }

        _basePosition += 17;
    }

    public override void LoadContent()
    {
        Position.X = 40;

        _origin = new Vector2(4, 8);
        _walkSprite = new AnimatedSpriteSheet(WindowData.Content.Load<Texture2D>("Sprites/Entities/Player_Walk"), new Vector2(4, 1), 0.2f, false, _origin);
        _idleSprite = new AnimatedSpriteSheet(WindowData.Content.Load<Texture2D>("Sprites/Entities/Player_Idle"), Vector2.One, 0.2f, false, _origin);
        _jumpSprite = new AnimatedSpriteSheet(WindowData.Content.Load<Texture2D>("Sprites/Entities/Player_Jump"), Vector2.One, 0.2f, false, _origin);

        _sprite = _idleSprite;

        Components.AddComponent(new RectCollider());
        Collider = Components.GetComponent<RectCollider>()!;

        Components.AddComponent(new Jump(1));
        _jump = Components.GetComponent<Jump>()!;

        Components.AddComponent(new Timer(1, false, true, "ImmunityTimer"));
        _immunityTimer = Components.GetComponent<Timer>()!;
        _immunityTimer.OnTimeOut = () => {
            Collider.Enabled = true;
        };

        Components.AddComponent(new Health(3));
        Health = Components.GetComponent<Health>()!;

        _healthSheet = WindowData.Content.Load<Texture2D>("Sprites/UI/Health");
        for (int i = 0; i < Health.HealthPoints; i++)
        { 
            SpriteSheet sheet = new SpriteSheet(_healthSheet, new Vector2(1, 2), new Vector2(_basePosition, 20));
            sheet.IncrementY();

            _health.Add(sheet);

            _basePosition += 17;
        }

        _shapes = new Shapes(WindowData.GraphicsDevice);
    }

    public override void Update(GameTime time)
    {
        _immunityTimer.Cycle(time);

        if (InputManager.IsKeyDown(GBGame.KeyboardLeft) || InputManager.IsGamePadDown(GBGame.ControllerLeft))
        {
            Velocity.X = MathUtility.MoveTowards(Velocity.X, -TerminalVelocity, Acceleration);
            FacingRight = false;

            CycleWalk(time);
        } 
        else if (InputManager.IsKeyDown(GBGame.KeyboardRight) || InputManager.IsGamePadDown(GBGame.ControllerRight))
        {
            Velocity.X = MathUtility.MoveTowards(Velocity.X, TerminalVelocity, Acceleration);
            FacingRight = true;

            CycleWalk(time);
        }
        else 
        {
            if (_sprite != _idleSprite) _sprite = _idleSprite;
            Velocity.X = MathUtility.MoveTowards(Velocity.X, 0, Acceleration);
        }

        if (IsOnFloor && (InputManager.IsKeyPressed(GBGame.KeyboardJump) || InputManager.IsGamePadPressed(GBGame.ControllerJump)))
        {
            HandleJump();
            IsOnFloor = false;
        }

        if (!IsOnFloor && _jump.Count > 0 && (InputManager.IsKeyPressed(GBGame.KeyboardJump) || InputManager.IsGamePadPressed(GBGame.ControllerJump)))
        {
            HandleJump();
            if (FallDecrease != 0)
                FallDecrease = 0;

            _jump.Count--;
        }

        if (!IsOnFloor)
        {
            if (IsJumping)
            {

                if (Velocity.Y > 0)
                { 
                    GravityMultiplier = 2f;
                    FallDecrease = 0.5f;
                }
            }

            Velocity.Y = MathUtility.MoveTowards(Velocity.Y, TerminalVelocity + GravityMultiplier, 0.8f - FallDecrease);
            if (_sprite != _jumpSprite) _sprite = _jumpSprite;
        }

        Position += Velocity;
        Position.X = float.Round(Position.X);

        Collider.Bounds = new Rectangle((int)Position.X - 2, (int)Position.Y - 4, 4, 10);
    }

    public override void Draw(SpriteBatch batch, GameTime time)
    {
        _sprite.Draw(batch, Position, !FacingRight);
        foreach (SpriteSheet health in _health)
        {
            health.Draw(batch, camera);
        }
    }
}
