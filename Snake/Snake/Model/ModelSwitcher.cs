using Snake.Model.Client;
using Snake.Model.Server;
using Snake.GameObjects;
using Snake.view;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;

namespace Snake.Model
{
    public class ModelSwitcher
    {
        private List<GameParameters> gamesParameters;

        //private GameStateData gameStateData;

        private List<IPEndPoint> endPoints;

        private GamePresenter gamePresenter;

        public ModelSwitcher(GamePresenter gamePresenter)
        {
            this.gamePresenter = gamePresenter;
        }
        public void SetEndPoints(List<IPEndPoint> endPoints)
        {
            this.endPoints = endPoints;
        }

        public void SetGameParameters(List<GameParameters> gamesParameters)
        {
            this.gamesParameters = gamesParameters;
        }

        public async Task<GameModelOfClient> SelectClientModel(int serverForConnection)
        {
            if (endPoints == null || gamePresenter == null)
            {
                throw new ArgumentNullException("endPoints or gamePresenter cannot be null");
            }

            GameModelOfClient model = new GameModelOfClient(gamePresenter, gamesParameters[0]);
            try
            {
                await model.InitializeNetworkAsync(endPoints[serverForConnection]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing network: {ex.Message}");
                throw; // или вернуть null, если это допустимо
            }
            return model;
        }


        public GameModelOfServer SelectServerModel(int width, int height, int delay)
        {
            SettingsForm settingsForm = new SettingsForm();
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                width = settingsForm.Width;
                height = settingsForm.Height;
                delay = settingsForm.Delay;
                return new GameModelOfServer(width, height, delay, gamePresenter);
            }
            return null;
        }



        public GameModelOfServer SwitchToServerModelWithReadyState(GameParameters gameParameters, GameStateData gameStateData, int snakeId, IPEndPoint pastMasterIpEndPoint)
        {
            return new GameModelOfServer(gameParameters, gameStateData, snakeId, gamePresenter, pastMasterIpEndPoint);
        }
        public GameModelOfClient SwitchToClientModelWithReadyState(GameParameters gameParameters, GameStateData gameStateData, int snakeId, IPEndPoint iPEndPoint)
        {
            return new GameModelOfClient(gamePresenter, gameStateData, gameParameters, snakeId, iPEndPoint);
        }

        internal IGameModel SwitchToServerModelWithReadyState(GameParameters gameParameters, GameStateData gameState)
        {
            throw new NotImplementedException();
        }
    }
}
