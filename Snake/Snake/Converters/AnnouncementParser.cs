using Snake.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Snakes.GameMessage.Types;

namespace Snake.Converters
{
    public class AnnouncementParser
    {
        public static GameParameters ParseAnnouncementMsg(AnnouncementMsg announcementMsg)
        {
            if (announcementMsg.Games.Count == 0)
            {
                throw new ArgumentException("AnnouncementMsg does not contain any games.");
            }

            var gameAnnouncement = announcementMsg.Games[0];

            GameParameters gameParameters = new GameParameters
            {
                Name = gameAnnouncement.GameName,
                FieldParameters = new FieldParameters
                {
                    width = gameAnnouncement.Config.Width,
                    height = gameAnnouncement.Config.Height
                },
                Delay = gameAnnouncement.Config.StateDelayMs,
                FoodSpawn = gameAnnouncement.Config.FoodStatic
            };

            return gameParameters;
        }
    }
}
