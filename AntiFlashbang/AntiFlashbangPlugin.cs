using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;

namespace AntiFlashbang
{
    public class AntiFlashbangPlugin : RocketPlugin<AntiFlashbangConfig>
    {

        //I can't wait for open mod
        public static AntiFlashbangPlugin Instance;
        private bool _overrided;

        public List<CSteamID> playersToIgnore;
        public OverrideWrapper ov;
        
        protected override void Load()
        {
            Instance = this;
            
            if (_overrided)
                return;

            SteamChannel.onTriggerSend += OnTriggerSend;

            ov = new OverrideWrapper(typeof(Provider).GetMethod("send", BindingFlags.Public | BindingFlags.Static),
                typeof(Overridable).GetMethod("send", BindingFlags.Static | BindingFlags.Public));
            
            if (!ov.Override())
                Logger.LogError("Could not override send method");
            
            _overrided = true;
            playersToIgnore = new List<CSteamID>();
        }
        
        private void OnTriggerSend(SteamPlayer player, string s, ESteamCall mode, ESteamPacket type, object[] arguments)
        {
            if (s != "askToss")
                return;
            
            UnturnedPlayer pla = UnturnedPlayer.FromSteamPlayer(player);
            
            if (!Configuration.Instance.WhitelistedIds.Contains(pla.Player.clothing.glasses))
                return;
            
            playersToIgnore.Add(pla.CSteamID);
            
        }
        
        protected override void Unload()
        {
            Instance = null;
        }
    }
}