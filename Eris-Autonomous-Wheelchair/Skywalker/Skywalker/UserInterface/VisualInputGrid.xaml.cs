using System;
using System.Runtime.InteropServices;
using System.Timers;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Skywalker.Input;

namespace Skywalker.UserInterface
{
    /// <summary>
    /// Interaction logic for VisualInputGrid.xaml
    /// </summary>
    public partial class VisualInputGrid : UserControl
    {
        //TODO: add comments for these timers,
        //I'm not sure what all they do.
        private Timer timer;

        private Timer buttonTimer;

        private DispatcherTimer hoverTimer;

        /// <summary>
        /// The currently selected button.
        /// </summary>
        private string selectedButton;

        /// <summary>
        /// The default style of buttons
        /// </summary>
        private Style defaultButtonStyle;

        /// <summary>
        /// The style of a selected button.
        /// </summary>
        private Style selectedButtonStyle;

        /// <summary>
        /// Association of boundaries to button names.
        /// </summary>
        private Dictionary<Rect, String> boundsDict;

        /// <summary>
        /// List of buttons in the control
        /// </summary>
        private List<Button> buttonList;

        /// <summary>
        /// Association of buttons to directions.
        /// </summary>
        private Dictionary<String, Vector> directionDict;

        /// <summary>
        /// Name of the currently hovered-on button.
        /// </summary>
        private string hoverButtonName;

        /// <summary>
        /// Current input direction,
        /// as part of the IInputDevice interface
        /// </summary>
        public Vector InputDirection { get; private set; }

         /// <summary>
        /// Public constructor
        /// </summary>
        public VisualInputGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the Visual Input Grid
        /// </summary>
        public void Initialize()
        {
            //var defaultButtonStyle = BackButton.Style;
            defaultButtonStyle = (Style) FindResource("DefaultButtonStyle");
            selectedButtonStyle = (Style)FindResource("SelectedButtonStyle");
            timer = new Timer(100);
            timer.Elapsed += timer_Elapsed;
            timer.Enabled = false;
            buttonTimer = new Timer(Data.GazeDelay);
            buttonTimer.Elapsed += TimerOnElapsed;
            hoverTimer = new DispatcherTimer();
            hoverTimer.Tick += hoverTimer_Tick;
            hoverTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            PopulateButtonList();
            PopulateDirectionDictionary();
            UpdateBounds();
            this.InputGrid.Visibility = Visibility.Hidden;

            Input.Input.DeviceChanged += Input_DeviceChanged;
        }

        //change input grid opacity if eyetribe is plugged in
        private void Input_DeviceChanged(object sender, EventArgs e)
        {
            DeviceChangedEventArgs args = (DeviceChangedEventArgs)e;
            string device = args.DeviceName;

            if (device == "EyeTribe")
            {
                this.InputGrid.Visibility = Visibility.Visible;
            }
            else
            {
                this.InputGrid.Visibility = Visibility.Hidden;
            }

        }

        /// <summary>
        /// Moves the cursor to (10,10)
        /// when the STOP action is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Stop_Clicked(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine("Stop Button clicked.");
            //timer.Enabled = true;
            Win32.POINT p = new Win32.POINT();
            p.x = 10;
            p.y = 10;
            Win32.SetCursorPos(p.x, p.y);

            //Point p = upLeftButton.PointToScreen(new Point(0, 0));
            ////Console.WriteLine("P({0},{1})", p.X, p.Y);
            //UpdateBounds();
            /*foreach (Point point in this.testPoints)
            {
                BoundsCheck(point);
            }*/
        }

        /// <summary>
        /// Starts the hover Timer when the cursor
        /// enters a button boundary.
        /// </summary>
        /// <param name="sender">sending object</param>
        /// <param name="e">mouse event arguments</param>
        private void button_enter(object sender, MouseEventArgs e)
        {
            Button enteredButton = (Button)sender;
            hoverButtonName = enteredButton.Name;
            //  DispatcherTimer setup

            hoverTimer.Start();
        }

        /// <summary>
        /// Stops the hover timer when
        /// the cursor exits a button's boundary.
        /// </summary>
        /// <param name="sender">sending object</param>
        /// <param name="e">Mouse Event Arguments</param>
        private void button_exit(object sender, MouseEventArgs e)
        {
            Button exitButton = (Button)sender;
            hoverTimer.Stop();
            if (hoverButtonName != exitButton.Name)
            {
                //Console.WriteLine("MISMATCH: hoverButtonName:{0}, exitButtonName{1}", hoverButtonName, exitButton.Name);
            }
            else
            {
                hoverButtonName = "";
            }
            SetButtonStyleByName(exitButton.Name, defaultButtonStyle);
            InputDirection = new Vector(0, 0);
        }

        /// <summary>
        /// Initializes the button collection.
        /// </summary>
        private void PopulateButtonList()
        {
            if (buttonList == null)
            {
                buttonList = new List<Button>();
            }
            else
            {
                buttonList.Clear();
            }
            buttonList.Add(UpLeftButton);
            buttonList.Add(UpButton);
            buttonList.Add(UpRightButton);
            buttonList.Add(LeftButton);
            buttonList.Add(StopButton);
            buttonList.Add(RightButton);
            buttonList.Add(BackLeftButton);
            buttonList.Add(BackButton);
            buttonList.Add(BackRightButton);
        }

        /// <summary>
        /// Initializes the association between buttons
        /// and directions.
        /// </summary>
        private void PopulateDirectionDictionary()
        {
            if (directionDict == null)
            {
                directionDict = new Dictionary<string, Vector>();
            }
            else { directionDict.Clear(); }
            directionDict.Add(UpLeftButton.Name, new Vector(-1, 1));
            directionDict.Add(UpButton.Name, new Vector(0, 1));
            directionDict.Add(UpRightButton.Name, new Vector(1, 1));
            directionDict.Add(LeftButton.Name, new Vector(-1, 0));
            directionDict.Add(StopButton.Name, new Vector(0, 0));
            directionDict.Add(RightButton.Name, new Vector(1, 0));
            directionDict.Add(BackLeftButton.Name, new Vector(-1, -1));
            directionDict.Add(BackButton.Name, new Vector(0, -1));
            directionDict.Add(BackRightButton.Name, new Vector(1, -1));
        }

        /// <summary>
        /// Updates the boundaries of the buttons.
        /// </summary>
        public void UpdateBounds()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (buttonList == null) return;
                //Console.WriteLine("Updating {0} boundaries...", buttonList.Count);
                if (boundsDict == null)
                {
                    boundsDict = new Dictionary<Rect, String>();
                }
                else
                {
                    boundsDict.Clear();
                }
                foreach (Button button in buttonList)
                {
 
                    Point buttonPoint = button.PointToScreen(new Point(0, 0));
                    Size buttonSize = new Size(button.ActualWidth, button.ActualHeight);
                    boundsDict.Add(new Rect(buttonPoint, buttonSize), button.Name);
                }
            }));

        }

        /// <summary>
        /// Finds which button the gazepoint is hovering over,
        /// if any.
        /// </summary>
        /// <param name="gazePoint">Point that is "looked at"</param>
        /// <returns></returns>
        private Rect BoundsCheck(Point gazePoint)
        {
            bool withinBounds = false;
            foreach (KeyValuePair<Rect, String> pair in boundsDict)
            {
                if (pair.Key.Contains(gazePoint))
                {
                    ////Console.WriteLine("P({0},{1}) in Button[{2}]", gazePoint.X, gazePoint.Y, pair.Value);
                    withinBounds = true;
                    return pair.Key;
                }
            }
            ////Console.WriteLine("P({0},{1}) not in bounds!", gazePoint.X, gazePoint.Y);
            return Rect.Empty;
        }

        /// <summary>
        /// Changes the button's color when it has been "clicked"
        /// </summary>
        /// <param name="sender">sending object</param>
        /// <param name="elapsedEventArgs">elapsed event arguments</param>
        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            // Select Button
            SetButtonStyleByName(selectedButton, selectedButtonStyle);
            //Console.WriteLine("Timer triggered, button[{0}]", selectedButton);
        }

        /// <summary>
        /// Utility for setting button styles by lookup.
        /// </summary>
        /// <param name="name">Name of the button</param>
        /// <param name="buttonStyle">Style to set</param>
        private void SetButtonStyleByName(string name, Style buttonStyle)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                Button button = (Button)FindName(name);
                if (button != null)
                    button.Style = buttonStyle;
            }));
        }

        /// <summary>
        /// "clicks" a button when it has been hovered on
        /// long enough.
        /// </summary>
        /// <param name="sender">sending object</param>
        /// <param name="e">elapsed event arguments</param>
        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Point mousePoint = GetMousePosition();
            ////Console.Write(" T({0}, {1}), ", mousePoint.X, mousePoint.Y);
            Rect buttonRect = BoundsCheck(mousePoint);
            if (!buttonRect.IsEmpty)
            {
                string currentButton = boundsDict[buttonRect];
                // No button selected.
                if (this.selectedButton == null)
                {
                    SetButtonStyleByName(currentButton, selectedButtonStyle);
                    selectedButton = currentButton;
                }
                else if (this.selectedButton == currentButton)
                {
                    return;
                }
                // Different button.
                else if (selectedButton != currentButton)
                {
                    SetButtonStyleByName(selectedButton, defaultButtonStyle);
                    buttonTimer = null;
                    selectedButton = currentButton;
                }
                buttonTimer = new Timer(Data.GazeDelay);
                buttonTimer.Elapsed += TimerOnElapsed;
                buttonTimer.Start();
                buttonTimer.AutoReset = false;
            }
            else
            {
                buttonTimer = null;
                selectedButton = null;
            }
        }

        /// <summary>
        /// Retrieves the cursor position from the operating system.
        /// </summary>
        /// <param name="pt">(out) cursor position.</param>
        /// <returns>False if unsuccessful.</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        /// <summary>
        /// Struct representing a win32 coordinate.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        /// <summary>
        /// Retrieves the mouse position.
        /// </summary>
        /// <returns>The mouse position.</returns>
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        /// <summary>
        /// Sets movement when a button is hovered on.
        /// </summary>
        /// <param name="sender">sending object</param>
        /// <param name="e">generic event arguments</param>
        private void hoverTimer_Tick(object sender, EventArgs e)
        {
            if (directionDict.ContainsKey(hoverButtonName))
            {
                InputDirection = directionDict[hoverButtonName];
                SetButtonStyleByName(hoverButtonName, selectedButtonStyle);
            }
            else
            {
                //Console.WriteLine("HoverTimer_Tick: no key found.");
            }
        }

        public static void ToggleGrid()
        {
            
        }
    }
}
