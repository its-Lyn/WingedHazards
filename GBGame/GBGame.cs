using Microsoft.Xna.Framework.Input;

namespace GBGame;

public static class GBGame
{
    public static Keys KeyboardRight;
    public static Keys KeyboardLeft;
    public static Keys KeyboardInventoryUp;
    public static Keys KeyboardInventoryDown;

    public static Keys KeyboardJump;
    public static Keys KeyboardAction;

    public static Keys KeyboardPause;

    public static Buttons ControllerRight;
    public static Buttons ControllerLeft;
    public static Buttons ControllerInventoryUp;
    public static Buttons ControllerInventoryDown;

    public static Buttons ControllerJump;
    public static Buttons ControllerAction;
    
    public static Buttons ControllerPause;

    public static void Main()
    {
        using GameWindow game = new GameWindow();
        game.Run();
    }
}