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
using GBGame.Entities.Enemies;
using MonoGayme.Controllers;
using MonoGayme.Components.Colliders;

namespace GBGame.States;

public class InGame(GameWindow windowData) : State(windowData)
{
    const int BatSpawnerHeight = -8;
    const int BatSpawnerWidth = 40;

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
    private RectCollider _strikeCollider = new RectCollider();

    private Player _player = null!;
    private RectCollider _playerCollider = null!;

    private SpriteFont _font = null!;

    private Timer _batTimer = new Timer(5, true, false);

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

                if(_bomb.Exploded)
                    _bomb.Exploded = false;

                return;
            }

            float rot = Random.Shared.NextSingle() * MathF.Tau;
            _shakeOffset = new Vector2(MathF.Cos(rot), MathF.Sin(rot)) * _shakeMagnitude * _intensity;
        }
    }
    
    private void StartShake(float intensity, float magnitude)
    { 
        _shaking = true;

        _shakeMagnitude = magnitude;
        _intensity = intensity;
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

    private void AddBat(Vector2 position)
    {
        if (Random.Shared.Next(0, 4) == 1)
        {
            ProjectileBat pbat = new ProjectileBat(WindowData, position); 
            pbat.LockOn(_player);

            _enemyController.AddEntity(pbat);
            
            return;
        }

        NormalBat bat = new NormalBat(WindowData, position); 
        bat.LockOn(_player);

        _enemyController.AddEntity(bat);

    }

    public override void LoadContent()
    {
        _strikeCollider.Bounds = new Rectangle();

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

        _player = new Player(WindowData, _camera);
        _player.Position.Y = _groundLine;
        _controller.AddEntity(_player);

        _playerCollider = _player.Components.GetComponent<RectCollider>()!;

        _island = WindowData.Content.Load<Texture2D>("Sprites/BackGround/Island");

        _sheet = new AnimatedSpriteSheet(WindowData.Content.Load<Texture2D>("Sprites/SpriteSheets/Strike"), new Vector2(6, 1), 0.02f);
        _sheet.OnSheetFinished = () => { 
            _striking = false;
        };
        
        _inventory.LoadContent(WindowData);
        _inventory.AddItem(new Sword(WindowData, _sheet, _player));

        _bomb = new Bomb(WindowData, _player);
        _inventory.AddItem(_bomb);

        _bomb.Sheet.OnSheetFinished = () => { 
            _bomb.CanPlace = true;
            _bomb.Exploded = true;

            StartShake(1, 3);
            _shakeOffset = Vector2.Zero;
        };

        _pause = new Pause(window);

        _enemyController.OnEntityUpdate = (device, time, entity) => {
            RectCollider? rect = entity.Components.GetComponent<RectCollider>("PlayerStriker");
            if (rect is null) return;

            RectCollider? playerHitter = entity.Components.GetComponent<RectCollider>("PlayerHitter");
            if (playerHitter is not null)
            {
                if (_playerCollider.Collides(playerHitter))
                {
                    _player.ApplyKnockBack(playerHitter);
                    StartShake(1, 2);
                }
            }

            if (_striking && rect.Collides(_strikeCollider))
            {
                Vector2 distance = rect.GetCentre() - _strikeCollider.GetCentre();

                Vector2 dir = Vector2.Normalize(distance);
                entity.Velocity += 2 * dir;

                rect.Enabled = false;

                // Basically "iframes" except it uses actual time.
                Timer? immunityTimer = entity.Components.GetComponent<Timer>("ImmunityTimer");
                if (immunityTimer is not null)
                {
                    if (!immunityTimer.Enabled) immunityTimer.Start();
                }

                StartShake(0.6f, 2);

                Health? health = entity.Components.GetComponent<Health>();
                if (health is null) return;

                health.HealthPoints--;
                if (health.HealthPoints <= 0) 
                {
                    _enemyController.QueueRemove(entity);
                }

                return;
            }

            if (_bomb.Exploded && rect.Collides(_bomb.KillRadius))
            {
                _enemyController.QueueRemove(entity);
                return;
            }
        };

        _batTimer.OnTimeOut = () => {
            int minPosition = (int)(_player.Position.X - BatSpawnerWidth);
            int width = minPosition + (BatSpawnerWidth * 2);
            Vector2 batPosition = new Vector2(Random.Shared.Next(minPosition, width), BatSpawnerHeight);

            AddBat(batPosition);

            // 1/4 chance to spawn two bats with one on the opposite side
            if (Random.Shared.Next(0, 4) == 1)
            {
                AddBat(batPosition with { X = 2 * _player.Position.X - batPosition.X });
            }
        };

        _shapes = new Shapes(window.GraphicsDevice);
        _font = WindowData.Content.Load<SpriteFont>("Sprites/Fonts/File");
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
        _controller.UpdateEntities(WindowData.GraphicsDevice, time);
        _enemyController.UpdateEntities(WindowData.GraphicsDevice, time);

        // Hardcoded ground checking (we don't need anything more complicated.)
        if (_player.Position.Y > _groundLine) 
        {
            _player.Velocity.Y = 0;
            _player.Position.Y = _groundLine;

            _player.IsOnFloor = true;
        }

        if (_player.Position.Y < _groundLine && _player.IsOnFloor)
        {
            _player.IsOnFloor = false;
        }

        // Keep the camera position between the game sizes, so the _player doesn't see outside the map.
        GameWindow window = (GameWindow)WindowData;
        _camera.X = Math.Clamp(MathF.Floor(_player.Position.X - _cameraOffset + _shakeOffset.X), 0, _gameWidth - window.GameSize.X);
        _camera.Y = _shakeOffset.Y; 

        HandleInventoryInput();

        if (!_sheet.Finished) _sheet.CycleAnimation(time);
        if (!_bomb.Sheet.Finished) _bomb.Sheet.CycleAnimation(time);

        ShakeCamera(time);

        _batTimer.Cycle(time);
    }
   
    public override void Draw(GameTime time, SpriteBatch batch)
    {
        WindowData.GraphicsDevice.Clear(BackDrop);

        batch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.Transform);
            // Draw the background
            batch.Draw(_island, _camera.ScreenToWorld(new Vector2(0, -10)), Color.White * 0.4f);

            foreach (GroundTile tile in _groundTiles) 
                batch.Draw(tile.Sprite, new Vector2(tile.X, tile.Y), Color.White);

            _enemyController.DrawEntities(batch, time);
            _controller.DrawEntities(batch, time); 

            if (!_sheet.Finished)
            {
                Vector2 strikePosition = new Vector2(
                    _player.FacingRight ? _player.Position.X + 4 : _player.Position.X - 12,
                    _player.Position.Y - 4
                );

                _strikeCollider.Bounds.X = (int)strikePosition.X + 2;
                _strikeCollider.Bounds.Y = (int)strikePosition.Y;
                _strikeCollider.Bounds.Width = 4;
                _strikeCollider.Bounds.Height = 8;

                _striking = true;

                _sheet.Draw(batch, strikePosition, !_player.FacingRight);
            }

            if(!_bomb.Sheet.Finished) _bomb.Draw(batch);

            _inventory.Draw(batch, _camera);

            if (_pause.Paused) _pause.Draw(batch, _camera);
        batch.End();
    }
}
