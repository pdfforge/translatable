using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translation
{
    public class TranslationFactory
    {
        private TranslationSource _translationSource;
        private TranslationSource _translationSource_de;

        public string Language { get; set; } = "English";

        public TranslationFactory()
        {
            _translationSource = new TranslationSource();
            _translationSource.Translations.Add("Main window title", "Main window title");
            _translationSource.Translations.Add("Messages", "Messages");
            _translationSource.Translations.Add("This is my content", "This is my content");
            _translationSource.Translations.Add("You have {0} new messages", "You have {0} new messages");

            _translationSource_de = new TranslationSource();
            _translationSource_de.Translations.Add("Main window title", "Hauptfenstertitel");
            _translationSource_de.Translations.Add("Messages", "Nachrichten");
            _translationSource_de.Translations.Add("This is my content", "Dies ist mein Inhalt");
            _translationSource_de.Translations.Add("You have {0} new messages", "Sie haben {0} neue Nachrichten");
        }

        public T CreateTranslation<T>() where T: ITranslatable
        {
            var instance = Activator.CreateInstance<T>();

            var source = Language.Equals("German", StringComparison.InvariantCultureIgnoreCase)
                ? _translationSource_de
                : _translationSource;

            source.Translate(instance);

            return instance;
        }
    }
}
