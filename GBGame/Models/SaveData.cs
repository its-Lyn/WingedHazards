using System.Xml.Serialization;

namespace GBGame;

public class SaveData
{
    [XmlElement("NormalSlays")]
    public int NormalSlays { get; set; }
    
    [XmlElement("ProjectileSlays")]
    public int ProjectileSlays { get; set; }
}