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

namespace KGM_TO_MP3
{
    /// <summary>
    /// About.xaml 的交互逻辑
    /// </summary>
    public partial class About : UserControl
    {
        public About()
        {
            InitializeComponent();
            this.bangzhu.Text = "软件用途：实现.kgm格式->.mp3格式转换 \n使用方法：①将待转换文件夹及输出文件夹拖入相应文本框(或点击相应按钮选择文件夹)以获取输入输出路径②点击最右侧大图标完成格式转换\n特别感谢：软件核心算法取自 https://github.com/unlock-music/cli";
        }
    }
}
