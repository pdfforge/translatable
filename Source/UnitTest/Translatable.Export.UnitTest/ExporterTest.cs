using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Translatable.Export.Po;
using Xunit;

namespace Translatable.Export.UnitTest
{
    public class ExporterTest
    {
        [Fact]
        public void ExportMainWindowTranslation_ContainsFiveTranslations()
        {
            var catalog = DoExport(typeof(TestTranslation));
            
            Assert.Equal(5, catalog.Entries.Count);
        }

        [Fact]
        public void ExportMainWindowTranslation_ContainsTitleTranslation()
        {
            var catalog = DoExport(typeof(TestTranslation));

            var translation = new TestTranslation();

            Assert.True(HasTranslation(catalog, translation.Title));
        }

        [Fact]
        public void ExportMainWindowTranslation_ContainsNewMessagesTextPluralTranslation()
        {
            var catalog = DoExport(typeof(TestTranslation));

            var translation = new TestTranslation();

            Assert.True(GetPlurals(catalog).Any(entry => entry.MsgIdSingular == translation.NewMessagesText[0] &&  entry.MsgIdPlural == translation.NewMessagesText[1]));
        }

        [Fact]
        public void ExportTestEnum_ContainsCorrectNumberofTranslations()
        {
            var catalog = DoExport(typeof(TestEnum));

            Assert.Equal(Enum.GetValues(typeof(TestEnum)).Length, catalog.Entries.Count);
        }

        [Fact]
        public void ExportTestEnum_ContainsTranslationsForEachEntry()
        {
            var catalog = DoExport(typeof(TestEnum));

            Assert.True(HasTranslation(catalog, TranslationAttribute.GetValue(TestEnum.FirstValue)));
            Assert.True(HasTranslation(catalog, TranslationAttribute.GetValue(TestEnum.SecondValue)));
            Assert.True(HasTranslation(catalog, TranslationAttribute.GetValue(TestEnum.ThirdValue)));
        }

        [Fact]
        public void ExportThisAssembly_ContainsTranslations()
        {
            var catalog = ExportCurrentAssembly();
            var translation = new TestTranslation();

            Assert.Equal(8, catalog.Entries.Count);

            Assert.True(HasTranslation(catalog, translation.Title));
            Assert.True(HasTranslation(catalog, TranslationAttribute.GetValue(TestEnum.FirstValue)));
        }

        private string GetCurrentAssemblyFile()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            return Uri.UnescapeDataString(uri.Path);
        }

        private bool HasTranslation(Catalog catalog, string msgId)
        {
            return GetSingulars(catalog).Any(entry => entry.MsgId == msgId);
        }

        private Catalog DoExport(Type t)
        {
            var catalog = new Catalog();
            var exporter = new Exporter();
            if (t.IsEnum)
                exporter.ExportEnum(t, catalog);
            else
                exporter.ExportClass(t, catalog);

            return catalog;
        }

        private Catalog ExportCurrentAssembly()
        {
            var exporter = new Exporter();
            var assembly = GetCurrentAssemblyFile();

            var catalog = exporter.DoExport(new []{assembly});

            return catalog.ValueOr(() => null);
        }

        private IEnumerable<SingularEntry> GetSingulars(Catalog catalog)
        {
            return catalog.Entries
                .Where(e => e is SingularEntry)
                .Cast<SingularEntry>();
        }

        private IEnumerable<PluralEntry> GetPlurals(Catalog catalog)
        {
            return catalog.Entries
                .Where(e => e is PluralEntry)
                .Cast<PluralEntry>();
        }
    }
}
