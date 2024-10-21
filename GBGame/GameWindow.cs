using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GBGame.Models;
using GBGame.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
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
    public ContentLoader ContentData { get; }
    
    public bool GameEnding { get; set; }
    public bool GameEnded { get; set; }

    private Vector2 _sizeBeforeResize;
    private bool _isFullScreen;

    public OptionData Options { get; private set; } = null!;
    
    public bool Loading { get; private set; }
    public bool Loaded { get; private set; }
    
    public string Version { get; } = "0.9.2-dev";

    private SpriteFont _font = null!;
    
    private readonly Color _overlayColour = new Color(40, 56, 24);
    private readonly Color _textColour = new Color(176, 192, 160);

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

        Loading = true;
        Loaded = false;
        
        ContentData = new ContentLoader();
    }

    public void UpdateOptions()
    {
        Options = Xml.Deserialise<OptionData>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "OptionData.xml"));
    }

    public void UpdateOptions(OptionData newOptions)
    {
        Xml.Serialise(newOptions, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "OptionData.xml"));
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
        SoundEffect.MasterVolume = Options.Volume;
        effect.Play();
    }

    private void LoadAllAssets()
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Sprites");
        string[] dirs = Directory.GetDirectories(path);
        foreach (string dir in dirs)
        {
            if (Path.GetFileName(dir) == "Fonts") continue; // Why is fonts in fucking Sprites?
            
            if (Path.GetFileName(dir) is "Ground" or "Bushes" or "Grass" or "Clouds")
            {
                string[] specialFiles = Directory.GetFiles(dir);
                List<Texture2D> textures = [];
                textures.AddRange(specialFiles.Select(file => 
                    Path
                        .Combine("Sprites", Path.GetFileName(dir), Path.GetFileNameWithoutExtension(file)))
                        .Select(loaderPath => Content.Load<Texture2D>(loaderPath))
                );

                ContentData.SpecialTextures.Add(Path.GetFileName(dir), textures);
                
                continue;
            }

            string[] files = Directory.GetFiles(dir);
            foreach (string file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                string loaderPath = Path.Combine("Sprites", Path.GetFileName(dir), name);
                ContentData.Set(name, Content.Load<Texture2D>(loaderPath));
            }
        }
    }

    private async void LoadGameDataAsync()
    {
        UpdateOptions();
        if (Options?.FullScreen == true)
        {
            ToggleFullScreen();
        }
        SetKeyBinds();
        
        await Task.Run(LoadAllAssets);
        
        Context.SwitchState(new MainMenu(this));

        Loading = false;
        Loaded = true;
    }
    
    protected override void Initialize()
    {
        base.Initialize();

        Window.Title = "Winged Hazards";
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>("Sprites/Fonts/File");
       
        LoadGameDataAsync();
    }

    protected override void Update(GameTime gameTime)
    {
        if (Loaded)
        {
            MousePosition = _renderer.GetVirtualMousePosition();
            Context.Update(gameTime);
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        InputManager.GetState();

        if (Loading)
        {
            _renderer.SetRenderer();
            GraphicsDevice.Clear(_overlayColour);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                Vector2 firstMeasurements = _font.MeasureString("Winged Hazards");
                Vector2 titlePos = new Vector2(
                    (GameSize.X - firstMeasurements.X) / 2,
                    (GameSize.Y - firstMeasurements.Y) / 2 - 6
                );

                Vector2 secondMeasurements = _font.MeasureString("itsEve");
                Vector2 namePos = new Vector2(
                    (GameSize.X - secondMeasurements.X) / 2,
                    (GameSize.Y - secondMeasurements.Y) / 2 + 2
                );

                Vector2 loadingMeasurements = _font.MeasureString("loading...");
                Vector2 loadingPos = new Vector2(
                    1, (GameSize.Y - loadingMeasurements.Y)
                );
                
                _spriteBatch.DrawString(_font, "Winged Hazards", titlePos, _textColour);
                _spriteBatch.DrawString(_font, "itsEve", namePos, _textColour);
                
                _spriteBatch.DrawString(_font, "loading...", loadingPos, _textColour);
            _spriteBatch.End();
            
            _renderer.DrawRenderer(_spriteBatch);
        }

        if (Loaded)
        {
            _renderer.SetRenderer();
            Context.Draw(gameTime, _spriteBatch);
    
            _renderer.DrawRenderer(_spriteBatch);
        }
        
        base.Draw(gameTime);
    }
}
