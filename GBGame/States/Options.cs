using System;
using GBGame.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Controllers;
using MonoGayme.States;
using MonoGayme.UI;

namespace GBGame.States;

public class Options(GameWindow window) : State
{
    private readonly UIController _controller = new UIController(true);
    
    private readonly Color _overlayColour = new Color(40, 56, 24);
    private readonly Color _textColour = new Color(176, 192, 160);
    private readonly Color _backDrop = new Color(232, 240, 223);
    
    private SpriteFont _font = null!;
    
    private Shapes _shapes = null!;
    
    public override void LoadContent()
    {
        window.UpdateOptions();
        
        _shapes = new Shapes(window.GraphicsDevice);
        
        _controller.OnActiveUpdating = btn => btn.Colour = _textColour;
        
        SoundEffect click = window.Content.Load<SoundEffect>("Sounds/Click");
        _controller.OnActiveUpdated = btn =>
        {
            btn.Colour = _overlayColour;
            window.PlayEffect(click);
        };

        _controller.SetKeyboardButtons(GBGame.KeyboardInventoryUp, GBGame.KeyboardInventoryDown, GBGame.KeyboardAction);
        _controller.SetControllerButtons(GBGame.ControllerInventoryUp, GBGame.ControllerInventoryDown, GBGame.ControllerAction);

        _font = window.Content.Load<SpriteFont>("Sprites/Fonts/File");
        TextButton ret = new TextButton(_font, "return", new Vector2((window.GameSize.X - _font.MeasureString("return").X) / 2, window.GameSize.Y - 12), _textColour, true)
        {
            OnClick = (btn) =>
            {
                window.UpdateOptions();
                window.Context.SwitchState(new MainMenu(window));
            }
        };
        
        _controller.AddIgnored(new Label("options", _overlayColour, _font, new Vector2((window.GameSize.X - _font.MeasureString("Options").X) / 2, 5)));
        _controller.AddIgnored(new Label("graphics", _overlayColour, _font, new Vector2(5, 20)));
        _controller.AddIgnored(new Label("audio", _overlayColour, _font, new Vector2(5, 50)));
        _controller.AddIgnored(new Label("misc", _overlayColour, _font, new Vector2(5, 70)));
        
        Texture2D normal = window.Content.Load<Texture2D>("Sprites/UI/CheckBox_Normal");
        Texture2D check = window.Content.Load<Texture2D>("Sprites/UI/CheckBox_Checked");

        CheckBox fs = new CheckBox(normal, check, _font, "fullscreen", new Vector2(1, 30), new Vector2(10, -1), _textColour, window.Options.FullScreen)
        {
            OnCheckChanged = checks =>
            {
                window.ToggleFullScreen(!checks);
                window.UpdateOptions(new OptionData { AllowScreenShake = window.Options.AllowScreenShake, MuteAudio = window.Options.MuteAudio, FullScreen = window.IsFullScreen(), ShowVersion = window.Options.ShowVersion, Keyboard = window.Options.Keyboard, GamePad = window.Options.GamePad });
            }
        };

        CheckBox allowShake = new CheckBox(normal, check, _font, "allow screenshake", new Vector2(1, 40), new Vector2(10, -1), _textColour, window.Options.AllowScreenShake)
        {
            OnCheckChanged = checks => 
                window.UpdateOptions(new OptionData { AllowScreenShake = checks, MuteAudio = window.Options.MuteAudio, FullScreen = window.IsFullScreen(), ShowVersion = window.Options.ShowVersion, Keyboard = window.Options.Keyboard, GamePad = window.Options.GamePad})
        };

        CheckBox mute = new CheckBox(normal, check, _font, "mute sounds", new Vector2(1, 60), new Vector2(10, -1), _textColour, window.Options.MuteAudio)
        {
            OnCheckChanged = checks =>
            {
                window.UpdateOptions(
                    new OptionData
                    {
                        AllowScreenShake = window.Options.AllowScreenShake, 
                        MuteAudio = checks, 
                        FullScreen = window.IsFullScreen(),
                        ShowVersion = window.Options.ShowVersion, 
                        Keyboard = window.Options.Keyboard, 
                        GamePad = window.Options.GamePad
                    }
                );
                
                window.UpdateOptions();
            }
        };

        CheckBox ver = new CheckBox(normal, check, _font, "show build num.", new Vector2(1, 80), new Vector2(10, -1), _textColour, window.Options.ShowVersion)
        {
            OnCheckChanged = checks =>
                window.UpdateOptions(new OptionData { AllowScreenShake = window.Options.AllowScreenShake, MuteAudio = window.Options.MuteAudio, FullScreen = window.IsFullScreen(), ShowVersion = checks, Keyboard = window.Options.Keyboard, GamePad = window.Options.GamePad })
        };
        
        _controller.AddIgnored(new Label("controls", _overlayColour, _font, new Vector2(5, 90)));

        TextButton kb = new TextButton(_font, "Set Keyboard Binds", new Vector2(1, 100), _textColour, true)
        {
            OnClick = (btn) =>
            {
                window.UpdateOptions();
                window.Context.SwitchState(new KeyboardBinds(window));
            }
        };
        
        TextButton gp = new TextButton(_font, "Set GamePad Binds", new Vector2(1, 110), _textColour, true)
        {
            OnClick = (btn) =>
            {
                window.UpdateOptions();
                window.Context.SwitchState(new GamePadBinds(window));
            }
        };
        
        _controller.Add(fs);
        _controller.Add(allowShake);
        _controller.Add(mute);
        _controller.Add(ver);
        _controller.Add(kb);
        _controller.Add(gp);
        _controller.Add(ret);
    }

    public override void Update(GameTime time)
    {
        if (!window.Updating)
            _controller.Update(window.MousePosition);
    }

    public override void Draw(GameTime time, SpriteBatch batch)
    {
        window.GraphicsDevice.Clear(_backDrop);
        batch.Begin(samplerState: SamplerState.PointClamp);
            _shapes.DrawRectangle(new Rectangle(0, 0, (int)window.GameSize.X, (int)window.GameSize.Y), _overlayColour, batch, 0.6f);
            _controller.Draw(batch);

            if (window.Updating)
            {
                _shapes.DrawRectangle(new Rectangle(0, 0, (int)window.GameSize.X, (int)window.GameSize.Y), _overlayColour, batch);
            }

            batch.End();
    }
}