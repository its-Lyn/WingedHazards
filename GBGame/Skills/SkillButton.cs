using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGayme.UI;

namespace GBGame.Skills;

public class SkillButton(SpriteFont font, string text, Vector2 position, Color colour, Skill skill) : TextButton(font, text, position, colour, true)
{
    public Skill Skill { get; } = skill;
}
