using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GBGame.Items;

public abstract class Item
{
    public Texture2D? InventorySprite;
    
    public string? Name;
    public string? Description;

    public abstract void LoadContent();
    public abstract void Use();
}
