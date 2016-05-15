using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationExport
{
    class Program
    {
        static void Main(string[] args)
        {
            var exporter = new Exporter();
            exporter.DoExport("translations");
        }
    }
}
