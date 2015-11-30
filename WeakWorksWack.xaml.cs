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
    /// Interaction logic for WeakWorksWack.xaml
    /// </summary>
    public partial class WeakWorksWack : Window
    {
        TextBox noteFist = new TextBox();
        TextBox noteM = new TextBox();
        TextBox noteProc = new TextBox();
        TextBox noteTask = new TextBox();
        DoubleAnimation animateNoteOn = new DoubleAnimation(1, Duration.Automatic);
        DoubleAnimation animateNoteOff = new DoubleAnimation(0, Duration.Automatic);
        private bool dragStarted = false;
        private bool dragCheckStarted = false;

        Processors[] allProcArr = new Processors[5];
        Processors[] workProcArr = new Processors[4];
        int[] procLoading = new int[4];
        List<TasksQueue>[] procOpsList = new List<TasksQueue>[4];

        private int minProc = 0;
        Regex regex = new Regex(@"[^\d]+", RegexOptions.IgnoreCase);
        int taskFreq = 1;
        int calcTime = 10000;
        double taskEmerg = 50;
        long limitLow = 1;
        long limitHigh = 2;

        Stopwatch tenSeconds = new Stopwatch();
        static object listLocker = new object();
        static object procLocker = new object();
        static object bestLocker = new object();
        int cou = 0;
        long opsCou = 0;

        int checkTime = 0;
        string actions = "Tony 2015   " + DateTime.Now.ToString("h:mm:ss tt") + "\n";
        
        public WeakWorksWack()
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
            }
            for (int i = 0; i < 4; i++)
            {
                procLoading[i] = -i - 1;
                workProcArr[i] = new Processors(i);
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

        private void MarkWeak()
        {
            switch (minProc)
            {
                case 0: textProc1.Text = "Weak";
                    break;
                case 1: textProc2.Text = "Weak";
                    break;
                case 2: textProc3.Text = "Weak";
                    break;
                case 3: textProc4.Text = "Weak";
                    break;
                default: textProc5.Text = "Weak";
                    break;
            }
        }

        private void FillRandom()
        {
            Random ranOps = new Random();
            textProc1.Text = (allProcArr[0].power = (long)Math.Round(ranOps.Next(66, 95) * 1e+5 * 0.997)).ToString();
            textProc2.Text = (allProcArr[1].power = (long)Math.Round(ranOps.Next(66, 95) * 1e+5 * 0.997)).ToString();
            textProc3.Text = (allProcArr[2].power = (long)Math.Round(ranOps.Next(66, 95) * 1e+5 * 0.997)).ToString();
            textProc4.Text = (allProcArr[3].power = (long)Math.Round(ranOps.Next(66, 95) * 1e+5 * 0.997)).ToString();
            textProc5.Text = (allProcArr[4].power = (long)Math.Round(ranOps.Next(66, 95) * 1e+5 * 0.997)).ToString();

            FindWeakProc();
            MarkWeak();
            SetOperationLimits(allProcArr[minProc].power);
        }

        private void SetOperationLimits(long procOps)
        {
            TextDownLimit.Text = (CustomSlider.Minimum = Math.Round(procOps * 0.02 * taskFreq)).ToString();
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
                    //FindWeakProc();
                    MarkWeak();

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
            System.IO.File.WriteAllText("../WriteText2.txt", text);
        }

        private void ButtonResults_Click(object sender, RoutedEventArgs e)
        {

            int tasksSum = allProcArr[0].tasksCounter + allProcArr[1].tasksCounter + allProcArr[2].tasksCounter + allProcArr[3].tasksCounter + allProcArr[4].tasksCounter;
            long opsDoneSum = allProcArr[0].opsCounter + allProcArr[1].opsCounter + allProcArr[2].opsCounter + allProcArr[3].opsCounter + allProcArr[4].opsCounter;
            long powerSum = allProcArr[0].power + allProcArr[1].power + allProcArr[2].power + allProcArr[3].power + allProcArr[4].power;
            long workPowerSum = 0;

            for (int i = 0; i < 5; i++)
            {
                if (!allProcArr[i].weak)
                {
                    workPowerSum += allProcArr[i].power;
                }
            }

            System.IO.File.WriteAllLines("../GeneralResults2.txt",
                (new GeneralResults(cou, tasksSum,
                    opsCou, opsDoneSum,
                    powerSum, workPowerSum, calcTime / 1000,
                tenSeconds, (int)taskEmerg, taskFreq, limitLow, limitHigh, checkTime)).lines);

            for (int i = 0; i < 5; i++)
            {
                string parttime = "All";
                if (allProcArr[i].weak)
                {
                    parttime = "Zero";
                }
                System.IO.File.AppendAllLines("../GeneralResults2.txt",
                (new GeneralResults(i, parttime, allProcArr[i].tasksCounter, tasksSum,
                    allProcArr[i].opsCounter, opsDoneSum, allProcArr[i].power, limitLow * 5.5)).lines);
            }

        }

        private void ButtonResults_MouseLeave(object sender, MouseEventArgs e)
        {
            ButtonResults.Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextTaskEmerg_Check();
            TextDownLimit_Check();

            SetOperationLimits(allProcArr[minProc].power);
            limitLow = Convert.ToInt64(TextDownLimit.Text);
            limitHigh = (long)CustomSlider.Value;
            
            checkTime = Convert.ToInt16(CustomSlider_Check.Value);

            for (int i = 0; i < 5; i++)
            {
                allProcArr[i].opsCounter = 0;
                allProcArr[i].opsInQueue = 0;
                allProcArr[i].tasksCounter = 0;
            }

            cou = 0;
            opsCou = 0;

            int j = 0;
            for (int i = 0; i < 4; i++)
            {
                if (allProcArr[j].weak)
                {
                    j++;
                }
                workProcArr[i] = allProcArr[j];
                j++;
            }


            tenSeconds.Reset();

            StartThreads();
            ButtonResults.Visibility = Visibility.Visible;
            DisplayResults.Visibility = Visibility.Visible;
            OutputResults(actions);
        }

        private void ComboFrequency_SelectionChanged(object sender, MouseEventArgs e)
        {
            if (ComboFriquency.Text != "")
            {
                /*CustomSlider_Check.Maximum = */
                taskFreq = Convert.ToInt16(ComboFriquency.Text.Trim(new char[3] { ' ', 'm', 's' }));
                //TextCheck.Text = (CustomSlider_Check.Value = CustomSlider_Check.Maximum).ToString();
                //CustomSlider_Check.Value = CustomSlider_Check.Maximum;

                SetOperationLimits(allProcArr[minProc].power);
            }
        }

        private int FindBest(int[] loadArray, int[] available)
        {
            int[] loading = new int[4];
            for (int i = 0; i < 4; i++)
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
            int diff;
            int bestProc;
            double beginPoint;
            int emrg;
            double hundEmrg = tenSeconds.Elapsed.TotalMilliseconds;

            Random rand = new Random();

            while (tenSeconds.ElapsedMilliseconds < calcTime)
            {
                beginPoint = tenSeconds.Elapsed.TotalMilliseconds;
                diff = 0;
                emrg = rand.Next(0, 101);
                if (rand.Next(0, 101) <= taskEmerg)
                {
                    proc1 = rand.Next(0, 2);
                    proc2 = rand.Next(0, 2);
                    proc3 = rand.Next(0, 2);
                    proc4 = rand.Next(0, 2);
                    diff = rand.Next((Int32)limitLow, (Int32)limitHigh + 1);

                    if (proc1 + proc2 + proc3 + proc4 == 0)
                    {
                        continue;
                    }

                    opsCou += diff;
                    bestProc = FindBest(procLoading, new int[4] { proc1, proc2, proc3, proc4 });
                    lock (listLocker)
                    {
                        procOpsList[bestProc].Add(new TasksQueue(diff));
                    }
                    lock (procLocker)
                    {
                        workProcArr[bestProc].opsInQueue += diff;
                    }
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
                lock (bestLocker)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        procLoading[i] = (int)Math.Round((double)((workProcArr[i].opsInQueue + 1) * 1000 / workProcArr[i].power));
                        //actions += procLoading[i].ToString() + " ";
                    }
                }
                //actions += "\n";
                Thread.Sleep(5);
            }
        }


        private void Proc5(int procNumber)
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
                    setDelay = ((difficulty + 1) * 1000 / workProcArr[procNumber].power);

                    Thread.Sleep(TimeSpan.FromMilliseconds(setDelay));

                    lock (procLocker)
                    {
                        workProcArr[procNumber].opsInQueue -= difficulty;
                    }

                    workProcArr[procNumber].tasksCounter++;
                    workProcArr[procNumber].opsCounter += difficulty;
                }
            }
        }

        private void StartThreads()
        {
            Thread tasks = new Thread(new ThreadStart(CreatTasks));
            Thread loading = new Thread(new ThreadStart(Loading));
            Thread proc1 = new Thread(() => Proc5(0));
            Thread proc2 = new Thread(() => Proc5(1));
            Thread proc3 = new Thread(() => Proc5(2));
            Thread proc4 = new Thread(() => Proc5(3));

            tenSeconds.Start();
            try
            {
                tasks.Start();
                loading.Start();
                proc1.Start();
                proc2.Start();
                proc3.Start();
                proc4.Start();

                tasks.Join();

                proc4.Abort();
                proc3.Abort();
                proc2.Abort();
                proc1.Abort();
                loading.Abort();

                for (int i = 0; i < 4; i++)
                {
                    procOpsList[i].Clear();
                }

            }
            catch (ThreadStateException e)
            {
                Console.WriteLine("Caught: {0}", e.Message);
            }
            tenSeconds.Stop();

            int j = 0;
            for (int i = 0; i < 4; i++)
            {
                if (allProcArr[j].weak)
                {
                    j++;
                }
                allProcArr[j] = workProcArr[i];
                j++;
            }


            for (int i = 0; i < 5; i++)
            {
                switch (i)
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

            actions += opsCou.ToString() + " " + (allProcArr[1].opsCounter + allProcArr[0].opsCounter + allProcArr[2].opsCounter + allProcArr[3].opsCounter + allProcArr[4].opsCounter).ToString();
            //textQueueProc1.Text = cou.ToString();
            //textDoneProc1.Text = proc1Counter.ToString();
            //textDoneProc2.Text = proc2Counter.ToString();
            //textDoneProc3.Text = proc3Counter.ToString();
            //textDoneProc4.Text = proc4Counter.ToString();
            //textDoneProc5.Text = proc5Counter.ToString();
        }

    }
}
