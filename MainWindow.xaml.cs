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

namespace KQMacro
{
    enum StepType
    {
        LeftClick,
        RightClick,
        TypeText,
        //ClickButton
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

                    Step NewStep = new Step
                    {
                        Point = (StepType == StepType.TypeText ? new System.Drawing.Point(-1,-1) : NewPoint),
                        StepType = StepType,
                        Text = Text,
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
                PointsList.Items.Add($"Step {i}: {step.Point} , {step.StepType} {(step.Text != null ? $", Text: {step.Text}" : "")}");
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
                    }
                    else if(step.StepType == StepType.LeftClick) 
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