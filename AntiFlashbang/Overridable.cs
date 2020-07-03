using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;

namespace AntiFlashbang
{
    public static class Overridable
    {
        
        public static void send(CSteamID steamID, ESteamPacket type, byte[] packet, int size, int channel)
        {
            object[] cool = {steamID, type, packet, size, channel};

            if (AntiFlashbangPlugin.Instance.playersToIgnore.Contains(steamID))
            {
                AntiFlashbangPlugin.Instance.playersToIgnore.Remove(steamID);
                if (AntiFlashbangPlugin.Instance.Configuration.Instance.TellUnflashedPlayers)
                    UnturnedChat.Say(UnturnedPlayer.FromCSteamID(steamID), "You were able to ignore a flashbang because of your glasses!");
                return;
            }

            AntiFlashbangPlugin.Instance.ov.CallOriginal(cool);

        }
        
    }
}