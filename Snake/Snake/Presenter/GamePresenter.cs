using Snake.Model;
using Snake.Model.Client;
using Snake.Model.RecipientOfAnnouncements;
using Snake.Model.Server;
using Snake.GameObjects;
using Snakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;

namespace Snake
{
    public class GamePresenter
    {
        private volatile IGameModel model;
        private readonly object modelLock = new object();

        public IGameView view;

        private AddressAndPortStorage addressAndPortStorage;

        private ModelSwitcher modelSwitcher;

        int width; int height; int delay;

        public GamePresenter(int width, int height, int interval)
        {
            this.width = width;
            this.height = height;
            this.delay = interval;

            this.view = new GameForm(this);

            this.modelSwitcher = new ModelSwitcher(this);

            this.addressAndPortStorage = new AddressAndPortStorage(this);

            view.DirectionChanged += View_DirectionChanged;
        }

        private void View_DirectionChanged(object sender, Direction direction)
        {
            lock (modelLock)
            {
                if (model != null)
                    model.ChangeDirectionSnake(direction);
            }
        }

        public void UpdateGame()
        {
            try
            {
                lock (modelLock)
                {
                    if (!ReferenceEquals(model, null))
                    {
                        view.UpdateView(model.GetNewGameStateData(), model.GetMainSnakeId());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UpdateGame: {ex.Message}");
            }
        }

        public void UpdateListOfServers(List<string> gameStr, List<IPEndPoint> endPoints, List<GameParameters> gamesParameters)
        {
            modelSwitcher.SetEndPoints(endPoints);
            modelSwitcher.SetGameParameters(gamesParameters);
            view.PrintListOfServers(gameStr);
        }

        public async void SwitchToClientModel(int selectNum)
        {
            model = await modelSwitcher.SelectClientModel(selectNum);
        }

        public void SwitchToServerModel()
        {
            lock (modelLock)
            {
                model = modelSwitcher.SelectServerModel(width, height, delay);
            }
        }

        public void SwitchingFromClientToServer(GameStateData gameState, GameParameters gameParameters, int snakeId, IPEndPoint pastMasterIpEndPoint)
        {
            lock (modelLock)
            {
                model = modelSwitcher.SwitchToServerModelWithReadyState(gameParameters, gameState, snakeId, pastMasterIpEndPoint);
            }
        }

        public void SwitchingFromServerToClient(GameStateData gameState, GameParameters gameParameters, int snakeId, IPEndPoint iPEndPoint)
        {
            lock (modelLock)
            {
                model = modelSwitcher.SwitchToClientModelWithReadyState(gameParameters, gameState, snakeId, iPEndPoint);
            }
        }

        public void ExitModel()
        {
            lock (modelLock)
            {
                model?.Exit();
                model = null;
                view.UpdateView(new GameStateData(), 0);
                view.InitialState();
            }
        }
    }
}
