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

public class GamePadBinds(GameWindow window) : State
{
    private readonly Color _backDrop = new Color(232, 240, 223);

    private readonly UIController _controller = new UIController(true);
    private readonly Color _overlayColour = new Color(40, 56, 24);
    private readonly Color _textColour = new Color(176, 192, 160);
    private TextButton? _btn;
    private bool _canPick;
    private bool _updateController;

    private ButtonPick _current;

    private SpriteFont _font = null!;

    private bool _picking;
    private Shapes _shapes = null!;
    
    private Timer _errorTimer = null!;
    private bool _showError;

    private void SetButton(Button btn, ButtonPick pick)
    {
        _picking = true;

        TextButton self = (TextButton)btn;
        _btn = self;
        _current = pick;

        self.SetText($"{_current}: picking...");
    }
    
    private bool IsButtonAlreadyAssigned(Buttons newKey)
    {
        return GBGame.ControllerLeft == newKey ||
               GBGame.ControllerRight == newKey ||
               GBGame.ControllerInventoryUp == newKey ||
               GBGame.ControllerInventoryDown == newKey ||
               GBGame.ControllerJump == newKey ||
               GBGame.ControllerAction == newKey ||
               GBGame.ControllerPause == newKey;
    }    

    private void HandlePick(ButtonPick button)
    {
        Buttons? newKey = InputManager.GetFirstButton();

        if (newKey is null) return;

        if (IsButtonAlreadyAssigned(newKey.Value))
        {
            _showError = true;
            _errorTimer.Start();
            
            return;
        }

        switch (button)
        {
            case ButtonPick.Left:
                GBGame.ControllerLeft = newKey.Value;
                break;
            case ButtonPick.Right:
                GBGame.ControllerRight = newKey.Value;
                break;
            case ButtonPick.Up:
                GBGame.ControllerInventoryUp = newKey.Value;
                break;
            case ButtonPick.Down:
                GBGame.ControllerInventoryDown = newKey.Value;
                break;
            case ButtonPick.Jump:
                GBGame.ControllerJump = newKey.Value;
                break;
            case ButtonPick.Action:
                GBGame.ControllerAction = newKey.Value;
                break;
            case ButtonPick.Pause:
                GBGame.ControllerPause = newKey.Value;
                break;
        }

        _picking = false;
        _canPick = false;
        
        _btn?.SetText($"{button}: {newKey}");

        window.UpdateButtons();
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

        _controller.AddIgnored(new Label("gamepad", _overlayColour, _font, new Vector2((window.GameSize.X - _font.MeasureString("gamepad").X) / 2, 5)));

        TextButton left = new TextButton(_font, $"left: {window.Options.GamePad.Left}", new Vector2(1, 20), _textColour, true) {
            OnClick = btn => { SetButton(btn, ButtonPick.Left); }
        };

        TextButton right = new TextButton(_font, $"right: {window.Options.GamePad.Right}", new Vector2(1, 30), _textColour, true) {
            OnClick = btn => { SetButton(btn, ButtonPick.Right); }
        };

        TextButton up = new TextButton(_font, $"up: {window.Options.GamePad.InventoryUp}", new Vector2(1, 40), _textColour, true) {
            OnClick = btn => { SetButton(btn, ButtonPick.Up); }
        };
        
        TextButton down = new TextButton(_font, $"down: {window.Options.GamePad.InventoryDown}", new Vector2(1, 50), _textColour, true) {
            OnClick = btn => { SetButton(btn, ButtonPick.Down); }
        };
        
        TextButton jump = new TextButton(_font, $"jump: {window.Options.GamePad.Jump}", new Vector2(1, 70), _textColour, true) {
            OnClick = btn => { SetButton(btn, ButtonPick.Jump); }
        };
        
        TextButton action = new TextButton(_font, $"action: {window.Options.GamePad.Action}", new Vector2(1, 80), _textColour, true) {
            OnClick = btn => { SetButton(btn, ButtonPick.Action); }
        };

        TextButton pause = new TextButton(_font, $"pause: {window.Options.GamePad.Pause}", new Vector2(1, 100), _textColour, true) {
            OnClick = btn => { SetButton(btn, ButtonPick.Pause); }
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
            _controller.SetControllerButtons(GBGame.ControllerInventoryUp, GBGame.ControllerInventoryDown, GBGame.ControllerAction);
            _updateController = false;
        }
        
        if (_picking)
        {
            if (!_canPick)
            {
                Buttons? first = InputManager.GetFirstButton();
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

    private enum ButtonPick
    {
        Left, Right, Up, Down,

        Jump, Action,

        Pause
    }
}