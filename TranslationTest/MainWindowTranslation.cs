using Translation;

namespace TranslationTest
{
    public class MainWindowTranslation : ITranslatable
    {
        public string Title { get; private set; } = "Main window title";

        public string SampleText { get; private set; } = "This is my content";

        public string Messages { get; private set; } = "Messages";

        private string NewMessagesText { get; set; } = "You have {0} new messages";

        [TranslatorComment("This page is intentionally left blank")]
        public string MissingTranslation { get; set; } = "This translation might be missing";

        public string FormatMessageText(int messages)
        {
            return string.Format(NewMessagesText, messages);
        }
    }
}
