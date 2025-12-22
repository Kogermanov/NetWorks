using Snake.GameObjects;
using Snakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Snakes.GameMessage.Types;

namespace Snake.Converters
{
    public class AnnouncementBuilder
    {
        public static AnnouncementMsg BuildAnnouncementMsg(GameParameters gameParameters)
        {
            GameAnnouncement gameAnnouncement = new GameAnnouncement
            {
                GameName = gameParameters.Name,
                Config = new GameConfig
                {
                    Width = gameParameters.FieldParameters.width,
                    Height = gameParameters.FieldParameters.height,
                    StateDelayMs = gameParameters.Delay,
                    FoodStatic = gameParameters.FoodSpawn
                },
                CanJoin = true
            };

            AnnouncementMsg announcementMsg = new AnnouncementMsg
            {
                Games = { gameAnnouncement }
            };

            return announcementMsg;
        }
    }
}
