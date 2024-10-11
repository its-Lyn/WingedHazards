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
using MonoGayme.Entities;

namespace GBGame.States;

public class InGame(GameWindow windowData) : State(windowData)
{
    public int ScoreMultiplier = 1;

    public bool SkipFrame;

    const int BatSpawnerHeight = -8;
    const int BatSpawnerWidth = 40;

    private record struct GroundTile(Texture2D Sprite, int X, int Y);

    private List<Texture2D> _grass = [];
    private List<Texture2D> _ground = [];
    private List<Texture2D> _bushes = [];
    private List<GroundTile> _groundTiles = [];

    private readonly int TileSize = 8;
    private readonly Color BackDrop = new Color(232, 240, 223);

    private Camera2D _camera = new Camera2D(Vector2.Zero);
    private float _cameraOffset = 40;

    public EntityController Controller = new EntityController();
    private EntityController _enemyController = new EntityController();

    public int GroundLine;
    private int _gameWidth;

    private Texture2D _island = null!;

    private Inventory _inventory = new Inventory();
    private Pause _pause = null!;
    private AnimatedSpriteSheet _sheet = null!;
    private AnimatedSpriteSheet _slash = null!;

    public Bomb Bomb = null!;

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
    private Jump _playerJump = null!;

    private readonly Color _levelColour = new Color(176, 192, 160);
    private readonly Color _xpColour = new Color(96, 112, 80);
    private SpriteFont _font = null!;

    private Texture2D _starSprite = null!;

    private Timer _batTimer = new Timer(5, true, false);

    private Timer _difficultyTimer = new Timer(30, true, false);
    private readonly float _maxDecrease = 3f;
    private bool _canTry = false;
    private bool _canSpawn = false;

    private ControlCentre _centre = null!;
    private RectCollider _centreCollider = null!;

    private readonly int _baseXP = 14;
    private int _toLevelUp;

    private GamePadVisualiser _gamepad = null!;

    private void SetupGround(int tileCountX, int tileCountY)
    {
        HashSet<int> usedGridPositions = new HashSet<int>();

        int bushCount = Random.Shared.Next(3, 5);
        for (int i = 0; i < bushCount; i++)
        {
            Texture2D bush = _bushes[Random.Shared.Next(0, 3)];

            int gridX;
            do gridX = Random.Shared.Next(2, tileCountX - 2);
            while (usedGridPositions.Contains(gridX));

            usedGridPositions.Add(gridX - 1);
            usedGridPositions.Add(gridX);
            usedGridPositions.Add(gridX + 1);

            _groundTiles.Add(new GroundTile(bush, gridX * TileSize, tileCountY - TileSize * 2));
        }

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
        if (!_shaking) return;
        _intensity -= (float)time.ElapsedGameTime.TotalSeconds / _shakeDuration;
        if (_intensity <= 0) 
        {
            _shaking = false;
            _shakeOffset = Vector2.Zero;

            if(Bomb.Exploded)
                Bomb.Exploded = false;

            return;
        }

        float rot = Random.Shared.NextSingle() * MathF.Tau;
        _shakeOffset = new Vector2(MathF.Cos(rot), MathF.Sin(rot)) * _shakeMagnitude * _intensity;
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
        if (Random.Shared.Next(0, 3) == 1)
        {
            ProjectileBat pbat = new ProjectileBat((GameWindow)WindowData, position); 
            pbat.LockOn(_player);

            _enemyController.AddEntity(pbat);
            
            return;
        }

        NormalBat bat = new NormalBat(WindowData, position); 
        bat.LockOn(_player);

        _enemyController.AddEntity(bat);

    }

    private void CalculateXP(Entity entity)
    { 
        XPDropper? dropper = entity.Components.GetComponent<XPDropper>();
        if (dropper is null) return;

        GameWindow window = (GameWindow)WindowData;
        _player.XP += dropper.XP * window.XPMultiplier;
        
        if (_player.XP < _toLevelUp) return;
        
        _player.Level++;

        if (_player.XP - _toLevelUp > 0)
            _player.XP -= _toLevelUp;
        else
            _player.XP = 0;

        // Double XP every level
        _toLevelUp *= 2;
        
        _centre.SkillPoints++;
        _centre.ChooseSkills();
    }

    public override void LoadContent()
    {
        _toLevelUp = _baseXP;
        _starSprite = WindowData.Content.Load<Texture2D>("Sprites/UI/LevelStar");

        _strikeCollider.Bounds = new Rectangle();

        for (int i = 1; i <= 4; i++) 
            _ground.Add(WindowData.Content.Load<Texture2D>($"Sprites/Ground/Ground_{i}"));
        
        for (int i = 1; i <= 2; i++)
            _grass.Add(WindowData.Content.Load<Texture2D>($"Sprites/Grass/Grass_{i}"));

        for (int i = 1; i <= 3; i++)
            _bushes.Add(WindowData.Content.Load<Texture2D>($"Sprites/Bushes/Bush_{i}"));

        GameWindow window = (GameWindow)WindowData;

        _gameWidth = (int)(window.GameSize.Y * 2);

        int tileCountY = (int)(window.GameSize.Y - TileSize);
        int tileCountX = _gameWidth / TileSize; 
        SetupGround(tileCountX, tileCountY);

        GroundLine = tileCountY - TileSize - TileSize / 2;

        _player = new Player(WindowData, _camera);
        _player.Position.Y = GroundLine;
        Controller.AddEntity(_player);

        _centre = new ControlCentre(window, this);
        _centre.LoadContent();
        _centreCollider = _centre.Components.GetComponent<RectCollider>()!;

        _playerCollider = _player.Components.GetComponent<RectCollider>()!;
        _playerJump = _player.Components.GetComponent<Jump>()!;

        _island = WindowData.Content.Load<Texture2D>("Sprites/BackGround/Island");

        _slash = new AnimatedSpriteSheet(WindowData.Content.Load<Texture2D>("Sprites/Entities/Player_Slash"), new Vector2(4, 1), 0.1f, false, new Vector2(0, 4));
        _sheet = new AnimatedSpriteSheet(WindowData.Content.Load<Texture2D>("Sprites/SpriteSheets/Strike"), new Vector2(6, 1), 0.02f);
        _sheet.OnSheetFinished = () => { 
            _striking = false;
        };
        
        _inventory.LoadContent(WindowData);
        _inventory.AddItem(new Sword(WindowData, _sheet, _slash, _player));

        Bomb = new Bomb(WindowData, _player);
        _inventory.AddItem(Bomb);

        Bomb.Sheet.OnSheetFinished = () => { 
            Bomb.CanPlace = true;
            Bomb.Exploded = true;

            StartShake(1, 3);
            _shakeOffset = Vector2.Zero;
        };

        _pause = new Pause(window);

        _enemyController.OnEntityUpdate = (_, _, entity) => {
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
                if (health.HealthPoints > 0) return;
                CalculateXP(entity);
                _enemyController.QueueRemove(entity);

                return;
            }

            if (!Bomb.Exploded || !rect.Collides(Bomb.KillRadius)) return;
            CalculateXP(entity);
            _enemyController.QueueRemove(entity);
        };

        _batTimer.OnTimeOut = () => {
            // 1/7 chance to be able to START trying to spawn 3 bats at once.
            if (_canTry && Random.Shared.Next(0, 7) == 1)
                _canSpawn = true;

            int minPosition = (int)(_player.Position.X - BatSpawnerWidth);
            int width = minPosition + (BatSpawnerWidth * 2);
            Vector2 batPosition = new Vector2(Random.Shared.Next(minPosition, width), BatSpawnerHeight);

            AddBat(batPosition);

            // 1/4 chance to spawn two bats with one on the opposite side
            if (Random.Shared.Next(0, 4) != 1) return;
            
            Vector2 secondBatPosition = batPosition with { X = 2 * _player.Position.X - batPosition. X }; 
            AddBat(secondBatPosition);

            // 1/5 chance to spawn 3 bats
            if (_canSpawn && Random.Shared.Next(0, 5) == 1)
            {
                AddBat(secondBatPosition with { X = 2 * _player.Position.X - secondBatPosition.X });
            }
        };

        _difficultyTimer.OnTimeOut = () => {
            _batTimer.Time -= 0.5f;

            if (!(_batTimer.Time <= _maxDecrease)) return;
            
            _difficultyTimer.Stop();
            _canTry = true;
        };

        _shapes = new Shapes(window.GraphicsDevice);
        _font = WindowData.Content.Load<SpriteFont>("Sprites/Fonts/File");

        _gamepad = new GamePadVisualiser(window);
    }

    public override void Update(GameTime time)
    {
        if (SkipFrame)
        {
            SkipFrame = false;
            return;
        }

        _gamepad.Update(time);

        if (!_centre.Interacting && (InputManager.IsGamePadPressed(Buttons.Start) || InputManager.IsKeyPressed(Keys.Escape)))
            _pause.Paused = !_pause.Paused;

        if (_pause.Paused)
        {
            _pause.Update();
            return;
        }

        _centre.CanInteract = _centreCollider.Collides(_playerCollider);
        _centre.Update(time);

        if (_centre.Interacting) return;

        // Update controllers.
        Controller.UpdateEntities(WindowData.GraphicsDevice, time);
        _enemyController.UpdateEntities(WindowData.GraphicsDevice, time);

        // Hardcoded ground checking (we don't need anything more complicated.)
        if (_player.Position.Y > GroundLine - 4) 
        {
            _player.Velocity.Y = 0;
            _player.Position.Y = GroundLine - 4;

            _playerJump.Count = _playerJump.BaseCount;

            _player.IsOnFloor = true;
            _player.IsJumping = false;
            _player.FallDecrease = 0;
            _player.GravityMultiplier = 0;
        }

        if (_player.Position.Y < GroundLine - 4 && _player.IsOnFloor)
        {
            _player.IsOnFloor = false;
        }

        // Keep the camera position between the game sizes, so the _player doesn't see outside the map.
        GameWindow window = (GameWindow)WindowData;
        _camera.X = Math.Clamp(MathF.Floor(_player.Position.X - _cameraOffset + _shakeOffset.X), 0, _gameWidth - window.GameSize.X);
        _camera.Y = _shakeOffset.Y; 

        if (!SkipFrame)
            HandleInventoryInput();

        if (!_sheet.Finished) _sheet.CycleAnimation(time);
        if (!_slash.Finished) _slash.CycleAnimation(time);
        if (!Bomb.Sheet.Finished) Bomb.Sheet.CycleAnimation(time);

        ShakeCamera(time);

        _batTimer.Cycle(time);
        _difficultyTimer.Cycle(time);
    }
   
    public override void Draw(GameTime time, SpriteBatch batch)
    {
        WindowData.GraphicsDevice.Clear(BackDrop);

        batch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.Transform);
        { 
            // Draw the background
            batch.Draw(_island, _camera.ScreenToWorld(new Vector2(0, -10)), Color.White * 0.4f);

            foreach (GroundTile tile in _groundTiles) 
                batch.Draw(tile.Sprite, new Vector2(tile.X, tile.Y), Color.White);

            _enemyController.DrawEntities(batch, time);
            Controller.DrawEntities(batch, time); 

            if (!_sheet.Finished)
            {
                Vector2 strikePosition = new Vector2(
                    _player.FacingRight ? _player.Position.X + 4 : _player.Position.X - 12,
                    _player.Position.Y - 2
                );

                _strikeCollider.Bounds.X = (int)strikePosition.X + 2;
                _strikeCollider.Bounds.Y = (int)strikePosition.Y;
                _strikeCollider.Bounds.Width = 4;
                _strikeCollider.Bounds.Height = 8;

                _striking = true;

                _sheet.Draw(batch, strikePosition, !_player.FacingRight);
            }

            if (!_slash.Finished)
            {
                Vector2 handPosition = _player.Position with { X = _player.FacingRight ? _player.Position.X - 3 : _player.Position.X - 5, Y = _player.Position.Y + 4 };
                _slash.Draw(batch, handPosition, !_player.FacingRight);
            }
            
            if(!Bomb.Sheet.Finished) Bomb.Draw(batch);

            _inventory.Draw(batch, _camera);

            // Draw the player XP
            batch.DrawString(_font, $"{_player.XP} - {_toLevelUp}", _camera.ScreenToWorld(new Vector2(1, 25)), _xpColour);

            // Draw the player level
            batch.Draw(_starSprite, _camera.ScreenToWorld(new Vector2(0, 34)), Color.White);
            batch.DrawString(_font, $"{_player.Level}", _camera.ScreenToWorld(new Vector2(10, 33)), _levelColour);

            _centre.Draw(batch, time);

            if (_pause.Paused) _pause.Draw(batch, _camera);

            _gamepad.Draw(batch, _camera);
        } 
        batch.End();
    }
}
