# Translatable

The `Translatable` project aims to bring powerful, typesafe and standard-compliant translations to C#. Inside the application, you work with C# classes to represent your translatable strings. `Translatable` will take care to extract the translatable strings and makes the translations available inside your application at runtime.

## Quick Start

First off, install the NuGet packages:

`PM> Install-Package Translatable Translatable.NGettext`

Even though this is probably the standard way, we have made good experience using [Paket](https://fsprojects.github.io/Paket/) to manage nuget packages and their dependencies.

To use the export during you build script, you have to use the export package:

`PM> Install-Package Translatable.Export`

## Motiviation

`Translatable` uses (quite) plain C# classes and the PO/gettext ecosystem as proven technology for translations. This brings a lot of advantages:

* You will immediately see in your IDE which translations are still used. Therefore, you can eventually remove obsolete translations.
* The english text is the translation key, so you always have a reasonable default if the translation is not complete, as opposed to using artificial translation keys. You will therefore never expose artificial translation keys in your UI.
* DI-friendly: `Translatable` is split into seperate packages. Your implementation will only need the Translatable assembly without further dependencies. Only at the composition root you have to reference `Translatable.NGettext` and have the dependency to `NGettext`. But even that implementation can be replaced.

The PO format specifically has some advantages:

* PO is widely accepted among translators, which is important for community-driven translations
* Good tool support (i.e. [Weblate](https://weblate.org), [Transifex](https://www.transifex.com), [PoEdit](https://poedit.net/), [gettext](https://www.gnu.org/software/gettext/)). The text-based format can even be edited without any tools though.
* Support for plural forms. This is suprising for many, but there are languages with multiple plural forms and complex [plural rules](http://www.unicode.org/cldr/charts/29/supplemental/language_plural_rules.html)
* If you do minor changes to your strings, the PO tools will detect that something was changed. The translators then will be able to compare the original and the changed source and decide if they have to fix the same thing. Often, when fixing a typo in the english source, the translators will be able to just keep the current translation.

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

### Export

### Translate po files

### Consume the translation

* build catalogs
* usage factory and source
* DI example
