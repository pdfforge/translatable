namespace Translatable.SampleProject
{
    public class MainWindowTranslation : ITranslatable
    {
        protected IPluralBuilder PluralBuilder { get; set; } = new DefaultPluralBuilder();

        public string Title { get; protected set; } = "Main window title";

        public string SampleText { get; protected set; } = "This is my content\r\nwith multiple lines";

        public string Messages { get; protected set; } = "Messages";

        [Context("Some context")]
        public string Messages2 { get; protected set; } = "Messages";

        protected string[] NewMessagesText { get; set; } = { "You have {0} new message", "You have {0} new messages" };

        [TranslatorComment("This page is intentionally left blank")]
        public string MissingTranslation { get; set; } = "This translation might be \"missing\"";

        public EnumTranslation<TestEnum>[] TestEnumTranslation { get; protected set; } = EnumTranslation<TestEnum>.CreateDefaultEnumTranslation();

        public string FormatMessageText(int messages)
        {
            var translation = PluralBuilder.GetPlural(messages, NewMessagesText);
            return string.Format(translation, messages);
        }
    }

    public class TestTranslation : MainWindowTranslation
    {
        public string Text { get; private set; } = "Test";
    }
}
