using Google.Protobuf;
using Snake;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Direction = Snakes.Direction;
using Snake.GameObjects;


static class Program
{
    [STAThread]
    static void Main()
    {
        int width = 15;
        int height = 15;
        int delay = 400;
        GameForm view = new GameForm(model);
        GamePresenter presenter = new GamePresenter(width, height, delay);
       
        Application.Run((GameForm)presenter.view);
    }
}
