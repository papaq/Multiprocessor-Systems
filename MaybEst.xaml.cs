using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Text.RegularExpressions;

namespace WpfLab
{
    /// <summary>
    /// Interaction logic for MaybEst.xaml
    /// </summary>
    public partial class MaybEst : Window
    {
        TextBox noteFist = new TextBox();
        TextBox noteM = new TextBox();
        TextBox noteProc = new TextBox();
        TextBox noteTask = new TextBox();
        DoubleAnimation animateNoteOn = new DoubleAnimation(1, Duration.Automatic);
        DoubleAnimation animateNoteOff = new DoubleAnimation(0, Duration.Automatic);
        private bool dragStarted = false;
        private bool dragCheckStarted = false;
        private bool isCheckedAuto = false;

        private int timeStrongCalc = 20;
        private int timeStrongPlan = 4;
        private double factorStrong = 1;

        Processors[] allProcArr = new Processors[5];                  // Information of each proc
        int[] procLoading = new int[5];                               // Array of meanings for each proc of operations in queue
        List<TasksQueue>[] procOpsList = new List<TasksQueue>[5];     // Queue of tasks for each proc 
        List<Tasks> taskList = new List<Tasks>();                     // Queue of just created tasks
        Thread[] threads = new Thread[5];

        private int maxProc = 0;
        private int minProc = 0;

        Regex regex = new Regex(@"[^\d]+", RegexOptions.IgnoreCase);
        int taskFreq = 1;
        int calcTime = 10000;
        double taskEmerg = 50;
        long limitLow = 1;
        long limitHigh = 2;

        Stopwatch tenSeconds = new Stopwatch();
        static object listLocker = new object();  // Locks array of tasks for each proc
        static object procLocker = new object();  // Locks array of info of proc
        static object taskLocker = new object();  // Locks taskList
        static object bestLocker = new object();  // Locks NOTHING? USED ONCE AND THAT'S WHY COMMENTED
        int cou = 0;
        long opsCou = 0;

        //int check
        int checkTime = 0;
        string actions = "Tony 2015   " + DateTime.Now.ToString("h:mm:ss tt") + "\n";

        public MaybEst()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();

            ButtonResults.Visibility = Visibility.Hidden;
            SecondProc.Visibility = Visibility.Hidden;
            ThirdProc.Visibility = Visibility.Hidden;
            FourthProc.Visibility = Visibility.Hidden;
            FifthProc.Visibility = Visibility.Hidden;
            Properties.Visibility = Visibility.Hidden;
            StartButton.Visibility = Visibility.Hidden;
            DisplayResults.Visibility = Visibility.Hidden;

            for (int i = 0; i < 5; i++)
            {
                allProcArr[i] = new Processors(i);
                procLoading[i] = -i - 1;
                procOpsList[i] = new List<TasksQueue>(i);
            }
        }
        private void InitialiseNote(TextBox note, string text, double leftMargin, double topMargin, double rightMargin, double bottomMargin)
        {
            note.Margin = new Thickness(leftMargin, topMargin, rightMargin, bottomMargin);
            note.Height = 23;
            //note.Width = 450;
            note.VerticalAlignment = VerticalAlignment.Top;
            note.HorizontalAlignment = HorizontalAlignment.Left;
            note.Background = new SolidColorBrush(Color.FromArgb(10, 237, 226, 50));
            note.Text = text;
            note.FontSize = 15;
            //note.IsEnabled = true;
            note.Opacity = 0;
            note.Focusable = false;
            note.AllowDrop = false;
            note.IsHitTestVisible = false;
            Grid.Children.Add(note);
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            if (noteFist.Text == "")
            {
                string text = "Here you can set amount of calculations per second to each processor [1 OOP = 1e-8 s]";
                InitialiseNote(noteFist, text, powerFist.Margin.Left + upperGrid.Margin.Left, powerFist.Margin.Top + powerFist.Height + 12, 0, 0);
            }

            //DoubleAnimation animateNote = new DoubleAnimation(1, Duration.Automatic);
            noteFist.BeginAnimation(OpacityProperty, animateNoteOn);
            //DoubleAnimationUsingPath moveNote = new DoubleAnimationUsingPath();
        }

        private void powerFist_MouseLeave(object sender, MouseEventArgs e)
        {
            //DoubleAnimation animateNote = new DoubleAnimation(0, Duration.Automatic);
            noteFist.BeginAnimation(OpacityProperty, animateNoteOff);
        }

        private void Image_MouseEnter_1(object sender, MouseEventArgs e)
        {
            if (noteM.Text == "")
            {
                string text = "Each digit 1-5 symbolyses processing unit";
                InitialiseNote(noteM, text, powerFist.Margin.Left + upperGrid.Margin.Left, powerFist.Margin.Top - 15, 0, 0);
            }
            //DoubleAnimation animateNote = new DoubleAnimation(1, Duration.Automatic);
            noteM.BeginAnimation(OpacityProperty, animateNoteOn);

        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            //DoubleAnimation animateNote = new DoubleAnimation(0, Duration.Automatic);
            noteM.BeginAnimation(OpacityProperty, animateNoteOff);
        }

        private void CheckBox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (noteProc.Text == "")
            {
                string text = "This autofills all processors' perfomances";
                InitialiseNote(noteProc, text, CheckProc.Margin.Left - 222, CheckProc.Margin.Top - 23, CheckProc.Margin.Right, CheckProc.Margin.Bottom - 3);
            }
            noteProc.BeginAnimation(OpacityProperty, animateNoteOn);
        }

        private void CheckProc_MouseLeave(object sender, MouseEventArgs e)
        {
            noteProc.BeginAnimation(OpacityProperty, animateNoteOff);
        }

        private void CheckTask_MouseEnter(object sender, MouseEventArgs e)
        {
            if (noteTask.Text == "")
            {
                string text = "This autofills the likelihood of tasks";
                InitialiseNote(noteTask, text, CheckTask.Margin.Left + CheckTask.Width + 7, CheckTask.Margin.Top - 23, 0, 0);
            }
            noteTask.BeginAnimation(OpacityProperty, animateNoteOn);
        }

        private void CheckTask_MouseLeave(object sender, MouseEventArgs e)
        {
            noteTask.BeginAnimation(OpacityProperty, animateNoteOff);
        }

        private void CustomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (dragStarted)
            {
                if (Convert.ToInt32(TextDownLimit.Text) > Convert.ToInt32(CustomSlider.Value.ToString()))
                {
                    TextDownLimit.Text = (CustomSlider.Value).ToString();
                }
                TextUpLimit.Text = (CustomSlider.Value).ToString();
            }
        }

        private void CustomSlider_MouseEnter(object sender, MouseEventArgs e)
        {
            dragStarted = true;
        }

        private void CustomSlider_Check_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (dragCheckStarted)
            {
                TextCheck.Text = (checkTime = (int)CustomSlider_Check.Value).ToString();
            }
        }

        private void CustomSlider_Check_MouseEnter(object sender, MouseEventArgs e)
        {
            dragCheckStarted = true;
        }

        private void MakeViewOnly(TextBox text)
        {
            text.AllowDrop = false;
            text.Focusable = false;
            text.IsHitTestVisible = false;
        }

        private void MakeActiveAgain(TextBox text)
        {
            text.AllowDrop = true;
            text.Focusable = true;
            text.IsHitTestVisible = true;
        }

        private void CheckProc_Checked(object sender, RoutedEventArgs e)
        {
            FillRandom();
            MakeViewOnly(textProc1);
            MakeViewOnly(textProc2);
            MakeViewOnly(textProc3);
            MakeViewOnly(textProc4);
            MakeViewOnly(textProc5);

            SecondProc.Visibility = Visibility.Visible;
            ThirdProc.Visibility = Visibility.Visible;
            FourthProc.Visibility = Visibility.Visible;
            FifthProc.Visibility = Visibility.Visible;
            Properties.Visibility = Visibility.Visible;
            StartButton.Visibility = Visibility.Visible;
            DisplayResults.Visibility = Visibility.Hidden;
        }

        private void CheckProc_Unchecked(object sender, RoutedEventArgs e)
        {
            MakeActiveAgain(textProc1);
            MakeActiveAgain(textProc2);
            MakeActiveAgain(textProc3);
            MakeActiveAgain(textProc4);
            MakeActiveAgain(textProc5);

            SecondProc.Visibility = Visibility.Hidden;
            ThirdProc.Visibility = Visibility.Hidden;
            FourthProc.Visibility = Visibility.Hidden;
            FifthProc.Visibility = Visibility.Hidden;
            Properties.Visibility = Visibility.Hidden;
            StartButton.Visibility = Visibility.Hidden;
            DisplayResults.Visibility = Visibility.Hidden;
        }

        private void FindStrongProc()
        {
            maxProc = 0;
            allProcArr[0].strong = false;
            for (int i = 1; i < 5; i++)
            {
                allProcArr[i].strong = false;
                if (allProcArr[i].power > allProcArr[maxProc].power)
                {
                    maxProc = i;
                }
            }
            allProcArr[maxProc].strong = true;
        }

        private void FindWeakProc()
        {
            minProc = 0;
            allProcArr[0].weak = false;
            for (int i = 1; i < 5; i++)
            {
                allProcArr[i].weak = false;
                if (allProcArr[i].power < allProcArr[minProc].power)
                {
                    minProc = i;
                }
            }
            allProcArr[minProc].weak = true;
        }

        private void SetZeros()
        {
            textProc2.Text = "0";
            textProc3.Text = "0";
            textProc4.Text = "0";
            textProc5.Text = "0";
        }

        //private void MarkStrong()
        //{
        //    switch (maxProc)
        //    {
        //        case 0: textProc1.Text = "Weak";
        //            break;
        //        case 1: textProc2.Text = "Weak";
        //            break;
        //        case 2: textProc3.Text = "Weak";
        //            break;
        //        case 3: textProc4.Text = "Weak";
        //            break;
        //        default: textProc5.Text = "Weak";
        //            break;
        //    }
        //}

        private void FillRandom()
        {
            Random ranOps = new Random();
            textProc1.Text = (allProcArr[0].power = (long)Math.Round(ranOps.Next(66, 95) * 1e+5 * 0.997)).ToString();
            textProc2.Text = (allProcArr[1].power = (long)Math.Round(ranOps.Next(66, 95) * 1e+5 * 0.997)).ToString();
            textProc3.Text = (allProcArr[2].power = (long)Math.Round(ranOps.Next(66, 95) * 1e+5 * 0.997)).ToString();
            textProc4.Text = (allProcArr[3].power = (long)Math.Round(ranOps.Next(66, 95) * 1e+5 * 0.997)).ToString();
            textProc5.Text = (allProcArr[4].power = (long)Math.Round(ranOps.Next(66, 95) * 1e+5 * 0.997)).ToString();

            FindStrongProc();
            FindWeakProc();
            SetOperationLimits(allProcArr[minProc].power);
        }

        private void SetOperationLimits(long procOps)
        {
            TextDownLimit.Text = (limitLow = (int)(CustomSlider.Minimum = Math.Round(procOps * 0.02 * taskFreq))).ToString();
            TextUpLimit.Text = (CustomSlider.Value = CustomSlider.Maximum = Math.Round(procOps * 0.2 * taskFreq)).ToString();
        }

        private bool CheckTextValid(TextBox text, int number)
        {
            Random ranOps = new Random();
            if (!((text.Text.Length > 9) || regex.IsMatch(text.Text) || (text.Text == "") || (Convert.ToDecimal(text.Text) == 0) || (Convert.ToDecimal(text.Text) < 1000)))
            {
                text.Text = (allProcArr[number - 1].power = (long)Math.Truncate(Convert.ToDecimal(text.Text))).ToString();
                if (allProcArr[minProc].power > allProcArr[number - 1].power)
                {
                    minProc = number - 1;
                    SetOperationLimits(allProcArr[minProc].power);
                }
                return true;
            }
            text.Text = (10000 + ranOps.Next(0, 90000)).ToString();
            return false;

        }

        private void Proc1OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            minProc = 0;
            if (e.Key == Key.Return)
            {
                if (CheckTextValid(textProc1, 1))
                {
                    SetZeros();
                    SecondProc.Visibility = Visibility.Visible;
                    ThirdProc.Visibility = Visibility.Hidden;
                    FourthProc.Visibility = Visibility.Hidden;
                    FifthProc.Visibility = Visibility.Hidden;
                    Properties.Visibility = Visibility.Hidden;
                    StartButton.Visibility = Visibility.Hidden;
                    DisplayResults.Visibility = Visibility.Hidden;
                }
            }
        }

        private void Proc2OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (CheckTextValid(textProc2, 2))
                {
                    ThirdProc.Visibility = Visibility.Visible;
                    FourthProc.Visibility = Visibility.Hidden;
                    FifthProc.Visibility = Visibility.Hidden;
                    Properties.Visibility = Visibility.Hidden;
                    StartButton.Visibility = Visibility.Hidden;
                    DisplayResults.Visibility = Visibility.Hidden;
                }
            }
        }

        private void Proc3OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (CheckTextValid(textProc3, 3))
                {
                    FourthProc.Visibility = Visibility.Visible;
                    FifthProc.Visibility = Visibility.Hidden;
                    Properties.Visibility = Visibility.Hidden;
                    StartButton.Visibility = Visibility.Hidden;
                    DisplayResults.Visibility = Visibility.Hidden;
                }
            }
        }

        private void Proc4OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (CheckTextValid(textProc4, 4))
                {
                    FifthProc.Visibility = Visibility.Visible;
                    Properties.Visibility = Visibility.Hidden;
                    StartButton.Visibility = Visibility.Hidden;
                    DisplayResults.Visibility = Visibility.Hidden;
                }
            }
        }

        private void Proc5OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (CheckTextValid(textProc5, 5))
                {
                    FindStrongProc();
                    FindWeakProc();
                    SetOperationLimits(allProcArr[minProc].power);

                    Properties.Visibility = Visibility.Visible;
                    StartButton.Visibility = Visibility.Visible;
                    DisplayResults.Visibility = Visibility.Hidden;
                }
            }
        }

        private void TextDownLimit_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((TextDownLimit.Text.Length > 7) || regex.IsMatch(TextDownLimit.Text) || (TextDownLimit.Text == "") || (Convert.ToInt32(TextDownLimit.Text) < CustomSlider.Minimum) || (Convert.ToInt32(TextDownLimit.Text) > CustomSlider.Value))
            {
                TextDownLimit.Text = CustomSlider.Minimum.ToString();
            }
            TextDownLimit.Text = (Math.Truncate(Convert.ToDecimal(TextDownLimit.Text))).ToString();
        }

        private void TextDownLimit_Check()
        {
            if ((TextDownLimit.Text.Length > 7) || regex.IsMatch(TextDownLimit.Text) || (TextDownLimit.Text == "") || (Convert.ToInt32(TextDownLimit.Text) < CustomSlider.Minimum) || (Convert.ToInt32(TextDownLimit.Text) > CustomSlider.Value))
            {
                TextDownLimit.Text = CustomSlider.Minimum.ToString();
            }
            TextDownLimit.Text = (Math.Truncate(Convert.ToDecimal(TextDownLimit.Text))).ToString();
        }

        private void CheckTask_Checked(object sender, RoutedEventArgs e)
        {
            Random ranBe = new Random();
            TextTaskEmerg.Text = (taskEmerg = ranBe.Next(1, 100)).ToString();
            MakeViewOnly(TextTaskEmerg);
        }

        private void CheckTask_Unchecked(object sender, RoutedEventArgs e)
        {
            MakeActiveAgain(TextTaskEmerg);
        }

        private void CheckCalc_Checked(object sender, RoutedEventArgs e)
        {
            isCheckedAuto = true;
        }

        private int Auto_CheckCalc(long maxProcPower)
        {
            double a = (double)limitLow * 1.5 * 1000 / maxProcPower;
            int retres = (int)Math.Round(a + Math.Sqrt(a * a + 16 * a)) / 2;
            return (int)Math.Round(a + Math.Sqrt(a * a + 16 * a)) / 2;
        }



        private void experiment()
        {
            double dexter = 0.2;
            double a;
            long opsDoneSum;
            long powerSum;
            long workPowerSum;

            for (int i = 0; i < 500; i++)
            {
                dexter += 0.05;
                a = (double)limitLow * (dexter) * 1000 / allProcArr[maxProc].power;
                timeStrongCalc = (int)Math.Round(a + Math.Sqrt(a * a + 16 * a)) / 2;

                TextTaskEmerg_Check();
                TextDownLimit_Check();

                limitLow = Convert.ToInt64(TextDownLimit.Text);
                limitHigh = (long)CustomSlider.Value;

                checkTime = Convert.ToInt16(CustomSlider_Check.Value);

                factorStrong = timeStrongCalc / (double)(timeStrongCalc + timeStrongPlan);
                allProcArr[maxProc].power = (long)(allProcArr[maxProc].power * factorStrong);

                for (int j = 0; j < 5; j++)
                {
                    allProcArr[j].opsCounter = 0;
                    allProcArr[j].opsInQueue = 0;
                    allProcArr[j].tasksCounter = 0;
                }
                cou = 0;
                opsCou = 0;

                tenSeconds.Reset();

                StartThreads();

                opsDoneSum = allProcArr[0].opsCounter + allProcArr[1].opsCounter + allProcArr[2].opsCounter + allProcArr[3].opsCounter + allProcArr[4].opsCounter;
                powerSum = allProcArr[0].power + allProcArr[1].power + allProcArr[2].power + allProcArr[3].power + allProcArr[4].power;
                workPowerSum = (long)(allProcArr[4].power * factorStrong);
                for (int k = 0; k < 4; k++)
                {
                    workPowerSum += allProcArr[k].power;
                }

                System.IO.File.AppendAllLines("../Gaaaaaaaaaaaaaaaaal.txt", new string[4] {
                    dexter.ToString(),
                    ((float)(opsDoneSum / (float)(powerSum * calcTime / 1000))).ToString(),
                    ((float)(opsDoneSum / (float)(workPowerSum * calcTime / 1000))).ToString(),
                    " "});
            }
        }


        private void CheckCalc_Unchecked(object sender, RoutedEventArgs e)
        {
            isCheckedAuto = false;
        }

        private void TextTaskEmerg_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (regex.IsMatch(TextTaskEmerg.Text) || (TextTaskEmerg.Text.Length > 2) && (Convert.ToInt32(TextTaskEmerg.Text) > 100) || (TextTaskEmerg.Text.Length > 3))
            {
                taskEmerg = 50;
                TextTaskEmerg.Text = taskEmerg.ToString();
            }
        }

        private void TextTaskEmerg_Check()
        {
            try
            {
                if (regex.IsMatch(TextTaskEmerg.Text) || ((TextTaskEmerg.Text.Length > 2) && (Convert.ToInt32(TextTaskEmerg.Text) > 100)) || (TextTaskEmerg.Text.Length > 3) || (Convert.ToInt32(TextTaskEmerg.Text) < 1) || (TextTaskEmerg.Text.Length == 0))
                {
                    taskEmerg = 50;
                    TextTaskEmerg.Text = taskEmerg.ToString();
                }
                else
                {
                    TextTaskEmerg.Text = (taskEmerg = Convert.ToInt32(TextTaskEmerg.Text)).ToString();
                }
            }
            catch (System.FormatException)
            {
                taskEmerg = 50;
                TextTaskEmerg.Text = taskEmerg.ToString();
            }
        }

        private void TextTaskEmerg_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((Convert.ToInt32(TextTaskEmerg.Text) < 1) || (TextTaskEmerg.Text.Length == 0))
                {
                    taskEmerg = 50;
                    TextTaskEmerg.Text = taskEmerg.ToString();
                }
                else
                {
                    TextTaskEmerg.Text = (taskEmerg = Convert.ToInt32(TextTaskEmerg.Text)).ToString();
                }
            }
            catch (System.FormatException)
            {
                taskEmerg = 50;
                TextTaskEmerg.Text = taskEmerg.ToString();
            }
        }

        private void OutputResults(string text)
        {
            System.IO.File.WriteAllText("../WriteText3.txt", text);
        }

        private void ButtonResults_Click(object sender, RoutedEventArgs e)
        {

            int tasksSum = allProcArr[0].tasksCounter + allProcArr[1].tasksCounter + allProcArr[2].tasksCounter + allProcArr[3].tasksCounter + allProcArr[4].tasksCounter;
            long opsDoneSum = allProcArr[0].opsCounter + allProcArr[1].opsCounter + allProcArr[2].opsCounter + allProcArr[3].opsCounter + allProcArr[4].opsCounter;
            long powerSum = allProcArr[0].power + allProcArr[1].power + allProcArr[2].power + allProcArr[3].power + allProcArr[4].power;
            long workPowerSum = (long)(allProcArr[4].power * factorStrong);

            for (int i = 0; i < 4; i++)
            {
                workPowerSum += allProcArr[i].power;
            }

            System.IO.File.WriteAllText("../GeneralResults3.txt", "Tony 2015   " + DateTime.Now.ToString("h:mm:ss tt") + "\n");
            System.IO.File.AppendAllLines("../GeneralResults3.txt",
                (new GeneralResults(cou, tasksSum,
                    opsCou, opsDoneSum,
                    powerSum, workPowerSum, calcTime / 1000,
                tenSeconds, (int)taskEmerg, taskFreq, limitLow, limitHigh, checkTime)).lines);

            for (int i = 0; i < 5; i++)
            {
                string parttime = "All";
                double midOps = limitLow * 5.5;
                if (allProcArr[i].strong)
                {
                    parttime = (Math.Round((double)timeStrongCalc / (double)(timeStrongCalc + timeStrongPlan), 3)).ToString()
                        + " ( "
                        + timeStrongCalc.ToString()
                        + " / "
                        + (timeStrongCalc + timeStrongPlan).ToString()
                        + " )";
                    midOps /= factorStrong;
                }
                System.IO.File.AppendAllLines("../GeneralResults3.txt",
                (new GeneralResults(i, parttime, allProcArr[i].tasksCounter, tasksSum,
                    allProcArr[i].opsCounter, opsDoneSum, allProcArr[i].power, midOps)).lines);
            }

        }

        private void ButtonResults_MouseLeave(object sender, MouseEventArgs e)
        {
            //ButtonResults.Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextTaskEmerg_Check();
            TextDownLimit_Check();

            limitLow = Convert.ToInt64(TextDownLimit.Text);
            limitHigh = (long)CustomSlider.Value;

            checkTime = Convert.ToInt16(CustomSlider_Check.Value);

            if (isCheckedAuto)
            {
                timeStrongCalc = Auto_CheckCalc(allProcArr[maxProc].power);
                if (timeStrongCalc < 1)
                {
                    timeStrongCalc = 1;
                }
            }
            else
            {
                timeStrongCalc = 20;
            }

            factorStrong = timeStrongCalc / (double)(timeStrongCalc + timeStrongPlan);
            allProcArr[maxProc].power = (long)((double)allProcArr[maxProc].power * factorStrong);

            for (int i = 0; i < 5; i++)
            {
                allProcArr[i].opsCounter = 0;
                allProcArr[i].opsInQueue = 0;
                allProcArr[i].tasksCounter = 0;
            }
            cou = 0;
            opsCou = 0;

            tenSeconds.Reset();


            //experiment();
            StartThreads();

            allProcArr[maxProc].power = (long)((double)allProcArr[maxProc].power / factorStrong);
            ButtonResults.Visibility = Visibility.Visible;
            DisplayResults.Visibility = Visibility.Visible;
            OutputResults(actions);
        }

        private void ComboFrequency_SelectionChanged(object sender, MouseEventArgs e)
        {
            if (ComboFrequency.Text != "")
            {
                /*CustomSlider_Check.Maximum = */
                taskFreq = Convert.ToInt16(ComboFrequency.Text.Trim(new char[3] { ' ', 'm', 's' }));
                //TextCheck.Text = (CustomSlider_Check.Value = CustomSlider_Check.Maximum).ToString();
                //CustomSlider_Check.Value = CustomSlider_Check.Maximum;

                SetOperationLimits(allProcArr[minProc].power);
            }
        }

        private int FindBest(int[] loadArray, int[] available)
        {
            int[] loading = new int[5];
            for (int i = 0; i < 5; i++)
            {
                if (available[i] == 0)
                {
                    loading[i] = int.MaxValue;
                }
                else
                {
                    loading[i] = loadArray[i];
                }

                //actions += loading[i].ToString() + " ";
            }





            // aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa




            int retValue = Array.IndexOf(loading, loading.Min());

            //actions += "\n" + retValue.ToString() + "\n";
            return retValue;
        }

        // Threads further

        private void CreatTasks()
        {
            int counter = 0;

            int proc1;
            int proc2;
            int proc3;
            int proc4;
            int proc5;
            int diff;

            double beginPoint;

            //int emrg;
            double hundEmrg = tenSeconds.Elapsed.TotalMilliseconds;

            Random rand = new Random();

            while (tenSeconds.ElapsedMilliseconds < calcTime)
            {
                beginPoint = tenSeconds.Elapsed.TotalMilliseconds;
                diff = 0;
                //emrg = rand.Next(0, 101);
                if (rand.Next(0, 101) <= taskEmerg)
                {
                    proc1 = rand.Next(0, 2);
                    proc2 = rand.Next(0, 2);
                    proc3 = rand.Next(0, 2);
                    proc4 = rand.Next(0, 2);
                    proc5 = rand.Next(0, 2);
                    diff = rand.Next((Int32)limitLow, (Int32)limitHigh + 1);

                    actions += proc1.ToString() + " " + proc2.ToString() + " " + proc3.ToString() + " " + proc4.ToString() + " " + proc5.ToString() + "\n"; 
                    //actions += diff.ToString() + "\n";
                    if (proc1 + proc2 + proc3 + proc4 + proc5 == 0)
                    {
                        continue;
                    }

                    lock (taskLocker)
                    {
                        taskList.Add(new Tasks(proc1, proc2, proc3, proc4, proc5, diff));
                    }

                    opsCou += diff;
                    counter++;
                }

                if (counter % 10 == 0)
                {
                    hundEmrg = counter * taskFreq / (taskEmerg / 100) - tenSeconds.Elapsed.TotalMilliseconds;
                    //actions += hundEmrg.ToString() + "\n";
                    if (hundEmrg > 0)
                        Thread.Sleep(TimeSpan.FromMilliseconds(hundEmrg));
                    //hundEmrg = tenSeconds.Elapsed.TotalMilliseconds;
                }
            }

            cou = counter;
        }

        private void Loading()
        {
            while (true)
            {
                lock (procLocker)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        procLoading[i] = (int)Math.Round((double)((allProcArr[i].opsInQueue + 1) * 1000 / allProcArr[i].power));
                        //actions += procLoading[i].ToString() + " ";
                        //actions += allProcArr[i].opsInQueue.ToString() + " ";
                    }
                }
                //actions += "\n";
                Thread.Sleep(5);
            }
        }

        private void ProcStrong(int procNumber)
        {
            double setDelay = 0,  // General delay of temp task
                tempDelay = 0;    // Temporary delay during this calc sector
            long difficulty = 1,
                caclDifficulty = 1;

            double beginPoint;

            int bestProc;

            while (true)
            {
                beginPoint = tenSeconds.Elapsed.TotalMilliseconds;
                // Planning sector
                while (tenSeconds.Elapsed.TotalMilliseconds - beginPoint < timeStrongPlan)
                {
                    if (taskList.Count() > 0)
                    {
                        lock (taskLocker)
                        {
                            bestProc = FindBest(procLoading, new int[5] { 
                            taskList[0].proces[0], 
                            taskList[0].proces[1], 
                            taskList[0].proces[2], 
                            taskList[0].proces[3], 
                            taskList[0].proces[4] });
                            difficulty = taskList[0].difficulty;
                            if (bestProc == -1)
                            {
                                continue;
                            }
                            taskList.RemoveAt(0);
                        }

                        lock (listLocker)
                        {
                            procOpsList[bestProc].Add(new TasksQueue(difficulty));
                        }

                        lock (procLocker)
                        {
                            allProcArr[bestProc].opsInQueue += difficulty;
                        }
                    }
                }

                beginPoint = tenSeconds.Elapsed.TotalMilliseconds;
                // Calculating sector
                while (tenSeconds.Elapsed.TotalMilliseconds - beginPoint < timeStrongCalc)
                {
                    if (procOpsList[procNumber].Count() != 0 || setDelay != 0)
                    {
                        if (setDelay == 0)
                        {
                            lock (listLocker)
                            {
                                caclDifficulty = procOpsList[procNumber][0].difficulty;
                                procOpsList[procNumber].RemoveAt(0);
                            }
                            setDelay = ((caclDifficulty + 1) * 1000 / allProcArr[procNumber].power);
                        }

                        if (setDelay > timeStrongCalc + beginPoint - tenSeconds.Elapsed.TotalMilliseconds)
                        {
                            tempDelay = timeStrongCalc + beginPoint - tenSeconds.Elapsed.TotalMilliseconds;
                        }
                        else
                        {
                            tempDelay = setDelay;

                            lock (procLocker)
                            {
                                allProcArr[procNumber].opsInQueue -= caclDifficulty;
                            }

                            allProcArr[procNumber].tasksCounter++;
                            allProcArr[procNumber].opsCounter += caclDifficulty;
                        }

                        if (tempDelay > 0)
                        {
                            setDelay = setDelay - tempDelay;

                            Thread.Sleep(TimeSpan.FromMilliseconds(tempDelay));
                        }
                    }
                }
            }
        }

        private void Proc(int procNumber)
        {
            long setDelay = 0;
            long difficulty = 1;
            while (true)
            {
                //setDelay = checkTime;
                if (procOpsList[procNumber].Count() != 0)
                {

                    lock (listLocker)
                    {
                        difficulty = procOpsList[procNumber][0].difficulty;
                        procOpsList[procNumber].RemoveAt(0);
                    }
                    setDelay = ((difficulty) * 1000 / allProcArr[procNumber].power);

                    Thread.Sleep(TimeSpan.FromMilliseconds(setDelay));

                    lock (procLocker)
                    {
                        allProcArr[procNumber].opsInQueue -= difficulty;
                    }

                    allProcArr[procNumber].tasksCounter++;
                    allProcArr[procNumber].opsCounter += difficulty;
                }
            }
        }

        private void StartThreads()
        {
            Thread tasks = new Thread(new ThreadStart(CreatTasks));
            Thread loading = new Thread(new ThreadStart(Loading));

            for (int d = 0; d < 5; d++)
            {
                if (d != maxProc)
                {
                    switch (d)
                    {
                        case 0: threads[d] = new Thread(() => Proc(0));
                            break;
                        case 1: threads[d] = new Thread(() => Proc(1));
                            break;
                        case 2: threads[d] = new Thread(() => Proc(2));
                            break;
                        case 3: threads[d] = new Thread(() => Proc(3));
                            break;
                        case 4: threads[d] = new Thread(() => Proc(4));
                            break;
                    }
                }
                else
                {
                    threads[d] = new Thread(() => ProcStrong(maxProc));
                }
            }

            tenSeconds.Start();
            try
            {
                tasks.Start();
                loading.Start();
                foreach (Thread thrd in threads)
                {
                    thrd.Start();
                }

                tasks.Join();

                foreach (Thread thrd in threads)
                {
                    thrd.Abort();
                }

                loading.Abort();

                for (int k = 0; k < 5; k++)
                {
                    procOpsList[k].Clear();
                }

                taskList.Clear();

            }
            catch (ThreadStateException e)
            {
                Console.WriteLine("Caught: {0}", e.Message);
            }
            tenSeconds.Stop();

            for (int l = 0; l < 5; l++)
            {
                switch (l)
                {
                    case 0: textQueueProc1.Text = allProcArr[0].opsInQueue.ToString();
                        textDoneProc1.Text = allProcArr[0].opsCounter.ToString();
                        break;
                    case 1: textQueueProc2.Text = allProcArr[1].opsInQueue.ToString();
                        textDoneProc2.Text = allProcArr[1].opsCounter.ToString();
                        break;
                    case 2: textQueueProc3.Text = allProcArr[2].opsInQueue.ToString();
                        textDoneProc3.Text = allProcArr[2].opsCounter.ToString();
                        break;
                    case 3: textQueueProc4.Text = allProcArr[3].opsInQueue.ToString();
                        textDoneProc4.Text = allProcArr[3].opsCounter.ToString();
                        break;
                    default: textQueueProc5.Text = allProcArr[4].opsInQueue.ToString();
                        textDoneProc5.Text = allProcArr[4].opsCounter.ToString();
                        break;
                }
            }

            //actions += opsCou.ToString() + " "
            //    + (allProcArr[1].opsCounter
            //    + allProcArr[0].opsCounter
            //    + allProcArr[2].opsCounter
            //    + allProcArr[3].opsCounter
            //    + allProcArr[4].opsCounter).ToString();
        }
    }
}
