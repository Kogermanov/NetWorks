using Snakes;
using System.Collections.Generic;
using System.Linq;
using static Snakes.GameState.Types.Snake.Types;
using Snake;
using System.Net;
using System;
using Snake.GameObjects;

public class GameStateParser
{
    private static long IPAddressToLong(string ipAddress)
    {
        // Преобразуем строку в объект IPAddress
        IPAddress address = IPAddress.Parse(ipAddress);

        // Получаем байтовый массив из IP-адреса
        byte[] bytes = address.GetAddressBytes();

        return BitConverter.ToUInt32(bytes, 0);
    }
    public static GameStateData ParseGameState(GameState gameState)
    {
        var snakes = new List<Snake.PlayerSnake>();

        foreach (var snake in gameState.Snakes)
        {
            var snakePosition = new List<Point>();
            foreach (var point in snake.Points)
            {
                snakePosition.Add(new Point(point.X, point.Y));
            }

            var player = gameState.Players.Players.FirstOrDefault(p => p.Id == snake.PlayerId);
            var snakeName = player?.Name ?? "Unknown";

            var snakeType = player.Type;

            var snakeRole = player.Role;

            var snakeScore = player.Score;

            if(snakeRole == NodeRole.Master || snakeRole == NodeRole.Viewer)
            {
                snakes.Add(new Snake.PlayerSnake(snake.PlayerId, snakeName, snakePosition, snake.HeadDirection, snakeRole, snakeType, snakeScore));
            }
            else
            {
                var snakeIp = player.IpAddress;

                var snakePort = player.Port;

                var snakeEndPoint = new IPEndPoint(IPAddressToLong(snakeIp), snakePort);

                snakes.Add(new Snake.PlayerSnake(snake.PlayerId, snakeName, snakeEndPoint, snakePosition, snake.HeadDirection, snakeRole, snakeType, snakeScore));
            }
        }

        var foodsCord = new List<Point>();
        foreach (var food in gameState.Foods)
        {
            foodsCord.Add(new Point(food.X, food.Y));
        }

        return new GameStateData
        {
            foodsCord = foodsCord,
            snakes = snakes
        };
    }
}
