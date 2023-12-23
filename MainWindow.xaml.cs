using System;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Linq;
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
        public int Delay { get; set; }
    }

    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        List<Step> steps = new List<Step>();

        bool Recording = false;
        StepType StepType = StepType.LeftClick;
        int Loops = 1;
        bool isMacroRunning = false;
        bool cancelMacro = false;

        const int MOUSEEVENTF_LEFTDOWN = 0x02;
        const int MOUSEEVENTF_LEFTUP = 0x04;
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const int MOUSEEVENTF_RIGHTUP = 0x0010;

        public MainWindow()
        {
            InitializeComponent();

            UpdateStepType();

            CheckForStartStop();
        }

        private async void CheckForStartStop()
        {
            bool check = true;
            while (true)
            {
                if (Keyboard.IsKeyDown(Key.F6) && check)
                {
                    check = false;
                    Console.WriteLine("Clicked f6");
                    if (isMacroRunning == true)
                    {
                        cancelMacro = true;
                    }
                    else
                    {
                        cancelMacro = false;
                        Console.WriteLine("1");
                        PlayMacro();
                        Console.WriteLine("2");
                    }
                    await Task.Delay(100);
                }
                else if (Keyboard.IsKeyUp(Key.F6) && !check)
                {
                    check = true;
                    await Task.Delay(10);
                }
                await Task.Delay(10);
            }
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
            var customMessageBox = new ComboBoxPopUp(Title, "Choose key:",options);

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
                PointsList.Items.Add($"Step {i}: {step.Point} , {(step.Delay != 0 ? $"Delay: {step.Delay}ms" : $"{step.StepType}")} {(step.Text != null ? $", Text: {step.Text}" : "")} {(step.Button != Key.None ? $" Key: {step.Button}" : "")}");
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

        private async void PlayMacro()
        {
            isMacroRunning = true;
            if (steps.Count != 0)
            {
                for (int i = 0; i < Loops; i++)
                {
                    foreach (Step step in steps)
                    {
                        if (cancelMacro == true) break;

                        if (step.Delay != 0)
                        {
                            await Task.Delay(step.Delay);
                        }
                        else if (step.StepType == StepType.TypeText)
                        {
                            SendKeys.SendWait(step.Text);
                        }
                        else if (step.StepType == StepType.ClickButton)
                        {
                            string Key = "{" + step.Button + "}";
                            SendKeys.SendWait(Key);
                        }
                        else if (step.StepType == StepType.LeftClick)
                        {
                            System.Windows.Forms.Cursor.Position = step.Point;

                            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                            await Task.Delay(500);
                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        }
                        else if (step.StepType == StepType.RightClick)
                        {
                            System.Windows.Forms.Cursor.Position = step.Point;

                            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                            await Task.Delay(500);
                            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        }
                    }
                }
            }
            isMacroRunning = false;
        }

        private void PlayMacroB(object sender, RoutedEventArgs e)
        {
            if(isMacroRunning == true)
            {
                cancelMacro = true;
            } else
            {
                cancelMacro = false;
                PlayMacro();
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

        private void ChangeLoopsNumber(object sender, RoutedEventArgs e)
        {
            var messageBox = new NumericEntryPopUp(Title, "Type in number of loops:");
            if (messageBox.ShowDialog() == true)
            {
                Loops = messageBox.selectedNumber;

                LoopsText.Content = $"Loops: {Loops}";

                Console.WriteLine($"Selected number of loops: {Loops}");
            }
        }
        private void AddDelay(object sender, RoutedEventArgs e)
        {
            var messageBox = new NumericEntryPopUp(Title, "Type in delay (ms):");
            if(messageBox.ShowDialog() == true)
            {
                Step newStep = new Step
                {
                    Point = new System.Drawing.Point(-1,-1),
                    Delay = messageBox.selectedNumber
                };
                steps.Add(newStep);
                UpdateList();
            }
        }

        private void AddDelay500ms(object sender, RoutedEventArgs e)
        {
            Step newStep = new Step
            {
                Point = new System.Drawing.Point(-1, -1),
                Delay = 500
            };
            steps.Add(newStep);
            UpdateList();
        }

        private void PointsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listings = PointsList.Items;
            if (PointsList.SelectedItems.Count > 0)
            {
                var listing = PointsList.SelectedItem.ToString();
                int index = listings.IndexOf(listing);

                if (steps[index].Delay != 0)
                {
                    var messageBox = new NumericEntryPopUp(Title, "Type in delay (ms):");
                    if (messageBox.ShowDialog() == true)
                    {
                        steps[index].Delay = messageBox.selectedNumber;
                        UpdateList();
                    }
                }
            }
        }
    }
}