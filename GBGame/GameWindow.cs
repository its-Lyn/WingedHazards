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

    public Vector2 GameSize { get; private set;}
    public StateContext Context { get; private set; }

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
            _graphics.ToggleFullScreen();
        }

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
