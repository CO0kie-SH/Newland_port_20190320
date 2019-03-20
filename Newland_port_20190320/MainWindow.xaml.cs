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
            double gz = 0, ljz = 0;
            da[0] = toD(zigBee.IN1Original, "光");
            da[1] = toD(zigBee.IN2Original, "温");
            da[2] = toD(zigBee.IN3Original, "湿");
            da[3] = toD(zigBee.IN4Original, "");
            string s = "四通道：\n";
            for (int i = 0; i < 4; i++)
            {
                s += da[i] + "\n";
            }
            s += $"灯={of1} now={ofn1}\n";
            s += $"扇={of2} now={ofn2}\n";
            Dispatcher.Invoke(new Action(() =>
            {
                double.TryParse(textBox.Text, out ljz);
            }));
            double.TryParse(zigBee.lightValue, out gz);
            s += $"光={gz} {ljz}";
            if(gz>0 && ljz>0 && isA){ of1 = gz < ljz; }
            
            Dispatcher.Invoke(new Action(() =>
            {
                lbgz.Content = zigBee.lightValue;
                label.Content = s;
                lbd.Content = ofn1 ? "●" : "○";
                lbfs.Content = ofn2 ? "●" : "○";

                lbhy.Content = !isA ? "null" : adam.DI1 ? "●" : "○";
                lbyw.Content = !isA ? "null" : adam.DI2 ? "●" : "○";
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
                    adam.SetData();
                    if (of1 != ofn1)
                    {
                        adam.OnOff(of1 ? ADAM4150FuncID.OnDO2 : ADAM4150FuncID.OffDO2);
                        ofn1 = of1;
                    }
                    if (of2 != ofn2)
                    {
                        adam.OnOff(of2 ? ADAM4150FuncID.OnDO1 : ADAM4150FuncID.OffDO1);
                        ofn2 = of2;
                    }
                }
                if (isZ)
                {
                    zigBee.GetSet();
                }
            }
        }

        private static bool isRun = true, isA = false, isZ = false, of1 = false, ofn1 = false, of2 = false, ofn2 = false;

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Convert.ToInt32(textBox.Text);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            of2 = false;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            of2 = true;
        }

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
                adam.Open();
                isA = true;
            }
            else
            {
                isZ = false;
                zigBee.Close();
                isA = true;
                adam.Close();
            }
        }
    }
}
