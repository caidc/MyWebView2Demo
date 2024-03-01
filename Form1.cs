using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
//using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MyWebView2Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            webView21.Source = new Uri("UI.html");
            InitializeAsync();

        }
        async void InitializeAsync()
        {
            await webView21.EnsureCoreWebView2Async(null);
            // 接收来自web页面的消息
            webView21.CoreWebView2.WebMessageReceived += UpdateAddressBar;
            webView21.CoreWebView2.AddHostObjectToScript("customWebView2HostObject", new CustomWebView2HostObject());

        }

        private void UpdateAddressBar(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            String uri = e.TryGetWebMessageAsString();
            label1.Text = uri;
            //webView21.CoreWebView2.PostWebMessageAsString(uri);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            webView21.CoreWebView2.PostWebMessageAsString(textBox1.Text);

            //webView21.CoreWebView2.ExecuteScriptAsync("receiveMsgToCSharp('这是发送消息后显示内容')");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var weatherForecast = new WeatherForecast
            {
                Date = DateTime.Parse("2019-08-01"),
                TemperatureCelsius = 23,
                Summary = "Hot",
                num = new List<int>{ 1, 23, 4 }
            };
            //这里才用到Newtonsoft.Json;
            string jsonString = JsonConvert.SerializeObject(weatherForecast);
            webView21.CoreWebView2.ExecuteScriptAsync($"alert('收到C#要求执行的js函数，弹窗显示js_value：{jsonString}')");
            webView21.CoreWebView2.ExecuteScriptAsync($"receiveMsgFromCSharp('{jsonString}')");
        }
        ////多个值用json序列化传递，前端用js反序列化解析获取
        public class WeatherForecast
        {
            public DateTimeOffset Date { get; set; }
            public int TemperatureCelsius { get; set; }
            public string Summary { get; set; }
            public List<int> num { get; set; }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            var weatherForecast = new WeatherForecast
            {
                Date = DateTime.Parse("2019-08-01"),
                TemperatureCelsius = 25,
                Summary = "Hot"
            };
            //这里才用到Newtonsoft.Json;
            string jsonString = JsonConvert.SerializeObject(weatherForecast);
            webView21.CoreWebView2.PostWebMessageAsJson(jsonString);
        }
    }
}
