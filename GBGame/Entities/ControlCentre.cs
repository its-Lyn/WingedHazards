using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Components.Colliders;
using MonoGayme.Entities;
using MonoGayme.Utilities;

namespace GBGame.Entities;

public class ControlCentre(Game windowData, int groundLine, int zIndex = -1) : Entity(windowData, zIndex)
{
    private Texture2D _sprite = null!;
    private Texture2D _questionSprite = null!;
    private float _questionOpacity = 0;

    private RectCollider _collider = null!;

    public int SkillPoints = 0;

    public bool CanInteract = false;
    public bool Interacting = false;

    private Texture2D _overlay = null!;
    private Rectangle _size;
    private Color _overlayColour = new Color(40, 56, 24);
    private Color _textColour = new Color(176, 192, 160);

    private SpriteFont _font = null!;

    private readonly string _noSP = "No skill points!";
    private Vector2 _noSPMeasuremets;

    public override void LoadContent()
    {
        GameWindow window = (GameWindow)WindowData;
        _overlay = new Texture2D(window.GraphicsDevice, 1, 1);
        _overlay.SetData(new[] { _overlayColour });
        _size = new Rectangle(0, 0, (int)window.GameSize.X, (int)window.GameSize.Y);

        _sprite = WindowData.Content.Load<Texture2D>("Sprites/Objects/CommandCentre");
        _questionSprite = WindowData.Content.Load<Texture2D>("Sprites/Objects/CommandCentre_Interact");

        Position = new Vector2(
            0,
            groundLine - 12
        );

        Components.AddComponent(new RectCollider("Centre"));
        _collider = Components.GetComponent<RectCollider>()!;
        _collider.Bounds = new Rectangle(
            (int)Position.X, (int)Position.Y + 8, 16, 8
        );

        _font = WindowData.Content.Load<SpriteFont>("Sprites/Fonts/File");
        Vector2 noSP = _font.MeasureString(_noSP);
        _noSPMeasuremets = new Vector2((window.GameSize.X - noSP.X) / 2, (window.GameSize.Y - noSP.Y) / 2);
    }

    public override void Update(GameTime time)
    {
        if (CanInteract)
        {
            if (_questionOpacity < 1f)
            {
                _questionOpacity += 0.4f;
            }

            if (InputManager.IsGamePadPressed(GBGame.ControllerAction) || InputManager.IsKeyPressed(GBGame.KeyboardAction))
            {
                Interacting = !Interacting;
            }
        }
        else
        {
            if (_questionOpacity > 0)
            {
                _questionOpacity -= 0.4f;
            }
        }
    }
  
    public override void Draw(SpriteBatch batch, GameTime time)
    {
        batch.Draw(_sprite, Position, Color.White);
        batch.Draw(_questionSprite, Position with { Y = Position.Y - 10 }, Color.White * _questionOpacity);

        if (Interacting)
        {
            batch.Draw(_overlay, _size, _overlayColour * 0.8f);

            if (SkillPoints == 0)
            {
                batch.DrawString(_font, _noSP, _noSPMeasuremets, _textColour);
            }
            else
            {
                batch.DrawString(_font, $"SP: {SkillPoints}", new Vector2(2), _textColour);

                batch.DrawString(_font, "Maybe one day...", new Vector2(20, 60), _textColour);
            }
        }
    }
}
