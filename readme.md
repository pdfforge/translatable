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

Exporting content is pretty easy. You run the export application and specify the pot file to create (with the argument `--outputfile` and a list of assemblies to scan).

`> Translatable.Export.exe --outputfile <yourfile>.pot <assembly1> <assembly2> <assembly> ...`

To export the translation for the test application, you can use this: (Please compile first!)

`> Translatable.Export.exe --outputfile "Source\TranslationTest\Languages\messages.pot"  "Source\TranslationTest\bin\Debug\TranslationTest.exe"`

You will receive a pot file, which will be used as a starting point to all translations. Basically, it is a po file without translation.

```
msgid ""
msgstr ""
"Content-Type: text/plain; charset=UTF-8"

#: Translatable.TranslationTest.MainWindowTranslation
msgid "Main window title"
msgstr ""

#: Translatable.TranslationTest.MainWindowTranslation
msgid "This is my content\nwith multiple lines"
msgstr ""

(...)
```

### Translate po files

The translation process is not part of what `Translatable` does, but there are good specialized tools for this. In our workflow at [PDFCreator](https://www.pdfforge.org/pdfcreator), we start creating the translation classes, add it to our code and do the german translation locally. Then we start the application to see if the translation works, i.e. everything is translated as expected, nothing is missing etc.

When we merge a change into the master, we push the translation to our [Weblate](https://weblate.org) instance and make it available to our translators. Then we regularly pull the translations to our git repository.

### Consume the translation

Now it is time to actually use the translation. This requires a few things to be done:

* Define the `TranslationFactory` and distribute it in your application
* Define a `TranslationSource` that is used to fill in the translation in the `TranslationFactory`
* Get concrete translations from the `TranslationFactory` where you need it

Creating and initializing a new `TranslationFactory` is quite easy:

```
_translationFactory = new TranslationFactory();
_translationFactory.TranslationSource = new GettextTranslationSource(_translationFolder, "messages", new CultureInfo("de"));
```

You can also seperate these steps and first create the instance and define the language later. You will receive english translations until a different language is set.

Right now, there is only one implementation for ITranslationSource, which is the GettextTranslationSource. It uses `NGettext` to load `mo` files. With the constructor from the example, it will look for a file `messages.mo`in $"{_translationFolder}\\de\\LC_MESSAGES". This is a convention with gettext, but can be overridden. There are more overloads, i.e. to specify a `mo` file directly.

**Note:** NGettext loads mo files, which are compiled po files. If you do not see any or only outdated translations, this often is because the po files have not been compiled.

As we have defined a language, we can now create translated classes:

```
Translation = _translationFactory.CreateTranslation<MainWindowTranslation>();
```

#### Inject with SimpleInjector

Of course it depends on your setup how to inject dependencies. Every DI container is slightly different, but you should be able to accomplish this with most containers. In this example, we use [SimpleInjector](https://simpleinjector.org) as very advanced and well-documented DI container.

Most of the time, you will use translations in a places, where the language is defined and won't change, i.e. in a Window in the application or a View in the web app. If the language is set when the constructor is called and will not change during the lifetime of that object, you can simply inject the translation class. In SimpleInjector, you can use this:

```
var translatables = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(t => typeof(ITranslatable).IsAssignableFrom(t) && !t.IsAbstract).ToList();
foreach (var t in translatables)
{
    var reg = Lifestyle.Transient.CreateRegistration(t, () => translationFactory.CreateTranslation(t), container);
    container.AddRegistration(t, reg);
}
```

You can then request the translation in the constructor of the consuming class:

```
public class MainWindowViewModel : INotifyPropertyChanged
{
    private MainWindowTranslation Translation;

    public MainWindowViewModel(MainWindowTranslation translation)
    {
        Translation = translation;
    }
}
```

SimpleInjector will do the resolution for you. However, if the object is built when the language is not yet set, the translation will be english. Also, if you would like to switch the translation while this class is used, you have to request it again in the TranslationFactory.
