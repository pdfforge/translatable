using NGettext;
using Translatable.NGettext;
using Translatable.TranslationTest;
using Xunit;

namespace Translatable.UnitTest
{
    public class CachedTranslationFactoryTest
    {
        private readonly TranslationFactory _translationFactory;
        private readonly CachedTranslationFactory _cachedTranslationFactory;

        public CachedTranslationFactoryTest()
        {
            _translationFactory = new TranslationFactory(new GettextTranslationSource(new Catalog()));
            _cachedTranslationFactory = new CachedTranslationFactory(_translationFactory);
        }

        [Fact]
        public void CreateTranslation_InvokedTwice_ReturnsSameInstance()
        {
            var tr1 = _cachedTranslationFactory.CreateTranslation<TestTranslation>();
            var tr2 = _cachedTranslationFactory.CreateTranslation<TestTranslation>();

            Assert.Same(tr1, tr2);
        }

        [Fact]
        public void CreateTranslation_InvokedTwice_WithCachedCleared_ReturnsNewInstance()
        {
            var tr1 = _cachedTranslationFactory.CreateTranslation<TestTranslation>();

            _cachedTranslationFactory.ClearCache();

            var tr2 = _cachedTranslationFactory.CreateTranslation<TestTranslation>();
            Assert.NotSame(tr1, tr2);
        }

        [Fact]
        public void CreateTranslationNonGeneric_InvokedTwice_ReturnsSameInstance()
        {
            var tr1 = _cachedTranslationFactory.CreateTranslation(typeof(TestTranslation));
            var tr2 = _cachedTranslationFactory.CreateTranslation(typeof(TestTranslation));

            Assert.Same(tr1, tr2);
        }

        [Fact]
        public void TranslationChanged_WhenSourceTranslationCahnged_IsRaised()
        {
            var wasCalled = false;
            _cachedTranslationFactory.TranslationChanged += (sender, args) => wasCalled = true;

            RaiseTranslationChanged();

            Assert.True(wasCalled);
        }

        [Fact]
        public void TranslationChanged_WhenRaised_ClearsCache()
        {
            var tr1 = _cachedTranslationFactory.CreateTranslation<TestTranslation>();

            RaiseTranslationChanged();

            var tr2 = _cachedTranslationFactory.CreateTranslation<TestTranslation>();
            Assert.NotSame(tr1, tr2);
        }

        [Fact]
        public void UpdateOrCreateTranslation_WithNull_CreatesNewTranslation()
        {
            var tr = _cachedTranslationFactory.UpdateOrCreateTranslation<TestTranslation>(null);

            Assert.NotNull(tr);
        }

        [Fact]
        public void UpdateOrCreateTranslation_WithTranslationInCache_UsesCachedTranslation()
        {
            var tr1 = _cachedTranslationFactory.CreateTranslation<TestTranslation>();

            var tr2 = _cachedTranslationFactory.UpdateOrCreateTranslation<TestTranslation>(null);

            Assert.Same(tr1, tr2);
        }

        [Fact]
        public void UpdateOrCreateTranslation_WithTranslationAndTranslationInCache_ModifiedPassedTranslation()
        {
            var tr1 = _cachedTranslationFactory.CreateTranslation<TestTranslation>();

            var tr2 = _cachedTranslationFactory.UpdateOrCreateTranslation(new TestTranslation());

            Assert.NotSame(tr1, tr2);
        }

        [Fact]
        public void UpdateOrCreateTranslation_WhenUpdatingSameObjectTwice_DoesNotUpdateSecondTime()
        {
            var translation = new TestTranslation();
            var expectedText = "SOME_TRANSLATION";

            _cachedTranslationFactory.UpdateOrCreateTranslation(translation);
            // change some text to test the caching
            translation.NextMail = expectedText;
            _cachedTranslationFactory.UpdateOrCreateTranslation(translation);

            Assert.Equal(expectedText, translation.NextMail);
        }

        [Fact]
        public void UpdateOrCreateTranslation_WhenUpdatingSameObjectTwiceAndClearingCacheInbetween_UpdatesSecondTime()
        {
            var translation = new TestTranslation();
            var expectedText = "SOME_TRANSLATION";

            _cachedTranslationFactory.UpdateOrCreateTranslation(translation);

            // change some text to test the caching
            translation.NextMail = expectedText;
            _cachedTranslationFactory.ClearCache();

            _cachedTranslationFactory.UpdateOrCreateTranslation(translation);

            Assert.NotEqual(expectedText, translation.NextMail);
        }

        [Fact]
        public void CreateEnumTranslation_CalledTwice_ReturnsSameInstance()
        {
            var tr1 = _cachedTranslationFactory.CreateEnumTranslation<TestEnum>();
            var tr2 = _cachedTranslationFactory.CreateEnumTranslation<TestEnum>();

            Assert.Same(tr1, tr2);
        }

        [Fact]
        public void CreateEnumTranslation_CalledTwiceWithClearCache_ReturnsTwoInstances()
        {
            var tr1 = _cachedTranslationFactory.CreateEnumTranslation<TestEnum>();
            _cachedTranslationFactory.ClearCache();
            var tr2 = _cachedTranslationFactory.CreateEnumTranslation<TestEnum>();

            Assert.NotSame(tr1, tr2);
        }

        [Fact]
        public void UpdateOrCreateEnumTranslation_CalledTwiceWithNull_ReturnsSameInstance()
        {
            var tr1 = _cachedTranslationFactory.UpdateOrCreateEnumTranslation<TestEnum>(null);
            var tr2 = _cachedTranslationFactory.UpdateOrCreateEnumTranslation<TestEnum>(null);

            Assert.Same(tr1, tr2);
        }

        [Fact]
        public void UpdateOrCreateEnumTranslation_CalledTwiceOnObjectAndCacheCleared_UpdateSecondTime()
        {
            var wasCalled = false;
            var tr1 = _cachedTranslationFactory.UpdateOrCreateEnumTranslation<TestEnum>(null);
            tr1[0].PropertyChanged += (sender, args) => wasCalled = true;
            _cachedTranslationFactory.ClearCache();
            var tr2 = _cachedTranslationFactory.UpdateOrCreateEnumTranslation(tr1);

            Assert.Same(tr1, tr2);
            Assert.True(wasCalled);
        }

        [Fact]
        public void UpdateOrCreateEnumTranslation_CalledTwiceOnObject_DoesNotUpdateSecondTime()
        {
            var wasCalled = false;
            var tr1 = _cachedTranslationFactory.UpdateOrCreateEnumTranslation<TestEnum>(null);
            tr1[0].PropertyChanged += (sender, args) => wasCalled = true;
            var tr2 = _cachedTranslationFactory.UpdateOrCreateEnumTranslation(tr1);

            Assert.Same(tr1, tr2);
            Assert.False(wasCalled);
        }

        private void RaiseTranslationChanged()
        {
            _translationFactory.TranslationSource = new GettextTranslationSource(new Catalog());
        }
    }
}
