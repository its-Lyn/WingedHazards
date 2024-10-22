using GBGame.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Components;
using MonoGayme.Components.Colliders;
using MonoGayme.Controllers;
using MonoGayme.Entities;
using MonoGayme.Utilities;
using System;

namespace GBGame.Entities.Enemies;

public class ProjectileBat(GameWindow windowData, Vector2 pos, int zIndex = 0) : Entity(zIndex)
{
    private Entity? _lockedEntity;
    private bool _locked;

    private const float Speed = 0.75f;

    private Timer _stateTimer = null!;
    private Timer _waitTimer = null!;
    private Timer _immunityTimer = null!;

    private EntityController _bulletController = null!;

    private RectCollider _collider = null!;
    private RectCollider _hitterCollider = null!;

    private bool _hasShot;

    private AnimatedSpriteSheet _activeSprite = null!;

    private AnimatedSpriteSheet _walkSprite = null!;
    private AnimatedSpriteSheet _shootSprite = null!;

    private bool _flipped;

    private enum AIState
    {
        Moving,
        Shooting
    }

    private AIState _state = AIState.Moving;

    public void LockOn(Entity entity)
    { 
        _lockedEntity = entity;
        _locked = true;
    }

    public override void LoadContent()
    {
        Position = pos;

        Texture2D walk = windowData.ContentData.Get("ProjectileBat_Walk");
        _walkSprite = new AnimatedSpriteSheet(walk, new Vector2(3, 1), 0.15f, true);

        Texture2D shoot = windowData.ContentData.Get("ProjectileBat_Shoot");
        _shootSprite = new AnimatedSpriteSheet(shoot, new Vector2(4, 1), 0.1f);

        _activeSprite = _walkSprite;

        Components.AddComponent(new Timer(1, true, false, "StateTimer"));
        Components.AddComponent(new Timer(1, false, false, "WaitTimer"));
        Components.AddComponent(new Timer(0.5f, true, true, "ImmunityTimer"));
        Components.AddComponent(new EntityController());
        Components.AddComponent(new RectCollider("PlayerStriker"));
        Components.AddComponent(new RectCollider("PlayerHitter"));
        Components.AddComponent(new XPDropper(3));
        Components.AddComponent(new Health(3));

        _collider = Components.GetComponent<RectCollider>("PlayerStriker")!;
        _hitterCollider = Components.GetComponent<RectCollider>("PlayerHitter")!;

        _bulletController = Components.GetComponent<EntityController>()!;
        _bulletController.OnEntityUpdate = (_, _, entity) => {
            // Remove bullet if it does offscreen.
            if (entity.Position.X > windowData.GameSize.X * 2 + 8 || entity.Position.X < -8)
                _bulletController.QueueRemove(entity);
            
            if (entity.Position.Y > windowData.GameSize.Y + 8 || entity.Position.Y < -8) 
                _bulletController.QueueRemove(entity);

            if (!_locked || _lockedEntity is null) return;
            
            if (_lockedEntity is not Player player) return;
            RectCollider? collider = entity.Components.GetComponent<RectCollider>();
            if (collider is null) return;

            if (!player.Collider.Collides(collider)) return;
            player.ApplyKnockBack(collider);
            _bulletController.QueueRemove(entity);
        };

        _stateTimer = Components.GetComponent<Timer>("StateTimer")!;
        _stateTimer.OnTimeOut = () =>
        {
            // 1/3 chance to switch states every second
            if (Random.Shared.Next(0, 3) != 1) return;
            
            if (_state == AIState.Moving)
            {
                _state = AIState.Shooting;
            }
        };

        _waitTimer = Components.GetComponent<Timer>("WaitTimer")!;
        _waitTimer.OnTimeOut = () =>
        {
            if (_hasShot)
            {
                _state = AIState.Moving;

                _waitTimer.Stop();
                _hasShot = false;
            }
            else
            {
                _hasShot = true;

                _activeSprite = _shootSprite;
                _activeSprite.Finished = false;
                
                if (_lockedEntity is null || !_locked) return;
                _bulletController.AddEntity(new Bullet(windowData, Position, _lockedEntity.Position));
            }
        };

        _immunityTimer = Components.GetComponent<Timer>("ImmunityTimer")!;
        _immunityTimer.OnTimeOut = () => { _collider.Enabled = true; };

    }

    public override void Update(GameTime time)
    {
        _stateTimer.Cycle(time);
        _waitTimer.Cycle(time);
        _immunityTimer.Cycle(time);

        _bulletController.UpdateEntities(windowData.GraphicsDevice, time);

        switch (_state)
        { 
            case AIState.Moving:
                if (_locked && _lockedEntity is not null)
                { 
                    Vector2 dir = _lockedEntity.Position with { Y = _lockedEntity.Position.Y - 4 } - Position;
                    dir.Normalize();

                    Vector2 target = dir * Speed;
                    Velocity = MathUtility.MoveTowards(Velocity, target, 0.05f);
                }
                break;
            
            case AIState.Shooting:
                Velocity = MathUtility.MoveTowards(Velocity, Vector2.Zero, 0.05f);
                if (Velocity == Vector2.Zero) _waitTimer.Start();
                break;
        }

        if (_lockedEntity is not null)
            _flipped = !(Position.X - _lockedEntity.Position.X < 0);

        Position += Velocity;
        _collider.Bounds = new Rectangle((int)Position.X, (int)Position.Y, 8, 8);
        _hitterCollider.Bounds = new Rectangle((int)Position.X + 2, (int)Position.Y + 2, 4, 4);

        _activeSprite.CycleAnimation(time);

        if (_activeSprite == _shootSprite && _activeSprite.Finished)
            _activeSprite = _walkSprite;
    }
    
    public override void Draw(SpriteBatch batch, GameTime time)
    {
        _bulletController.DrawEntities(batch, time);
        _activeSprite.Draw(batch, Position, _flipped);
    }
}
