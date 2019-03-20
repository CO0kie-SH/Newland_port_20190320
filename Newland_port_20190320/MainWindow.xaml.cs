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
using LEDLibrary;
using DigitalLibrary;
using ZigBeeLibrary;
using System.Threading;

namespace Newland_port_20190320
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread seach;

        ZigBee zigBee;
        ZigBeeLibrary.ComSettingModel comZ;

        ADAM4150 adam;
        DigitalLibrary.ComSettingModel comA;

        public MainWindow()
        {
            InitializeComponent();
            comZ = new ZigBeeLibrary.ComSettingModel();
            comZ.ZigbeeCom = "COM6";
            zigBee = new ZigBee(comZ);
            zigBee.DataReceivedCallback += ZigBee_DataReceivedCallback;

            comA = new DigitalLibrary.ComSettingModel();
            comA.DigitalQuantityCom = "COM4";
            adam = new ADAM4150(comA);

            seach = new Thread(new ThreadStart(se));
            seach.Start();
        }

        private void ZigBee_DataReceivedCallback(object sender, ComLibrary.MsgEventArgs e)
        {
            double[] da = new double[4];

            da[0] = toD(zigBee.IN1Original, "光");
            da[1] = toD(zigBee.IN2Original, "温");
            da[2] = toD(zigBee.IN3Original, "湿");
            da[3] = toD(zigBee.IN4Original, "");
            string s = "四通道：\n";
            for (int i = 0; i < 4; i++)
            {
                s += da[i] + "\n";
            }
            Dispatcher.Invoke(new Action(() =>
            {
                lbgz.Content = zigBee.lightValue;
                label.Content = s;
            }));
        }

        private double toD(int v1, string v2)
        {
            double re = v1 * 3300.0 / 1023.0 / 150.0;
            if (re <= 4.0) re = 4.0;
            switch (v2)
            {
                case "光":
                    re = (re - 4.0) / 16.0 * 2000 + 0;
                    break;
                case "温":
                    re = (re - 4.0) / 16.0 * 50 + 0;
                    break;
                case "湿":
                    re = (re - 4.0) / 16.0 * 100 + 0;
                    break;
                case "":
                    re = re - 4.0;
                    break;
                default:
                    break;
            }

            return re;
        }

        private void se()
        {
            while (isRun)
            {
                Thread.Sleep(1000);
                if (isA)
                {
                    zigBee.GetSet();
                }
                if (isZ)
                {
                    adam.SetData();
                }
            }
        }

        private static bool isRun = true, isA = false, isZ = false, isP = false, nowP = false;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isRun = false;
            seach.Abort();
            try
            {
                if (isA) adam.Close();
                if (isZ) zigBee.Close();
            }
            catch (Exception)
            {
            }
        }

        private void checkBox_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked.Value)
            {
                zigBee.Open();
                isZ = true;
                //adam.Open();
                //isA = true;
            }
            else
            {
                isZ = false;
                zigBee.Close();
            }
        }
    }
}
