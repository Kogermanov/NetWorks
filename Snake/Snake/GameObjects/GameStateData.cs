using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Snake.GameObjects
{


    public struct GameStateData
    {
        public List<Point> foodsCord;
        public List<PlayerSnake> snakes;
        public FieldParameters field;


        public GameStateData(GameStateData other)
        {
            // Копируем список змей
            snakes = new List<PlayerSnake>(other.snakes.Select(snake => new PlayerSnake(snake)));

            // Копируем точку еды
            foodsCord = new List<Point>();

            field = new FieldParameters();
        }

    }
}
