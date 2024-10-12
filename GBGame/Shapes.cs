using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GBGame;

public class Shapes
{
    private readonly Texture2D _pixel;

    public Shapes(GraphicsDevice device)
    {
        _pixel = new Texture2D(device, 1, 1);
        _pixel.SetData([Color.White]);
    }

    public void DrawRectangleLines(float x, float y, float width, float height, Color colour, SpriteBatch batch, float alpha = 1)
    {
        // Top
        batch.Draw(_pixel, new Rectangle((int)x, (int)y, (int)width, 1), colour * alpha);

        // Left
        batch.Draw(_pixel, new Rectangle((int)x, (int)y, 1, (int)height), colour * alpha);

        // Right
        batch.Draw(_pixel, new Rectangle((int)(x + width - 1), (int)y, 1, (int)height), colour * alpha);

        // Bottom
        batch.Draw(_pixel, new Rectangle((int)x, (int)(y + height - 1), (int)width, 1), colour * alpha);
    }

    public void DrawRectangle(Rectangle rect, Color colour, SpriteBatch batch, float alpha = 1)
        => batch.Draw(_pixel, rect, colour * alpha);

    public void DrawRectangleMinimal(Rectangle rect, Color colour, int size, SpriteBatch batch)
    {
        // Top
        batch.Draw(_pixel, new Rectangle(rect.X, rect.Y, size, 1), colour);
        batch.Draw(_pixel, new Rectangle(rect.X + rect.Width - size, rect.Y, size, 1), colour);

        // Left
        batch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 1, size), colour);
        batch.Draw(_pixel, new Rectangle(rect.X, rect.Y + rect.Height - size, 1, size), colour);

        // Right
        batch.Draw(_pixel, new Rectangle(rect.X + rect.Width - 1, rect.Y, 1, size), colour);
        batch.Draw(_pixel, new Rectangle(rect.X + rect.Width - 1, rect.Y + rect.Height - size, 1, size), colour);

        // Bottom
        batch.Draw(_pixel, new Rectangle(rect.X, rect.Y + rect.Height - 1, size, 1), colour);
        batch.Draw(_pixel, new Rectangle(rect.X + rect.Width - size, rect.Y + rect.Height - 1, size, 1), colour);
    }

    public void DrawRectangleLines(Rectangle rect, Color colour, SpriteBatch batch, float alpha = 1)
        => DrawRectangleLines(rect.X, rect.Y, rect.Width, rect.Height, colour, batch, alpha);
}
