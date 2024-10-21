using System;
using GBGame.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Components;
using MonoGayme.Components.Colliders;
using MonoGayme.Entities;
using MonoGayme.Utilities;

namespace GBGame.Entities.Enemies;

public class NormalBat(GameWindow windowData, Vector2 pos, int zIndex = 0) : Entity(zIndex)
{
    private AnimatedSpriteSheet _sprite = null!;
    private bool _flipped;
    
    private bool _locked;
    private Entity? _lockedEntity;

    private const float Speed = 0.6f;

    private RectCollider _rectCollider = null!;
    private RectCollider _playerHitter = null!;
    private Timer _immunityTimer = null!;

    public void LockOn(Entity entity)
    {
        _lockedEntity = entity;
        _locked = true;
    }

    public override void LoadContent()
    {
        Texture2D batSheet = windowData.ContentData.Get("NormalBat");
        _sprite = new AnimatedSpriteSheet(batSheet, new Vector2(3, 1), 0.25f, true);
        Position = pos;

        Components.AddComponent(new RectCollider("PlayerStriker"));
        Components.AddComponent(new RectCollider("PlayerHitter"));
        Components.AddComponent(new Timer(0.5f, true, true, "ImmunityTimer"));
        Components.AddComponent(new XPDropper(2));
        Components.AddComponent(new Health(2));

        _rectCollider = Components.GetComponent<RectCollider>("PlayerStriker")!;
        _playerHitter = Components.GetComponent<RectCollider>("PlayerHitter")!;

        _immunityTimer = Components.GetComponent<Timer>("ImmunityTimer")!;
        _immunityTimer.OnTimeOut = () => { _rectCollider.Enabled = true; };
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
            
            Vector2 dir = _lockedEntity.Position with { Y = _lockedEntity.Position.Y - 4 } - Position;
            dir.Normalize();

            Vector2 target = dir * Speed;
            Velocity = MathUtility.MoveTowards(Velocity, target, 0.05f);

            _flipped = !(Position.X - _lockedEntity.Position.X < 0);
        }

        Position += Velocity;

        _rectCollider.Bounds = new Rectangle((int)Position.X, (int)Position.Y, 8, 8);
        _playerHitter.Bounds = new Rectangle((int)Position.X + 2, (int)Position.Y + 2, 4, 4);

        _immunityTimer.Cycle(time);
        _sprite.CycleAnimation(time);
    }

    public override void Draw(SpriteBatch batch, GameTime time)
    {
        _sprite.Draw(batch, Position, _flipped);
    }
}
