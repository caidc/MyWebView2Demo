# MyWebView2Demo

[Microsoft Edge WebView2简介](https://learn.microsoft.com/zh-cn/microsoft-edge/webview2/)
## C#调用js代码
###  C#发送message（文本或者json数据）消息给js页面
#### 采用PostWebMessageAsString()函数发送文本
```csharp
private void button1_Click(object sender, EventArgs e)
{
    webView21.CoreWebView2.PostWebMessageAsString(textBox1.Text);
}
```
web页面采用window.chrome.webview.addEventListener('message', handle)接受消息
```javascript
window.chrome.webview.addEventListener('message',function (event) {
  document.getElementById("msg").innerText = event.data;
});
```
注：其实AsString也可以发送json数据，C#中将其序列化为字符串，然后前端解析该字符串（参考：）
```csharp
////多个值用json序列化传递，前端用js反序列化解析获取
public class WeatherForecast
{
    public DateTimeOffset Date { get; set; }
    public int TemperatureCelsius { get; set; }
    public string Summary { get; set; }
}
private void button2_Click(object sender, EventArgs e)
{
        var weatherForecast = new WeatherForecast
        {
            Date = DateTime.Parse("2019-08-01"),
            TemperatureCelsius = 25,
            Summary = "Hot"
        };
    //这里才用到System.Text.Json;
    string jsonString = JsonSerializer.Serialize(weatherForecast);
    webView21.CoreWebView2.PostWebMessageAsString(jsonString);
}
```
```javascript
//接收json
window.chrome.webview.addEventListener("message", function(event) {
  const json = event.data;
  console.log(json);
  const obj = JSON.parse(json);
  // alert(123);
  console.log(obj);
  var count = Object.keys(obj).length;
  console.log(count);
  console.log(obj.Date);
  console.log(obj.TemperatureCelsius);
  console.log(obj.Summary);
} );
```
#### 采用PostWebMessageAsJSON()函数发送json数据
C#代码里先把json数据序列化，然后前端可以直接读取到json数据
```csharp
////多个值用json序列化传递，前端用js反序列化解析获取
public class WeatherForecast
{
    public DateTimeOffset Date { get; set; }
    public int TemperatureCelsius { get; set; }
    public string Summary { get; set; }
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
```
web页面采用window.chrome.webview.addEventListener('message', handle)接受消息
```csharp
//接收json
window.chrome.webview.addEventListener("message", function(event) {
    const json = event.data;
    console.log(json);
} );
```
### C#中向web页面植入并执行js代码块或函数
ExecuteScriptAsync()
```csharp
js_value参数就是字符串，可以是单纯的字符串，也可以是反序列化后的json字符串。
webView21.CoreWebView2.ExecuteScriptAsync($"alert('收到C#要求执行的js函数，弹窗显示js_value：{js_value}')");
```

```javascript
// 该函数供C#调用
function receiveMsgFromCSharp(msg) {
  console.log(msg);
  if(isJsonString(msg)){
    var obj=JSON.pares(msg)
  }
  document.getElementById("msg").innerText = msg;
}
// js判断字符串是否可转为json数据,如果可以则返回true
function isJsonString(str) {
  try {
    if (typeof JSON.parse(str) == "object") {
      return true;
    }
  } catch(e) {
  }
  return false;
}
```
## js调用C#函数
### 步骤1
C#定义一个主机对象，如：CustomWebView2HostObject类，在类中编写方法并实现内部业务逻辑。
```csharp
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

```
自定义的 CustomWebView2HostObject 类，必须标记 [ClassInterface(ClassInterfaceType.AutoDual)]、[ComVisible(true)] 特性，否则JS无法访问到该类
### 步骤2
在访问目标网页之前，通过webView2.CoreWebView2.AddHostObjectToScript()方法向网页中注入主机对象，其中第一个参数是自定义名称（随意命名），JS中访问主机对象时就需要与该参数名称一致。
```csharp
async void InitializeAsync()
{
    await webView21.EnsureCoreWebView2Async(null);
    // 接收来自web页面的消息
    webView21.CoreWebView2.WebMessageReceived += UpdateAddressBar;
    // 注册该函数
    webView21.CoreWebView2.AddHostObjectToScript("customWebView2HostObject", new CustomWebView2HostObject());
}
```
### 步骤3
```javascript
// 获取主机对象
var hostObject = window.chrome.webview.hostObjects.customWebView2HostObject;
// 通过主机对象调用C#方法
var result = await hostObject.TestCalcAddByCsharpMethod(12, 14, "加法计算");
```
点击事件中，第31行获取主机对象，customWebView2HostObject 与 C#中定义的名称需要完全相同。
使用主机对象调用C#方法，由于调用过程是异步的，所以需要使用 await，方法定义前需要加上 async。
以上三步完成后即实现了JS访问C#方法。

参考：

- [WebView2使用PostMessage方法进行前后端传值](https://zhuanlan.zhihu.com/p/628454801)
- [WinForms 应用中的 WebView2 入门 - Microsoft Edge Developer documentation](https://learn.microsoft.com/zh-cn/microsoft-edge/webview2/get-started/winforms#step-4---install-the-webview2-sdk)
- [使用 VebView2，在C#WinForm中显示前端效果，做到C#与js通讯_c# webview2通信-CSDN博客](https://blog.csdn.net/Yueqin0512/article/details/115787248?spm=1001.2101.3001.6661.1&utm_medium=distribute.pc_relevant_t0.none-task-blog-2%7Edefault%7ECTRLIST%7ERate-1-115787248-blog-115859825.235%5Ev43%5Epc_blog_bottom_relevance_base9&depth_1-utm_source=distribute.pc_relevant_t0.none-task-blog-2%7Edefault%7ECTRLIST%7ERate-1-115787248-blog-115859825.235%5Ev43%5Epc_blog_bottom_relevance_base9&utm_relevant_index=1)
