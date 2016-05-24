namespace TranslationExport
{
    class Program
    {
        static void Main(string[] args)
        {
            var exporter = new Exporter();
            exporter.DoExport(assemblyDirectory: @"..\..\..\TranslationTest\bin\Debug", outputDirectory: @"..\..\..\Languages\en-US\LC_MESSAGES");
        }
    }
}
