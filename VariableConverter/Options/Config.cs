using System.ComponentModel;
using System.Runtime.InteropServices;

namespace VariableConverter
{
    internal partial class OptionsProvider
    {
        // Register the options with this attribute on your package class:
        // [ProvideOptionPage(typeof(OptionsProvider.ConfigOptions), "VariableConverter", "Config", 0, 0, true, SupportsProfiles = true)]
        [ComVisible(true)]
        public class ConfigOptions : BaseOptionPage<Config> { }
    }

    public class Config : BaseOptionModel<Config>
    {
        [Category("变量转换")]
        [DisplayName("APPId")]
        [Description("百度翻译的APPId，可通过https://fanyi-api.baidu.com/product/11获取")] 
        public string APPId { get; set; }
        [Category("变量转换")]
        [DisplayName("Secret")]
        [Description("百度翻译的Secret，可通过https://fanyi-api.baidu.com/product/11获取")]
        public string Secret { get; set; }
        [Category("变量转换")]
        [DisplayName("使用默认APPId-Secret")]
        [Description("默认使用的是作者百度翻译的APPId-Secret，长期使用建议使用自己的APPId")]
        [DefaultValue(false)]
        public bool IsUseDefault { get; set; } = false;
    }
}
