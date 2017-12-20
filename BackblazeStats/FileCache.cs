using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace backblazestats
{
    class FileCache
    {
        private static IDictionary<string, Tuple<DateTime, XDocument>> cache = new Dictionary<string, Tuple<DateTime, XDocument>>();
        private static TimeSpan maxAge = TimeSpan.FromMinutes(1);
        private static string path = @"C:\ProgramData\Backblaze\bzdata\";

        public static XDocument GetXmlFile(string filename)
        {
            if(cache.ContainsKey(filename) && DateTime.Now - cache[filename].Item1 < maxAge)
            {
                return cache[filename].Item2;
            }
            else
            {
                var file = ReadFile(filename);
                cache[filename] = new Tuple<DateTime, XDocument>(DateTime.Now, file);
                return file;
            }
        }

        private static XDocument ReadFile(string filename)
        {
            var filepath = Path.Combine(path, filename);
            XDocument doc = null;
            using (FileStream xmlFile = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                doc = XDocument.Load(xmlFile);
            }
            return doc;
        }
    }
}
