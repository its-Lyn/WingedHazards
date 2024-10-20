using System;
using System.IO;
using GBGame.Models;
using GBGame.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGayme.Extensions;
using MonoGayme.States;
using MonoGayme.Utilities;

namespace GBGame;

public class GameWindow : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;

    private readonly Renderer _renderer;

    public int XPMultiplier { get; set; } = 1;
    public Vector2 GameSize { get; }
    public Vector2 MousePosition { get; private set; }
    public StateContext Context { get; }
    
    public bool GameEnding { get; set; }
    public bool GameEnded { get; set; }

    private Vector2 _sizeBeforeResize;
    private bool _isFullScreen;
    
    public OptionData Options { get; private set; }
    public bool Updating { get; private set; }

    public string Version { get; } = "0.9.1";

    public GameWindow()
    {
        
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;

        GameSize = new Vector2(160, 144);
        _graphics.SetWindowSize(GameSize * 3);

        _renderer = new Renderer(GameSize, GraphicsDevice); 

        Window.AllowUserResizing = true;

        Context = new StateContext();
        
        UpdateOptions();
        if ((bool)Options?.FullScreen)
        {
            ToggleFullScreen();
        }
        
        SetKeyBinds();
    }

    public void UpdateOptions()
    {
        Options = Xml.Deserialise<OptionData>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "OptionData.xml"));
    }

    public void UpdateOptions(OptionData newOptions)
    {
        Updating = true;
        Xml.Serialise(newOptions, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "OptionData.xml"));
        Updating = false;
    }

    public void UpdateKeys()
    {
        KeyBinds binds = new KeyBinds
        {
            Left = GBGame.KeyboardLeft.ToString(),
            Right = GBGame.KeyboardRight.ToString(),
            InventoryUp = GBGame.KeyboardInventoryUp.ToString(),
            InventoryDown = GBGame.KeyboardInventoryDown.ToString(),
            
            Action = GBGame.KeyboardAction.ToString(),
            Jump = GBGame.KeyboardJump.ToString(),
            
            Pause = GBGame.KeyboardPause.ToString(),
        };
        
        Options.Keyboard = binds;
        UpdateOptions(Options);
    }

    public void UpdateButtons()
    {
        KeyBinds binds = new KeyBinds
        {
            Left = GBGame.ControllerLeft.ToString(),
            Right = GBGame.ControllerRight.ToString(),
            InventoryUp = GBGame.ControllerInventoryUp.ToString(),
            InventoryDown = GBGame.ControllerInventoryDown.ToString(),
            
            Action = GBGame.ControllerAction.ToString(),
            Jump = GBGame.ControllerJump.ToString(),
            
            Pause = GBGame.ControllerPause.ToString(),
        };
        
        Options.GamePad = binds;
        UpdateOptions(Options);
    }
    
    private Keys ParseKey(string key)
    {
        bool success = Enum.TryParse<Keys>(key, out Keys result);
        if (!success)
            throw new Exception();
        
        return result;
    }

    private Buttons ParseButton(string button)
    {
        bool success = Enum.TryParse<Buttons>(button, out Buttons result);
        if (!success)
            throw new Exception();
        
        return result;
    }

    public void SetKeyBinds()
    {
        // Please forgive me
        GBGame.KeyboardLeft = ParseKey(Options.Keyboard.Left);
        GBGame.KeyboardRight = ParseKey(Options.Keyboard.Right);
        GBGame.KeyboardInventoryUp = ParseKey(Options.Keyboard.InventoryUp);
        GBGame.KeyboardInventoryDown = ParseKey(Options.Keyboard.InventoryDown);
        
        GBGame.KeyboardJump = ParseKey(Options.Keyboard.Jump);
        GBGame.KeyboardAction = ParseKey(Options.Keyboard.Action);
        
        GBGame.KeyboardPause = ParseKey(Options.Keyboard.Pause);
        
        GBGame.ControllerLeft = ParseButton(Options.GamePad.Left);
        GBGame.ControllerRight = ParseButton(Options.GamePad.Right);
        GBGame.ControllerInventoryUp = ParseButton(Options.GamePad.InventoryUp);
        GBGame.ControllerInventoryDown = ParseButton(Options.GamePad.InventoryDown);
        
        GBGame.ControllerJump = ParseButton(Options.GamePad.Jump);
        GBGame.ControllerAction = ParseButton(Options.GamePad.Action);
        
        GBGame.ControllerPause = ParseButton(Options.GamePad.Pause);
    }

    public void ToggleFullScreen()
    {
        if (_isFullScreen)
        {
            _graphics.SetWindowSize(_sizeBeforeResize);
        }
        else
        {
            _sizeBeforeResize = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            _graphics.SetWindowSize(
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
            );
        }

        _isFullScreen = !_isFullScreen;
        _graphics.ToggleFullScreen();
    }

    public void ToggleFullScreen(bool fs)
    {
        if (fs)
        {
            _graphics.SetWindowSize(_sizeBeforeResize);
        }
        else
        {
            _sizeBeforeResize = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            _graphics.SetWindowSize(
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
            );
        }

        _isFullScreen = !_isFullScreen;
        _graphics.ToggleFullScreen();
    }

    public bool IsFullScreen() => _isFullScreen;

    public void PlayEffect(SoundEffect effect)
    {
        if (Options.MuteAudio) return;
        effect.Play();
    }

    protected override void Initialize()
    {
        base.Initialize();

        Window.Title = "Winged Hazards";
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Context.SwitchState(new MainMenu(this));
    }

    protected override void Update(GameTime gameTime)
    {
        MousePosition = _renderer.GetVirtualMousePosition();
        Context.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        InputManager.GetState();
        
        _renderer.SetRenderer();
        Context.Draw(gameTime, _spriteBatch);

        _renderer.DrawRenderer(_spriteBatch);

        base.Draw(gameTime);
    }
}
