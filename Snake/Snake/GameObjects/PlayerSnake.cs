using Snakes;
using System.Collections.Generic;
using System.Data;
using Snake.GameObjects;
using System.Net;
using static Snakes.GameState.Types.Snake.Types;

namespace Snake
{
    public struct PlayerSnake
    {

        public string Name { get; private set; }

        public int Id { get; private set; }
        public List<Point> SnakePosition { get; private set; }

        public Direction Direction { get; private set; }

        public NodeRole Role { get; set; }

        public PlayerType SnakeType { get; private set; }

        public SnakeState SnakeState { get; set; }

        public int Score { get; private set; }

        public IPEndPoint IPEndPoint { get; set; }

        public PlayerSnake AddScore() { Score++; return this; }
        public void DirectionSet(Direction NewDirection) {  Direction = NewDirection; }

        public PlayerSnake(string v, List<Point> newSnake, Direction right) : this()
        {
            this.Name = v;
            this.SnakePosition = newSnake;
            this.Direction = right;
            Score = 0;
        }

        public PlayerSnake(int id, string v, IPEndPoint ipEndPoint, List<Point> newSnake, Direction right, NodeRole role, PlayerType snakeType, int score) : this()
        {
            this.Name = v;
            this.SnakePosition = newSnake;
            this.Direction = right;
            this.Role = role; 
            this.SnakeType = snakeType;
            this.Id = id;
            this.Score = 0;
            this.IPEndPoint = ipEndPoint;
            this.Score = score;
        }
        public PlayerSnake(int id, string v, List<Point> newSnake, Direction right, NodeRole role, PlayerType snakeType, int score) : this()
        {
            this.Name = v;
            this.SnakePosition = newSnake;
            this.Direction = right;
            this.Role = role;
            this.SnakeType = snakeType;
            this.Id = id;
            this.Score = score;
        }

        public PlayerSnake(PlayerSnake snake) : this()
        {
            this = snake;
        }
    }
}
