using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Components;
using MonoGayme.Extensions;

namespace GBGame.Components;

public class Clouds : Component
{
    private readonly GameWindow _window;
    
    private record Cloud(Texture2D Sprite, Vector2 Position, float Speed, float Opacity);
    private readonly List<Cloud> _clouds = [];

    public Clouds(GameWindow window, int minCount, int maxCount, int minSpawnY, int maxSpawnY)
    {
        _window = window;
        
        List<Texture2D> sprites = window.ContentData.SpecialTextures["Clouds"];

        // Create a bunch of clouds
        for (int i = 0; i < Random.Shared.Next(minCount, maxCount + 1); i++)
        {
            Vector2 pos = new Vector2(
                Random.Shared.Next(20, (int)window.GameSize.X - 20),
                Random.Shared.Next(minSpawnY, maxSpawnY)
            );
            
            // Randomly generate speed between 0.3 and 1.2
            float speed = Random.Shared.NextSingle(0.3f, 1.2f);
            
            // Slow down the bigger clouds
            Texture2D sprite = sprites[Random.Shared.Next(sprites.Count)];
            if (sprite.Width > 8)
                speed -= 0.15f;

            // Calculate opacity based on speed; adjust range as needed
            float opacity = MathHelper.Clamp((speed - 0.3f) / (1.2f - 0.3f), 0.5f, 1.0f);
            
            _clouds.Add(new Cloud(sprite, pos, speed, opacity));
        }
    }

    public void Update()
    {
        foreach (ref Cloud cloud in CollectionsMarshal.AsSpan(_clouds))
        {
            cloud = cloud with { Position = cloud.Position with { X = cloud.Position.X + cloud.Speed } };
            if (cloud.Position.X > _window.GameSize.X + 20)
            {
                cloud = cloud with { Position = cloud.Position with { X = -20 } };
            }
        } 
    }

    public void Draw(SpriteBatch batch)
    {
        foreach (Cloud cloud in _clouds) batch.Draw(cloud.Sprite, cloud.Position, Color.White * cloud.Opacity);
    }
}