using GBGame.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Components;
using MonoGayme.Controllers;
using MonoGayme.UI;

namespace GBGame.Components;

public class Pause
{
    public bool Paused;

    private readonly Texture2D _overlay;
    private Rectangle _size;
    private readonly Color _overlayColour = new Color(40, 56, 24);
    private readonly Color _textColour = new Color(176, 192, 160);

    private readonly Vector2 _titlePos;

    private readonly SpriteFont _fontBig;

    private readonly GameWindow _window;

    private readonly UIController _controller = new UIController(true);

    public Pause(GameWindow window)
    {
        _overlay = new Texture2D(window.GraphicsDevice, 1, 1);
        _overlay.SetData([_overlayColour]);
        _size = new Rectangle(0, 0, (int)window.GameSize.X, (int)window.GameSize.Y);

        _fontBig = window.Content.Load<SpriteFont>("Sprites/Fonts/FontBig");
        SpriteFont font = window.Content.Load<SpriteFont>("Sprites/Fonts/File");

        Vector2 titleSize = _fontBig.MeasureString("PAUSED");
        _titlePos = new Vector2(
            (window.GameSize.X - titleSize.X) / 2,
            10
        );

        _window = window;

        TextButton resume = new TextButton(font, "resume", new Vector2((window.GameSize.X - font.MeasureString("resume").X) / 2, 40), _textColour, true) 
        {
            OnClick = (_) =>
            { 
                Paused = !Paused;
            }
        };

        TextButton mainMenu = new TextButton(font, "main menu", new Vector2((window.GameSize.X - font.MeasureString("main menu").X) / 2, 50), _textColour, true) 
        {
            OnClick = (_) => 
            {
                window.Context.SwitchState(new MainMenu(window));
            }
        };

        TextButton quit = new TextButton(font, "quit game", new Vector2((window.GameSize.X - font.MeasureString("quit game").X) / 2, 60), _textColour, true) 
        {
            OnClick = (_) => window.Exit()
        };

        _controller.SetControllerButtons(GBGame.ControllerInventoryUp, GBGame.ControllerInventoryDown, GBGame.ControllerAction);
        _controller.SetKeyboardButtons(GBGame.KeyboardInventoryUp, GBGame.KeyboardInventoryDown, GBGame.KeyboardAction);

        _controller.OnActiveUpdating = btn => {
            btn.Colour = _textColour;
        };

        SoundEffect click = window.ContentData.GetAudio("Click");
        _controller.OnActiveUpdated = btn => {
            btn.Colour = _overlayColour;
            window.PlayEffect(click);
        };

        _controller.Add(resume);
        _controller.Add(mainMenu);
        _controller.Add(quit);
    }

    public void Update() 
    {
        _controller.Update(_window.MousePosition);
    }

    public void Draw(SpriteBatch batch, Camera2D camera)
    {
        Vector2 relative = camera.ScreenToWorld(Vector2.Zero);
        _size.X = (int)relative.X;
        _size.Y = (int)relative.Y;

        batch.Draw(_overlay, _size, Color.White * 0.6f);

        batch.DrawString(_fontBig, "PAUSED", camera.ScreenToWorld(_titlePos), _textColour);

        _controller.Draw(batch, camera);
    }
}
