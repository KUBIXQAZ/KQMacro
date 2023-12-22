using System;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading;
using System.Windows.Controls;
using System.Linq;
using System.Text;
using KQMacro.Custom;

namespace KQMacro
{
    enum StepType
    {
        LeftClick,
        RightClick,
        TypeText,
        ClickButton
    }

    class Step
    {
        public System.Drawing.Point Point { get; set; }
        public StepType StepType { get; set; }
        public string Text { get; set; }
        public Key Button { get; set; }
    }

    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out System.Windows.Point lpPoint);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        List<Step> steps = new List<Step>();

        bool Recording = false;
        StepType StepType = StepType.LeftClick;

        const int MOUSEEVENTF_LEFTDOWN = 0x02;
        const int MOUSEEVENTF_LEFTUP = 0x04;
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const int MOUSEEVENTF_RIGHTUP = 0x0010;

        public MainWindow()
        {
            InitializeComponent();

            UpdateStepType();
        }

        public Key ChooseKey()
        {
            var options = new string[] { "Esc", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12",
                                          "Print Screen", "Scroll Lock", "Pause/Break", "Insert", "Home", "Page Up",
                                          "`", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=", "Backspace",
                                          "Num Lock", "/", "*", "-",
                                          "Tab", "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "[", "]", "\\",
                                          "7 (Num Pad)", "8 (Num Pad)", "9 (Num Pad)", "+",
                                          "Caps Lock", "A", "S", "D", "F", "G", "H", "J", "K", "L", ";", "'", "Enter",
                                          "4 (Num Pad)", "5 (Num Pad)", "6 (Num Pad)",
                                          "Shift", "Z", "X", "C", "V", "B", "N", "M", ",", ".", "/", "Shift",
                                          "1 (Num Pad)", "2 (Num Pad)", "3 (Num Pad)", "Enter",
                                          "Ctrl", "Win", "Alt", "Spacebar", "Alt", "Win", "Menu", "Ctrl",
                                          "0 (Num Pad)", ". (Num Pad)" };
            var customMessageBox = new CustomMessageBox(options);

            Key key = Key.None;
            if (customMessageBox.ShowDialog() == true)
            {
                var selectedOption = customMessageBox.SelectedOption;
                Console.WriteLine($"selected option {selectedOption}");
                Enum.TryParse(selectedOption, out key);
            }
            return key;
        }

        private async Task Check()
        {
            bool CheckKey = true;
            while (Recording == true)
            {
                if (Keyboard.IsKeyDown(Key.H) && CheckKey)
                {
                    CheckKey = false;

                    System.Drawing.Point NewPoint = System.Windows.Forms.Cursor.Position;

                    string Text = (StepType == StepType.TypeText) ? Microsoft.VisualBasic.Interaction.InputBox("Type in text: ", "Input") : null;
                    Key Key = (StepType == StepType.ClickButton) ? ChooseKey() : Key.None;

                    Step NewStep = new Step
                    {
                        Point = (StepType == StepType.TypeText || StepType == StepType.ClickButton ? new System.Drawing.Point(-1,-1) : NewPoint),
                        StepType = StepType,
                        Text = Text,
                        Button = Key
                    };

                    steps.Add(NewStep);
                    UpdateList();

                    Console.WriteLine($"New Step Added: {NewPoint}.");
                }
                else if (Keyboard.IsKeyUp(Key.H) && !CheckKey) CheckKey = true;
                await Task.Delay(1);
            }
        }

        private void UpdateList()
        {
            PointsList.Items.Clear();
            int i = 1;
            foreach (Step step in steps)
            {
                PointsList.Items.Add($"Step {i}: {step.Point} , {step.StepType} {(step.Text != null ? $", Text: {step.Text}" : "")} {(step.Button != Key.None ? $" Key: {step.Button}" : "")}");
                i++;
            }
        }

        private async void StartRecording(object sender, RoutedEventArgs e)
        {
            Recording = true;
            await Check();
        }

        private void StopRecording(object sender, RoutedEventArgs e)
        {
            Recording = false;
        }

        private void PlayMacro(object sender, RoutedEventArgs e)
        {
            if(steps.Count != 0)
            {
                foreach (Step step in steps)
                {
                    if(step.StepType == StepType.TypeText)
                    {
                        SendKeys.SendWait(step.Text);
                    } else if(step.StepType == StepType.ClickButton)
                    {
                        string Key = "{"+step.Button+"}";
                        SendKeys.SendWait(Key);
                    } else if(step.StepType == StepType.LeftClick) 
                    {
                        System.Windows.Forms.Cursor.Position = step.Point;

                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        Thread.Sleep(500);
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    } else if (step.StepType == StepType.RightClick)
                    {
                        System.Windows.Forms.Cursor.Position = step.Point;

                        mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                        Thread.Sleep(500);
                        mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    }
                }
            }
        }

        public void Reset(object sender, RoutedEventArgs e)
        {
            steps.Clear();
            UpdateList();
        }

        private void UpdateStepType()
        {
            foreach (System.Windows.Controls.Button b in StepTypeGrid.Children.OfType<System.Windows.Controls.Button>())
            {
                if (Enum.TryParse(b.Content.ToString(), out StepType ButtonStepType))
                {
                    if (ButtonStepType == StepType)
                    {
                        b.Background = System.Windows.Media.Brushes.DarkGray;
                    } else
                    {
                        b.Background = System.Windows.Media.Brushes.Gray;
                    }
                }
            }
        }

        private void ChangeStepType(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button b = (System.Windows.Controls.Button)sender;
            if (Enum.TryParse(b.Content.ToString(), out StepType NewStepType))
            {
                StepType = NewStepType;

                Console.WriteLine($"Selected StepType: {StepType}");
            }
            UpdateStepType();
        }
    }
}