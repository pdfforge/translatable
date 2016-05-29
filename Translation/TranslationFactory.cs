using System;
using System.Globalization;

namespace Translation
{
    public class TranslationFactory
    {
        private readonly string _translationDir;
        private TranslationSource _translationSource;

        public TranslationFactory(string translationDir)
        {
            _translationDir = translationDir;
            SetLanguage(new CultureInfo("en-US"));
        }

        public void SetLanguage(CultureInfo culture)
        {
            _translationSource = new TranslationSource(_translationDir, culture);
        }

        public T CreateTranslation<T>() where T: ITranslatable
        {
            var instance = Activator.CreateInstance<T>();

            _translationSource.Translate(instance);

            return instance;
        }
    }
}
