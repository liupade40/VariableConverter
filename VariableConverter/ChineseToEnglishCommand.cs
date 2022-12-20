using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using TinyPinyin; 

namespace VariableConverter
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ChineseToEnglishCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("b7f9edf4-94b5-4d63-9b53-a5f3985007de");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChineseToEnglishCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ChineseToEnglishCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ChineseToEnglishCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ChineseToEnglishCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ChineseToEnglishCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            if (!Config.Instance.IsUseDefault && (string.IsNullOrEmpty(Config.Instance.APPId) || string.IsNullOrEmpty(Config.Instance.Secret)))
            {
                Type optionsPageType = typeof(Config);
                Instance.package.ShowOptionPage(optionsPageType);
            }
            else
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                DTE dte = ServiceProvider.GetServiceAsync(typeof(EnvDTE.DTE)).Result as DTE;
                if (dte.ActiveDocument != null && dte.ActiveDocument.Type == "Text")
                {
                    var selection = (TextSelection)dte.ActiveDocument.Selection;
                    if (!string.IsNullOrEmpty(selection?.Text))
                    {
                        var r = Translation(selection?.Text);
                        if (r.Item1)
                        {
                            var texts = r.Item2.Split(' ');
                            selection.Text = Converter.ToTarget(texts);
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(r.Item2,"错误提示");
                        }
                    }

                }
            }
            
        }
        private string GetMD5WithString(string input)
        {
            if (input == null)
            {
                return null;
            }
            MD5 md5Hash = MD5.Create();
            //将输入字符串转换为字节数组并计算哈希数据  
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            //创建一个 Stringbuilder 来收集字节并创建字符串  
            StringBuilder sBuilder = new StringBuilder();
            //循环遍历哈希数据的每一个字节并格式化为十六进制字符串  
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            //返回十六进制字符串  
            return sBuilder.ToString();
        }
        /// <summary>
        /// 调用百度翻译API进行翻译
        /// 详情可参考http://api.fanyi.baidu.com/api/trans/product/apidoc
        /// </summary>
        /// <param name="q">待翻译字符</param>
        /// <param name="from">源语言</param>
        /// <param name="to">目标语言</param>
        /// <returns></returns>
        private TranslationResult GetTranslationFromBaiduFanyi(string q, Language from, Language to)
        {
            //可以直接到百度翻译API的官网申请
            //一定要去申请，不然程序的翻译功能不能使用
            string appId = Config.Instance.IsUseDefault ? "20221212001494371" : Config.Instance.APPId;
            string password = Config.Instance.IsUseDefault ? "4Ko8TdCHhZN9R4hgvBzE" : Config.Instance.Secret;

            //源语言
            string languageFrom = from.ToString().ToLower();
            //目标语言
            string languageTo = to.ToString().ToLower();
            //随机数
            string randomNum = System.DateTime.Now.Millisecond.ToString();
            //md5加密
            string md5Sign = GetMD5WithString(appId + q + randomNum + password);
            //url
            string url = String.Format("http://api.fanyi.baidu.com/api/trans/vip/translate?q={0}&from={1}&to={2}&appid={3}&salt={4}&sign={5}",
                HttpUtility.UrlEncode(q, Encoding.UTF8),
                languageFrom,
                languageTo,
                appId,
                randomNum,
                md5Sign
                );
            HttpClient wc = new HttpClient();
            var result = wc.GetAsync(url).Result;

            //解析json
            return JsonConvert.DeserializeObject<TranslationResult>(result.Content.ReadAsStringAsync().Result);
        }
        /// <summary>
        /// 将中文翻译为英文
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public (bool,string) Translation(string source)
        {
            TranslationResult result = GetTranslationFromBaiduFanyi(source, Language.zh, Language.en);
            //判断是否出错
            if (result.Error_code == null)
            {
                return (true, result.Trans_result[0].Dst);
            }
            else
            {
                //检查appid和密钥是否正确
                return (false,"翻译出错，错误码：" + result.Error_code + "，错误信息：" + result.Error_msg);
            }
        }
    }
    public class Translation
    {
        public string Src { get; set; }
        public string Dst { get; set; }
    }

    public class TranslationResult
    {
        //错误码，翻译结果无法正常返回
        public string Error_code { get; set; }
        public string Error_msg { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Query { get; set; }
        //翻译正确，返回的结果
        //这里是数组的原因是百度翻译支持多个单词或多段文本的翻译，在发送的字段q中用换行符（\n）分隔
        public Translation[] Trans_result { get; set; }
    }
    public enum Language
    {
        //百度翻译API官网提供了多种语言，这里只列了几种
        auto = 0,
        zh = 1,
        en = 2,
        cht = 3,
    }
}

