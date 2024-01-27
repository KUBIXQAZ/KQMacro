﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Linq;
using KQMacro.Custom;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Runtime.InteropServices.ComTypes;

namespace KQMacro
{
    public enum StepType
    {
        LeftClick,
        RightClick,
        TypeText,
        ClickButton
    }

    public class Step
    {
        public System.Drawing.Point Point { get; set; }
        public StepType StepType { get; set; }
        public string Text { get; set; }
        public Key Button { get; set; }
        public int Delay { get; set; }
    }

    [Serializable]
    public class Macro
    {
        public List<Step> Steps { get; set; }
        public int Loops { get; set; }
    }
    
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out System.Drawing.Point lpPoint);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        List<Step> steps = new List<Step>();

        bool Recording = false;
        StepType StepType = StepType.LeftClick;
        int Loops = 1;
        bool isMacroRunning = false;
        bool cancelMacro = false;
        Macro macro = new Macro();

        const int MOUSEEVENTF_LEFTDOWN = 0x02;
        const int MOUSEEVENTF_LEFTUP = 0x04;
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const int MOUSEEVENTF_RIGHTUP = 0x0010;

        bool instruction = false;

        public MainWindow()
        {
            InitializeComponent();

            UpdateStepType();

            CheckForStartStop();

            PointsList.SelectionChanged += SelectedItemInList;

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(bool));

                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string myAppFolder = Path.Combine(appDataPath, "KQMacro");
                string filePath = Path.Combine(myAppFolder, "settings.xml");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                {
                    instruction = (bool)serializer.Deserialize(fileStream);
                }
            } catch { }

            if (instruction == false) ShowInstruction(null,null);
        }

        public void ShowInstruction(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("To begin recording, click the 'Start Recording' button. Press the 'H' key to add a new step. You can choose the type of step, such as left-click, right-click, type text, or click a button. To delete a step, click on it in the list and use the 'Delete' button. Additionally, for delay steps, you can double-click on them in the list to edit the delay duration. You can specify delays between actions and set the number of times you want the actions to be repeated by clicking on the 'Loops' button. To start and stop the macro, use the F6 key. There's also a reset button to clear all actions from the macro. Save and load your macros to/from a file for convenience.", "Info",MessageBoxButton.OK);
            if(instruction != true)
            {
                instruction = true;
                XmlSerializer serializer = new XmlSerializer(typeof(bool));

                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string myAppFolder = Path.Combine(appDataPath, "KQMacro");
                string filePath = Path.Combine(myAppFolder, "settings.xml");

                if(!Directory.Exists(myAppFolder)) Directory.CreateDirectory(myAppFolder);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(fileStream, instruction);
                }
            }
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
            if(Recording == false)
            {
                Recording = true;
                await Check();
            }
        }

        private void StopRecording(object sender, RoutedEventArgs e)
        {
            if(Recording == true)
            {
                Recording = false;
            }
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
                        b.Background = (Brush)new BrushConverter().ConvertFromString("#383838");
                    } else
                    {
                        b.Background = (Brush)new BrushConverter().ConvertFromString("#242424");
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

        private void AddStepDelay(object sender, RoutedEventArgs e)
        {
            var b = (System.Windows.Controls.Button)sender;

            Step newStep = new Step();
            int delay = 0;

            if (b.Tag.ToString() == "Custom")
            {
                var messageBox = new NumericEntryPopUp(Title, "Type in delay (ms):");
                if (messageBox.ShowDialog() == true) delay = messageBox.selectedNumber;
            }
            else delay = int.Parse(b.Tag.ToString());

            newStep.Point = new System.Drawing.Point(-1, -1);
            newStep.Delay = delay;
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

        private void SelectedItemInList(object sender, SelectionChangedEventArgs e)
        {
            var listings = PointsList.Items;
            if (PointsList.SelectedItems.Count > 0)
            {
                deleteButton.IsEnabled = true;
            }
            else
            {
                deleteButton.IsEnabled = false;
            }
        }

        private void DeleteStep(object sender, RoutedEventArgs e)
        {
            var listings = PointsList.Items;
            if (PointsList.SelectedItems.Count > 0)
            {
                int index = listings.IndexOf(PointsList.SelectedItem);
                steps.RemoveAt(index);
                UpdateList();
            }
        }

        private void LoadMacroB(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.DefaultExt = ".xml";
            dialog.Filter = "XML Files (*.xml)|*.xml";
            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Macro));
                using(XmlReader reader = XmlReader.Create(dialog.FileName))
                {
                    macro = (Macro)serializer.Deserialize(reader);
                    steps = macro.Steps;
                    Loops = macro.Loops;
                    LoopsText.Content = $"Loops: {Loops}";
                    UpdateList();
                }
            }
        }

        private void SaveMacroB(object sender, RoutedEventArgs e)
        {
            if(steps.Count > 0)
            {
                var dialog = new SaveFileDialog();
                dialog.DefaultExt = ".xml";
                dialog.FileName = "new_macro";
                dialog.Filter = "XML Files (*.xml)|*.xml";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fileName = dialog.FileName;

                    XmlSerializer serializer = new XmlSerializer(typeof(Macro));

                    using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
                    {
                        macro.Steps = steps;
                        macro.Loops = Loops;
                        serializer.Serialize(fileStream, macro);
                    }
                }
            }
        }
    }
}