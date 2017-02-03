using NGettext;
using Translatable.NGettext;
using Xunit;

namespace Translatable.UnitTest
{
    public class TranslationFactoryTest
    {
        private readonly Catalog _catalog;
        private readonly TranslationFactory _translationFactory;
        private readonly TestTranslation _defaultTranslation;

        public TranslationFactoryTest()
        {
            _catalog = new Catalog();
            _translationFactory = new TranslationFactory(new GettextTranslationSource(_catalog));
            _defaultTranslation = new TestTranslation();
        }

        [Fact]
        public void SingularString_IsLoadedFromCatalog()
        {
            var translatedText = "Nächste";

            _catalog.Translations.Add(_defaultTranslation.NextMail, new[] { translatedText });

            var translation = _translationFactory.CreateTranslation<TestTranslation>();

            Assert.Equal(translatedText, translation.NextMail);
        }

        [Fact]
        public void SingularString_WithContext_IsNotSameAsWithoutContext()
        {
            _catalog.Translations.Add(_defaultTranslation.NextMail, new[] { "Nächste" });
            _catalog.Translations.Add("PageNavigation" + Catalog.CONTEXT_GLUE + _defaultTranslation.NextMail, new[] { "Weiter" });

            var translation = _translationFactory.CreateTranslation<TestTranslation>();

            Assert.NotEqual(translation.NextMail, translation.NextPage);
        }

        [Fact]
        public void PluralString_WithContext_IsNotSameAsWithoutContext()
        {
            _catalog.Translations.Add(_defaultTranslation.Messages[0], new[] { "{0} Messages", "{0} Messagess" });
            _catalog.Translations.Add("OtherContext" + Catalog.CONTEXT_GLUE + _defaultTranslation.Messages[0], new[] { "{0} MessagesWithContext", "{0} MessagesWithContext" });

            var translation = _translationFactory.CreateTranslation<TestTranslation>();

            Assert.NotEqual(translation.Messages, translation.Messages2);
        }
    }
}
