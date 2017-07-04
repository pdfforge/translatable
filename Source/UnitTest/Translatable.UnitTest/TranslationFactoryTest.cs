using System;
using System.Linq;
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

        [Fact]
        public void UpdateOrCreate_WithNullProperty_CreatesNewTranslation()
        {
            var translation = _translationFactory.UpdateOrCreateTranslation<TestTranslation>(null);

            Assert.NotNull(translation);
        }

        [Fact]
        public void UpdateOrCreate_WithTranslatedSingularString_UpdatesTranslation()
        {
            var translation = new TestTranslation();
            translation.NextMail = "SOME_TRANSLATED_TEXT";

            translation = _translationFactory.UpdateOrCreateTranslation(translation);

            Assert.Equal(_defaultTranslation.NextMail, translation.NextMail);
        }

        [Fact]
        public void UpdateOrCreate_WithTranslatedPluralString_UpdatesTranslation()
        {
            var translation = new TestTranslation();
            translation.Messages[0] = "SOME_TRANSLATED_TEXT";

            translation = _translationFactory.UpdateOrCreateTranslation(translation);

            Assert.Equal(_defaultTranslation.Messages[0], translation.Messages[0]);
        }

        [Fact]
        public void UpdateOrCreate_WithTranslatedEnumArray_UpdatesTranslation()
        {
            var expectedTranslation = "SOME_TRANSLATED_TEXT";
            var translation = new TestTranslation();
            var translatedArray = translation.TestValues;
            var firstTranslatedItem = translation.TestValues[0];

            _catalog.Translations.Add(_defaultTranslation.TestValues[0].Translation, new[] { expectedTranslation });

            var updatedTranslation = _translationFactory.UpdateOrCreateTranslation(translation);

            Assert.Same(translation, updatedTranslation);
            Assert.Same(translatedArray, updatedTranslation.TestValues);
            Assert.Same(firstTranslatedItem, updatedTranslation.TestValues[0]);
            Assert.Equal(expectedTranslation, updatedTranslation.TestValues[0].Translation);
        }
    }
}
