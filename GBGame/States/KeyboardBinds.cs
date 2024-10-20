using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGayme.Components;
using MonoGayme.Controllers;
using MonoGayme.States;
using MonoGayme.UI;
using MonoGayme.Utilities;

namespace GBGame.States;

public class KeyboardBinds(GameWindow window) : State
{
    private readonly Color _backDrop = new Color(232, 240, 223);

    private readonly UIController _controller = new UIController(true);
    private readonly Color _overlayColour = new Color(40, 56, 24);
    private readonly Color _textColour = new Color(176, 192, 160);
    private TextButton? _btn;
    private bool _canPick;
    private bool _updateController;

    private KeyPick _current;

    private SpriteFont _font = null!;

    private bool _picking;
    private Shapes _shapes = null!;
    
    private Timer _errorTimer = null!;
    private bool _showError;

    private void SetKey(Button btn, KeyPick pick)
    {
        _picking = true;

        TextButton self = (TextButton)btn;
        _btn = self;
        _current = pick;

        self.SetText($"{_current}: picking...");
    }
    
    private bool IsKeyAlreadyAssigned(Keys newKey)
    {
        return GBGame.KeyboardLeft == newKey ||
               GBGame.KeyboardRight == newKey ||
               GBGame.KeyboardInventoryUp == newKey ||
               GBGame.KeyboardInventoryDown == newKey ||
               GBGame.KeyboardJump == newKey ||
               GBGame.KeyboardAction == newKey ||
               GBGame.KeyboardPause == newKey;
    }    

    private void HandlePick(KeyPick key)
    {
        Keys? newKey = InputManager.GetFirstKey();

        if (newKey is null) return;

        if (IsKeyAlreadyAssigned(newKey.Value))
        {
            _showError = true;
            _errorTimer.Start();
            
            return;
        }

        switch (key)
        {
            case KeyPick.Left:
                GBGame.KeyboardLeft = newKey.Value;
                break;
            case KeyPick.Right:
                GBGame.KeyboardRight = newKey.Value;
                break;
            case KeyPick.Up:
                GBGame.KeyboardInventoryUp = newKey.Value;
                break;
            case KeyPick.Down:
                GBGame.KeyboardInventoryDown = newKey.Value;
                break;
            case KeyPick.Jump:
                GBGame.KeyboardJump = newKey.Value;
                break;
            case KeyPick.Action:
                GBGame.KeyboardAction = newKey.Value;
                break;
            case KeyPick.Pause:
                GBGame.KeyboardPause = newKey.Value;
                break;
        }

        _picking = false;
        _canPick = false;
        
        _btn?.SetText($"{key}: {newKey}");

        window.UpdateKeys();
    }

    public override void LoadContent()
    {
        window.UpdateOptions();

        _shapes = new Shapes(window.GraphicsDevice);

        _errorTimer = new Timer(0.8f, false, true)
        {
            OnTimeOut = () => _showError = false
        };
        
        SoundEffect click = window.Content.Load<SoundEffect>("Sounds/Click");
        _controller.OnActiveUpdating = btn => btn.Colour = _textColour;
        _controller.OnActiveUpdated = btn => {
            btn.Colour = _overlayColour;
            window.PlayEffect(click);
        };

        _controller.SetKeyboardButtons(GBGame.KeyboardInventoryUp, GBGame.KeyboardInventoryDown, GBGame.KeyboardAction);
        _controller.SetControllerButtons(GBGame.ControllerInventoryUp, GBGame.ControllerInventoryDown, GBGame.ControllerAction);

        _font = window.Content.Load<SpriteFont>("Sprites/Fonts/File");
        TextButton ret = new TextButton(_font, "return", new Vector2((window.GameSize.X - _font.MeasureString("return").X) / 2, window.GameSize.Y - 12), _textColour, true)
        {
            OnClick = _ =>
            {
                window.UpdateOptions();
                window.Context.SwitchState(new Options(window));
            }
        };

        _controller.AddIgnored(new Label("keyboard", _overlayColour, _font, new Vector2((window.GameSize.X - _font.MeasureString("keyboard").X) / 2, 5)));

        TextButton left = new TextButton(_font, $"left: {window.Options.Keyboard.Left}", new Vector2(1, 20), _textColour, true) {
            OnClick = btn => { SetKey(btn, KeyPick.Left); }
        };

        TextButton right = new TextButton(_font, $"right: {window.Options.Keyboard.Right}", new Vector2(1, 30), _textColour, true) {
            OnClick = btn => { SetKey(btn, KeyPick.Right); }
        };

        TextButton up = new TextButton(_font, $"up: {window.Options.Keyboard.InventoryUp}", new Vector2(1, 40), _textColour, true) {
            OnClick = btn => { SetKey(btn, KeyPick.Up); }
        };
        
        TextButton down = new TextButton(_font, $"down: {window.Options.Keyboard.InventoryDown}", new Vector2(1, 50), _textColour, true) {
            OnClick = btn => { SetKey(btn, KeyPick.Down); }
        };
        
        TextButton jump = new TextButton(_font, $"jump: {window.Options.Keyboard.Jump}", new Vector2(1, 70), _textColour, true) {
            OnClick = btn => { SetKey(btn, KeyPick.Jump); }
        };
        
        TextButton action = new TextButton(_font, $"action: {window.Options.Keyboard.Action}", new Vector2(1, 80), _textColour, true) {
            OnClick = btn => { SetKey(btn, KeyPick.Action); }
        };

        TextButton pause = new TextButton(_font, $"pause: {window.Options.Keyboard.Pause}", new Vector2(1, 100), _textColour, true) {
            OnClick = btn => { SetKey(btn, KeyPick.Pause); }
        };
        
        _controller.Add(left);
        _controller.Add(right);
        _controller.Add(up);
        _controller.Add(down);

        _controller.Add(jump);
        _controller.Add(action);

        _controller.Add(pause);
        
        _controller.Add(ret);
    }

    public override void Update(GameTime time)
    {
        if (_updateController)
        {
            _controller.SetKeyboardButtons(GBGame.KeyboardInventoryUp, GBGame.KeyboardInventoryDown, GBGame.KeyboardAction);
            _updateController = false;
        }
        
        if (_picking)
        {
            if (!_canPick)
            {
                Keys? first = InputManager.GetFirstKey();
                if (first is null)
                    _canPick = true;
            }

            if (_canPick)
            {
                HandlePick(_current);
                _updateController = true;
            }
        }

        if (!_picking && !_showError)
            _controller.Update(window.MousePosition);
        
        _errorTimer.Cycle(time);
    }

    public override void Draw(GameTime time, SpriteBatch batch)
    {
        window.GraphicsDevice.Clear(_backDrop);
        batch.Begin(samplerState: SamplerState.PointClamp);
            _shapes.DrawRectangle(new Rectangle(0, 0, (int)window.GameSize.X, (int)window.GameSize.Y), _overlayColour, batch, 0.6f);

            _controller.Draw(batch);

            if (_showError)
            {
                Vector2 measurements = _font.MeasureString("Already assigned!");
                Vector2 pos = new Vector2(
                    (window.GameSize.X - measurements.X) / 2,
                    (window.GameSize.Y - measurements.Y) / 2
                );
                
                _shapes.DrawRectangle(new Rectangle(0, 0, (int)window.GameSize.X, (int)window.GameSize.Y), _textColour, batch, 0.6f);
                batch.DrawString(_font, "Already assigned!", pos, _overlayColour);
            }
        batch.End();
    }

    private enum KeyPick
    {
        Left, Right, Up, Down,

        Jump, Action,

        Pause
    }
}