using System.Threading;
using GBGame.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGayme.States;
using MonoGayme.Utilities;

namespace GBGame;

public class GameWindow : Game
{
    private GraphicsDeviceManager _graphics = null!;
    private SpriteBatch _spriteBatch = null!;

    private Renderer _renderer;

    public Vector2 GameSize { get; private set; }
    public Vector2 MousePosition { get; private set; }
    public StateContext Context { get; private set; }

    private Vector2 _sizeBeforeResize;
    private bool _isFullScreen = false;

    public GameWindow()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        GameSize = new Vector2(160, 144);
        _graphics.SetWindowSize(GameSize * 3);

        _renderer = new Renderer(GameSize, GraphicsDevice); 

        Window.AllowUserResizing = true;

        Context = new StateContext();
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Context.SwitchState(new InGame(this));
    }

    protected override void Update(GameTime gameTime)
    {
        if (InputManager.IsKeyPressed(Keys.F))
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
