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

    private List<Skill> _skills = [];
    private ButtonController _controller = new ButtonController(true);

    private TextButton CreateButton(Skill skill, bool first)
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

        TextButton btn = new TextButton(_font, skill.Name, position, Color.White);
        btn.OnClick = () => {
            skill.OnActivate();

            SkillPoints--;
            if (SkillPoints <= 0)
            {
                _picking = false;
            }
            else
            {
                ChooseSkills(true, skill);
            }
        };

        return btn;
    }

    public void ChooseSkills(bool remove = false, Skill? skill = null)
    {
        if (remove) _skills.Remove(skill!);

        _controller.QueueRemoveAll();

        int firstIndex, secondIndex;
        if (_skills.Count >= 2)
        {
            firstIndex = Random.Shared.Next(0, _skills.Count);

            do secondIndex = Random.Shared.Next(0, _skills.Count);
            while (firstIndex == secondIndex);

            _controller.Add(CreateButton(_skills[firstIndex], true));
            _controller.Add(CreateButton(_skills[secondIndex], false));

            return;
        }

        if (_skills.Count == 1)
        {
            _controller.Add(CreateButton(_skills[0], true));
            return;
        }

        if (_skills.Count == 0)
            _controller.Add(CreateButton(new PlusBomb(game.Bomb), false));
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

        _controller.SetControllerButtons(GBGame.ControllerInventoryUp, GBGame.ControllerInventoryDown, GBGame.ControllerAction);
        _controller.SetKeyboardButtons(GBGame.KeyboardInventoryUp, GBGame.KeyboardInventoryDown, GBGame.KeyboardAction);

        _skills = [
            new DoubleJump(game.Controller.GetFirst<Player>()!),
            new MultiplyXP(_window)
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

            if ((InputManager.IsGamePadPressed(GBGame.ControllerAction) || InputManager.IsKeyPressed(GBGame.KeyboardAction)) && !Interacting)
            {
                Interacting = true;

                if (SkillPoints > 0) _picking = true;
            }

            if ((InputManager.IsGamePadPressed(GBGame.ControllerJump) || InputManager.IsKeyPressed(GBGame.KeyboardJump)) && Interacting)
            {
                Interacting = false;

                _picking = false;
                _canPick = false;
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
            }
            else
            {
                batch.DrawString(_font, $"SP: {SkillPoints}", new Vector2(2), _textColour);
                _controller.Draw(batch);

                if (!_canPick) _canPick = true;
            }
        }
    }
}
