using System;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;

namespace KQMacro
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out System.Windows.Point lpPoint);

        List<System.Drawing.Point> steps = new List<System.Drawing.Point>();

        bool Recording = false;

        public MainWindow()
        {
            InitializeComponent();
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

                    steps.Add(NewPoint);
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
            foreach (System.Drawing.Point p in steps)
            {
                PointsList.Items.Add($"Step {i}: {p}");
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
    }
}
