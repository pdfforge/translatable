using System.ComponentModel;
using Translation;

namespace TranslationTest
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowTranslation Translation { get; private set; }
        private TranslationFactory _translationFactory;

        public MainWindowViewModel()
        {
            _translationFactory = new TranslationFactory();
            Translation = _translationFactory.CreateTranslation<MainWindowTranslation>();
        }

        private int _messages;
        public int Messages
        {
            get
            {
                return _messages;
            }
            set
            {
                _messages = value;
                RaisePropertyChanged(nameof(MessageText));
            }
        }

        private string _language = "English";
        public string Language
        {
            get { return _language; }
            set
            {
                _language = value;
                _translationFactory.Language = value;
                Translation = _translationFactory.CreateTranslation<MainWindowTranslation>();
                RaisePropertyChanged(nameof(Translation));
                RaisePropertyChanged(nameof(MessageText));
            }
        }

        public string MessageText => Translation.FormatMessageText(_messages);

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
