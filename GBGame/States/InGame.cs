using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGayme.States;
using System.Collections.Generic;
using System;
using MonoGayme.Components;
using GBGame.Entities;
using GBGame.Components;
using GBGame.Items;
using MonoGayme.Utilities;
using Microsoft.Xna.Framework.Input;
using MonoGayme.UI;
using GBGame.Entities.Enemies;
using MonoGayme.Entities.Colliders;

namespace GBGame.States;

public class InGame(GameWindow windowData) : State(windowData)
{
    private record struct GroundTile(Texture2D Sprite, int X, int Y);

    private List<Texture2D> _grass = [];
    private List<Texture2D> _ground = [];
    private List<GroundTile> _groundTiles = [];

    private readonly int TileSize = 8;
    private readonly Color BackDrop = new Color(232, 240, 223);

    private Camera2D _camera = new Camera2D(Vector2.Zero);
    private float _cameraOffset = 40;

    private EntityController _controller = new EntityController();
    private EntityController _enemyController = new EntityController();

    private int _groundLine;
    private int _gameWidth;

    private Texture2D _island = null!;

    private Inventory _inventory = new Inventory();
    private Pause _pause = null!;
    private AnimatedSpriteSheet _sheet = null!;

    private Bomb _bomb = null!;

    private Vector2 _shakeOffset;
    private bool _shaking = false;
    private float _intensity = 0;
    private float _shakeDuration = 0.2f;
    private float _shakeMagnitude = 3f;

    private Shapes _shapes = null!;

    private bool _striking = false;
    private Rectangle _strikeCollider;

    private Player _player = null!;

    private void SetupGround(int tileCountX, int tileCountY)
    { 
        int basePosition = 0;
        for (int i = 0; i < _gameWidth / TileSize; i++)
        {
            // Get a random ground tile
            Texture2D tile = _ground[Random.Shared.Next(0, 3)]; // 0, 2 range
            _groundTiles.Add(new GroundTile(tile, basePosition, tileCountY - TileSize));

            // Add the plain ground below, so we get a set of 2.
            _groundTiles.Add(new GroundTile(_ground[3], basePosition, tileCountY));

            basePosition += TileSize;
        }

        int grassCount = Random.Shared.Next(5, 10);
        HashSet<int> usedGridPositions = new HashSet<int>();
        for (int i = 0; i < grassCount; i++) 
        {
            Texture2D tile = _grass[Random.Shared.Next(0, 2)];

            // Get a random position on the grid
            int gridX;
            do gridX = Random.Shared.Next(0, tileCountX);
            while (usedGridPositions.Contains(gridX));

            usedGridPositions.Add(gridX);

            _groundTiles.Add(new GroundTile(tile, gridX * TileSize, tileCountY - TileSize * 2));
        }  
    }

    private void ShakeCamera(GameTime time)
    { 
        if (_shaking)
        {
            _intensity -= (float)time.ElapsedGameTime.TotalSeconds / _shakeDuration;
            if (_intensity <= 0) 
            {
                _shaking = false;
                _shakeOffset = Vector2.Zero;
                return;
            }

            float rot = Random.Shared.NextSingle() * MathF.Tau;
            _shakeOffset = new Vector2(MathF.Cos(rot), MathF.Sin(rot)) * _shakeMagnitude * _intensity;
        }
    }

    private void HandleInventoryInput()
    { 
        if (InputManager.IsKeyPressed(GBGame.KeyboardInventoryUp) || InputManager.IsGamePadPressed(GBGame.ControllerInventoryUp))
            _inventory.ActiveItemIndex--;

        if (InputManager.IsKeyPressed(GBGame.KeyboardInventoryDown) || InputManager.IsGamePadPressed(GBGame.ControllerInventoryDown))
            _inventory.ActiveItemIndex++;

        if (InputManager.IsKeyPressed(GBGame.KeyboardAction) || InputManager.IsGamePadPressed(GBGame.ControllerAction))
            _inventory.UseActive();
    }

    private void StartShake(float intensity, float magnitude)
    { 
        _shaking = true;

        _shakeMagnitude = magnitude;
        _intensity = intensity;
    }

    public override void LoadContent()
    {
        for (int i = 1; i <= 4; i++) 
            _ground.Add(WindowData.Content.Load<Texture2D>($"Sprites/Ground/Ground_{i}"));
        
        for (int i = 1; i <= 2; i++)
            _grass.Add(WindowData.Content.Load<Texture2D>($"Sprites/Grass/Grass_{i}"));

        GameWindow window = (GameWindow)WindowData;

        _gameWidth = (int)(window.GameSize.Y * 2);

        int tileCountY = (int)(window.GameSize.Y - TileSize);
        int tileCountX = _gameWidth / TileSize; 
        SetupGround(tileCountX, tileCountY);

        // TileSize / 2 is the player width origin.
        _groundLine = tileCountY - TileSize - TileSize / 2;

        _player = new Player(WindowData);
        _player.Position.Y = _groundLine;
        _controller.AddEntity(_player);

        _island = WindowData.Content.Load<Texture2D>("Sprites/BackGround/Island");

        _sheet = new AnimatedSpriteSheet(WindowData.Content.Load<Texture2D>("Sprites/Strike"), new Vector2(6, 1), 0.02f);
        _sheet.OnSheetFinished = () => { 
            _striking = false;
        };
        
        _inventory.LoadContent(WindowData);
        _inventory.AddItem(new Sword(WindowData, _sheet, _player));

        _bomb = new Bomb(WindowData, _player);
        _inventory.AddItem(_bomb);

        _bomb.Sheet.OnSheetFinished = () => { 
            _bomb.CanPlace = true;

            StartShake(1, 3);
            _shakeOffset = Vector2.Zero;
        };

        _pause = new Pause(window);

        Bat bat = new Bat(window, new Vector2(50, 50));
        bat.Lock(_player);

        _enemyController.AddEntity(bat);
        _enemyController.OnEntityUpdate = (device, time, entity) => {
            if (entity is IRectCollider collider)
            {
                if (_striking && Collision.CheckRects(_strikeCollider, collider.Collider))
                {
                    StartShake(1, 1.5f);
                    _enemyController.QueueRemove(entity);
                }
            }
        };

        _shapes = new Shapes(window.GraphicsDevice);
    }

    public override void Update(GameTime time)
    {
        if (InputManager.IsGamePadPressed(Buttons.Start) || InputManager.IsKeyPressed(Keys.Escape))
            _pause.Paused = !_pause.Paused;

        if (_pause.Paused)
        {
            _pause.Update();
            return;
        }

        // Update controllers.
        _enemyController.UpdateEntities(WindowData.GraphicsDevice, time);
        _controller.UpdateEntities(WindowData.GraphicsDevice, time);

        // Hardcoded ground checking (we don't need anything more complicated.)
        if (_player.Position.Y > _groundLine) 
        {
            _player.Velocity.Y = 0;
            _player.Position.Y = _groundLine;

            _player.IsOnFloor = true;
        }

        // Keep the camera position between the game sizes, so the _player doesn't see outside the map.
        GameWindow window = (GameWindow)WindowData;
        _camera.X = Math.Clamp(MathF.Floor(_player.Position.X - _cameraOffset + _shakeOffset.X), 0, _gameWidth - window.GameSize.X);
        _camera.Y = _shakeOffset.Y; 

        HandleInventoryInput();

        if (!_sheet.Done) _sheet.CycleAnimation(time);
        if (!_bomb.Sheet.Done) _bomb.Sheet.CycleAnimation(time);

        ShakeCamera(time);
    }
   
    public override void Draw(GameTime time, SpriteBatch batch)
    {
        WindowData.GraphicsDevice.Clear(BackDrop);

        batch.Begin(transformMatrix: _camera.Transform);
            // Draw the background
            batch.Draw(_island, _camera.ScreenToWorld(new Vector2(0, -10)), Color.White * 0.4f);

            foreach (GroundTile tile in _groundTiles) 
                batch.Draw(tile.Sprite, new Vector2(tile.X, tile.Y), Color.White);

            _enemyController.DrawEntities(batch, time);
            _controller.DrawEntities(batch, time); 

            if (!_sheet.Done)
            {
                Vector2 strikePosition = new Vector2(
                    _player.FacingRight ? _player.Position.X + 4 : _player.Position.X - 12,
                    _player.Position.Y - 4
                );

                _strikeCollider.X = (int)strikePosition.X + 2;
                _strikeCollider.Y = (int)strikePosition.Y;
                _strikeCollider.Width = 4;
                _strikeCollider.Height = 8;

                _striking = true;

                _sheet.Draw(batch, strikePosition, !_player.FacingRight);
            }

            // Draw the sprite sheets.
            if(!_bomb.Sheet.Done)
                _bomb.Draw(batch);

            if (_pause.Paused)
                _pause.Draw(batch, _camera);

            // Draw the inventory on top of everything else.
            _inventory.Draw(batch, _camera);

            // Test
            if (_striking)
            {
                batch.Draw(_grass[0], Vector2.Zero, Color.White);
            }
        batch.End();
    }
}
