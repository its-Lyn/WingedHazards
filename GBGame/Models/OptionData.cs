using System.Xml.Serialization;

namespace GBGame.Models;

public class OptionData
{
    [XmlElement("FullScreen")]
    public bool FullScreen { get; set; }
    
    [XmlElement("AllowScreenShake")]
    public bool AllowScreenShake { get; set; }
    
    [XmlElement("MuteAudio")]
    public bool MuteAudio { get; set; }
    
    [XmlElement("ShowVersion")]
    public bool ShowVersion { get; set; }
}