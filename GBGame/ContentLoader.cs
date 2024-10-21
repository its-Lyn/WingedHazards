using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace GBGame;

public class ContentLoader
{
    private Dictionary<string, Texture2D> Textures { get; } = new Dictionary<string, Texture2D>();
    public Dictionary<string, SoundEffect> Audio { get; } = new Dictionary<string, SoundEffect>();
    public Dictionary<string, List<Texture2D>> SpecialTextures { get; } = new Dictionary<string, List<Texture2D>>();

    public Texture2D Get(string name)
        => Textures[name];
    
    public SoundEffect GetAudio(string name)
        => Audio[name];
    
    public void Set(string name, Texture2D texture)
        => Textures.Add(name, texture);
}