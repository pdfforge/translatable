
namespace Translatable.Export.UnitTest
{
    public class TestTranslation : ITranslatable
    {
        private IPluralBuilder PluralBuilder { get; set; } = new DefaultPluralBuilder();

        public string Title { get; private set; } = "Main window title";

        public string SampleText { get; private set; } = "This is my content\r\nwith multiple lines";


        [Context("Menu")]
        public string Messages2 { get; private set; } = "Messages";

        public string Messages { get; private set; } = "Messages";

        public string[] NewMessagesText { get; set; } = { "You have {0} new message", "You have {0} new messages" };

        [TranslatorComment("This page is intentionally left blank")]
        public string MissingTranslation { get; set; } = "This translation might be \"missing\"";

        public EnumTranslation<TestEnum>[] TestEnumTranslation { get; private set; } = EnumTranslation<TestEnum>.CreateDefaultEnumTranslation();

        public string FormatMessageText(int messages)
        {
            var translation = PluralBuilder.GetPlural(messages, NewMessagesText);
            return string.Format(translation, messages);
        }
    }
}