using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Translation;

namespace TranslationTest
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowTranslation Translation { get; private set; }
        private TranslationFactory _translationFactory;

        public MainWindowViewModel()
        {
            var translationFolder = GetTranslationFolder();

            LoadLanguages(translationFolder);
            
            _translationFactory = new TranslationFactory(translationFolder);
            Language = new CultureInfo("en-US");

        }

        private string GetTranslationFolder()
        {
            var candidates = new[]
            {
                "Languages",
                @"..\..\Languages"
            };

            foreach (var candidate in candidates)
            {
                if (Directory.Exists(candidate))
                    return Path.GetFullPath(candidate);
            }

            return null;
        }

        private void LoadLanguages(string translationFolder)
        {
            if (!Directory.Exists(translationFolder))
                return;

            foreach (var directory in Directory.EnumerateDirectories(translationFolder))
            {
                var cultureName = Path.GetFileName(directory);
                var cultureInfo = CultureInfo.GetCultureInfo(cultureName);
                if (cultureInfo != null)
                    Languages.Add(cultureInfo);
            }
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

        public IList<CultureInfo> Languages { get; set; } = new List<CultureInfo>();

        private CultureInfo _language;
        public CultureInfo Language
        {
            get { return _language; }
            set
            {
                _language = value;
                _translationFactory.SetLanguage(value);
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
