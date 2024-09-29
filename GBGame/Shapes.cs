using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GBGame;

public class Shapes
{
    private readonly Texture2D _pixel;

    public Shapes(GraphicsDevice device)
    {
        _pixel = new Texture2D(device, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void DrawRectangleLines(float x, float y, float width, float height, Color colour, SpriteBatch batch)
    {
        // Top
        batch.Draw(_pixel, new Rectangle((int)x, (int)y, (int)width, 1), colour);

        // Left
        batch.Draw(_pixel, new Rectangle((int)x, (int)y, 1, (int)height), colour);

        // Right
        batch.Draw(_pixel, new Rectangle((int)(x + width - 1), (int)y, 1, (int)height), colour);

        // Bottom
        batch.Draw(_pixel, new Rectangle((int)x, (int)(y + height - 1), (int)width, 1), colour);
    }

    public void DrawRectangleLines(float x, float y, float width, float height, float thickness, Color colour, SpriteBatch batch)
        => DrawRectangleLines(x * thickness, y * thickness, width * thickness, height * thickness, colour, batch);

    public void DrawRectangleLines(Rectangle rect, Color colour, SpriteBatch batch)
        => DrawRectangleLines(rect.X, rect.Y, rect.Width, rect.Height, colour, batch);
}
