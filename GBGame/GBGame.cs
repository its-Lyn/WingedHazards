using Microsoft.Xna.Framework.Input;

namespace GBGame;

public static class GBGame 
{
    public static Keys KeyboardRight = Keys.D;
    public static Keys KeyboardLeft = Keys.A;
    public static Keys KeyboardInventoryUp = Keys.W;
    public static Keys KeyboardInventoryDown = Keys.S;

    public static Keys KeyboardJump = Keys.Space;
    public static Keys KeyboardAction = Keys.LeftShift;

    public static Buttons ControllerRight = Buttons.DPadRight;
    public static Buttons ControllerLeft = Buttons.DPadLeft;
    public static Buttons ControllerInventoryUp = Buttons.DPadUp;
    public static Buttons ControllerInventoryDown = Buttons.DPadDown;

    public static Buttons ControllerJump = Buttons.B;
    public static Buttons ControllerAction = Buttons.A;

    public static void Main()
    {
        using var game = new GameWindow();
        game.Run();
    }
}
