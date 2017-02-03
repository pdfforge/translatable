using Translatable.TranslationTest;

namespace Translatable.UnitTest
{
    public class TestTranslation : ITranslatable
    {
        private IPluralBuilder PluralBuilder { get; set; } = new DefaultPluralBuilder();

        public string[] Messages { get; set; } = new[] {"{0} Message", "{0} Messages"};

        [Context("OtherContext")]
        public string[] Messages2 { get; set; } = new[] { "{0} Message", "{0} Messages" };


        public string NextMail { get; set; } = "Next";

        [Context("PageNavigation")]
        public string NextPage { get; set; } = "Next";

        public EnumTranslation<TestEnum>[] TestValues { get; private set; } = EnumTranslation<TestEnum>.CreateDefaultEnumTranslation();
    }
}