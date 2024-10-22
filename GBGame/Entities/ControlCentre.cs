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

public class ControlCentre(GameWindow windowData, InGame game, int zIndex = -1) : Entity(zIndex)
{
    private bool _picking;
    private bool _canPick;

    private Texture2D _sprite = null!;
    private Texture2D _questionSprite = null!;
    private float _questionOpacity;

    private RectCollider _collider = null!;

    public int SkillPoints;

    public bool CanInteract = false;
    public bool Interacting;

    private Texture2D _overlay = null!;
    private Rectangle _size;
    private readonly Color _overlayColour = new Color(40, 56, 24);
    private readonly Color _textColour = new Color(176, 192, 160);
    private readonly Color _activeTextColour = new Color(136, 152, 120);

    private SpriteFont _font = null!;

    private const string NoSp = "No skill points!";
    private Vector2 _noSpMeasurements;

    private const string Return = "Return";
    private Vector2 _returnMeasurements;
    private TextButton _returnButton = null!;

    private List<Skill> _skills = [];
    private readonly UIController _controller = new UIController(true);

    private SkillButton CreateButton(Skill skill, bool first)
    {
        Vector2 measurements = _font.MeasureString(skill.Name);
        Vector2 position = new Vector2(
            (windowData.GameSize.X - measurements.X) / 2,
            (windowData.GameSize.Y - measurements.Y) / 2
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
        btn.OnClick = (_) => {
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

        if (_skills.Count >= 2)
        {
            int firstIndex = Random.Shared.Next(0, _skills.Count);

            int secondIndex;
            do secondIndex = Random.Shared.Next(0, _skills.Count);
            while (firstIndex == secondIndex);

            _controller.Add(CreateButton(_skills[firstIndex], true));
            _controller.Add(CreateButton(_skills[secondIndex], false));
        }

        if (_skills.Count == 1)
            _controller.Add(CreateButton(_skills[0], true));

        if (_skills.Count == 0)
            _controller.Add(CreateButton(new PlusBomb(game.Bomb), true));

        _returnButton = new TextButton(_font, Return, _returnMeasurements, _textColour)
        {
            OnClick = (_) => {
                Interacting = false;

                _canPick = false;
                _picking = false;

                game.SkipFrame = true;
            }
        };

        _controller.Add(_returnButton);
    }

    public override void LoadContent()
    {
        _overlay = new Texture2D(windowData.GraphicsDevice, 1, 1);
        _overlay.SetData([_overlayColour]);
        _size = new Rectangle(0, 0, (int)windowData.GameSize.X, (int)windowData.GameSize.Y);

        _sprite = windowData.ContentData.Get("CommandCentre");
        _questionSprite = windowData.ContentData.Get("CommandCentre_Interact");

        Position = new Vector2(
            0,
            game.GroundLine - 12
        );

        Components.AddComponent(new RectCollider("Centre"));
        _collider = Components.GetComponent<RectCollider>()!;
        _collider.Bounds = new Rectangle(
            (int)Position.X, (int)Position.Y + 8, 16, 8
        );

        _font = windowData.Content.Load<SpriteFont>("Sprites/Fonts/File");

        Vector2 noSp = _font.MeasureString(NoSp);
        _noSpMeasurements = new Vector2((windowData.GameSize.X - noSp.X) / 2, (windowData.GameSize.Y - noSp.Y) / 2);

        Vector2 ret = _font.MeasureString(Return);
        _returnMeasurements = new Vector2((windowData.GameSize.X - ret.X) / 2, windowData.GameSize.Y - ret.Y - 1);

        _controller.SetControllerButtons(GBGame.ControllerInventoryUp, GBGame.ControllerInventoryDown, GBGame.ControllerAction);
        _controller.SetKeyboardButtons(GBGame.KeyboardInventoryUp, GBGame.KeyboardInventoryDown, GBGame.KeyboardAction);

        Player player = game.Controller.GetFirst<Player>()!; 
        _skills = [
            new DoubleJump(player),
            new MultiplyXP(windowData),
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

        if (_picking && _canPick) _controller.Update(windowData.MousePosition);
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
                batch.DrawString(_font, NoSp, _noSpMeasurements, _textColour);

                batch.DrawString(_font, Return, _returnMeasurements, _activeTextColour); 
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
