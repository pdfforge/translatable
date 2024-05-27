using System.Globalization;
using NGettext;
using NUnit.Framework;
using Translatable.NGettext;

namespace Translatable.UnitTest
{
    [TestFixture]
    public class TranslationFactoryTest
    {
        private Catalog _catalog;
        private TranslationFactory _translationFactory;
        private TestTranslation _defaultTranslation;

        [SetUp]
        public void SetUp()
        {
            _catalog = new Catalog(new CultureInfo("En"));
            _translationFactory = new TranslationFactory(new GettextTranslationSource(_catalog));
            _defaultTranslation = new TestTranslation();
        }

        [Test]
        public void SingularString_IsLoadedFromCatalog()
        {
            var translatedText = "Nächste";

            _catalog.Translations.Add(_defaultTranslation.NextMail, new[] { translatedText });

            var translation = _translationFactory.CreateTranslation<TestTranslation>();

            Assert.AreEqual(translatedText, translation.NextMail);
        }

        [Test]
        public void SingularString_WithContext_IsNotSameAsWithoutContext()
        {
            _catalog.Translations.Add(_defaultTranslation.NextMail, new[] { "Nächste" });
            _catalog.Translations.Add("PageNavigation" + Catalog.CONTEXT_GLUE + _defaultTranslation.NextMail, new[] { "Weiter" });

            var translation = _translationFactory.CreateTranslation<TestTranslation>();

            Assert.AreNotEqual(translation.NextMail, translation.NextPage);
        }

        [Test]
        public void PluralString_WithContext_IsNotSameAsWithoutContext()
        {
            _catalog.Translations.Add(_defaultTranslation.Messages[0], new[] { "{0} Messages", "{0} Messagess" });
            _catalog.Translations.Add("OtherContext" + Catalog.CONTEXT_GLUE + _defaultTranslation.Messages[0], new[] { "{0} MessagesWithContext", "{0} MessagesWithContext" });

            var translation = _translationFactory.CreateTranslation<TestTranslation>();

            Assert.AreNotEqual(translation.Messages, translation.Messages2);
        }

        [Test]
        public void UpdateOrCreate_WithNullProperty_CreatesNewTranslation()
        {
            var translation = _translationFactory.UpdateOrCreateTranslation<TestTranslation>(null);

            Assert.NotNull(translation);
        }

        [Test]
        public void UpdateOrCreate_WithTranslatedSingularString_UpdatesTranslation()
        {
            var translation = new TestTranslation();
            translation.NextMail = "SOME_TRANSLATED_TEXT";

            translation = _translationFactory.UpdateOrCreateTranslation(translation);

            Assert.AreEqual(_defaultTranslation.NextMail, translation.NextMail);
        }

        [Test]
        public void UpdateOrCreate_WithTranslatedPluralString_UpdatesTranslation()
        {
            var translation = new TestTranslation();
            translation.Messages[0] = "SOME_TRANSLATED_TEXT";

            translation = _translationFactory.UpdateOrCreateTranslation(translation);

            Assert.AreEqual(_defaultTranslation.Messages[0], translation.Messages[0]);
        }

        [Test]
        public void UpdateOrCreate_WithTranslatedEnumArray_UpdatesTranslation()
        {
            var expectedTranslation = "SOME_TRANSLATED_TEXT";
            var translation = new TestTranslation();
            var translatedArray = translation.TestValues;
            var firstTranslatedItem = translation.TestValues[0];

            _catalog.Translations.Add(_defaultTranslation.TestValues[0].Translation, new[] { expectedTranslation });

            var updatedTranslation = _translationFactory.UpdateOrCreateTranslation(translation);

            Assert.AreSame(translation, updatedTranslation);
            Assert.AreSame(translatedArray, updatedTranslation.TestValues);
            Assert.AreSame(firstTranslatedItem, updatedTranslation.TestValues[0]);
            Assert.AreEqual(expectedTranslation, updatedTranslation.TestValues[0].Translation);
        }
    }
}