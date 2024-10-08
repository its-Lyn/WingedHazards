using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGayme.Components;
using MonoGayme.Utilities;

namespace GBGame.Components;

public class GamePadVisualiser : Component
{
    private readonly Texture2D _pixel;

    private record struct DPadButton(Rectangle Bounds, Color Colour);

    private DPadButton _dpadUp;
    private DPadButton _dpadDown;

    private DPadButton _dpadLeft;
    private DPadButton _dpadRight;

    private DPadButton _dpadA;
    private DPadButton _dpadB;

    private readonly Color _dpadPressed = new Color(96, 112, 80);
    private readonly Color _dpadNormal = new Color(176, 192, 160);  

    public GamePadVisualiser(GameWindow window)
    {
        _pixel = new Texture2D(window.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        int width = 6;
        int height = 7;
        int offset = 2;

        int btnSize = 6;

        _dpadUp = new DPadButton(new Rectangle((int)window.GameSize.X - (height * 2) - offset, offset - 1, width, height - 2), _dpadNormal);
        _dpadDown = new DPadButton(new Rectangle((int)window.GameSize.X - (height * 2) - offset, offset + height + 3, width, height - 2), _dpadNormal);

        _dpadLeft = new DPadButton(new Rectangle((int)window.GameSize.X - (width * 3) - offset - 1, height - 1, height - 2, width), _dpadNormal);
        _dpadRight = new DPadButton(new Rectangle((int)window.GameSize.X - (width * 2) + offset, height - 1, height - 2, width), _dpadNormal);

        _dpadA = new DPadButton(new Rectangle((int)window.GameSize.X - (width * 3) - (offset * 2) - 10 + 2, offset, btnSize, btnSize), _dpadNormal);
        _dpadB = new DPadButton(new Rectangle((int)window.GameSize.X - (width * 3) - (offset * 2) - 10 - 2, offset + btnSize + 1, btnSize, btnSize), _dpadNormal);
    }

    private Rectangle HandleRectCam(Rectangle rect, Camera2D? camera)
    { 
        if (camera is null) return rect;

        Vector2 relativePos = camera.ScreenToWorld(new Vector2(rect.X, rect.Y));
        rect.X = (int)relativePos.X;
        rect.Y = (int)relativePos.Y;

        return rect;
    }

    private void HandleDPadButton(ref DPadButton btn, Keys key, Buttons dpad)
    {
        if (InputManager.IsKeyDown(key) || InputManager.IsGamePadDown(dpad))
            btn.Colour = _dpadPressed;
        else if (InputManager.IsKeyUp(key) || InputManager.IsGamePadUp(dpad))
            btn.Colour = _dpadNormal;
    }

    public void Update(GameTime time)
    {
        HandleDPadButton(ref _dpadUp, GBGame.KeyboardInventoryUp, GBGame.ControllerInventoryUp);
        HandleDPadButton(ref _dpadDown, GBGame.KeyboardInventoryDown, GBGame.ControllerInventoryDown);

        HandleDPadButton(ref _dpadLeft, GBGame.KeyboardLeft, GBGame.ControllerLeft);
        HandleDPadButton(ref _dpadRight, GBGame.KeyboardRight, GBGame.ControllerRight);

        HandleDPadButton(ref _dpadA, GBGame.KeyboardJump, GBGame.ControllerJump);
        HandleDPadButton(ref _dpadB, GBGame.KeyboardAction, GBGame.ControllerAction);
    }

    public void Draw(SpriteBatch batch, Camera2D? camera = null)
    {
        batch.Draw(_pixel, HandleRectCam(_dpadUp.Bounds, camera), _dpadUp.Colour);
        batch.Draw(_pixel, HandleRectCam(_dpadDown.Bounds, camera), _dpadDown.Colour);
    
        batch.Draw(_pixel, HandleRectCam(_dpadLeft.Bounds, camera), _dpadLeft.Colour);
        batch.Draw(_pixel, HandleRectCam(_dpadRight.Bounds, camera), _dpadRight.Colour);

        batch.Draw(_pixel, HandleRectCam(_dpadA.Bounds, camera), _dpadA.Colour);
        batch.Draw(_pixel, HandleRectCam(_dpadB.Bounds, camera), _dpadB.Colour);
    }
}
