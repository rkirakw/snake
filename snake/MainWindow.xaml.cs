using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace snake {
    class Segment {

        //Предыдущий сегмент
        private Segment prev;
        //Положение на карте
        public Point pos;
        //Фигура, которая будет отображатсья в качестве сегмента змеи
        public Rectangle rect;
        //Размер
        private int size;
        private Canvas canv;

        public Segment(int size, Point pos, Canvas canv, Segment prev = null) {
            this.rect = new Rectangle() { Width = size, Height = size, Fill = Brushes.Red };
            this.prev = prev;
            this.pos = pos;
            this.size = size;
            this.canv = canv;

            canv.Children.Add(rect);
            this.Update();
        }

        //Метод для перемещения сегмента на расстояние to
        public void Move(Point to) {
            if(prev != null) {
                //Расстояние от предыдущего сегмента до текущего
                Point p = new Point((this.pos.X - prev.pos.X) / this.size, (this.pos.Y - prev.pos.Y) / this.size);
                this.prev.Move(p);
            }
            //Непосредственное перемещение сегмента
            this.pos.X += to.X * this.size;
            this.pos.Y += to.Y * this.size;
        }

        public void Init(int counter) {
            if (counter == 0)
                return;

            Point p = new Point(this.pos.X - this.size, this.pos.Y);
            this.prev = new Segment(this.size, p, this.canv);
            this.prev.Init(--counter);
        }

        public void Update() {
            if(this.prev != null) {
                this.prev.Update();
            }
            Canvas.SetLeft(this.rect, this.pos.X);
            Canvas.SetTop(this.rect, this.pos.Y);
        }
    }

    class Snake {
        //Расстояние, на которое будет смещаться каждый сегмент змеи за N единицу времени
        public int speed { get; private set; }
        private Segment head;
        private int segmentSize;
        private int length;

        //Направление движения
        public Point dir;

        public Snake(Point start, Canvas canv, int speed = 1, int size = 10, int length = 3) {
            this.speed = speed;
            this.dir = new Point(speed, 0);
            this.segmentSize = size;
            this.length = length;

            this.head = new Segment(this.segmentSize, start, canv);
            this.head.Init(--length);
        }

        public void Move() => head.Move(dir);

        public void Update() => this.head.Update();
    }

    public partial class MainWindow : Window {
        private Snake snake;

        public MainWindow() {
            InitializeComponent();

            snake = new Snake(new Point(200, 50), field, length:5);
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e) {
            snake.Move();
            snake.Update();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.D when snake.dir.X == 0:
                    snake.dir = new Point(snake.speed, 0);
                    break;
                case Key.A when snake.dir.X == 0:
                    snake.dir = new Point(-snake.speed, 0);
                    break;
                case Key.W when snake.dir.Y == 0:
                    snake.dir = new Point(0, -snake.speed);
                    break;
                case Key.S when snake.dir.Y == 0:
                    snake.dir = new Point(0, snake.speed);
                    break;
            }
        }
    }
}
