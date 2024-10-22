using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Components;
using MonoGayme.UI;
using MonoGayme.Utilities;

namespace GBGame.UI;

public class VolumeSlider(SpriteFont font, string text, Vector2 position, Color colour, float value) : IElement
{
    public bool IsActive { get; set; } = false;

    private string _text = text;

    private float _value = value;
    public Action<float>? ValueChanged;

    public Color Colour { get; set; } = colour;

    public void SetText(string other)
    {
        _text = other;
    }

    public void RunAction() { }

    public void Update(Vector2 mouse)
    {
        if (!IsActive) return;
        
        if (InputManager.IsKeyPressed(GBGame.KeyboardLeft))
        {
            _value -= 0.1f;
            if (_value <= 0.0f)
                _value = 0.0f;
                
            ValueChanged?.Invoke(_value);
        }

        if (InputManager.IsKeyPressed(GBGame.KeyboardRight))
        {
            _value += 0.1f;
            if (_value >= 1.0f)
                _value = 1.0f;
                
            ValueChanged?.Invoke(_value);
        }
    }

    public void Draw(SpriteBatch batch, Camera2D? camera)
    {
        batch.DrawString(font, _text, position, Colour);
    }
}