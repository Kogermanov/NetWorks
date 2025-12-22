using Snakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Snake.GameObjects;
using System.Threading.Tasks;

namespace Snake.Model
{
    internal interface IGameModel
    {
        public void ChangeDirectionSnake(Direction newDirection);
        public GameStateData GetNewGameStateData();

        public void Exit();

        public int GetMainSnakeId();
    }
}
