using NGettext;
using System.Globalization;
using NUnit.Framework;
using Translatable.NGettext;
using Translatable.TranslationTest;

namespace Translatable.UnitTest
{
    [TestFixture]
    public class CachedTranslationFactoryTest
    {
        private TranslationFactory _translationFactory;
        private CachedTranslationFactory _cachedTranslationFactory;

        [SetUp]
        public void SetUp()
        {
            _translationFactory = new TranslationFactory(new GettextTranslationSource(new Catalog(new CultureInfo("En"))));
            _cachedTranslationFactory = new CachedTranslationFactory(_translationFactory);
        }

        [Test]
        public void CreateTranslation_InvokedTwice_ReturnsSameInstance()
        {
            var tr1 = _cachedTranslationFactory.CreateTranslation<TestTranslation>();
            var tr2 = _cachedTranslationFactory.CreateTranslation<TestTranslation>();

            Assert.AreSame(tr1, tr2);
        }

        [Test]
        public void CreateTranslation_InvokedTwice_WithCachedCleared_ReturnsNewInstance()
        {
            var tr1 = _cachedTranslationFactory.CreateTranslation<TestTranslation>();

            _cachedTranslationFactory.ClearCache();

            var tr2 = _cachedTranslationFactory.CreateTranslation<TestTranslation>();
            Assert.AreNotSame(tr1, tr2);
        }

        [Test]
        public void CreateTranslationNonGeneric_InvokedTwice_ReturnsSameInstance()
        {
            var tr1 = _cachedTranslationFactory.CreateTranslation(typeof(TestTranslation));
            var tr2 = _cachedTranslationFactory.CreateTranslation(typeof(TestTranslation));

            Assert.AreSame(tr1, tr2);
        }

        [Test]
        public void TranslationChanged_WhenSourceTranslationCahnged_IsRaised()
        {
            var wasCalled = false;
            _cachedTranslationFactory.TranslationChanged += (sender, args) => wasCalled = true;

            RaiseTranslationChanged();

            Assert.True(wasCalled);
        }

        [Test]
        public void TranslationChanged_WhenRaised_ClearsCache()
        {
            var tr1 = _cachedTranslationFactory.CreateTranslation<TestTranslation>();

            RaiseTranslationChanged();

            var tr2 = _cachedTranslationFactory.CreateTranslation<TestTranslation>();
            Assert.AreNotSame(tr1, tr2);
        }

        [Test]
        public void UpdateOrCreateTranslation_WithNull_CreatesNewTranslation()
        {
            var tr = _cachedTranslationFactory.UpdateOrCreateTranslation<TestTranslation>(null);

            Assert.NotNull(tr);
        }

        [Test]
        public void UpdateOrCreateTranslation_WithTranslationInCache_UsesCachedTranslation()
        {
            var tr1 = _cachedTranslationFactory.CreateTranslation<TestTranslation>();

            var tr2 = _cachedTranslationFactory.UpdateOrCreateTranslation<TestTranslation>(null);

            Assert.AreSame(tr1, tr2);
        }

        [Test]
        public void UpdateOrCreateTranslation_WithTranslationAndTranslationInCache_ModifiedPassedTranslation()
        {
            var tr1 = _cachedTranslationFactory.CreateTranslation<TestTranslation>();

            var tr2 = _cachedTranslationFactory.UpdateOrCreateTranslation(new TestTranslation());

            Assert.AreNotSame(tr1, tr2);
        }

        [Test]
        public void UpdateOrCreateTranslation_WhenUpdatingSameObjectTwice_DoesNotUpdateSecondTime()
        {
            var translation = new TestTranslation();
            var expectedText = "SOME_TRANSLATION";

            _cachedTranslationFactory.UpdateOrCreateTranslation(translation);
            // change some text to test the caching
            translation.NextMail = expectedText;
            _cachedTranslationFactory.UpdateOrCreateTranslation(translation);

            Assert.AreEqual(expectedText, translation.NextMail);
        }

        [Test]
        public void UpdateOrCreateTranslation_WhenUpdatingSameObjectTwiceAndClearingCacheInbetween_UpdatesSecondTime()
        {
            var translation = new TestTranslation();
            var expectedText = "SOME_TRANSLATION";

            _cachedTranslationFactory.UpdateOrCreateTranslation(translation);

            // change some text to test the caching
            translation.NextMail = expectedText;
            _cachedTranslationFactory.ClearCache();

            _cachedTranslationFactory.UpdateOrCreateTranslation(translation);

            Assert.AreNotEqual(expectedText, translation.NextMail);
        }

        [Test]
        public void CreateEnumTranslation_CalledTwice_ReturnsSameInstance()
        {
            var tr1 = _cachedTranslationFactory.CreateEnumTranslation<TestEnum>();
            var tr2 = _cachedTranslationFactory.CreateEnumTranslation<TestEnum>();

            Assert.AreSame(tr1, tr2);
        }

        [Test]
        public void CreateEnumTranslation_CalledTwiceWithClearCache_ReturnsTwoInstances()
        {
            var tr1 = _cachedTranslationFactory.CreateEnumTranslation<TestEnum>();
            _cachedTranslationFactory.ClearCache();
            var tr2 = _cachedTranslationFactory.CreateEnumTranslation<TestEnum>();

            Assert.AreNotSame(tr1, tr2);
        }

        [Test]
        public void UpdateOrCreateEnumTranslation_CalledTwiceWithNull_ReturnsSameInstance()
        {
            var tr1 = _cachedTranslationFactory.UpdateOrCreateEnumTranslation<TestEnum>(null);
            var tr2 = _cachedTranslationFactory.UpdateOrCreateEnumTranslation<TestEnum>(null);

            Assert.AreSame(tr1, tr2);
        }

        [Test]
        public void UpdateOrCreateEnumTranslation_CalledTwiceOnObjectAndCacheCleared_UpdateSecondTime()
        {
            var wasCalled = false;
            var tr1 = _cachedTranslationFactory.UpdateOrCreateEnumTranslation<TestEnum>(null);
            tr1[0].PropertyChanged += (sender, args) => wasCalled = true;
            _cachedTranslationFactory.ClearCache();
            var tr2 = _cachedTranslationFactory.UpdateOrCreateEnumTranslation(tr1);

            Assert.AreSame(tr1, tr2);
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void UpdateOrCreateEnumTranslation_CalledTwiceOnObject_DoesNotUpdateSecondTime()
        {
            var wasCalled = false;
            var tr1 = _cachedTranslationFactory.UpdateOrCreateEnumTranslation<TestEnum>(null);
            tr1[0].PropertyChanged += (sender, args) => wasCalled = true;
            var tr2 = _cachedTranslationFactory.UpdateOrCreateEnumTranslation(tr1);

            Assert.AreSame(tr1, tr2);
            Assert.False(wasCalled);
        }

        private void RaiseTranslationChanged()
        {
            _translationFactory.TranslationSource = new GettextTranslationSource(new Catalog());
        }
    }
}