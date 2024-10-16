using System.Xml.Serialization;

namespace GBGame;

public class OptionData
{
    [XmlElement("FullScreen")]
    public bool FullScreen { get; set; }
    
    [XmlElement("AllowScreenShake")]
    public bool AllowScreenShake { get; set; }
    
    [XmlElement("MuteAudio")]
    public bool MuteAudio { get; set; }
}