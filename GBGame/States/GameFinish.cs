using System;
using System.Diagnostics;
using System.IO;
using GBGame.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Components;
using MonoGayme.Controllers;
using MonoGayme.States;
using MonoGayme.UI;
using MonoGayme.Utilities;

namespace GBGame.States;

public sealed class GameFinish(GameWindow window, int normal, int projectile, int level, Stopwatch watch) : State
{
    private Texture2D _book = null!;
    private Vector2 _bookPosition;
    private float _bookTimer;
    private bool _bookEase = true;
    
    private SpriteFont _font = null!;
    private readonly Color _fontColour = new Color(40, 56, 24);
    private readonly Color _activeColour = new Color(96, 112, 80);

    private SpriteSheet _normal = null!;
    private SpriteSheet _projectile = null!;

    private UIController _controller = null!;
    private bool _showButtons;
    
    private Texture2D _star = null!;
    private Texture2D _portrait = null!;
    private Texture2D _skull = null!;
    private Texture2D _watch = null!;

    private int _highNormal;
    private int _highProjectile;

    private void EditSave(SaveData data, string path) => Xml.Serialise(data, path);

    public override void LoadContent()
    {
        _bookPosition = new Vector2(0, -window.GameSize.Y - 10);
        _book = window.ContentData.Get("EndBook");
        
        _font = window.Content.Load<SpriteFont>("Sprites/Fonts/File");
        
        _normal = new SpriteSheet(window.ContentData.Get("NormalBat"), new Vector2(3, 1));
        _projectile = new SpriteSheet(window.ContentData.Get("ProjectileBat_Walk"), new Vector2(3, 1));

        _star = window.ContentData.Get("LevelStar");
        _portrait = window.ContentData.Get("Player_Portrait");
        _skull = window.ContentData.Get("Skull");
        _watch = window.ContentData.Get("Watch");

        _controller = new UIController(true);
        _controller.SetKeyboardButtons(GBGame.KeyboardLeft, GBGame.KeyboardRight, GBGame.KeyboardAction);
        _controller.SetControllerButtons(GBGame.ControllerLeft, GBGame.ControllerRight, GBGame.ControllerAction);
        _controller.OnActiveUpdating = btn =>
        {
            btn.Colour = _activeColour;
        };

        _controller.OnActiveUpdated = btn =>
        {
            btn.Colour = _fontColour;
        };

        string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SaveData.xml");
        SaveData save = Xml.Deserialise<SaveData>(savePath);
        _highNormal = save.NormalSlays;
        _highProjectile = save.ProjectileSlays;
        
        if (normal > save.NormalSlays)
        {
            _highNormal = normal;
            EditSave(new SaveData { NormalSlays = _highNormal, ProjectileSlays = _highProjectile }, savePath);
        }

        if (projectile <= save.ProjectileSlays) return;
        
        _highProjectile = projectile;
        EditSave(new SaveData { NormalSlays = _highNormal, ProjectileSlays = _highProjectile }, savePath);
    }

    public override void Update(GameTime time)
    {
        if (_bookEase)
        {
            _bookTimer += (float)time.ElapsedGameTime.TotalSeconds;
            if (_bookTimer >= 0.8f)
            {
                _bookTimer = 0.8f;
                _bookEase = false;

                _showButtons = true;
                TextButton retry = new TextButton(_font, "Retry", new Vector2(15, 120), _activeColour, true)
                {
                    OnClick = (_) =>
                    {
                        window.GameEnded = false;
                        window.GameEnding = false;
                    
                        window.Context.SwitchState(new InGame(window));
                    }
                };

                TextButton menu = new TextButton(_font, "Menu", new Vector2(115, 120), _activeColour, true)
                {
                    OnClick = (_) =>
                    {
                        window.GameEnded = false;
                        window.GameEnding = false;
                        
                        window.Context.SwitchState(new MainMenu(window));
                    }
                };

                _controller.Add(retry);
                _controller.Add(menu);
            }
            
            float timerNormalised = _bookTimer / 0.8f;
            _bookPosition.Y = float.Lerp(-window.GameSize.Y - 10, 0, Easings.EaseOutCubic(timerNormalised));
        }

        if (_showButtons)
            _controller.Update(window.MousePosition);
    }

    public override void Draw(GameTime time, SpriteBatch batch)
    {
        window.GraphicsDevice.Clear(Color.Black);
        
        batch.Begin();
        {
            batch.Draw(_book, _bookPosition, Color.White);
                
            batch.Draw(_skull, new Vector2(15, _bookPosition.Y + 10), Color.White);
            batch.DrawString(_font, "Slain", new Vector2(24, _bookPosition.Y + 9), _fontColour);
            
            _normal.Draw(batch, new Vector2(15, _bookPosition.Y + 20));
            batch.DrawString(_font, $"{normal}", new Vector2(24, _bookPosition.Y + 19), _fontColour);
            
            _projectile.Draw(batch, new Vector2(15, _bookPosition.Y + 30));
            batch.DrawString(_font, $"{projectile}", new Vector2(24, _bookPosition.Y + 29), _fontColour);
           
            batch.Draw(_star, new Vector2(13, _bookPosition.Y + 50), Color.Gray);
            batch.DrawString(_font, "Level", new Vector2(23, _bookPosition.Y + 49), _fontColour);
            batch.Draw(_star, new Vector2(13, _bookPosition.Y + 58), Color.Gray);
            batch.DrawString(_font, $"{level}", new Vector2(22, _bookPosition.Y + 57), _fontColour);
           
            batch.Draw(_watch, new Vector2(14, _bookPosition.Y + 80), Color.White);
            batch.DrawString(_font, "Time", new Vector2(23, _bookPosition.Y + 79), _fontColour);
            batch.DrawString(_font, $"{watch.Elapsed.Minutes:D2}:{watch.Elapsed.Seconds:D2}", new Vector2(15, _bookPosition.Y + 87), _fontColour);
            
            batch.Draw(_portrait, new Vector2(125, _bookPosition.Y + 20), Color.White);
            
            batch.DrawString(_font, "Name\nRiver", new Vector2(85, _bookPosition.Y + 10), _fontColour);
            batch.DrawString(_font, "Age\n19", new Vector2(85, _bookPosition.Y + 35), _fontColour);
            
            batch.Draw(_star, new Vector2(83, _bookPosition.Y + 65), Color.Gray);
            batch.DrawString(_font, "record", new Vector2(92, _bookPosition.Y + 59), _fontColour);
            batch.Draw(_star, new Vector2(138, _bookPosition.Y + 65), Color.Gray);
            
            batch.DrawString(_font, "Tracks", new Vector2(92, _bookPosition.Y + 70), _fontColour);

            _normal.Draw(batch, new Vector2(85, _bookPosition.Y + 85));
            batch.DrawString(_font, $"{_highNormal}", new Vector2(94, _bookPosition.Y + 84), _fontColour);
            
            _projectile.Draw(batch, new Vector2(85, _bookPosition.Y + 95));
            batch.DrawString(_font, $"{_highProjectile}", new Vector2(94, _bookPosition.Y + 94), _fontColour);
            
            if (_showButtons)
                _controller.Draw(batch);
        }
        batch.End();
    }
}