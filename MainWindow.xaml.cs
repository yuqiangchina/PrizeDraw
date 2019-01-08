using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Configuration;

namespace Luck
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 图像队列
        /// </summary>
        private List<BitmapImage> lstImage = new List<BitmapImage>();

        /// <summary>
        /// 乱序索引队列
        /// </summary>
        private List<int> lstId = new List<int>();    

        /// <summary>
        /// 索引队列ID
        /// </summary>
        private int nShowItem = 0;

        /// <summary>
        /// 计时器
        /// </summary>
        private DispatcherTimer timer = new DispatcherTimer();

        /// <summary>
        /// 抽奖等级。 3：三等奖；2：二等奖；1：一等奖；0：特等奖
        /// </summary>
        private int nPrize = 3;

        /// <summary>
        /// 抽奖图片路径 三等奖路径：path/3；二等奖路径：path/2；一等奖路径：path/1；特等奖路径：path/0
        /// </summary>
        private string strInitPath = ConfigurationManager.AppSettings["Path"].Trim();

        /// <summary>
        /// 中奖纪录文件名称，包括路径。
        /// </summary>
        private string strLuckFile = "";        

        public MainWindow()
        {
            //防止启动两个实例
            if(System.Diagnostics.Process.GetProcessesByName("Luck").Length > 1)
            {
                Application.Current.Shutdown();
                return;
            }
            InitializeComponent();
        }        

        //抽奖开始、停止切换
        private void button_Click(object sender, RoutedEventArgs e)
        {                       
            if (timer.IsEnabled == true)
            {
                timer.Stop();
                try {
                    string strShowPic = ""; 
                    string strF = "";
                    string strName = "";
                    switch(nPrize)
                    {
                        case 0:
                            strShowPic = myImage3.Source.ToString();
                            strF = strShowPic.Split('/')[strShowPic.Split('/').Count() - 1];
                            strName = "特等奖：" + strF.Substring(0, strF.Length - 4);
                            break;
                        case 1:
                            strShowPic = myImage3.Source.ToString();
                            strF = strShowPic.Split('/')[strShowPic.Split('/').Count() - 1];
                            strName = "一等奖：" + strF.Substring(0, strF.Length - 4);
                            break;
                        case 2:
                            strShowPic = myImage3.Source.ToString();
                            strF = strShowPic.Split('/')[strShowPic.Split('/').Count() - 1];
                            strName = "二等奖：" + strF.Substring(0, strF.Length - 4);                            
                            break;
                        default:
                            strShowPic = myImage3.Source.ToString();
                            strF = strShowPic.Split('/')[strShowPic.Split('/').Count() - 1];
                            strName = "三等奖：" + strF.Substring(0, strF.Length - 4);                            

                            break;
                    }

                    SaveFile(strName);
                    txtB.Text = strName;                      
                }
                catch (Exception fe)
                {
                    MessageBox.Show("本次抽奖无效！\n" + fe.Message, "抽奖失败");
                }
                
            }
            else
            {
                txtB.Text = "";
                timer.Start();
            }
        }        

        /// <summary>
        /// 保存中奖信息
        /// </summary>
        /// <param name="strName">中奖人</param>
        private void SaveFile(string strName)
        {            
            using (System.IO.StreamWriter sw = System.IO.File.AppendText(strLuckFile))
            {
                sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "：" + strName);
            }
        }

        /// <summary>
        /// 取得乱序队列当前ID
        /// </summary>
        /// <returns></returns>
        private int GetShowId()
        {
            //索引复位
            if (nShowItem >= lstImage.Count())
            {
                nShowItem = 0;
            }

            return lstId[nShowItem++];
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                #region 乱序队列依次显示
                myImage3.Source = lstImage[GetShowId()];
                #endregion

                //休息5毫秒
                System.Threading.Thread.Sleep(5);                

            } catch { }                      
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //窗体背景处理
            LoadBackImage(3);

            //生成抽奖队列
            LoadImage(3);

            #region 显示初始图片
            try
            {
                myImage3.Stretch = Stretch.Fill;
                myImage3.Source = lstImage[GetShowId()];
            }
            catch (Exception ie)
            {
                MessageBox.Show(ie.Message, "初始化图像失败");
            }
            #endregion

            //生成中奖纪录文件名
            strLuckFile = strInitPath + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";

            #region 初始化计时器
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Stop();
            #endregion
        }

        /// <summary>
        /// 装入抽奖图片
        /// </summary>
        /// <param name="nFlag">抽奖等级标志。3：三等奖；2：二等奖；1：一等奖；0：特等奖</param>
        private void LoadImage(int nFlag)
        {
            #region 读取图片
            lock (lstImage)
            {
                //清空图像队列
                lstImage.Clear();

                string[] strF = System.IO.Directory.GetFiles(path: @strInitPath + "\\" + nFlag.ToString(), searchPattern: "*.jpg");

                if (strF.Length > 0)
                {
                    foreach (string f in strF)
                    {
                        try
                        {                                                        
                            BitmapImage bi = new BitmapImage();
                            bi.BeginInit();
                            bi.UriSource = new Uri(f, UriKind.Absolute);
                            bi.EndInit();
                            lstImage.Add(bi);
                        }
                        catch (Exception fe)
                        {
                            MessageBox.Show(f + "\n" + fe.Message, "载入图像失败");
                        }
                    }
                }
            }            
            #endregion

            #region 生成乱序队列            
            //生成正序索引队列
            List<int> lstTT = new List<int>();
            for (int i = 0; i < lstImage.Count(); i++)
            {
                lstTT.Add(i);
            }

            Random r = new Random(System.DateTime.Now.Second);
            int d, c;
            //清空乱序索引队列
            lock (lstId)
            {
                lstId.Clear();

                //生成乱序队列
                while (lstTT.Count() > 0)
                {
                    d = lstTT.Count();
                    c = r.Next(0, d);
                    lstId.Add(lstTT[c]);
                    lstTT.RemoveAt(c);
                }
            }            
            #endregion
        }

        /// <summary>
        /// 装入窗体背景图
        /// </summary>
        /// <param name="nFlag">抽奖等级标志。3：三等奖；2：二等奖；1：一等奖；0：特等奖</param>
        private void LoadBackImage(int nFlag)
        {
            ImageBrush ib = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(strInitPath + "\\back" + nFlag.ToString() + ".jpg")),
                Stretch = Stretch.Fill
            };
            this.Background = ib;
        }        

        //二等奖按钮
        private void btn2_Click(object sender, RoutedEventArgs e)
        {           
            if(MessageBox.Show("将开始抽取二等奖，是否继续？","提示",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            //装入背景
            LoadBackImage(2);

            //生成抽奖队列
            LoadImage(2);

            //设置抽奖等级
            nPrize = 2;

            txtB.Text = "";

            button.Focus();

        }

        //一等奖按钮
        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("将开始抽取一等奖，是否继续？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            //装入背景
            LoadBackImage(1);

            //生成抽奖队列
            LoadImage(1);

            //设置抽奖等级
            nPrize = 1;

            txtB.Text = "";

            button.Focus();
        }

        //特等奖按钮
        private void btn0_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("将开始抽取【特等奖】，是否继续？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            if (MessageBox.Show("再考虑一下？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                return;
            }

            if (MessageBox.Show("真的要开始抽奖了！！！", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            //装入背景
            LoadBackImage(0);

            //生成抽奖队列
            LoadImage(0);

            //设置抽奖等级
            nPrize = 0;

            txtB.Text = "";

            button.Focus();
        }

        //最小化按钮
        private void btnMin_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        //退出按钮
        private void btnClose_Click(object sender, RoutedEventArgs e) => this.Close();       
    }
}
