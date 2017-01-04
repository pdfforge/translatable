using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translatable.TranslationTest
{
    [Translatable]
    public enum TestEnum
    {
        [Translation("This is the first value")]
        FirstValue,
        [Translation("This is the second value")]
        SecondValue,
        [Translation("This is some extraordinary third value!")]
        ThirdValue
    }
}
