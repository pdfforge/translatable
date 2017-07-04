using System;
using System.Collections.Generic;

namespace Translatable
{
    public class CachedTranslationFactory : ITranslationFactory
    {
        private readonly Dictionary<Type, ITranslatable> _instanceCache = new Dictionary<Type, ITranslatable>();
        private readonly Dictionary<Type, object> _enumCache = new Dictionary<Type, object>();
        private readonly HashSet<object> _updateCache = new HashSet<object>();
        private readonly ITranslationFactory _baseTranslationFactory;

        public CachedTranslationFactory(ITranslationFactory baseTranslationFactory)
        {
            baseTranslationFactory.TranslationChanged += (sender, args) =>
            {
                ClearCache();
                TranslationChanged?.Invoke(this, args);
            };

            _baseTranslationFactory = baseTranslationFactory;
        }

        public void ClearCache()
        {
            _instanceCache.Clear();
            _updateCache.Clear();
            _enumCache.Clear();
        }

        public T CreateTranslation<T>() where T : ITranslatable, new()
        {
            var type = typeof(T);
            if (_instanceCache.ContainsKey(type))
                return (T)_instanceCache[type];

            var translation = _baseTranslationFactory.CreateTranslation<T>();
            _instanceCache[type] = translation;

            return translation;
        }

        public T UpdateOrCreateTranslation<T>(T translation) where T : ITranslatable, new()
        {
            var type = typeof(T);

            var isCached = _instanceCache.ContainsKey(type);

            if (_updateCache.Contains(translation))
                return translation;

            if (translation == null && isCached)
                return (T)_instanceCache[type];

            translation = _baseTranslationFactory.UpdateOrCreateTranslation(translation);

            if (!isCached)
                _instanceCache[type] = translation;

            _updateCache.Add(translation);

            return translation;
        }

        public ITranslatable CreateTranslation(Type t)
        {
            if (_instanceCache.ContainsKey(t))
                return _instanceCache[t];

            var translation = _baseTranslationFactory.CreateTranslation(t);
            _instanceCache[t] = translation;

            return translation;
        }

        public EnumTranslation<T>[] CreateEnumTranslation<T>() where T : struct, IConvertible
        {
            var type = typeof(T);
            if (_enumCache.ContainsKey(type))
                return (EnumTranslation<T>[])_enumCache[type];

            var translation = _baseTranslationFactory.CreateEnumTranslation<T>();
            _enumCache[type] = translation;

            return translation;
        }

        public EnumTranslation<T>[] UpdateOrCreateEnumTranslation<T>(EnumTranslation<T>[] translations) where T : struct, IConvertible
        {
            var type = typeof(T);
            if (translations == null && _enumCache.ContainsKey(type))
            {
                translations = (EnumTranslation<T>[])_enumCache[type];
                _updateCache.Add(translations);

                return translations;
            }

            if (translations != null && _updateCache.Contains(translations))
                return translations;

            translations = _baseTranslationFactory.UpdateOrCreateEnumTranslation(translations);

            _enumCache[type] = translations;
            _updateCache.Add(translations);

            return translations;
        }

        public event EventHandler TranslationChanged;
    }
}
