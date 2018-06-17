using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Threading;

namespace TheShow
{
    public partial class Form1 : Form
    {
        private String pathNone;
        private String pathAny;
        private FileInfo[] infoNone;
        private FileInfo[] infoAny;
        private Boolean isNeedFullScreen = false;

        private Rectangle recScreen = new Rectangle();

        public Form1()
        {
            InitializeComponent();
            recScreen = Screen.GetBounds(this);
            // MessageBox.Show("屏幕： " + recScreen.Width + "," + recScreen.Height);

            this.initFile();
        }

        // 文件初始化
        private void initFile()
        {
            DirectoryInfo dir = new DirectoryInfo(Application.StartupPath).Parent.Parent.Parent;
            string target = dir.FullName;
            pathAny = target + "\\有人时播放这里";
            pathNone = target + "\\没有人时播放这里";

            DirectoryInfo dirNone = new DirectoryInfo(pathNone);
            infoNone = dirNone.GetFiles();
            DirectoryInfo dirAny = new DirectoryInfo(pathAny);
            infoAny = dirAny.GetFiles();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.playerInit();
            this.cmbSerials.Items.AddRange(SerialPort.GetPortNames());
            this.cmbSerials.SelectedIndex = this.cmbSerials.Items.Count - 1;//Arduino一般在最后一个串口
        }

        // 播放器初始化
        private void playerInit()
        {
            axWindowsMediaPlayer1.PlayStateChange += new AxWMPLib._WMPOCXEvents_PlayStateChangeEventHandler(axWindowsMediaPlayer1_PlayStateChange);
            axWindowsMediaPlayer1.uiMode = "none";
            axWindowsMediaPlayer1.settings.autoStart = false;
            // 设置连续播放两遍
            axWindowsMediaPlayer1.settings.playCount = 2;
        }

        // 播放
        private void goStart(String url)
        {
            axWindowsMediaPlayer1.URL = url;
            axWindowsMediaPlayer1.Ctlcontrols.play();
        }

        // 有人时，随机播放文件
        private void goStartWithBody()
        {
            Random rd = new Random();
            int index = rd.Next(infoAny.Length);
            goStart(pathAny + "\\" + infoAny[index]);
        }

        // 没有人时 播放最后一个文件
        private void goStartNoBody()
        {
            Random rd = new Random();
            int index = rd.Next(infoNone.Length);
            goStart(pathNone + "\\" + infoNone[index]);
        }
        
        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e)
        {
            
        }

        private Boolean needChange = false;

        private void axWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            // 如果需要全屏，设置之
            if (e.newState == 3 && isNeedFullScreen)
            {
                axWindowsMediaPlayer1.fullScreen = true;
            }

            // 只有在播放状态才可以切换歌曲
            // 当标志为需要切歌时，切歌并重置标志
            if (e.newState == 3 && needChange)
            {
                needChange = false;
                if (state == STATE_BODY)
                {
                    goStartWithBody();
                }
                else
                {
                    goStartNoBody();
                }
            }
            // 第一遍播放结束后，标志：需要切歌
            if (e.newState == 8)
            {
                needChange = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            this.RefreshInfoTextBox();
        }

        private SerialPort port = null;
        /// <summary>
        /// 初始化串口实例
        /// </summary>
        private void InitialSerialPort()
        {
            try
            {
                string portName = this.cmbSerials.SelectedItem.ToString();
                port = new SerialPort(portName, 9600);
                port.Encoding = Encoding.ASCII;
                port.DataReceived += port_DataReceived;
                port.Open();
                this.ChangeArduinoSendStatus(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化串口发生错误：" + ex.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 关闭并销毁串口实例
        /// </summary>
        private void DisposeSerialPort()
        {
            if (port != null)
            {
                try
                {
                    this.ChangeArduinoSendStatus(false);
                    if (port.IsOpen)
                    {
                        port.Close();
                    }
                    port.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("关闭串口发生错误：" + ex.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// 改变Arduino串口的发送状态
        /// </summary>
        /// <param name="allowSend">是否允许发送数据</param>
        private void ChangeArduinoSendStatus(bool allowSend)
        {
            if (port != null && port.IsOpen)
            {
                if (allowSend)
                {
                    port.WriteLine("serial start");
                }
                else
                {
                    port.WriteLine("serial stop");
                }
            }
        }

        /// <summary>
        /// 从串口读取数据并转换为字符串形式
        /// </summary>
        /// <returns></returns>
        private string ReadSerialData()
        {
            string value = "";
            try
            {
                if (port != null && port.BytesToRead > 0)
                {
                    value = port.ReadExisting();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取串口数据发生错误：" + ex.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return value;
        }

        private const int STATE_NONE = 0;
        private const int STATE_BODY = 1;
        private int prveState = -1;
        private int state = -1;

        // 在读取到数据时刷新文本框的信息
        private void RefreshInfoTextBox()
        {
            string value = this.ReadSerialData();

            // 根据串口获取的数据，判断状态
            if (value == "0")
            {
                state = STATE_NONE;
            }
            else
            {
                state = STATE_BODY;
            }

            Action<string> setValueAction = text => this.label2.Text = text;
            if (this.label2.InvokeRequired)
            {
                this.label2.Invoke(setValueAction, state == STATE_BODY ? "有人" : "无人");
            }
            else
            {
                this.label2.Text = state == STATE_BODY ? "有人" : "无人";
            }

            // 判断状态是否改变， 如果状态改变
            // 按照新状态播放
            if (prveState != state)
            {
                prveState = state;
                if (state == STATE_NONE)
                {
                    this.goStartNoBody();
                }
                else
                {
                    this.goStartWithBody();
                }
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.InitialSerialPort();
        }

        private void buttonCloseSerial_Click(object sender, EventArgs e)
        {
            this.DisposeSerialPort();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            if (!isNeedFullScreen)
            {
                axWindowsMediaPlayer1.fullScreen = true;
                button4.Text = "取消全屏";
                isNeedFullScreen = true;
            }
            else
            {
                button4.Text = "全屏";
                isNeedFullScreen = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
