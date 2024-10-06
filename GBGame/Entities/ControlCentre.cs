using GBGame.Skills;
using GBGame.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Components.Colliders;
using MonoGayme.Controllers;
using MonoGayme.Entities;
using MonoGayme.UI;
using MonoGayme.Utilities;
using System;
using System.Collections.Generic;

namespace GBGame.Entities;

public class ControlCentre(Game windowData, InGame game, int zIndex = -1) : Entity(windowData, zIndex)
{
    private bool _picking = false;
    private bool _canPick = false;

    private GameWindow _window = null!;

    private Texture2D _sprite = null!;
    private Texture2D _questionSprite = null!;
    private float _questionOpacity = 0;

    private RectCollider _collider = null!;

    public int SkillPoints = 0;

    public bool CanInteract = false;
    public bool Interacting = false;

    private Texture2D _overlay = null!;
    private Rectangle _size;
    private Color _overlayColour = new Color(40, 56, 24);
    private Color _textColour = new Color(176, 192, 160);
    private Color _activeTextColour = new Color(136, 152, 120);

    private SpriteFont _font = null!;

    private readonly string _noSP = "No skill points!";
    private Vector2 _noSPMeasuremets;

    private readonly string _return = "Return";
    private Vector2 _returnMeasurements;
    private TextButton _returnButton = null!;

    private List<Skill> _skills = [];
    private ButtonController _controller = new ButtonController(true);

    private SkillButton CreateButton(Skill skill, bool first)
    {
        Vector2 measurements = _font.MeasureString(skill.Name);
        Vector2 position = new Vector2(
            (_window.GameSize.X - measurements.X) / 2,
            (_window.GameSize.Y - measurements.Y) / 2
        );

        if (first)
        {
            position.Y -= 3;
        }
        else
        {
            position.Y += 6;
        }

        SkillButton btn = new SkillButton(_font, skill.Name, position, _textColour, skill);
        btn.OnClick = () => {
            skill.OnActivate();

            SkillPoints--;
            _skills.Remove(btn.Skill);
            if (SkillPoints <= 0)
            {
                _picking = false;
            }
            else
            {
                ChooseSkills();
            }
        };

        return btn;
    }

    public void ChooseSkills()
    {
        _controller.QueueRemoveAll();

        int firstIndex, secondIndex;
        if (_skills.Count >= 2)
        {
            firstIndex = Random.Shared.Next(0, _skills.Count);

            do secondIndex = Random.Shared.Next(0, _skills.Count);
            while (firstIndex == secondIndex);

            _controller.Add(CreateButton(_skills[firstIndex], true));
            _controller.Add(CreateButton(_skills[secondIndex], false));
        }

        if (_skills.Count == 1)
            _controller.Add(CreateButton(_skills[0], true));

        if (_skills.Count == 0)
            _controller.Add(CreateButton(new PlusBomb(game.Bomb), true));

        _returnButton = new TextButton(_font, _return, _returnMeasurements, _textColour);
        _returnButton.OnClick = () => {
            Interacting = false;

            _canPick = false;
            _picking = false;

            game.SkipFrame = true;
        };

        _controller.Add(_returnButton);
    }

    public override void LoadContent()
    {
        _window = (GameWindow)WindowData;
        _overlay = new Texture2D(_window.GraphicsDevice, 1, 1);
        _overlay.SetData(new[] { _overlayColour });
        _size = new Rectangle(0, 0, (int)_window.GameSize.X, (int)_window.GameSize.Y);

        _sprite = WindowData.Content.Load<Texture2D>("Sprites/Objects/CommandCentre");
        _questionSprite = WindowData.Content.Load<Texture2D>("Sprites/Objects/CommandCentre_Interact");

        Position = new Vector2(
            0,
            game.GroundLine - 12
        );

        Components.AddComponent(new RectCollider("Centre"));
        _collider = Components.GetComponent<RectCollider>()!;
        _collider.Bounds = new Rectangle(
            (int)Position.X, (int)Position.Y + 8, 16, 8
        );

        _font = WindowData.Content.Load<SpriteFont>("Sprites/Fonts/File");

        Vector2 noSP = _font.MeasureString(_noSP);
        _noSPMeasuremets = new Vector2((_window.GameSize.X - noSP.X) / 2, (_window.GameSize.Y - noSP.Y) / 2);

        Vector2 ret = _font.MeasureString(_return);
        _returnMeasurements = new Vector2((_window.GameSize.X - ret.X) / 2, _window.GameSize.Y - ret.Y - 1);

        _controller.SetControllerButtons(GBGame.ControllerInventoryUp, GBGame.ControllerInventoryDown, GBGame.ControllerAction);
        _controller.SetKeyboardButtons(GBGame.KeyboardInventoryUp, GBGame.KeyboardInventoryDown, GBGame.KeyboardAction);

        Player player = game.Controller.GetFirst<Player>()!; 
        _skills = [
            new DoubleJump(player),
            new MultiplyXP(_window),
            new MoreHP(player),
            new BombRadius(game)
        ];

        _controller.OnActiveUpdating = (btn) => {
            btn.Colour = _textColour;
        };

        _controller.OnActiveUpdated = (btn) => {
            btn.Colour = _activeTextColour;
        };
    }

    public override void Update(GameTime time)
    {
        if (CanInteract)
        {
            if (_questionOpacity < 1f) _questionOpacity += 0.4f;

            if (Interacting && SkillPoints == 0)
            { 
                if (InputManager.IsGamePadPressed(GBGame.ControllerAction) || InputManager.IsKeyPressed(GBGame.KeyboardAction))
                {
                    Interacting = false;
                    game.SkipFrame = true;
                    _canPick = false;
                    return;
                }
            }

            if ((InputManager.IsGamePadPressed(GBGame.ControllerAction) || InputManager.IsKeyPressed(GBGame.KeyboardAction)) && !Interacting)
            {
                Interacting = true;

                if (SkillPoints > 0) _picking = true;
            }
        }
        else
        {
            if (_questionOpacity > 0) _questionOpacity -= 0.4f;
        }

        if (_picking && _canPick) _controller.Update(_window.MousePosition);
    }
  
    public override void Draw(SpriteBatch batch, GameTime time)
    {
        batch.Draw(_sprite, Position, Color.White);
        batch.Draw(_questionSprite, Position with { Y = Position.Y - 10 }, Color.White * _questionOpacity);

        if (Interacting)
        {
            batch.Draw(_overlay, _size, _overlayColour * 0.8f);

            if (SkillPoints == 0)
            {
                batch.DrawString(_font, _noSP, _noSPMeasuremets, _textColour);

                batch.DrawString(_font, _return, _returnMeasurements, _activeTextColour); 
            }
            else
            {
                batch.DrawString(_font, $"SP: {SkillPoints}", new Vector2(2), _textColour);
                _controller.Draw(batch);

                batch.DrawString(_font, $"{_skills.Count}", new Vector2(10), Color.White);

                if (!_canPick) _canPick = true;
            }
        }
    }
}
