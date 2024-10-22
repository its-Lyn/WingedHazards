using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.Components;

namespace GBGame.Components;

public class Flash : Component
{
    public Action? OnFlashFinished;
    
    private enum FlashState
    {
        FadeIn,
        FadeOut
    }
    private FlashState _state = FlashState.FadeIn;

    private readonly Color _colour;
    private readonly Shapes _shapes;

    public bool Flashing;

    private float _alpha;
    private readonly float _speed;

    private readonly Rectangle _data;

    private readonly bool _oneWay;
    
    public Flash(Game windowData, Color colour, Rectangle data, float speed, string? name = null, bool oneWay = false)
    {
        Name = name;
        _colour = colour;

        _shapes = new Shapes(windowData.GraphicsDevice);
        _speed = speed;
        
        _data = data;
        
        _oneWay = oneWay;
    }

    public void Begin() => Flashing = true;

    public void Update(GameTime time)
    {
        switch (_state)
        {
            case FlashState.FadeIn:
                _alpha += _speed;
                if (_alpha >= 1)
                {
                    _alpha = 1;

                    if (_oneWay)
                    {
                        Flashing = false;
                        OnFlashFinished?.Invoke();

                        return;
                    }

                    _state = FlashState.FadeOut;
                }
                break;
            case FlashState.FadeOut:
                _alpha -= _speed;
                if (_alpha <= 0)
                {
                    _alpha = 0;
                    _state = FlashState.FadeIn;

                    Flashing = false;
                    OnFlashFinished?.Invoke();
                }
                break;
            default:
                Console.Error.WriteLine("Do bad things.");
                break;
        }
    }

    public void Draw(SpriteBatch batch, Camera2D? camera = null)
    {
        Rectangle data = _data;
        if (camera is not null)
        {
            Vector2 pos = camera.ScreenToWorld(new Vector2(_data.X, _data.Y));
            data = new Rectangle((int)pos.X, (int)pos.Y, _data.Width, _data.Height);
        }

        _shapes.DrawRectangle(data, _colour, batch, _alpha);
    }
}