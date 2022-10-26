using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public Segment prev;
        //Положение на карте
        public Point pos;
        //Фигура, которая будет отображатсья в качестве сегмента змеи
        public Rectangle rect;
        //Размер
        private int size;
        private static Canvas canv;

        public Segment(int size, Point pos, Canvas canv, Segment prev = null) {
            if (Segment.canv == null)
                Segment.canv = canv;

            this.rect = new Rectangle() { Width = size, Height = size, Fill = Brushes.Red };
            this.prev = prev;
            this.pos = pos;
            this.size = size; 

            Segment.canv.Children.Add(rect);
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

            if (pos.X > MainWindow.WIDTH - this.size)
                pos.X = 0;
            else if (pos.X < 0)
                pos.X = MainWindow.WIDTH - this.size;
            else if (pos.Y > MainWindow.HEIGHT - this.size)
                pos.Y = 0;
            else if (pos.Y < 0)
                pos.Y = MainWindow.HEIGHT - this.size;
        }

        public bool IntersectsAny(Point p) {
            if (this.prev != null && this.prev.IntersectsAny(p))
                return true;

            return Intersects(p);
        }

        public bool Intersects(Point p) => p == pos;

        public void Init(int counter) {
            if (counter == 0)
                return;

            Point p = new Point(this.pos.X - this.size, this.pos.Y);
            this.prev = new Segment(this.size, p, canv);
            this.prev.Init(--counter);
        }

        public void Update() {
            if(this.prev != null) {
                this.prev.Update();
            }
            Canvas.SetLeft(this.rect, this.pos.X);
            Canvas.SetTop(this.rect, this.pos.Y);
        }

        public void Destroy() {
            if(this.prev != null) {
                this.prev.Destroy();
                this.prev = null;
            }
            canv.Children.Remove(rect);

            return;
        }
    }

    class FruitGenerator {
        public static bool doGenerate = true;
        public static Rectangle rect;
        public static Point pos;
        public static int size;

        public static Canvas canv;
        private static Random rand;

        public static void Init(Brush color, Canvas canvas, int size = 10) {
            canv = canvas;
            rect = new Rectangle() { Width = size, Height = size, Fill = color };
            rand = new Random();
            FruitGenerator.size = size;

        }

        public static void Generate() {
            if (!doGenerate)
                return;

            doGenerate = false;

            int column = (int)MainWindow.WIDTH / size;
            int row = (int)MainWindow.HEIGHT / size;

            do {
                pos = new Point(rand.Next(0, column - 1) * size, rand.Next(0, row - 1) * size);
            } while (Snake.Intersects(pos));

            Canvas.SetLeft(rect, pos.X);
            Canvas.SetTop(rect, pos.Y);
            canv.Children.Add(rect);
        }

        public static void Destroy() {
            canv.Children.Remove(rect);
            doGenerate = true;
        }
    }

    class Snake {
        //Расстояние, на которое будет смещаться каждый сегмент змеи за N единицу времени
        public static int speed { get; private set; }
        private static Segment head;
        private static int segmentSize;
        private static int length;
        public static Canvas canv;

        //Направление движения
        public static Point dir;

        public static void Init(Point start, Canvas canv, int speed = 1, int size = 10, int length = 3) {
            Snake.canv = canv;
            Snake.speed = speed;
            dir = new Point(speed, 0);
            segmentSize = size;
            Snake.length = length;

            head = new Segment(segmentSize, start, canv);
            head.Init(--length);
        }

        public static void AddSegment() {
            Segment newHead = new Segment(segmentSize, head.pos, null);
            newHead.Move(dir);
            newHead.prev = head;
            head = newHead;
        }

        public static void Move() => head.Move(dir);

        public static void Update() {
            head.Update();

            if (head.pos == FruitGenerator.pos) {
                AddSegment();
                FruitGenerator.Destroy();
            }

            if (head.prev.IntersectsAny(head.pos)) {
                Destroy();
                FruitGenerator.Destroy();

                Snake.Init(new Point(50, 0), canv, length: 5);
                FruitGenerator.Init(Brushes.DarkCyan, canv);
            }
        }

        public static bool Intersects(Point p) => head.IntersectsAny(p);

        public static void Destroy() {
            head.Destroy(); 
        }
    }

    public partial class MainWindow : Window {
        public static double WIDTH;
        public static double HEIGHT;  

        public MainWindow() {
            InitializeComponent();

            WIDTH = field.Width;
            HEIGHT = field.Height;

            Snake.Init(new Point(50, 0), field, length: 5);
            FruitGenerator.Init(Brushes.DarkCyan, field);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e) {
            Snake.Move();
            Snake.Update();
            FruitGenerator.Generate();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.D when Snake.dir.X == 0:
                    Snake.dir = new Point(Snake.speed, 0);
                    break;
                case Key.A when Snake.dir.X == 0:
                    Snake.dir = new Point(-Snake.speed, 0);
                    break;
                case Key.W when Snake.dir.Y == 0:
                    Snake.dir = new Point(0, -Snake.speed);
                    break;
                case Key.S when Snake.dir.Y == 0:
                    Snake.dir = new Point(0, Snake.speed);
                    break;
            }
        }
    }
}
