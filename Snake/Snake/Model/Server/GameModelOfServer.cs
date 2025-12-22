using Snake.Model;
using Snake.GameObjects;
using Snakes;
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Snakes.GameState.Types;
using static Snakes.GameState.Types.Snake.Types;
using Timer = System.Windows.Forms.Timer;
using System.Net.Sockets;
//using Snake = Snake.GameObjects.Snake;

namespace Snake.Model.Server
{
    public partial class GameModelOfServer : IGameModel
    {
        private List<int> lifeClients = new List<int>();

        private int mainSnakeId;

        public List<PlayerSnake> Snakes { get; private set; }

        private FieldParameters field;

        public Point Food { get; private set; }
        public bool IsGameOver { get; private set; }

        private Random random = new Random();

        private Timer gameTimer;

        public int delay;

        private GamePresenter presenter;

        ServerNetworkInteraction server;

        public GameModelOfServer(GameParameters gameParameters, GameStateData gameStateData, int mainSnakeId, 
            GamePresenter presenter, IPEndPoint pastMasterIpEndPoint)
        {
            for (int i = 0; i < gameStateData.snakes.Count; i++)
            {
                var snake = gameStateData.snakes[i]; // Получаем элемент по индексу

                switch (snake.Role)
                {
                    case NodeRole.Deputy:
                        snake.Role = NodeRole.Master; // Deputy → Master
                        break;
                    case NodeRole.Master:
                        snake.Role = NodeRole.Normal; // Master → Normal
                        snake.SnakeState = SnakeState.Alive;
                        snake.IPEndPoint = pastMasterIpEndPoint;
                        break;
                }
                gameStateData.snakes[i] = snake;
            }

            Dictionary<int, IPEndPoint> snakeEndPoints = gameStateData.snakes
            .Where(snake => snake.Id != mainSnakeId) // Исключаем змейку с mainSnakeId
            .ToDictionary(snake => snake.Id, snake => snake.IPEndPoint);

            this.server = new ServerNetworkInteraction(this, gameParameters, new UdpClient(gameStateData.snakes[mainSnakeId].IPEndPoint)
               , snakeEndPoints, gameStateData.snakes.Max(snake => snake.Id) + 1);

            this.Snakes = gameStateData.snakes;

            this.field = gameStateData.field;

            this.mainSnakeId = mainSnakeId;

            this.presenter = presenter;

            this.Food = gameStateData.foodsCord[0];

            gameTimer = new Timer
            {
                Interval = gameParameters.Delay
            };

            Thread serverThread = new Thread(server.Start);
            serverThread.Start();

            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        public GameModelOfServer(int width, int height, int delay, GamePresenter presenter)
        {
            mainSnakeId = 0;

            this.presenter = presenter;
            this.delay = delay;
            field.width = width;
            field.height = height;

            Snakes = new List<PlayerSnake>();

            GameParameters gameParameters = new GameParameters {
                Name = "Game_" + random.Next() % 9000,
                Delay = delay,
                FoodSpawn = 1,
                FieldParameters = new FieldParameters { 
                    width = width ,
                    height = height
                }
            };

            //для сетевых взаимодействий с клиентами 
            server = new ServerNetworkInteraction(this, gameParameters);

            Thread serverThread = new Thread(server.Start);
            serverThread.Start();

            SpawnFood();

            AddNewSnake(mainSnakeId, "main", NodeRole.Master, null);

            gameTimer = new Timer
            {
                Interval = delay
            };

            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (!IsGameOver)
            {
                CheckingPlayerLives();
                presenter.UpdateGame();
            }
            else
            {
                // Отписаться от события Tick
                gameTimer.Tick -= GameTimer_Tick;

                // Остановить таймер
                gameTimer.Stop();
            }
        }

        private static int ChangeNumberByModulo(int number, int ringSize)
        {
            // Выполняем операцию по модулю
            int result = number % ringSize;

            // Если результат отрицательный, добавляем размер кольца, чтобы получить положительное значение
            if (result < 0)
            {
                result += ringSize;
            }

            return result;
        }

        private void CheckingPlayerLives()
        {
            for (int i = 0; i < Snakes.Count; i++)
            {
                var snake = Snakes[i];

                if (IsIdInLifeClients(snake.Id) || snake.Role == NodeRole.Master)
                {
                    snake.SnakeState = SnakeState.Alive;
                    Console.WriteLine($"Snake {snake.Id} is live");
                    Snakes[i] = snake;
                }
                else
                {
                    Console.WriteLine($"Snake {snake.Id} is dead");
                    snake.SnakeState = SnakeState.Zombie;
                    Snakes[i] = snake;
                }
            }
            lifeClients.Clear();
        }

        public bool IsIdInLifeClients(int id)
        {
            return lifeClients.Contains(id);
        }

        private bool CheckCollision(Point newHead, int snakeIndex)
        {
            //код для проверки столкновения новой головы с другими змейками
            foreach (var snake in Snakes)
            {
                if (snake.SnakePosition.Contains(newHead))
                {
                    return true;
                }
            }
            return false;
        }

        private void SpawnFood()
        {
            do
            {
                Food = new Point(random.Next(field.width), random.Next(field.height));
            } while (Snakes.Any(snake => snake.SnakePosition.Contains(Food)));
        }

        public void ChangeDirectionSnake(Direction newDirection)
        {
            ChangeDirectionSnakeById(newDirection, mainSnakeId);
        }

        public void ChangeDirectionSnakeById(Direction newDirection, int id)
        {
            PlayerSnake snake = Snakes.FirstOrDefault(snake => snake.Id == id);

            switch (snake.Direction)
            {
                case Direction.Up:
                    if (newDirection != Direction.Down)
                    {
                        snake.DirectionSet(newDirection);
                    }
                break;
                case Direction.Down:
                    if (newDirection != Direction.Up)
                    {
                        snake.DirectionSet(newDirection);
                    }
                    break;
                case Direction.Left:
                    if (newDirection != Direction.Right)
                    {
                        snake.DirectionSet(newDirection);
                    }
                    break;
                case Direction.Right:
                    if (newDirection != Direction.Left)
                    {
                        snake.DirectionSet(newDirection);
                    }
                    break;
            }
            if (snake.SnakePosition != null)
            {
                Snakes[Snakes.FindIndex(snake => snake.Id == id)] = snake;
            }
        }

        public bool AddNewSnake(int id, string name, NodeRole nodeRole, IPEndPoint endPoint)
        {
            Random random = new Random();
            int fieldWidth = field.width;
            int fieldHeight = field.height;
            bool foundFreeSquare = false;
            Point center = new Point(1, 1);
            Point tail = new Point(1, 2);
            Direction tailDirection = Direction.Right; // Направление хвоста

            while (!foundFreeSquare)
            {
                // Выбираем случайный центр квадрата 5x5
                center = new Point(random.Next(0, fieldWidth), random.Next(0, fieldHeight));

                // Проверяем, что в квадрате 5x5 нет клеток, занятых змейками
                bool isFree = true;
                for (int x = center.X - 2; x <= center.X + 2; x++)
                {
                    for (int y = center.Y - 2; y <= center.Y + 2; y++)
                    {
                        Point current = new Point(x % fieldWidth, y % fieldHeight); // Учитываем замкнутые края поля
                        if (Snakes.Any(snake => snake.SnakePosition.Contains(current)))
                        {
                            isFree = false;
                            break;
                        }
                    }
                    if (!isFree) break;
                }

                // Если квадрат свободен, проверяем, что в нем нет еды
                if (isFree && Food != center)
                {
                    foundFreeSquare = true;
                }
            }

            if (!foundFreeSquare) {
                return false;
            }

            // Выбираем случайное направление для хвоста
            Direction[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
            tailDirection = directions[random.Next(0, 4)];

            // Вычисляем позицию хвоста
            switch (tailDirection)
            {
                case Direction.Up:
                    tail = new Point(center.X, (center.Y + 1) % fieldHeight);
                    break;
                case Direction.Down:
                    tail = new Point(center.X, (center.Y - 1 + fieldHeight) % fieldHeight);
                    break;
                case Direction.Left:
                    tail = new Point((center.X + 1) % fieldWidth, center.Y);
                    break;
                case Direction.Right:
                    tail = new Point((center.X - 1 + fieldWidth) % fieldWidth, center.Y);
                    break;
            }

            // Проверяем, что хвост не находится на еде
            if (Food == tail)
            {
                // Если хвост на еде, ищем другое расположение
                return AddNewSnake(id, name, nodeRole, endPoint);
            }

            // Создаем змейку
            List<Point> newSnakePosition = new List<Point> { center, tail };


            PlayerSnake newSnake;
            if (nodeRole == NodeRole.Master)
            {
                newSnake = new PlayerSnake(id, name, newSnakePosition, tailDirection, nodeRole, PlayerType.Human, 0);
            }
            else
            {
                if (Snakes.Any(snake => snake.Role == NodeRole.Deputy))
                {
                    newSnake = new PlayerSnake(id, name, endPoint, newSnakePosition, tailDirection, nodeRole, PlayerType.Human, 0);
                }
                else
                {
                    newSnake = new PlayerSnake(id, name, endPoint, newSnakePosition, tailDirection, NodeRole.Deputy, PlayerType.Human, 0);
                }
            }

                // Добавляем змейку в список
            Snakes.Add(newSnake);
            return true;
        }



        public void MakePlayerAlive(int ClientID)
        {
            lock (this)
            {
                lifeClients.Add(ClientID);
            }
        }

        private void UpdateField()
        {
            for (int i = 0; i < Snakes.Count; i++)
            {
                var snake = Snakes[i];
                if (snake.Role == NodeRole.Viewer) // Пропускаем змейку, если она уже Viewer
                {
                    continue;
                }

                Point head = snake.SnakePosition[0];
                Point newHead = new Point(head.X, head.Y);

                switch (snake.Direction)
                {
                    case Direction.Up:
                        newHead.Y = ChangeNumberByModulo(newHead.Y - 1, field.height);
                        break;
                    case Direction.Down:
                        newHead.Y = ChangeNumberByModulo(newHead.Y + 1, field.height);
                        break;
                    case Direction.Left:
                        newHead.X = ChangeNumberByModulo(newHead.X - 1, field.width);
                        break;
                    case Direction.Right:
                        newHead.X = ChangeNumberByModulo(newHead.X + 1, field.width);
                        break;
                }

                if (newHead.X < 0 || newHead.X >= field.width || newHead.Y < 0
                    || newHead.Y >= field.height || CheckCollision(newHead, i))
                {
                    Snakes[i] = snake;

                    if (snake.Role == NodeRole.Master) // Если это была Master-змейка
                    {
                        snake.Role = NodeRole.Viewer;

                        // Переключение на client
                        List<Point> foods = new List<Point>();
                        foods.Add(Food);

                        IPEndPoint iPEndPoint = server.GetServerEndPoint();

                        Exit();

                        // Проверяем, есть ли хотя бы одна змейка с ролью Deputy
                        bool hasDeputy1 = Snakes.Any(s => s.Role == NodeRole.Deputy);
                        if (!hasDeputy1)
                        {
                            presenter.ExitModel();
                            continue;
                        }

                        var thread = new Thread(() =>
                        {
                            presenter.SwitchingFromServerToClient(
                                new GameStateData
                                {
                                    foodsCord = foods,
                                    snakes = Snakes,
                                    field = field
                                },
                                new GameParameters
                                {
                                    Delay = delay,
                                    FieldParameters = field,
                                    FoodSpawn = 1,
                                    Name = "----"
                                },
                                mainSnakeId,
                                iPEndPoint
                            );
                        });

                        thread.Start();

                        break;
                    }
                    else
                    {
                        snake.Role = NodeRole.Viewer;
                    }

                    Snakes[i] = snake;
                    continue;
                }

                snake.SnakePosition.Insert(0, newHead);

                if (newHead.Equals(Food))
                {
                    SpawnFood();
                    Snakes[i] = snake.AddScore();
                }
                else
                {
                    snake.SnakePosition.RemoveAt(snake.SnakePosition.Count - 1);
                }

            }

            // Очистка позиций змей, которые стали Viewer
            for (int i = 0; i < Snakes.Count; i++)
            {
                if (Snakes[i].Role == NodeRole.Viewer)
                {
                    Snakes[i].SnakePosition.Clear();
                }
            }



            bool hasDeputy = Snakes.Any(s => s.Role == NodeRole.Deputy);

            if (!hasDeputy)
            {
                // Находим первую змейку с ролью Normal
                for (int i = 0; i < Snakes.Count; i++)
                {
                    if (Snakes[i].Role == NodeRole.Normal)
                    {
                        var snake = Snakes[i];
                        snake.Role = NodeRole.Deputy;
                        Snakes[i] = snake;
                        break;
                    }
                }
            }
        }

        public GameStateData GetNewGameStateData()
        {
            UpdateField();

            List<Point> Foods = new List<Point>();
            Foods.Add(Food);
            GameStateData newGameStateData = new GameStateData
            {
                foodsCord = Foods,
                snakes = Snakes,
                field = field
            };
            server.SendGameStatePeriodically(newGameStateData);
            return newGameStateData;
        }

        public int GetMainSnakeId()
        {
            return mainSnakeId;
        }

        public void Exit()
        {
            IsGameOver = true;
            server.CloseSocket();
        }
    }
}
