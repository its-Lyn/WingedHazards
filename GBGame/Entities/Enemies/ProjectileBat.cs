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

public class ProjectileBat(Game windowData, Vector2 pos, int zIndex = 0) : Entity(windowData, zIndex)
{
    private Entity? _lockedEntity;
    private bool _locked = false;

    private readonly float _speed = 0.75f;

    private Timer _stateTimer = null!;
    private Timer _waitTimer = null!;
    private Timer _immunityTimer = null!;

    private EntityController _bulletController = null!;

    private RectCollider _collider = null!;
    private RectCollider _hitterCollider = null!;

    private bool _hasShot = false;

    private Texture2D _sprite = null!;

    private enum AIState
    {
        Moving,
        Shooting
    };

    private AIState _state = AIState.Moving;

    public void LockOn(Entity entity)
    { 
        _lockedEntity = entity;
        _locked = true;
    }

    public override void LoadContent()
    {
        Position = pos;

        _sprite = WindowData.Content.Load<Texture2D>("Sprites/Ground/Ground_4");

        Components.AddComponent(new Timer(1, true, false, "StateTimer"));
        Components.AddComponent(new Timer(1, false, false, "WaitTimer"));
        Components.AddComponent(new Timer(0.5f, true, true, "ImmunityTimer"));
        Components.AddComponent(new EntityController());
        Components.AddComponent(new RectCollider("PlayerStriker"));
        Components.AddComponent(new RectCollider("PlayerHitter"));
        Components.AddComponent(new Health(3));

        _collider = Components.GetComponent<RectCollider>("PlayerStriker")!;
        _hitterCollider = Components.GetComponent<RectCollider>("PlayerHitter")!;

        _bulletController = Components.GetComponent<EntityController>()!;
        _bulletController.OnEntityUpdate = (device, time, entity) => {
            if (_locked && _lockedEntity is not null)
            {
                if (_lockedEntity is Player player)
                {
                    RectCollider? collider = entity.Components.GetComponent<RectCollider>();
                    if (collider is null) return;

                    if (player.Collider.Collides(collider))
                    {
                        player.ApplyKnockBack(collider);
                        _bulletController.QueueRemove(entity);
                    }
                }
            }
        };

        _stateTimer = Components.GetComponent<Timer>("StateTimer")!;
        _stateTimer.OnTimeOut = () => {
            // 1/3 chance to switch states every second
            if (Random.Shared.Next(0, 3) == 1)
            {
                if (_state == AIState.Moving)
                {
                    _state = AIState.Shooting;
                }
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
                
                if (_lockedEntity is null || !_locked) return;
                _bulletController.AddEntity(new Bullet(WindowData, Position, _lockedEntity.Position));
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

        _bulletController.UpdateEntities(WindowData.GraphicsDevice, time);

        switch (_state)
        { 
            case AIState.Moving:
                if (_locked && _lockedEntity is not null)
                { 
                    Vector2 dir = _lockedEntity.Position with { Y = _lockedEntity.Position.Y - 4 } - Position;
                    dir.Normalize();

                    Vector2 target = dir * _speed;
                    Velocity = MathUtility.MoveTowards(Velocity, target, 0.05f);
                }
            break;
            
            case AIState.Shooting:
                Velocity = MathUtility.MoveTowards(Velocity, Vector2.Zero, 0.05f);
                if (Velocity == Vector2.Zero) _waitTimer.Start();
            break;
        }

        Position += Velocity;
        _collider.Bounds = new Rectangle((int)Position.X, (int)Position.Y, 8, 8);
        _hitterCollider.Bounds = new Rectangle((int)Position.X + 2, (int)Position.Y + 2, 4, 4);
    }
    
    public override void Draw(SpriteBatch batch, GameTime time)
    {
        _bulletController.DrawEntities(batch, time);
        batch.Draw(_sprite, Position, Color.Blue);
    }
}
