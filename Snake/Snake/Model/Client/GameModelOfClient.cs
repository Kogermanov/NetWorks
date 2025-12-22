using Snake.GameObjects;
using Snakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Snakes.GameState.Types;

namespace Snake.Model.Client
{
    public class GameModelOfClient : IGameModel
    {
        //private static TaskCompletionSource<int> yourSnakeId = new TaskCompletionSource<int>(-1);

        private int yourSnakeId;

        private GameStateData gameStateData;

        public bool IsGameOver { get; private set; }
        public GameParameters gameParameters { get; private set; }

        private ClientNetworkInteraction client;

        private GamePresenter presenter;

        public  GameModelOfClient(GamePresenter presenter, GameParameters gameParameters)
        {
            this.presenter = presenter;
            this.gameParameters = gameParameters;

            IsGameOver = false;

            this.yourSnakeId = -1;
        }

        public GameModelOfClient(GamePresenter presenter, GameStateData gameStateData, GameParameters gameParameters, int id, IPEndPoint iPEndPoint)
        {
            this.presenter = presenter;
            this.gameParameters = gameParameters;
            this.gameStateData = gameStateData;

            IsGameOver = false;
            this.yourSnakeId = id;

            IPEndPoint deputyEndPoint = gameStateData.snakes
                .FirstOrDefault(snake => snake.Role == NodeRole.Deputy)
                .IPEndPoint;

            for (int i = 0; i < gameStateData.snakes.Count; i++)
            {
                if (gameStateData.snakes[i].Role == NodeRole.Deputy)
                {
                    var snake = gameStateData.snakes[i]; // Получаем копию структуры
                    snake.Role = NodeRole.Master; // Меняем роль на Master
                    gameStateData.snakes[i] = snake; // Сохраняем изменения обратно в список
                    break; // Прерываем цикл после изменения первой Deputy
                }
            }


            Thread.Sleep(1000);
            client = new ClientNetworkInteraction(this, deputyEndPoint);
            client.StartAsyncWithoutJoining(iPEndPoint);
        }

        public void SetYourSnakeId(int id)
        {
            this.yourSnakeId = id;
            //yourSnakeId.SetResult(id);
        }

        public async Task InitializeNetworkAsync(IPEndPoint serverEndPoint)
        {
            client = new ClientNetworkInteraction(this, serverEndPoint);
            await client.StartAsync();
        }

        public void ChangeDirectionSnake(Direction newDirection)
        {
            if (yourSnakeId == -1)
            {
                return;
            }
            _ = client.SendSteerMessageAsync(newDirection);
        }

        public void UpdateClientModel(GameStateData gameStateData)
        {
            if (yourSnakeId == -1)
            {
                return;
            }
            this.gameStateData = gameStateData;
            this.gameStateData.field = new FieldParameters { 
                width = gameParameters.FieldParameters.width,
                height = gameParameters.FieldParameters.height
            };

            presenter.UpdateGame();
        }

        public GameStateData GetNewGameStateData()
        {
            return gameStateData;
        }

        public int GetMainSnakeId()
        {
            return yourSnakeId;
        }

        public void MasterIsDead()
        {
            if (gameStateData.snakes.Find(item => item.Id == yourSnakeId).Role == NodeRole.Deputy)
            {
                //переключиться на мастера при этом не меняя ip и port или вообще протащить socket в MasterModel
                Exit();
                presenter.SwitchingFromClientToServer(gameStateData, gameParameters, yourSnakeId, client.GetServerEndPoint());
                
            }
            else
            {
                //поставить другой ip и port для master node
                IPEndPoint newMasterEndPoint = gameStateData.snakes.Find(item => item.Role == NodeRole.Deputy).IPEndPoint;
                if (newMasterEndPoint != null)
                {
                    client.SetNewServerEndPoint(gameStateData.snakes.Find(item => item.Role == NodeRole.Deputy).IPEndPoint);
                }
                else
                {
                    presenter.ExitModel();
                }

            }
        }

        public void Exit()
        {
            IsGameOver = true;
        }
    }
}
