# Translatable

## Quick Start

Install NuGet packages

`PM> Install-Package Translatable Translatable.NGettext`

To use the export during you build script, you have to use the export package:

`PM> Install-Package Translatable.Export`

## Usage



### Build your translation

```C#
public class MainWindowTranslation : ITranslatable
{
    private IPluralBuilder PluralBuilder { get; set; } = new DefaultPluralBuilder();

    public string SampleText { get; private set; } = "This is my sample text";

    private string[] Converted { get; set; } = { "You have converted {0} file!", "You have converted {0} files!"};

    public string GetConvertedMessage(int numberOfPrintJobs)
    {
        return PluralBuilder.GetFormattedPlural(numberOfPrintJobs, Converted);
    }

    public EnumTranslation<MyEnum>[] TestEnumTranslation { get; private set; } = EnumTranslation<MyEnum>.CreateDefaultEnumTranslation();
}

[Translatable]
public enum MyEnum
{
    [Translation("This is the first value")]
    FirstValue,
    [Translation("This is the second value")]
    SecondValue,
    [Translation("This is some extraordinary third value!")]
    ThirdValue
}
```
