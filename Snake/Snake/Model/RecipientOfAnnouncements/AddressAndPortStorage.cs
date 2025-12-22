using Snake.Converters;
using Snake.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Snakes.GameMessage.Types;

namespace Snake.Model.RecipientOfAnnouncements
{
    internal class AddressAndPortStorage
    {
        private Dictionary<IPEndPoint, AnnouncementMsg> storage = new Dictionary<IPEndPoint, AnnouncementMsg>();
        private Timer cleanupTimer;
        private AnnouncementReceiver announcementReceiver;
        private GamePresenter gamePresenter;
        public AddressAndPortStorage(GamePresenter gamePresenter)
        {
            this.announcementReceiver = new AnnouncementReceiver();
            this.gamePresenter = gamePresenter;

            this.announcementReceiver.AnnouncementReceived += OnAnnouncementReceived;

            // Запускаем таймер для очистки хранилища каждые две секунды
            cleanupTimer = new Timer(CleanupStorage, null, TimeSpan.Zero, TimeSpan.FromSeconds(4));
        }

        private void OnAnnouncementReceived(object sender, AnnouncementReceivedEventArgs e)
        {
            lock (storage)
            {
                // Добавляем или обновляем запись в хранилище
                storage[e.EndPoint] = e.AnnouncementMsg;
            }
        }

        private void CleanupStorage(object state)
        {
            List<string> gameNames = new List<string>();
            
            List<IPEndPoint> endPoints = new List<IPEndPoint>();
            
            List<GameParameters> gamesParameters = new List<GameParameters>();

            lock (storage)
            {
                foreach (var kvp in storage)
                {
                    GameParameters newParameters = AnnouncementParser.ParseAnnouncementMsg(kvp.Value);
                    gamesParameters.Add(newParameters);
                    gameNames.Add("Game Name: " + newParameters.Name + " height: " + newParameters.FieldParameters.height 
                        + " width: " + newParameters.FieldParameters.width + " delay: " + newParameters.Delay + " food spawn: " + newParameters.FoodSpawn);
                    endPoints.Add(kvp.Key);
                }

                // Очищаем хранилище
                storage.Clear();
            }

            // Отправляем данные в Presenter
            gamePresenter.UpdateListOfServers(gameNames, endPoints, gamesParameters);
        }

        public Dictionary<IPEndPoint, AnnouncementMsg> GetStorage()
        {
            lock (storage)
            {
                // Возвращаем копию хранилища
                return new Dictionary<IPEndPoint, AnnouncementMsg>(storage);
            }
        }
    }
}
