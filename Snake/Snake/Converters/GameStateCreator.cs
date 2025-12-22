using Snakes;
using Snake;
using System.Collections.Generic;
using System.Net;
using System.Data;
using Snake.GameObjects;
using System;
using System.Linq;

public class GameStateCreator
{
    public static GameState CreateGameState(GameStateData gameStateData)
    {
        var gameState = new GameState
        {
            StateOrder = 1,
            Players = new GamePlayers(),
            Foods = { },
            Snakes = { }
        };

        // еда
        foreach (var foodCord in gameStateData.foodsCord.ToList())
        {
            gameState.Foods.Add(new GameState.Types.Coord { X = foodCord.X, Y = foodCord.Y });
        }

        // змейка
        foreach (var snake in gameStateData.snakes.ToList())
        {
            var snakePoints = new List<GameState.Types.Coord>();
            foreach (var point in snake.SnakePosition)
            {
                snakePoints.Add(new GameState.Types.Coord { X = point.X, Y = point.Y });
            }

            var gameSnake = new GameState.Types.Snake
            {
                PlayerId = snake.Id,
                Points = { snakePoints },
                State = snake.SnakeState,
                HeadDirection = snake.Direction
            };

            gameState.Snakes.Add(gameSnake);

            if (snake.Role == NodeRole.Normal || snake.Role == NodeRole.Deputy)
            {
                var gamePlayer = new GamePlayer
                {
                    Name = snake.Name,
                    Id = gameSnake.PlayerId,
                    Role = snake.Role,
                    Type = snake.SnakeType,
                    Score = snake.Score,
                    IpAddress = snake.IPEndPoint.Address.ToString(),
                    Port = snake.IPEndPoint.Port,
                };
                gameState.Players.Players.Add(gamePlayer);
            }
            else
            {
                var gamePlayer = new GamePlayer
                {
                    Name = snake.Name,
                    Id = gameSnake.PlayerId,
                    Role = snake.Role,
                    Type = snake.SnakeType,
                    Score = snake.Score,
                };
                gameState.Players.Players.Add(gamePlayer);
            }

        }

        return gameState;
    }
}
