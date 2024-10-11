using Microsoft.Xna.Framework.Input;

namespace GBGame;

public static class GBGame
{
    public const Keys KeyboardRight = Keys.D;
    public const Keys KeyboardLeft = Keys.A;
    public const Keys KeyboardInventoryUp = Keys.W;
    public const Keys KeyboardInventoryDown = Keys.S;

    public const Keys KeyboardJump = Keys.Space;
    public const Keys KeyboardAction = Keys.LeftShift;

    public const Buttons ControllerRight = Buttons.DPadRight;
    public const Buttons ControllerLeft = Buttons.DPadLeft;
    public const Buttons ControllerInventoryUp = Buttons.DPadUp;
    public const Buttons ControllerInventoryDown = Buttons.DPadDown;

    public const Buttons ControllerJump = Buttons.B;
    public const Buttons ControllerAction = Buttons.A;

    public static void Main()
    {
        using GameWindow game = new GameWindow();
        game.Run();
    }
}