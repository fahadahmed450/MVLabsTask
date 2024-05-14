using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MVLabsTask
{
    class Program
    {
        public const int BoardSize = 100;
        public static char[,] board = new char[BoardSize, BoardSize];
        public static List<Animal> animals = new List<Animal>();
        public static object locker = new object();
        public static DateTime lastMoveTime = DateTime.MinValue;

        static void Main(string[] args)
        {
            InitializeAnimals();
            Thread soundThread = new Thread(SoundEmitter);
            soundThread.Start();

            while (true)
            {
                DrawBoard();
                var key = Console.ReadKey(true).Key;
                lock (locker)
                {
                    foreach (var animal in animals)
                    {
                        if (key == ConsoleKey.A)
                            animal.Move(0, -1);
                        else if (key == ConsoleKey.B)
                            animal.Move(0, 1);
                        else if (key == ConsoleKey.D)
                            animal.Move(1, 0);
                        else if (key == ConsoleKey.S)
                            animal.Move(-1, 0);
                    }
                }
            }
        }

        static void InitializeAnimals()
        {
            Random rand = new Random();
            void AddAnimal(Animal animal, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    int x, y;
                    do
                    {
                        x = rand.Next(BoardSize);
                        y = rand.Next(BoardSize);
                    } while (board[x, y] != '\0');
                    animal.X = x;
                    animal.Y = y;
                    animals.Add(animal.Clone());
                    board[x, y] = animal.Representation;
                }
            }

            AddAnimal(new Goose(), 3);
            AddAnimal(new Duck(), 2);
            AddAnimal(new Hen(), 4);
            AddAnimal(new Turkey(), 3);
            AddAnimal(new Peacock(), 4);
        }

        static void DrawBoard()
        {
            Console.Clear();
            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    if (board[i, j] == '\0')
                        Console.Write(".");
                    else
                        Console.Write(board[i, j]);
                }
                Console.WriteLine();
            }
        }

        static void SoundEmitter()
        {
            while (true)
            {
                lock (locker)
                {
                    foreach (var animal in animals)
                    {
                        if (animal.ShouldEmitSound())
                        {
                            Console.SetCursorPosition(animal.X, animal.Y);
                            Console.Write(animal.Sound);
                            Thread.Sleep((int)(animal.SoundDuration * 1000));
                            Console.SetCursorPosition(animal.X, animal.Y);
                            Console.Write(animal.Representation);
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }
    }

    abstract class Animal
    {
        public int X { get; set; }
        public int Y { get; set; }
        public abstract char Representation { get; }
        public abstract char Sound { get; }
        public abstract double SoundInterval { get; }
        public abstract double SoundDuration { get; }
        private DateTime lastSoundTime = DateTime.MinValue;

        public void Move(int dx, int dy)
        {
            if (this is Peacock && (DateTime.Now - Program.lastMoveTime).TotalSeconds < 2)
            {
                Console.WriteLine("Peacock complains!");
                return;
            }

            int newX = X + dx;
            int newY = Y + dy;

            if (newX >= 0 && newX < Program.BoardSize && newY >= 0 && newY < Program.BoardSize && Program.board[newX, newY] == '\0')
            {
                Program.board[X, Y] = '\0';
                X = newX;
                Y = newY;
                Program.board[X, Y] = Representation;
                Program.lastMoveTime = DateTime.Now;
            }
        }

        public bool ShouldEmitSound()
        {
            if ((DateTime.Now - lastSoundTime).TotalSeconds >= SoundInterval)
            {
                lastSoundTime = DateTime.Now;
                return true;
            }
            return false;
        }

        public abstract Animal Clone();
    }

    class Goose : Animal
    {
        public override char Representation => 'O';
        public override char Sound => 'H';
        public override double SoundInterval => 1.3;
        public override double SoundDuration => 0.5;
        public override Animal Clone() => new Goose();
    }

    class Duck : Animal
    {
        public override char Representation => 'A';
        public override char Sound => 'Q';
        public override double SoundInterval => 2.4;
        public override double SoundDuration => 0.8;
        public override Animal Clone() => new Duck();
    }

    class Hen : Animal
    {
        public override char Representation => 'G';
        public override char Sound => 'C';
        public override double SoundInterval => 1.6;
        public override double SoundDuration => 0.6;
        public override Animal Clone() => new Hen();
    }

    class Turkey : Animal
    {
        public override char Representation => 'T';
        public override char Sound => 'G';
        public override double SoundInterval => 2.0;
        public override double SoundDuration => 1.0;
        public override Animal Clone() => new Turkey();
    }

    class Peacock : Animal
    {
        public override char Representation => 'P';
        public override char Sound => 'C';
        public override double SoundInterval => 3.0;
        public override double SoundDuration => 1.3;
        public override Animal Clone() => new Peacock();
    }
}
