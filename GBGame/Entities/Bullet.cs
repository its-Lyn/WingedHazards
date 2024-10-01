using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Components.Colliders;
using MonoGayme.Entities;
using MonoGayme.Utilities;

namespace GBGame.Entities;

public class Bullet(Game windowData, Vector2 pos, Vector2 target, int zIndex = 0) : Entity(windowData, zIndex)
{
    private Texture2D _sprite = null!;

    private Vector2 _direction;
    private float _accel = 1f;

    private RectCollider _collider = null!;

    public override void LoadContent()
    {
        _sprite = WindowData.Content.Load<Texture2D>("Sprites/Objects/Bullet");
        Position = pos;

        _direction = Vector2.Normalize(target - Position);

        Components.AddComponent(new RectCollider());
        _collider = Components.GetComponent<RectCollider>()!;
    }

    public override void Update(GameTime time)
    {
        Velocity = MathUtility.MoveTowards(Velocity, _direction * 2, _accel);
        Position += Velocity;

        _collider.Bounds = new Rectangle((int)Position.X + 2, (int)Position.Y + 2, 4, 4);
    }

    public override void Draw(SpriteBatch batch, GameTime time)
    {
        batch.Draw(_sprite, Position, Color.White);
    }
}
