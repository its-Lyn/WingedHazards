using System;
using GBGame.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Components;
using MonoGayme.Controllers;
using MonoGayme.States;
using MonoGayme.UI;

namespace GBGame.States;

public class MainMenu(GameWindow window) : State(window)
{
    private ButtonController _controller = null!;
    private SpriteFont _font = null!;
    private readonly Color _overlayColour = new Color(40, 56, 24);
    private readonly Color _textColour = new Color(176, 192, 160);
    private readonly Color _backDrop = new Color(232, 240, 223);

    private Vector2 _logoPos;
    private float _timer;
    private float _measurement;

    private AnimatedSpriteSheet _bat = null!;
    private Clouds _clouds = null!;
    
    public override void LoadContent()
    {
        _font = window.Content.Load<SpriteFont>("Sprites/Fonts/File");
        _controller = new ButtonController(true)
        {
            OnActiveUpdating = btn =>
            {
                btn.Colour = _textColour;
            },
            
            OnActiveUpdated = btn =>
            {
                btn.Colour = _overlayColour;
            }
        };
        
        _controller.SetKeyboardButtons(GBGame.KeyboardInventoryUp, GBGame.KeyboardInventoryDown, GBGame.KeyboardAction);

        TextButton quit = new TextButton(_font, "quit", new Vector2((window.GameSize.X - _font.MeasureString("quit").X) / 2, window.GameSize.Y - 30), _textColour)
        {
            OnClick = window.Exit
        };

        TextButton options = new TextButton(_font, "options", new Vector2((window.GameSize.X - _font.MeasureString("options").X) / 2, window.GameSize.Y - 40), _textColour)
        {
            OnClick = () =>
            {
                window.Context.SwitchState(new InGame(window));
            }
        };
        
        TextButton play = new TextButton(_font, "play", new Vector2((window.GameSize.X - _font.MeasureString("play").X) / 2, window.GameSize.Y - 50), _textColour)
        {
            OnClick = () =>
            {
                window.Context.SwitchState(new InGame(window));
            }
        };
        
        _controller.Add(play);
        _controller.Add(options);
        _controller.Add(quit);

        _measurement = _font.MeasureString("Winged Hazards").X;
        _logoPos = new Vector2( 
            (window.GameSize.X - _measurement) / 2,
            30
        );

        _bat = new AnimatedSpriteSheet(window.Content.Load<Texture2D>("Sprites/Entities/NormalBat"), new Vector2(3, 1), 0.25f, true);
        _clouds = new Clouds(window, 7, 15, 8, (int)window.GameSize.Y - 8);
    }

    public override void Update(GameTime time)
    {
        _clouds.Update();
        
        _timer += (float)time.ElapsedGameTime.TotalSeconds;
        _logoPos.Y = 30 + 5 * MathF.Sin(_timer * 2.5f);
        
        _bat.CycleAnimation(time);
        _controller.Update(window.MousePosition);
    }

    public override void Draw(GameTime time, SpriteBatch batch)
    {
        window.GraphicsDevice.Clear(_backDrop);
        batch.Begin(samplerState: SamplerState.PointClamp);
        {
            _clouds.Draw(batch);
            
            _bat.Draw(batch, _logoPos with { X = _logoPos.X - 9, Y = _logoPos.Y + 1 });
            batch.DrawString(_font, "Winged Hazards", _logoPos, _overlayColour);
            _bat.Draw(batch, _logoPos with { X = _logoPos.X + _measurement - 1, Y = _logoPos.Y + 1 }, true);
            
            _controller.Draw(batch);
        }
        batch.End();
    }
}