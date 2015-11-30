using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfLab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)radioButton1.IsChecked)
            {
                FIFO fifoForm = new FIFO();
                fifoForm.ShowDialog();

            }
            if ((bool)radioButton2.IsChecked)
            {
                WeakWorksWack weakForm = new WeakWorksWack();
                weakForm.ShowDialog();

            }
            if ((bool)radioButton3.IsChecked)
            {
                MaybEst MaybEstForm = new MaybEst();
                MaybEstForm.ShowDialog();

            }
        }



    }
}
