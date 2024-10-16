using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using GBGame.States;
using Microsoft.Xna.Framework;
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
        if (InputManager.IsKeyPressed(Keys.F))
            ToggleFullScreen();

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
