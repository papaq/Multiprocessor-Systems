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
    /// Interaction logic for FIFO.xaml
    /// </summary>
    public partial class FIFO : Window
    {
        TextBox noteFist = new TextBox();
        TextBox noteM = new TextBox();
        TextBox noteProc = new TextBox();
        TextBox noteTask = new TextBox();
        DoubleAnimation animateNoteOn = new DoubleAnimation(1, Duration.Automatic);
        DoubleAnimation animateNoteOff = new DoubleAnimation(0, Duration.Automatic);
        private bool dragStarted = false;
        private bool dragCheckStarted = false;
        private int minProc = 1;
        Regex regex = new Regex(@"[^\d]+", RegexOptions.IgnoreCase);
        int taskFreq = 1;
        int calcTime = 10000;
        int taskEmerg = 50;
        long limitLow = 1;
        long limitHi = 2;
        //System.Windows.Threading.DispatcherTimer timer;
        Stopwatch tenSeconds = new Stopwatch();
        static object listLocker = new object();
        int cou = 0;
        long opsCou = 0;
        //long misecsInTicks = 2437;
        List<Tasks> taskList = new List<Tasks>();
        int checkTime = 16;
        string actions = "Tony 2015   " + DateTime.Now.ToString("h:mm:ss tt") + "\n";

        long proc1Ops = 0;
        long proc1Power = 1;
        int proc1Counter = 0;

        long proc2Ops = 0;
        long proc2Power = 1;
        int proc2Counter = 0;

        long proc3Ops = 0;
        long proc3Power = 1;
        int proc3Counter = 0;

        long proc4Ops = 0;
        long proc4Power = 1;
        int proc4Counter = 0;

        long proc5Ops = 0;
        long proc5Power = 1;
        int proc5Counter = 0;

        public FIFO()
        {
            InitializeComponent();
            FillRandom();
            ButtonResults.Visibility = Visibility.Hidden;
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
                    TextDownLimit.Text = CustomSlider.Value.ToString();
                }
                TextUpLimit.Text = CustomSlider.Value.ToString();
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
                TextCheck.Text = CustomSlider_Check.Value.ToString();
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
        }

        private void CheckProc_Unchecked(object sender, RoutedEventArgs e)
        {
            MakeActiveAgain(textProc1);
            MakeActiveAgain(textProc2);
            MakeActiveAgain(textProc3);
            MakeActiveAgain(textProc4);
            MakeActiveAgain(textProc5);
        }

        private void FillRandom()
        {
            Random ranOps = new Random();
            textProc1.Text = (Math.Round(ranOps.Next(66, 95) * 1e+5 * 0.997)).ToString();
            textProc2.Text = (Math.Round(ranOps.Next(66, 95) * 1e+5 * 0.997)).ToString();
            textProc3.Text = (Math.Round(ranOps.Next(66, 95) * 1e+5 * 0.997)).ToString();
            textProc4.Text = (Math.Round(ranOps.Next(66, 95) * 1e+5 * 0.997)).ToString();
            textProc5.Text = (Math.Round(ranOps.Next(66, 95) * 1e+5 * 0.997)).ToString();

            minProc = Convert.ToInt32(textProc5.Text);
            SetOperationLimits(minProc);
        }

        private void SetOperationLimits(int procOps)
        {
            TextDownLimit.Text = (CustomSlider.Minimum = Math.Round(minProc * 0.02 * taskFreq)).ToString();
            TextUpLimit.Text = (CustomSlider.Value = CustomSlider.Maximum = Math.Round(minProc * 0.2 * taskFreq)).ToString();
        }

        private void CheckTextValid(TextBox text)
        {
            Random ranOps = new Random();
            if ((text.Text.Length > 9) || regex.IsMatch(text.Text) || (text.Text == "") || (Convert.ToDecimal(text.Text) == 0) || (Convert.ToDecimal(text.Text) < 1000))
            {
                text.Text = (10000 + ranOps.Next(0, 90000)).ToString();
            }
            text.Text = (Math.Truncate(Convert.ToDecimal(text.Text))).ToString();
            if (minProc > Convert.ToInt32(text.Text))
            {
                minProc = Convert.ToInt32(text.Text);
                SetOperationLimits(minProc);
            }
        }

        private void textProc1_LostFocus(object sender, RoutedEventArgs e)
        {
            //MatchCollection matches = regex.Matches(textProc1.Text);
            CheckTextValid(textProc1);
        }

        private void textProc2_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckTextValid(textProc2);
        }

        private void textProc3_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckTextValid(textProc3);
        }

        private void textProc4_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckTextValid(textProc4);
        }

        private void textProc5_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckTextValid(textProc5);
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
            System.IO.File.WriteAllText("../WriteText.txt", text);
        }

        private void ButtonResults_Click(object sender, RoutedEventArgs e)
        {
            //GeneralResults outStr = new GeneralResults(cou, proc1Counter, proc2Counter, proc3Counter, proc4Counter, proc5Counter,
            //    tenSeconds, taskEmerg, taskFriq, limitLow, limitHi, checkTime);
            //GeneralResults outProc1 = new GeneralResults(1, "All", proc1Counter, cou, proc1Power, limitLow, limitHi);
            //GeneralResults outProc2 = new GeneralResults(2, "All", proc2Counter, cou, proc2Power, limitLow, limitHi);
            //GeneralResults outProc3 = new GeneralResults(3, "All", proc3Counter, cou, proc3Power, limitLow, limitHi);
            //GeneralResults outProc4 = new GeneralResults(4, "All", proc4Counter, cou, proc4Power, limitLow, limitHi);
            //GeneralResults outProc5 = new GeneralResults(5, "All", proc5Counter, cou, proc5Power, limitLow, limitHi);

            long powerSum = proc1Power + proc2Power + proc3Power + proc4Power + proc5Power;

            System.IO.File.WriteAllLines("../GeneralResults.txt",
                (new GeneralResults(cou, proc1Counter + proc2Counter + proc3Counter + proc4Counter + proc5Counter,
                    opsCou, proc1Ops + proc2Ops + proc3Ops + proc4Ops + proc5Ops,
                    powerSum, powerSum, calcTime / 1000,
                tenSeconds, taskEmerg, taskFreq, limitLow, limitHi, checkTime)).lines);
            //System.IO.File.AppendAllText("../GeneralResults.txt", "\n\n");
            System.IO.File.AppendAllLines("../GeneralResults.txt",
                (new GeneralResults(1, "All", proc1Counter, proc1Counter + proc2Counter + proc3Counter + proc4Counter + proc5Counter,
                    proc1Ops, proc1Ops + proc2Ops + proc3Ops + proc4Ops + proc5Ops, proc1Power, limitLow * 5.5)).lines);
            System.IO.File.AppendAllLines("../GeneralResults.txt",
                (new GeneralResults(2, "All", proc2Counter, proc1Counter + proc2Counter + proc3Counter + proc4Counter + proc5Counter,
                    proc2Ops, proc1Ops + proc2Ops + proc3Ops + proc4Ops + proc5Ops, proc2Power, limitLow * 5.5)).lines);
            System.IO.File.AppendAllLines("../GeneralResults.txt",
                (new GeneralResults(3, "All", proc3Counter, proc1Counter + proc2Counter + proc3Counter + proc4Counter + proc5Counter,
                    proc3Ops, proc1Ops + proc2Ops + proc3Ops + proc4Ops + proc5Ops, proc3Power, limitLow * 5.5)).lines);
            System.IO.File.AppendAllLines("../GeneralResults.txt",
                (new GeneralResults(4, "All", proc4Counter, proc1Counter + proc2Counter + proc3Counter + proc4Counter + proc5Counter,
                    proc4Ops, proc1Ops + proc2Ops + proc3Ops + proc4Ops + proc5Ops, proc4Power, limitLow * 5.5)).lines);
            System.IO.File.AppendAllLines("../GeneralResults.txt",
                (new GeneralResults(5, "All", proc5Counter, proc1Counter + proc2Counter + proc3Counter + proc4Counter + proc5Counter,
                    proc5Ops, proc1Ops + proc2Ops + proc3Ops + proc4Ops + proc5Ops, proc5Power, limitLow * 5.5)).lines);
        }

        private void ButtonResults_MouseLeave(object sender, MouseEventArgs e)
        {
            ButtonResults.Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Application.Current.Shutdown();

            CheckTextValid(textProc1);
            CheckTextValid(textProc1);
            CheckTextValid(textProc3);
            CheckTextValid(textProc4);
            CheckTextValid(textProc5);

            TextTaskEmerg_Check();
            TextDownLimit_Check();

            limitLow = Convert.ToInt64(TextDownLimit.Text);
            limitHi = (long)CustomSlider.Value;

            proc1Power = Convert.ToInt32(textProc1.Text);
            proc2Power = Convert.ToInt32(textProc2.Text);
            proc3Power = Convert.ToInt32(textProc3.Text);
            proc4Power = Convert.ToInt32(textProc4.Text);
            proc5Power = Convert.ToInt32(textProc5.Text);

            checkTime = Convert.ToInt16(CustomSlider_Check.Value);
            
            proc1Counter = 0;
            proc2Counter = 0;
            proc3Counter = 0;
            proc4Counter = 0;
            proc5Counter = 0;

            cou = 0;
            opsCou = 0;

            proc1Ops = 0;
            proc2Ops = 0;
            proc3Ops = 0;
            proc4Ops = 0;
            proc5Ops = 0;

            tenSeconds.Reset();

            StartThreads();
            ButtonResults.Visibility = Visibility.Visible;
            OutputResults(actions);
        }

        private void ComboFriquency_SelectionChanged(object sender, MouseEventArgs e)
        {
            if (ComboFriquency.Text != "")
            {
                /*CustomSlider_Check.Maximum = */
                taskFreq = Convert.ToInt16(ComboFriquency.Text.Trim(new char[3] { ' ', 'm', 's' }));
                //TextCheck.Text = (CustomSlider_Check.Value = CustomSlider_Check.Maximum).ToString();
                //CustomSlider_Check.Value = CustomSlider_Check.Maximum;
                
                SetOperationLimits(minProc);
            }
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
            Random rand = new Random();
            double beginPoint;

            //int emrg;
            double hundEmrg = tenSeconds.Elapsed.TotalMilliseconds;

            while (tenSeconds.ElapsedMilliseconds < calcTime)
            {
                beginPoint = tenSeconds.Elapsed.TotalMilliseconds;
                if (rand.Next(0, 100) <= taskEmerg)
                {
                    proc1 = rand.Next(0, 2);
                    proc2 = rand.Next(0, 2);
                    proc3 = rand.Next(0, 2);
                    proc4 = rand.Next(0, 2);
                    proc5 = rand.Next(0, 2);
                    diff = rand.Next((Int32)limitLow, (Int32)limitHi);
                    if (proc1+proc2+proc3+proc4+proc5 == 0)
                    {
                        continue;
                    }
                    opsCou += diff;
                    lock (listLocker)
                    {
                        taskList.Add(new Tasks(proc1, proc2, proc3, proc4, proc5, diff));
                    }
                    counter++;
                    actions += "  _TASKS_  " + counter.ToString() + ".   " + proc1.ToString() + " " + proc2.ToString() + " " + proc3.ToString() + " " + proc4.ToString() + " " + proc5.ToString() + " " + diff.ToString() + "\n";
                }

                if (counter % 10 == 0)
                {
                    hundEmrg = counter * taskFreq / ((double)taskEmerg / 100) - tenSeconds.Elapsed.TotalMilliseconds;
                    //actions += hundEmrg.ToString() + "\n";
                    if (hundEmrg > 0)
                        Thread.Sleep(TimeSpan.FromMilliseconds(hundEmrg));
                    //hundEmrg = tenSeconds.Elapsed.TotalMilliseconds;
                }
            }

            cou = counter;
        }

        private void Proc1()
        {
            int setDelay;
            while (true)
            {
                actions += "  __P1__  " + proc1Counter.ToString() + ".        TUUUK - TUUUK" + "\n";
                setDelay = checkTime;
                lock (listLocker)
                {
                    actions += "  __P1__  " + proc1Counter.ToString() + ".        CCHHEECCKK" + "\n";
                    if ((taskList.Count() != 0) && (taskList[0].proces[0] == 1))
                    {
                        actions += "  __P1__  " + proc1Counter.ToString() + ".        COME IN" + "\n";
                        setDelay = (int)((proc1Ops += taskList[0].difficulty) / proc1Power * 1000);
                        taskList.RemoveAt(0);
                        proc1Counter++;
                        actions += "  __P1__  " + proc1Counter.ToString() + ".   " + setDelay.ToString() + "\n";
                    }
                }
                Thread.Sleep(setDelay);
            }
        }

        private void Proc2()
        {
            int setDelay;
            while (true)
            {
                actions += "  __P2__  " + proc1Counter.ToString() + ".        TUUUK - TUUUK" + "\n";

                setDelay = checkTime;
                lock (listLocker)
                {
                    actions += "  __P2__  " + proc1Counter.ToString() + ".        CCHHEECCKK" + "\n";
                    if ((taskList.Count() != 0) && (taskList[0].proces[1] == 1))
                    {
                        setDelay = (int)((proc2Ops += taskList[0].difficulty) / proc2Power * 1000);
                        taskList.RemoveAt(0);
                        proc2Counter++;
                        actions += "  __P2__  " + proc2Counter.ToString() + ".        COME IN" + "\n";
                    }
                }
                Thread.Sleep(setDelay);
            }
        }

        private void Proc3()
        {
            int setDelay;
            while (true)
            {
                actions += "  __P3__  " + proc1Counter.ToString() + ".        TUUUK - TUUUK" + "\n";

                setDelay = checkTime;
                lock (listLocker)
                {
                    actions += "  __P3__  " + proc1Counter.ToString() + ".        CCHHEECCKK" + "\n";
                    if ((taskList.Count() != 0) && (taskList[0].proces[2] == 1))
                    {
                        setDelay = (int)((proc3Ops += taskList[0].difficulty) / proc3Power * 1000);
                        taskList.RemoveAt(0);
                        proc3Counter++;
                        actions += "  __P3__  " + proc3Counter.ToString() + ".        COME IN" + "\n";
                    }
                }
                Thread.Sleep(setDelay);
            }
        }

        private void Proc4()
        {
            int setDelay;
            while (true)
            {
                actions += "  __P4__  " + proc1Counter.ToString() + ".        TUUUK - TUUUK" + "\n";

                setDelay = checkTime;
                lock (listLocker)
                {
                    actions += "  __P4__  " + proc1Counter.ToString() + ".        CCHHEECCKK" + "\n";
                    if ((taskList.Count() != 0) && (taskList[0].proces[3] == 1))
                    {
                        setDelay = (int)((proc4Ops += taskList[0].difficulty) / proc4Power * 1000);
                        taskList.RemoveAt(0);
                        proc4Counter++;
                        actions += "  __P4__  " + proc4Counter.ToString() + ".        COME IN" + "\n";
                    }
                }
                Thread.Sleep(setDelay);
            }
        }

        private void Proc5()
        {
            int setDelay;
            while (true)
            {
                actions += "  __P5__  " + proc1Counter.ToString() + ".        TUUUK - TUUUK" + "\n";

                setDelay = checkTime;
                lock (listLocker)
                {
                    actions += "  __P5__  " + proc1Counter.ToString() + ".        CCHHEECCKK" + "\n";
                    if ((taskList.Count() != 0) && (taskList[0].proces[4] == 1))
                    {
                        setDelay = (int)((proc5Ops += taskList[0].difficulty) / proc5Power * 1000);
                        taskList.RemoveAt(0);
                        proc5Counter++;
                        actions += "  __P5__  " + proc5Counter.ToString() + ".        COME IN" + "\n";
                    }
                }
                Thread.Sleep(setDelay);
            }
        }

        private void StartThreads()
        {
            Thread tasks = new Thread(new ThreadStart(CreatTasks));
            Thread proc1 = new Thread(new ThreadStart(Proc1));
            Thread proc2 = new Thread(new ThreadStart(Proc2));
            Thread proc3 = new Thread(new ThreadStart(Proc3));
            Thread proc4 = new Thread(new ThreadStart(Proc4));
            Thread proc5 = new Thread(new ThreadStart(Proc5));

            tenSeconds.Start();
            try
            {
                tasks.Start();
                proc1.Start();
                proc2.Start();
                proc3.Start();
                proc4.Start();
                proc5.Start();

                tasks.Join();

                proc1.Abort();
                proc2.Abort();
                proc3.Abort();
                proc4.Abort();
                proc5.Abort();

                taskList.Clear();
            }
            catch (ThreadStateException e)
            {
                Console.WriteLine("Caught: {0}", e.Message);
            }
            tenSeconds.Stop();
            textQueueProc1.Text = cou.ToString();
            textDoneProc1.Text = proc1Counter.ToString();
            textDoneProc2.Text = proc2Counter.ToString();
            textDoneProc3.Text = proc3Counter.ToString();
            textDoneProc4.Text = proc4Counter.ToString();
            textDoneProc5.Text = proc5Counter.ToString();
        }


    }
}
