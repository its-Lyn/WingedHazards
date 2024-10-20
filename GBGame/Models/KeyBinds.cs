using System.Xml.Serialization;

namespace GBGame.Models;

public class KeyBinds
{
    [XmlElement("Left")]
    public required string Left { get; set; }
    
    [XmlElement("Right")]
    public required string Right { get; set; }
    
    [XmlElement("InventoryUp")]
    public required string InventoryUp { get; set; }
    
    [XmlElement("InventoryDown")]
    public required string InventoryDown { get; set; }
    
    
    [XmlElement("Jump")]
    public required string Jump { get; set; }
    
    [XmlElement("Action")]
    public required string Action { get; set; }
    
    
    [XmlElement("Pause")]
    public required string Pause { get; set; }
}