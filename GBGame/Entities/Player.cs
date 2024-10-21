using System;
using GBGame.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Components;
using MonoGayme.Components.Colliders;
using MonoGayme.Entities;
using MonoGayme.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace GBGame.Entities;

public class Player(GameWindow windowData, Camera2D camera, int zIndex = 1) : Entity( zIndex)
{
    private AnimatedSpriteSheet _sprite = null!;
    private AnimatedSpriteSheet _walkSprite = null!;
    private AnimatedSpriteSheet _idleSprite = null!;
    private AnimatedSpriteSheet _jumpSprite = null!;

    private Vector2 _origin = Vector2.Zero;

    private const float TerminalVelocity = 2f;
    private const float Acceleration = 0.5f;
    public float FallDecrease;
    public float GravityMultiplier;

    public bool IsOnFloor;
    public bool IsJumping;
    public bool FacingRight = true;

    private const float JumpVelocity = 7f;

    public RectCollider Collider = null!;
    private Timer _immunityTimer = null!;

    public Health Health = null!;

    private record struct HealthData(SpriteSheet Sheet, Vector2 Position);
    private readonly List<HealthData> _health = [];

    private Jump _jump = null!;

    public int Level = 1;
    public int XP = 0;

    private Texture2D _healthSheet = null!;
    private int _basePosition = 1;
    
    private Flash _deathFlash = null!;

    public readonly Stopwatch SurvivalWatch = new Stopwatch();
    public bool AllowInput = true;

    private void CycleWalk(GameTime time)
    {
        if (!IsOnFloor) return;
        
        _sprite = _walkSprite;
        _sprite.CycleAnimation(time);
    }

    private bool ButtonsPressed(Keys keyboard, Buttons controller)
        => AllowInput && (InputManager.IsKeyPressed(keyboard) || InputManager.IsGamePadPressed(controller));
    
    private bool ButtonsDown(Keys keyboard, Buttons controller)
        => AllowInput && (InputManager.IsKeyDown(keyboard) || InputManager.IsGamePadDown(controller));

    private void HandleJump()
    {
        Velocity.Y = -JumpVelocity;
        IsJumping = true;
    }

    public void ApplyKnockBack(RectCollider other)
    {
        Vector2 dir = Vector2.Normalize(Collider.GetCentre() - other.GetCentre());
        Velocity += 5 * dir;

        _health[Health.HealthPoints - 1].Sheet.DecrementY();

        Health.HealthPoints--;
        if (Health.HealthPoints <= 0)
        {
            windowData.GameEnding = true;
            Collider.Enabled = false;
            AllowInput = false; 
            _deathFlash.Begin();
            
            SurvivalWatch.Stop();
        }

        Collider.Enabled = false;
        _immunityTimer.Start();
    }

    public void AddHealth()
    { 
        Health.HealthPoints++;

        SpriteSheet sheet = new SpriteSheet(_healthSheet, new Vector2(1, 2));
        sheet.IncrementY();

        _health.Add(new HealthData(sheet, new Vector2(_basePosition, 20)));
        if (Health.HealthPoints <= Health.OriginalHealthPoints)
        {
            sheet.DecrementY();
            foreach (HealthData health in _health.Where(health => health.Sheet.Y == 0))
            {
                health.Sheet.IncrementY();
                break;
            }
        }

        _basePosition += 17;
    }

    public override void LoadContent()
    {
        Position.X = 40;

        _origin = new Vector2(4, 8);
        _walkSprite = new AnimatedSpriteSheet(windowData.ContentData.Get("Player_Walk"), new Vector2(4, 1), 0.2f, false, _origin);
        _idleSprite = new AnimatedSpriteSheet(windowData.ContentData.Get("Player_Idle"), Vector2.One, 0.2f, false, _origin);
        _jumpSprite = new AnimatedSpriteSheet(windowData.ContentData.Get("Player_Jump"), Vector2.One, 0.2f, false, _origin);

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

        _healthSheet = windowData.ContentData.Get("Health");
        for (int i = 0; i < Health.HealthPoints; i++)
        { 
            SpriteSheet sheet = new SpriteSheet(_healthSheet, new Vector2(1, 2));
            sheet.IncrementY();

            _health.Add(new HealthData(sheet, new Vector2(_basePosition, 20)));

            _basePosition += 17;
        }

        Components.AddComponent(new Flash(windowData, Color.Wheat, new Rectangle(0, 0, (int)windowData.GameSize.X, (int)windowData.GameSize.Y), 0.05f, "DeathFlash"));
        _deathFlash = Components.GetComponent<Flash>("DeathFlash")!;
        _deathFlash.OnFlashFinished = () =>
        {
            windowData.GameEnded = true;
        };
        
        SurvivalWatch.Start();
    }

    public override void Update(GameTime time)
    {
        _immunityTimer.Cycle(time);

        if (ButtonsDown(GBGame.KeyboardLeft, GBGame.ControllerLeft))
        {
            Velocity.X = MathUtility.MoveTowards(Velocity.X, -TerminalVelocity, Acceleration);
            FacingRight = false;

            CycleWalk(time);
        }
        else if (ButtonsDown(GBGame.KeyboardRight, GBGame.ControllerRight))
        {
            Velocity.X = MathUtility.MoveTowards(Velocity.X, TerminalVelocity, Acceleration);
            FacingRight = true;

            CycleWalk(time);
        }
        else
        {
            _sprite = _idleSprite;
            Velocity.X = MathUtility.MoveTowards(Velocity.X, 0, Acceleration);
        }

        if (IsOnFloor && ButtonsPressed(GBGame.KeyboardJump, GBGame.ControllerJump))
        {
            HandleJump();
            IsOnFloor = false;
        }

        if (!IsOnFloor && _jump.Count > 0 && ButtonsPressed(GBGame.KeyboardJump, GBGame.ControllerJump))
        {
            HandleJump();
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
            _sprite = _jumpSprite;
        }
        
        if (_deathFlash.Flashing)
            _deathFlash.Update(time);

        if (windowData.GameEnding) return;
        
        Position += Velocity;
        
        // Clamp The player position
        Position.X = Math.Clamp(Position.X, 4, windowData.GameSize.X * 2 - 36);
        
        Position.X = float.Round(Position.X);

        Collider.Bounds = new Rectangle((int)Position.X - 2, (int)Position.Y - 4, 4, 10);
    }

    public override void Draw(SpriteBatch batch, GameTime time)
    {
        _sprite.Draw(batch, Position, !FacingRight);
        foreach (HealthData health in _health)
        {
            health.Sheet.Draw(batch, health.Position, camera);
        }
    }
}
