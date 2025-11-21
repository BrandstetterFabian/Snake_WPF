using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Snake
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    ///
    class GameField
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public Color BackgroundPixelCol;

        public List<Pixel> Pixels = new List<Pixel>();
        public Pixel[,] PixelField { get; set; }
        public Button[,] PixelFieldButtons { get; set; }
        public int SinglePixelWidth = 50;
        public int SinglePixelHeight = 50;

        public SnakeHead sh = null;

        private MainWindow mainWindow;

        public GameField(int height, int width, Color backgroundPixelCol, SnakeHead sh, int singlePixelWidth, int singlePixelHeight, MainWindow mw)
        {
            Height = height; 
            Width = width;
            BackgroundPixelCol = backgroundPixelCol;
            this.sh = sh;
            SinglePixelWidth = singlePixelWidth;
            SinglePixelHeight = singlePixelHeight;
            mainWindow = mw;

            PixelField = new Pixel[Height, Width];
            PixelFieldButtons = new Button[Height, Width];
        }
        public void FillPixelFields()
        {
            mainWindow.cv_gameField.Children.Clear();

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    PixelField[i, j] = new Pixel(j, i);

                    SolidColorBrush brush = new SolidColorBrush(BackgroundPixelCol);
                    Button b = new Button
                    {
                        Width = SinglePixelWidth,
                        Height = SinglePixelHeight,
                        Background = brush,
                        BorderThickness = new Thickness(0),
                        Focusable = false,
                        IsHitTestVisible = false
                    };
                    Canvas.SetLeft(b, j * SinglePixelWidth);
                    Canvas.SetTop(b, i * SinglePixelHeight);
                    mainWindow.cv_gameField.Children.Add(b);    

                    PixelFieldButtons[i, j] = b;
                }
            }
            PixelField[sh.YPos, sh.XPos] = sh;
        }

        public void PrintPixelField()
        {
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    PixelFieldButtons[i, j].Background = new SolidColorBrush(PixelField[i, j].PixelCol);
                }
            }
        }

        public bool CheckIfCollisions()
        {
            if(CheckIfWallHit() || CheckIfSnakeHit()) return true;
            return false;
        }
       
        private bool CheckIfSnakeHit()
        {
            if (PixelField[sh.YPos, sh.XPos].Value > 0) return true;
            return false;
        }
        private bool CheckIfWallHit()
        {
            if(sh.XPos < 0  || sh.YPos < 0 || sh.XPos >= PixelField.GetLength(1) || sh.YPos >= PixelField.GetLength(0)) return true;
            return false;
        }

        public void SpawnPowerUp(int lifeSpan, Color powerUpCol)
        {
            var rand = new Random();
            int randXPos = rand.Next(0, PixelField.GetLength(1));
            int randYpos = rand.Next(0, PixelField.GetLength(0));
            while (PixelField[randYpos, randXPos].Value > 0)
            {
                randXPos = rand.Next(0, PixelField.GetLength(1));
                randYpos = rand.Next(0, PixelField.GetLength(0));
            }
            PowerUp powerUp = new PowerUp(randXPos, randYpos, lifeSpan, powerUpCol);
            PixelField[randYpos, randXPos] = powerUp;
        }
        public void DeletePowerUp()
        {
            for (int i = 0; i < PixelField.GetLength(0); i++)
            {
                for (int j = 0; j < PixelField.GetLength(1); j++)
                {
                    if (PixelField[i, j] is PowerUp && PixelField[i, j].Value == 0)
                    {
                        PixelField[i, j] = new Pixel(j, i);
                    }
                }
            }
        }
        public bool PowerUpExists()
        {
            for(int i = 0;i < PixelField.GetLength(0); i++)
            {
                for( int j = 0; j < PixelField.GetLength(1); j++)
                {
                    if(PixelField[i, j] is PowerUp powerUp) return true;
                }
            }
            return false;
        }
        public void DecreasePixelValue()
        {
            for (int i = 0; i < PixelField.GetLength(0); i++)
            {
                for (int j = 0; j < PixelField.GetLength(1); j++)
                {
                    if(!(PixelField[i, j] is SnakeHead))
                    {
                        PixelField[i, j].ChangeValue();
                    }
                }
            }
        }
        public void SetPixelColor()
        {
            for (int i = 0; i < PixelField.GetLength(0); i++)
            {
                for (int j = 0; j < PixelField.GetLength(1); j++)
                {
                    if (PixelField[i, j].Value > 0) PixelField[i, j].PixelCol = sh.PixelCol;
                    else if (PixelField[i, j].Value == 0) PixelField[i,j].PixelCol = BackgroundPixelCol;

                }
            }
        }
        public void Update()
        {
            DecreasePixelValue();
            DeletePowerUp();
            SetPixelColor();
        }
    }
    interface IChangePixelValue
    {
        void ChangeValue();
    }
    class Pixel: IChangePixelValue
    {
        public int XPos { get; set; }
        public int YPos { get; set; }

        public int Value {  get; set; }
        public Color PixelCol { get; set; }

        public Pixel(int xpos, int ypos)
        {
            XPos = xpos;
            YPos = ypos;
            Value = 0;
            PixelCol = Color.FromRgb(143, 206, 255);
        }
        public virtual void ChangeValue()
        {
            if (Value > 0) Value--;
        }
    }
    class SnakeHead: Pixel
    {
        public int Direction { get; set; } // 0=up, 1=right, 2=down, 3=left
        public bool DirectionChanged { get; set; } = false;

        public SnakeHead(int xpos, int ypos, Color col) : base(xpos, ypos)
        {
            Value = 2;
            PixelCol = col;
            Direction = 2;
        }
        public void ChangeDirection(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Up || e.Key == Key.W) && Direction != 2 && !DirectionChanged)
            {
                Direction = 0;
                DirectionChanged = true;
            }
            else if ((e.Key == Key.Right || e.Key == Key.D) && Direction != 3 && !DirectionChanged)
            {
                Direction = 1;
                DirectionChanged = true;
            }
            else if ((e.Key == Key.Down || e.Key == Key.S) && Direction != 0 && !DirectionChanged)
            {
                Direction = 2;
                DirectionChanged = true;
            }
            else if ((e.Key == Key.Left || e.Key == Key.A) && Direction != 1 && !DirectionChanged)
            {
                Direction = 3;
                DirectionChanged = true;
            }        
        }
        public void ChangePos()
        {
            switch(Direction)
            {
                case 0:
                    MoveUp();
                    break;
                case 1: 
                    MoveRight(); 
                    break;
                case 2: 
                    MoveDown(); 
                    break;
                case 3: 
                    MoveLeft(); 
                    break;
            }
            DirectionChanged = false;
        }
        public void SetPos(int xPos, int yPos)
        {
            XPos = xPos;
            YPos = yPos;
        }
        public void MoveUp()
        {
            YPos--;
        }
        public void MoveRight()
        {
            XPos++;
        }
        public void MoveDown()
        {
            YPos++;
        }
        public void MoveLeft()
        {
            XPos--;
        }
       
        public override void ChangeValue()
        {
            Value++;
        }
    }
    class PowerUp : Pixel
    {
        //public int LifeSpan {  get; set; }

        public PowerUp(int xpos, int ypos, int value, Color powerUpCol) : base(xpos, ypos)
        {
            XPos = xpos;
            YPos = ypos;
            Value = value - (value * 2);
            PixelCol = powerUpCol;
        }
        public override void ChangeValue()
        {
            if(Value < 0) Value++;
        }
    }
    public partial class MainWindow : Window
    {
        DispatcherTimer gameTimer = new DispatcherTimer();
        GameField gf;
        int oldX, oldY;
        Color powerUpCol = Colors.Red;

        public MainWindow()
        {
            InitializeComponent();

            chbx_showSettings.IsChecked = true;
            gameTimer.Interval = TimeSpan.FromMilliseconds(Convert.ToInt16(txtb_snakeSpeed.Text));
            gameTimer.Tick += GameLoop;
        }

        public bool IsInteger(string input)
        {
            try
            {
                Convert.ToInt32(input);
                return true;
            }
            catch { return false; }
        }

        public bool IsInSpecificNumRange(int num, int start, int end)
        {
            if(num >= start && num <= end) return true;
            return false;
        }
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            gf.sh.ChangeDirection(sender, e);
        }

        private bool InputValidation()
        {
            if (txtb_gameFieldHeightInput.Text != string.Empty && txtb_gameFieldWidthInput.Text != string.Empty && txtb_snakeStartingLength.Text != string.Empty && txtb_lifeSpan.Text != string.Empty && txtb_gameFieldHeightInput.Text != string.Empty)
            {
                if (IsInteger(txtb_gameFieldHeightInput.Text) && IsInteger(txtb_gameFieldWidthInput.Text) && IsInteger(txtb_snakeStartingLength.Text) && IsInteger(txtb_lifeSpan.Text) && IsInteger(txtb_gameFieldWidthInput.Text) && IsInteger(txtb_gameFieldHeightInput.Text))
                {
                    int gameFieldHeight = Convert.ToInt32(txtb_gameFieldHeightInput.Text);
                    if (IsInSpecificNumRange(Convert.ToInt32(txtb_gameFieldHeightInput.Text), 3, 50) && IsInSpecificNumRange(Convert.ToInt32(txtb_gameFieldWidthInput.Text), 3, 50) && IsInSpecificNumRange(Convert.ToInt32(txtb_snakeStartingLength.Text), 2, Convert.ToInt32(txtb_gameFieldHeightInput.Text) * Convert.ToInt32(txtb_gameFieldWidthInput.Text) - 2))
                    {
                        return true;
                    }
                    else MessageBox.Show("Please enter a vaild number range!\n(Game-Field-Height/Width: 3-50,\nSnake-Starting-Length: Height*Width-2)", "invalid number range", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else MessageBox.Show("Please use valid data formats (integers) for input fields!", "invalid format", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else MessageBox.Show("Please fill in all fields!", "incomplete fields", MessageBoxButton.OK, MessageBoxImage.Error);
            
            return false;
        }

        private void btn_startGame_Click(object sender, RoutedEventArgs e)
        {
            if (InputValidation())
            {
                gameTimer.Interval = TimeSpan.FromMilliseconds(Convert.ToInt16(txtb_snakeSpeed.Text));
                this.KeyDown += MainWindow_KeyDown;
                this.Focus();
                Keyboard.Focus(this);
                this.PreviewKeyDown += MainWindow_PreviewKeyDown;
                lbl_gameOverMessage.Content = string.Empty;

                Color bgCol = Colors.Black;
                switch (cmbx_backgroundColor.SelectedIndex)
                {
                    case 0:
                        bgCol = Colors.Black;
                        break;
                    case 1:
                        bgCol = Colors.Yellow;
                        break;
                    case 2:
                        bgCol = Colors.Green;
                        break;
                    case 3:
                        bgCol = Colors.Red;
                        break;
                    case 4:
                        bgCol = Colors.Blue;
                        break;
                }

                Color snakeCol = Colors.Yellow;
                switch (cmbx_snakeColor.SelectedIndex)
                {
                    case 0:
                        snakeCol = Colors.Yellow;
                        break;
                    case 1:
                        snakeCol = Colors.Black;
                        break;
                    case 2:
                        snakeCol = Colors.Green;
                        break;
                    case 3:
                        snakeCol = Colors.Red;
                        break;
                    case 4:
                        snakeCol = Colors.Blue;
                        break;
                }

                switch (cmbx_powerUpColor.SelectedIndex)
                {
                    case 0:
                        powerUpCol = Colors.Red;
                        break;
                    case 1:
                        powerUpCol = Colors.Black;
                        break;
                    case 2:
                        powerUpCol = Colors.Green;
                        break;
                    case 3:
                        powerUpCol = Colors.Yellow;
                        break;
                    case 4:
                        powerUpCol = Colors.Blue;
                        break;
                }

                gf = new GameField(
                    Convert.ToInt32(txtb_gameFieldHeightInput.Text), 
                    Convert.ToInt32(txtb_gameFieldWidthInput.Text), 
                    bgCol, 
                    new SnakeHead(2, 2, snakeCol),
                    Convert.ToInt16(this.Height / Convert.ToInt32(txtb_gameFieldHeightInput.Text) * 2), 
                    Convert.ToInt16(this.Height / Convert.ToInt32(txtb_gameFieldHeightInput.Text) * 2), 
                    this);

                gf.sh.Value = Convert.ToInt32((txtb_snakeStartingLength.Text));
                gf.FillPixelFields();
                gf.PrintPixelField();
                gf.SpawnPowerUp(Convert.ToInt16(txtb_lifeSpan.Text), powerUpCol);
                gf.SetPixelColor();
                gf.Update();

                oldX = gf.sh.XPos;
                oldY = gf.sh.YPos;

                gameTimer.Start();
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            Pixel tail = new Pixel(oldX, oldY);
            tail.Value = gf.sh.Value;
            gf.PixelField[oldY, oldX] = tail;

            gf.sh.ChangePos();

            if (gf.CheckIfCollisions())
            {
                gameTimer.Stop();
                GameOver(); 
                this.PreviewKeyDown -= MainWindow_PreviewKeyDown;
                return;
            }
            if (gf.PixelField[gf.sh.YPos, gf.sh.XPos] is PowerUp)
            {
                gf.sh.ChangeValue();
                gf.SpawnPowerUp(Convert.ToInt16(txtb_lifeSpan.Text), powerUpCol);
            }
            if (!gf.PowerUpExists())
            {
                gf.SpawnPowerUp(Convert.ToInt16(txtb_lifeSpan.Text), powerUpCol);
            }

            oldX = gf.sh.XPos;  
            oldY = gf.sh.YPos;

            gf.PixelField[gf.sh.YPos, gf.sh.XPos] = gf.sh;
            gf.PixelField[gf.sh.YPos, gf.sh.XPos].Value = gf.sh.Value;

            gf.Update();
            //gf.SetPixelColor();
            gf.PrintPixelField();
        }

        void OutputStats()
        {
            lbl_statsMessage.Content += $"STATS:\n Score: {gf.sh.Value}";
        }

        void GameOver()
        {
            lbl_gameOverMessage.Content = "GAME OVER";
            OutputStats();
        }

        private void chbx_showSettings_Checked(object sender, RoutedEventArgs e)
        {
            grbx_settings.Visibility = Visibility.Visible;
        }

        private void chbx_showSettings_Unchecked(object sender, RoutedEventArgs e)
        {
            grbx_settings.Visibility = Visibility.Collapsed;
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            gf?.sh.ChangeDirection(sender, e);
            e.Handled = true;
        }
    }
}
