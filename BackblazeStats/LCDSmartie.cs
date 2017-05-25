using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace backblazestats
{
    public class LCDSmartie
    {
        private static string path = @"C:\ProgramData\Backblaze\bzdata\bzlogs\bzreports_lastfilestransmitted";
        //private static string path = @"C:\temp";

        #region "LcdSmartie functions"

        public string function1(string param1, string param2)
        {
            try
            {
                return GetLastTransferredFilename();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public string function2(string param1, string param2)
        {
            try
            {
                string path = GetLastTransferredFilepath();
                path = path.Replace('\\', '/');

                int maxLength;
                if (int.TryParse(param1, out maxLength))
                {
                    if (path.Length > maxLength)
                        return path.Substring(path.Length - maxLength);
                    else
                        return path;
                }
                else
                {
                    return path;
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public string function3(string param1, string param2)
        {
            try
            {
                return GetLastTransferredDate().ToLocalTime().ToString(param1);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public string function4(string param1, string param2)
        {
            try
            {
                var stringVal = GetAttributeValue("kBitsPerSec_of_lastActualTransmission");
                var val = decimal.Parse(stringVal);

                if (param1 == "mbit")
                    val = val / 1000;

                int decimals;
                if (int.TryParse(param2, out decimals))
                {
                    val = Math.Round(val, decimals);
                    if (decimals > 0)
                    {
                        var formatString = "0" + CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                        for (int i = 0; i < decimals; i++)
                            formatString += "0";
                        return val.ToString(formatString);
                    }
                }
                return val.ToString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        #endregion

        private static string GetLastTransferredFilename()
        {
            return Path.GetFileName(GetAttributeValue("filename"));
        }

        private static string GetLastTransferredFilepath()
        {
            return GetAttributeValue("filename");
        }

        private static DateTime GetLastTransferredDate()
        {
            string millisString = GetAttributeValue("gmt_millis");
            var millis = long.Parse(millisString);

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(millis);
        }

        private static string GetAttributeValue(string attribute)
        {
            var el = GetLastTransmittedFileElement().Attribute(attribute);
            return el.Value;
        }

        private static XElement GetLastTransmittedFileElement()
        {
            var doc = XDocument.Load(Path.Combine(path, "bzstat_lastfile_transmitted.xml"));
            return doc.Root.Descendants("lastfile_transmitted").FirstOrDefault();
        }
    }
}
