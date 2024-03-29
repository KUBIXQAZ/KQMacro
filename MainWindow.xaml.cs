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
        public String Button { get; set; }
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

        public static string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string myAppFolder = Path.Combine(appDataPath, "KUBIXQAZ/KQMacro");
        public static string filePath = Path.Combine(myAppFolder, "settings.xml");

        public MainWindow()
        {
            InitializeComponent();

            UpdateStepType();

            CheckForStartStop();

            PointsList.SelectionChanged += SelectedItemInList;

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(bool));

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

        public string ChooseKey()
        {
            Dictionary<string, Key> options = new Dictionary<string, Key>
        {
            {"AbntC1", Key.AbntC1},
            {"AbntC2", Key.AbntC2},
            {"Add", Key.Add},
            {"Apps", Key.Apps},
            {"Attn", Key.Attn},
            {"B", Key.B},
            {"Back", Key.Back},
            {"BrowserBack", Key.BrowserBack},
            {"BrowserFavorites", Key.BrowserFavorites},
            {"BrowserForward", Key.BrowserForward},
            {"BrowserHome", Key.BrowserHome},
            {"BrowserRefresh", Key.BrowserRefresh},
            {"BrowserSearch", Key.BrowserSearch},
            {"BrowserStop", Key.BrowserStop},
            {"C", Key.C},
            {"Cancel", Key.Cancel},
            {"Capital", Key.Capital},
            {"CapsLock", Key.CapsLock},
            {"Clear", Key.Clear},
            {"CrSel", Key.CrSel},
            {"D", Key.D},
            {"D0", Key.D0},
            {"D1", Key.D1},
            {"D2", Key.D2},
            {"D3", Key.D3},
            {"D4", Key.D4},
            {"D5", Key.D5},
            {"D6", Key.D6},
            {"D7", Key.D7},
            {"D8", Key.D8},
            {"D9", Key.D9},
            {"DbeAlphanumeric", Key.DbeAlphanumeric},
            {"DbeCodeInput", Key.DbeCodeInput},
            {"DbeDbcsChar", Key.DbeDbcsChar},
            {"DbeDetermineString", Key.DbeDetermineString},
            {"DbeEnterDialogConversionMode", Key.DbeEnterDialogConversionMode},
            {"DbeEnterImeConfigureMode", Key.DbeEnterImeConfigureMode},
            {"DbeEnterWordRegisterMode", Key.DbeEnterWordRegisterMode},
            {"DbeFlushString", Key.DbeFlushString},
            {"DbeHiragana", Key.DbeHiragana},
            {"DbeKatakana", Key.DbeKatakana},
            {"DbeNoCodeInput", Key.DbeNoCodeInput},
            {"DbeNoRoman", Key.DbeNoRoman},
            {"DbeRoman", Key.DbeRoman},
            {"DbeSbcsChar", Key.DbeSbcsChar},
            {"DeadCharProcessed", Key.DeadCharProcessed},
            {"Decimal", Key.Decimal},
            {"Delete", Key.Delete},
            {"Divide", Key.Divide},
            {"Down", Key.Down},
            {"E", Key.E},
            {"End", Key.End},
            {"Enter", Key.Enter},
            {"EraseEof", Key.EraseEof},
            {"Escape", Key.Escape},
            {"Execute", Key.Execute},
            {"ExSel", Key.ExSel},
            {"F", Key.F},
            {"F1", Key.F1},
            {"F10", Key.F10},
            {"F11", Key.F11},
            {"F12", Key.F12},
            {"F13", Key.F13},
            {"F14", Key.F14},
            {"F15", Key.F15},
            {"F16", Key.F16},
            {"F17", Key.F17},
            {"F18", Key.F18},
            {"F19", Key.F19},
            {"F2", Key.F2},
            {"F20", Key.F20},
            {"F21", Key.F21},
            {"F22", Key.F22},
            {"F23", Key.F23},
            {"F24", Key.F24},
            {"F3", Key.F3},
            {"F4", Key.F4},
            {"F5", Key.F5},
            {"F6", Key.F6},
            {"F7", Key.F7},
            {"F8", Key.F8},
            {"F9", Key.F9},
            {"FinalMode", Key.FinalMode},
            {"G", Key.G},
            {"H", Key.H},
            {"HangulMode", Key.HangulMode},
            {"HanjaMode", Key.HanjaMode},
            {"Help", Key.Help},
            {"Home", Key.Home},
            {"I", Key.I},
            {"ImeAccept", Key.ImeAccept},
            {"ImeConvert", Key.ImeConvert},
            {"ImeModeChange", Key.ImeModeChange},
            {"ImeNonConvert", Key.ImeNonConvert},
            {"ImeProcessed", Key.ImeProcessed},
            {"Insert", Key.Insert},
            {"J", Key.J},
            {"JunjaMode", Key.JunjaMode},
            {"K", Key.K},
            {"KanaMode", Key.KanaMode},
            {"KanjiMode", Key.KanjiMode},
            {"L", Key.L},
            {"LaunchApplication1", Key.LaunchApplication1},
            {"LaunchApplication2", Key.LaunchApplication2},
            {"LaunchMail", Key.LaunchMail},
            {"Left", Key.Left},
            {"LeftAlt", Key.LeftAlt},
            {"LeftCtrl", Key.LeftCtrl},
            {"LeftShift", Key.LeftShift},
            {"LineFeed", Key.LineFeed},
            {"LWin", Key.LWin},
            {"M", Key.M},
            {"MediaNextTrack", Key.MediaNextTrack},
            {"MediaPlayPause", Key.MediaPlayPause},
            {"MediaPreviousTrack", Key.MediaPreviousTrack},
            {"MediaStop", Key.MediaStop},
            {"Multiply", Key.Multiply},
            {"N", Key.N},
            {"Next", Key.Next},
            {"NoName", Key.NoName},
            {"None", Key.None},
            {"NumLock", Key.NumLock},
            {"NumPad0", Key.NumPad0},
            {"NumPad1", Key.NumPad1},
            {"NumPad2", Key.NumPad2},
            {"NumPad3", Key.NumPad3},
            {"NumPad4", Key.NumPad4},
            {"NumPad5", Key.NumPad5},
            {"NumPad6", Key.NumPad6},
            {"NumPad7", Key.NumPad7},
            {"NumPad8", Key.NumPad8},
            {"NumPad9", Key.NumPad9},
            {"O", Key.O},
            {"Oem1", Key.Oem1},
            {"Oem102", Key.Oem102},
            {"Oem2", Key.Oem2},
            {"Oem3", Key.Oem3},
            {"Oem4", Key.Oem4},
            {"Oem5", Key.Oem5},
            {"Oem6", Key.Oem6},
            {"Oem7", Key.Oem7},
            {"Oem8", Key.Oem8},
            {"OemAttn", Key.OemAttn},
            {"OemAuto", Key.OemAuto},
            {"OemBackslash", Key.OemBackslash},
            {"OemBackTab", Key.OemBackTab},
            {"OemClear", Key.OemClear},
            {"OemCloseBrackets", Key.OemCloseBrackets},
            {"OemComma", Key.OemComma},
            {"OemCopy", Key.OemCopy},
            {"OemEnlw", Key.OemEnlw},
            {"OemFinish", Key.OemFinish},
            {"OemMinus", Key.OemMinus},
            {"OemOpenBrackets", Key.OemOpenBrackets},
            {"OemPeriod", Key.OemPeriod},
            {"OemPipe", Key.OemPipe},
            {"OemPlus", Key.OemPlus},
            {"OemQuestion", Key.OemQuestion},
            {"OemQuotes", Key.OemQuotes},
            {"OemSemicolon", Key.OemSemicolon},
            {"OemTilde", Key.OemTilde},
            {"P", Key.P},
            {"Pa1", Key.Pa1},
            {"PageDown", Key.PageDown},
            {"PageUp", Key.PageUp},
            {"Pause", Key.Pause},
            {"Play", Key.Play},
            {"Print", Key.Print},
            {"PrintScreen", Key.PrintScreen},
            {"Prior", Key.Prior},
            {"Q", Key.Q},
            {"R", Key.R},
            {"Return", Key.Return},
            {"Right", Key.Right},
            {"RightAlt", Key.RightAlt},
            {"RightCtrl", Key.RightCtrl},
            {"RightShift", Key.RightShift},
            {"RWin", Key.RWin},
            {"S", Key.S},
            {"Scroll", Key.Scroll},
            {"Select", Key.Select},
            {"SelectMedia", Key.SelectMedia},
            {"Separator", Key.Separator},
            {"Sleep", Key.Sleep},
            {"Snapshot", Key.Snapshot},
            {"Space", Key.Space},
            {"Subtract", Key.Subtract},
            {"System", Key.System},
            {"T", Key.T},
            {"Tab", Key.Tab},
            {"U", Key.U},
            {"Up", Key.Up},
            {"V", Key.V},
            {"VolumeDown", Key.VolumeDown},
            {"VolumeMute", Key.VolumeMute},
            {"VolumeUp", Key.VolumeUp},
            {"W", Key.W},
            {"X", Key.X},
            {"Y", Key.Y},
            {"Z", Key.Z},
            {"Zoom", Key.Zoom}
        };

            var customMessageBox = new ComboBoxPopUp(Title, "Choose key:",options.Keys.ToArray());

            string key = "";
            if (customMessageBox.ShowDialog() == true)
            {
                var selectedOption = customMessageBox.SelectedOption;
                Console.WriteLine($"selected option {selectedOption}");
                return selectedOption;
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
                    string Key = (StepType == StepType.ClickButton) ? ChooseKey() : "";

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
                PointsList.Items.Add($"Step {i}: {step.Point} , {(step.Delay != 0 ? $"Delay: {step.Delay}ms" : $"{step.StepType}")} {(step.Text != null ? $", Text: {step.Text}" : "")} {(string.IsNullOrEmpty(step.Button) ? "" : $" Key: {step.Button}")}");
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
                            SendKeys.SendWait("{"+step.Button+"}");
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