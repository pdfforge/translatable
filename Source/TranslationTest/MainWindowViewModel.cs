using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Translatable.NGettext;

namespace Translatable.TranslationTest
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private const string MoDomain = "messages";
        private readonly string _translationFolder;

        private CultureInfo _language;

        private int _messages;
        private readonly TranslationFactory _translationFactory;
        private TestEnum _selectedTestEnum;

        public MainWindowViewModel()
        {
            _translationFolder = GetTranslationFolder();

            if (_translationFolder != null)
                LoadLanguages(_translationFolder);

            _translationFactory = new TranslationFactory();
            _translationFactory.TranslationChanged += OnTranslationChanged;

            Language = new CultureInfo("en-US");

            SelectedTestEnum = TestEnum.FirstValue;
        }

        public MainWindowTranslation Translation { get; private set; } = new MainWindowTranslation();

        public TestEnum SelectedTestEnum
        {
            get { return _selectedTestEnum; }
            set { _selectedTestEnum = value; RaisePropertyChanged(nameof(SelectedTestEnum)); }
        }

        public int Messages
        {
            get { return _messages; }
            set
            {
                _messages = value;
                RaisePropertyChanged(nameof(MessageText));
            }
        }

        public IList<CultureInfo> Languages { get; set; } = new List<CultureInfo>();

        public CultureInfo Language
        {
            get { return _language; }
            set
            {
                _language = value;
                SetLanguage(value);
            }
        }

        public string MessageText => Translation.FormatMessageText(_messages);

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetLanguage(CultureInfo cultureInfo)
        {
            if (_translationFolder == null)
            {
                return;
            }

            _translationFactory.TranslationSource = new GettextTranslationSource(_translationFolder, MoDomain, cultureInfo);
            _language = cultureInfo;
        }

        private void OnTranslationChanged(object sender, EventArgs eventArgs)
        {
            Translation = _translationFactory.UpdateOrCreateTranslation(Translation);
            RaisePropertyChanged(nameof(Translation));
            RaisePropertyChanged(nameof(MessageText));
        }

        private string GetTranslationFolder()
        {
            var candidates = new[]
            {
                "Languages",
                @"..\..\Languages"
            };

            foreach (var candidate in candidates)
                if (Directory.Exists(candidate))
                    return Path.GetFullPath(candidate);

            return null;
        }

        private void LoadLanguages(string translationFolder)
        {
            if (!Directory.Exists(translationFolder))
                return;

            Languages.Add(new CultureInfo("en-US"));

            foreach (var directory in Directory.EnumerateDirectories(translationFolder))
            {
                if (!Directory.EnumerateFiles(directory, "*.mo", SearchOption.AllDirectories).Any())
                    continue;

                try
                {
                    var cultureName = Path.GetFileName(directory);
                    var cultureInfo = CultureInfo.GetCultureInfo(cultureName);
                    Languages.Add(cultureInfo);
                }
                catch (CultureNotFoundException)
                {
                }
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
