using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Threading;
using Panuon.UI.Silver;
using System.Diagnostics;

namespace KGM_TO_MP3
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region 字段
        string inputpath=null, outputpath=null;
        int FilesOfSourceCounts, FilesOfSucessCounts = 0;//需转换文件数量 成功转换数量
        bool gyclick = false;
        About about;
        double successCount = 0, failedCount = 0, totalCount = 0;
        #endregion

        #region 属性的绑定
        /// <summary>
        /// 定义记录进度的字段及属性
        /// </summary>
        private string currentProgress = "";
        public string CurrentProgress
        {
            get { return currentProgress; }
            set
            {
                currentProgress = value;
                OnPropertyChanged("CurrentProgress");
            }
        }
        private string successFail = "";
        public string SuccessFail
        {
            get { return successFail; }
            set
            {
                successFail = value;
                OnPropertyChanged("SuccessFail");
            }
        }
        /// <summary>
        /// 属性改变的事件委托
        /// 实现INotifyPropertyChanged接口成员
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        #endregion

        #region 定时器（用于降低不必要的刷新率）
        /// <summary>
        /// 定时器 Timer1
        /// </summary>
        DispatcherTimer Timer1 = new DispatcherTimer();
        /// <summary>
        /// 定时器 Timer1 触发函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer1_Tick(object sender, EventArgs e)
        {
            double temp2 = (totalCount / FilesOfSourceCounts) * 100;
            ProgressBarHelper.SetAnimateTo(pb_import, (int)temp2);//定时更新进度条动画
            ProgressBarHelper.SetAnimationDuration(pb_import,new TimeSpan(0,0,0,0,200));//定时更新进度条动画
            CurrentProgress = Math.Min((int)temp2,100).ToString() + "%";//定时跟新绑定数据CurrentProgress "&#xf058;    &#xf057;"
            SuccessFail = "\xf058 "+ successCount + " \xf057 "+ failedCount;
            if (Math.Min((int)temp2, 100) == 100)
                Timer1.Stop();
        }
        /// <summary>
        /// 初始化 Timer1
        /// </summary>
        private void Timer1_Init()
        {
            Timer1.Tick += Timer1_Tick;
            Timer1.Interval = new TimeSpan(0, 0, 0, 0, 50);
        }
        #endregion

        #region UI初始化
        /// <summary>
        /// 构造函数
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            MyInitAsync();
            Timer1_Init(); 
        }

        
        /// <summary>
        /// About控件初始化
        /// </summary>
        void MyInitAsync()
        {
            about = new About();
            about.SetValue(Grid.RowProperty, 0);
            about.SetValue(Grid.RowSpanProperty, 2);
            about.SetValue(Grid.ColumnProperty, 0);
            about.SetValue(Grid.ColumnSpanProperty, 2);
            about.Margin = new Thickness(3, 2, 0, 2);
            about.Visibility = Visibility.Hidden;
            this.mainpart.Children.Add(about);
        }
        #endregion

        #region 按键回调
        #region Async Await 用法说明
        //用async来修饰一个方法，表明这个方法是异步的，声明的方法的返回类型必须为：void或Task或Task<TResult>。
        //方法内部必须含有await修饰的方法，如果方法内部没有await关键字修饰的表达式，哪怕函数被async修饰也只
        //能算作同步方法，执行的时候也是同步执行的。

        //被await修饰的只能是Task或者Task<TResule> 类型，通常情况下是一个返回类型是Task/Task<TResult> 的方法
        //当然也可以修饰一个Task/Task<TResult> 变量，await只能出现在已经用async关键字修饰的异步方法中。上面
        //代码中就是修饰了一个变量ResultFromTimeConsumingMethod。

        //关于被修饰的对象，也就是返回值类型是Task和Task<TResult> 函数或者Task/Task<TResult> 类型的变量：
        //如果是被修饰对象的前面用await修饰，那么返回值实际上是void或者TResult（示例中ResultFromTimeC
        //onsumingMethod是TimeConsumingMethod()函数的返回值，也就是Task<string> 类型，当ResultFromTime
        //ConsumingMethod在前面加了await关键字后 await ResultFromTimeConsumingMethod实际上完全等于 Re
        //sultFromTimeConsumingMethod.Result）。如果没有await，返回值就是Task或者Task<TResult>。
        #endregion
        /// <summary>
        /// 格式转换命令函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Ts0_Click(object sender, RoutedEventArgs e)
        {            
            inputpath = tb1.Text.Trim();
            outputpath = tb2.Text.Trim();
            FilesOfSucessCounts = 0; FilesOfSourceCounts = 0; successCount = 0; failedCount = 0; totalCount = 0;
            await Task.Run(() =>
            {
                try { FilesOfSourceCounts = FileCounter(inputpath, ".kgm|.kwm|.tkm"); } catch { }
                    
                if (FilesOfSourceCounts > 0)
                {
                    if (outputpath != null)
                    {
                        Timer1.Start();//一切合适才开启定时器
                        Transfer(Directory.GetCurrentDirectory());
                        FilesOfSucessCounts = FileCounter(outputpath, ".mp3");
                    }
                    else
                    {
                        MessageBox.Show($"输出路径有误\n请检查输出文件夹路径是否正确", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }
                }
                else
                {
                    MessageBox.Show($"无可转换文件\n请检查文件夹路径是否正确", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
            });
        }


        /// <summary>
        /// 点击按钮获取原文件路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Bt0_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog folder = new CommonOpenFileDialog();
            folder.IsFolderPicker = true;

            folder.ShowDialog();
            try
            {
                this.tb1.Text = folder.FileName;
                //inputpath = folder.FileName;
            }
            catch { }            
        }


        /// <summary>
        /// 点击按钮获取输出文件路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Bt1_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog folder = new CommonOpenFileDialog();

            folder.IsFolderPicker = true;
            folder.ShowDialog();
            try
            {
                this.tb2.Text = folder.FileName;
                //outputpath = folder.FileName;
            }
            catch { }            
        }

        /// <summary>
        /// 显示关于
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void About_Click(object sender, RoutedEventArgs e)
        {
            gyclick = !gyclick;
            if (gyclick)
            {
                about.Visibility = Visibility.Visible;
            }
            else about.Visibility = Visibility.Hidden;
        }
        #endregion

        #region 实现鼠标拖动获取文件路径
        /// <summary>
        /// 鼠标拖动滑过的函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        /// <summary>
        /// 鼠标落下时执行函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb1_PreviewDrop(object sender, DragEventArgs e)
        {
            foreach (string f in (string[])e.Data.GetData(DataFormats.FileDrop))
            {
                tb1.Text = f;                
            }
            //inputpath = tb1.Text;
        }

        /// <summary>
        /// 鼠标落下时执行函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb2_PreviewDrop(object sender, DragEventArgs e)
        {
            foreach (string f in (string[])e.Data.GetData(DataFormats.FileDrop))
            {
                tb2.Text = f;
            }
            //outputpath = tb2.Text;
        }
        #endregion

        #region 文件数量统计函数
        private int FileCounter(string inputpath,string kinds)
        {
            int n = 0;
            if (inputpath != null)
            { 
                string[] files = Directory.GetFiles(inputpath);//得到文件
                
                if (files.Length > 0)
                {
                    foreach (string file in files)//循环文件
                    {
                        string exname = file.ToLower().Substring(file.LastIndexOf(".") + 1);//得到后缀名
                        if (kinds.IndexOf(exname) > -1)
                            n++;
                    }               
                }
            }
            return n;
        }
        #endregion

        #region 外部调用Exe并监听输出
        System.Diagnostics.Process curProcess;
        private void Transfer(string Exepath)
        {
            //新建一个进程用于监听
            curProcess = new System.Diagnostics.Process();
            curProcess.OutputDataReceived -= new DataReceivedEventHandler(ProcessOutDataReceived);
            ProcessStartInfo p = new ProcessStartInfo();
            p.FileName = "cmd.exe";
            p.UseShellExecute = false;
            p.WindowStyle = ProcessWindowStyle.Hidden;
            p.CreateNoWindow = true;
            p.RedirectStandardError = true;
            p.RedirectStandardInput = true;
            p.RedirectStandardOutput = true;
            //p.Arguments = "ipconfig /all"; //用于输入命令
            curProcess.StartInfo = p;
            curProcess.Start();
            curProcess.BeginOutputReadLine();
            curProcess.OutputDataReceived += new DataReceivedEventHandler(ProcessOutDataReceived);
            curProcess.StandardInput.WriteLine($"cd {Exepath}");
            curProcess.StandardInput.WriteLine($"unlock -i {inputpath} -o {outputpath}");

            //curProcess.Close();
            //curProcess.StandardInput.WriteLine("ipconfig /all");
            //curProcess.StandardInput.WriteLine("exit");
            //curProcess.WaitForExit();
            //curProcess.Close();
        }

        /// <summary>
        /// 进程接受事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ProcessOutDataReceived(object sender, DataReceivedEventArgs e)
        {
            string str = e.Data;
            if(str.Contains("successfully converted"))successCount++;
            if (str.Contains("conversion failed")) failedCount++;
            totalCount = successCount + failedCount;
            //Console.Write(str);
            //Console.WriteLine("转换成功：" + successCount.ToString()+" 个");
            //Console.WriteLine("转换失败：" + failedCount.ToString() + " 个");
        }
        #endregion
    }
}
