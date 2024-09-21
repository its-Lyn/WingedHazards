using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGayme.States;
using System.Collections.Generic;
using System;
using MonoGayme.Components;
using MonoGayme.Utilities;
using Microsoft.Xna.Framework.Input;
using GBGame.Entities;

namespace GBGame.States;

public class InGame(GameWindow windowData) : State(windowData)
{
    private record struct GroundTile(Texture2D Sprite, int X, int Y);

    private List<Texture2D> _ground = [];
    private List<GroundTile> _groundTiles = [];

    private readonly int TileSize = 8;
    private readonly Color BackDrop = new Color(232, 240, 223);

    private Camera2D _camera = new Camera2D(Vector2.Zero);
    private float _cameraOffset = 40;
    private EntityController _controller = new EntityController();

    private float _groundLine;
    private int _gameWidth;

    private Texture2D _island = null!;

    public override void LoadContent()
    {
        for (int i = 1; i <= 3; i++) 
        {
            _ground.Add(WindowData.Content.Load<Texture2D>($"Sprites/Ground/Ground_{i}"));
        }

        GameWindow window = (GameWindow)WindowData;

        _gameWidth = (int)(window.GameSize.Y * 2);
        int tileCount = (int)(window.GameSize.Y - TileSize);

        int basePosition = 0;
        
        for (int i = 0; i < _gameWidth / TileSize; i++)
        {
            // Get a random ground tile
            Texture2D tile = _ground[Random.Shared.Next(0, 2)]; // 0, 1 range
            _groundTiles.Add(new GroundTile(tile, basePosition, tileCount - TileSize));

            // Add the plain ground below, so we get a set of 2.
            _groundTiles.Add(new GroundTile(_ground[2], basePosition, tileCount));

            basePosition += TileSize;
        }

        _groundLine = tileCount - TileSize - TileSize / 2; // TileSize / 2 is the player width origin.

        _island = WindowData.Content.Load<Texture2D>("Sprites/BackGround/Island");

        Player player = new Player(WindowData);
        player.Position.Y = _groundLine;
        _controller.AddEntity(player);
    }

    public override void Update(GameTime time)
    {
        _controller.UpdateEntities(WindowData.GraphicsDevice, time);

        Player? player = _controller.GetFirst<Player>();
        if (player is null) {
            Console.Error.WriteLine("This isn't supposed to happen... (The player is missing.)");
            return;
        }

        GameWindow window = (GameWindow)WindowData;

        // Keep the camera position between the game sizes, so the player doesn't see outside the map.
        _camera.X = Math.Clamp(MathF.Floor(player.Position.X - _cameraOffset), 0, _gameWidth - window.GameSize.X);

        if (player.Position.Y > _groundLine) 
        {
            player.Velocity.Y = 0;
            player.Position.Y = _groundLine;

            player.IsOnFloor = true;
        }
    }
   
    public override void Draw(GameTime time, SpriteBatch batch)
    {
        WindowData.GraphicsDevice.Clear(BackDrop);
        batch.Begin(transformMatrix: _camera.Transform);
            batch.Draw(_island, _camera.ScreenToWorld(new Vector2(0, -10)), Color.White * 0.6f);

            foreach (GroundTile tile in _groundTiles) 
            {
                batch.Draw(tile.Sprite, new Vector2(tile.X, tile.Y), Color.White);    
            }

            _controller.DrawEntities(batch, time);
        batch.End();
    }
}
