using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyWebView2Demo
{
    /// <summary>
    /// 自定义宿主类，用于向网页注册C#对象，供JS调用
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class CustomWebView2HostObject
    {
        public string TestCalcAddByCsharpMethod(int num1, int num2, string message)
        {
            MessageBox.Show($"C#方法接收到J传入的参数 num1={num1}，num2={num2}，message={message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return "计算结果为:" + (num1 + num2);
        }
    }
}
