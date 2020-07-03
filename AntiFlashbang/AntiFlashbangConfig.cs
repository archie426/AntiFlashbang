using System.Collections.Generic;
using System.Xml.Serialization;
using Rocket.API;

namespace AntiFlashbang
{
    public class AntiFlashbangConfig : IRocketPluginConfiguration
    {
        [XmlArrayItem("WhitelistedId")]
        [XmlAttribute("WhitelistedIds")]
        public List<int> WhitelistedIds;

        public bool TellUnflashedPlayers;
        
        public void LoadDefaults()
        {
            WhitelistedIds = new List<int>(){1};
            TellUnflashedPlayers = true;
        }
    }
}