using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace backblazestats
{
    public class LCDSmartie
    {
        #region "LcdSmartie functions"

        /// <summary>
        /// Get the name of the file currently being transferred.
        /// </summary>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public string function1(string param1, string param2)
        {
            try
            {
                return GetCurrentTransferredFileName();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        /// <summary>
        /// Get the full path of the file currently being transferred.
        /// </summary>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public string function2(string param1, string param2)
        {
            try
            {
                string path = GetCurrentTransferredFilePath();
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

        /// <summary>
        /// Get amount of free space on all volumes selected for backup.
        /// </summary>
        /// <param name="param1">Unit to convert the value to (KB, MB, GB, TB) or empty to return the value in bytes.</param>
        /// <param name="param2">Number of decimals to show</param>
        /// <returns></returns>
        public string function3(string param1, string param2)
        {
            try
            {
                var bytes = GetStorageInfo("numBytesFreeOnVolume");
                var convertedBytes = ComputeByteUnit(param1, bytes);
                return FormatNumberWithFixedDecimals(param2, convertedBytes);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        /// <summary>
        /// Get amount of space used on all volumes selected for backup
        /// </summary>
        /// <param name="param1">Unit to convert the value to (KB, MB, GB, TB) or empty to return the value in bytes.</param>
        /// <param name="param2">Number of decimals to show</param>
        /// <returns></returns>
        public string function4(string param1, string param2)
        {
            try
            {
                var bytes = GetStorageInfo("numBytesUsedOnVolume");
                var convertedBytes = ComputeByteUnit(param1, bytes);
                return FormatNumberWithFixedDecimals(param2, convertedBytes);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        /// <summary>
        /// Get total amount of storage space on all volumes selected for backup
        /// </summary>
        /// <param name="param1">Unit to convert the value to (KB, MB, GB, TB) or empty to return the value in bytes.</param>
        /// <param name="param2">Number of decimals to show</param>
        /// <returns></returns>
        public string function5(string param1, string param2)
        {
            try
            {
                var bytes = GetStorageInfo("numBytesTotalOnVolume");
                var convertedBytes = ComputeByteUnit(param1, bytes);
                return FormatNumberWithFixedDecimals(param2, convertedBytes);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        /// <summary>
        /// Get size of file currently being transferred
        /// </summary>
        /// <param name="param1">Unit to convert the value to (KB, MB, GB, TB) or empty to return the value in bytes.</param>
        /// <param name="param2">Number of decimals to show</param>
        /// <returns></returns>
        public string function6(string param1, string param2)
        {
            try
            {
                var bytes = GetCurrentTransferredFileSize();
                var convertedBytes = ComputeByteUnit(param1, bytes);
                return FormatNumberWithFixedDecimals(param2, convertedBytes);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        /// <summary>
        /// Get amount of data backed up so far
        /// </summary>
        /// <param name="param1">Unit to convert the value to (KB, MB, GB, TB) or empty to return the value in bytes.</param>
        /// <param name="param2">Number of decimals to show</param>
        /// <returns></returns>
        public string function7(string param1, string param2)
        {
            try
            {
                var bytes = GetNumberOfBytesBackedUp();
                var convertedBytes = ComputeByteUnit(param1, bytes);
                return FormatNumberWithFixedDecimals(param2, convertedBytes);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        /// <summary>
        /// Get total amount of data selected for backup
        /// </summary>
        /// <param name="param1">Unit to convert the value to (KB, MB, GB, TB) or empty to return the value in bytes.</param>
        /// <param name="param2">Number of decimals to show</param>
        /// <returns></returns>
        public string function8(string param1, string param2)
        {
            try
            {
                var bytes = GetNumberOfBytesScheduled();
                var convertedBytes = ComputeByteUnit(param1, bytes);
                return FormatNumberWithFixedDecimals(param2, convertedBytes);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        /// <summary>
        /// Get upload speed of last backup
        /// </summary>
        /// <param name="param1">"mbit" to show the upload speed in megabits. Otherwise kilobits.</param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public string function9(string param1, string param2)
        {
            try
            {
                var stringVal = GetAttributeValue(@"bzlogs\bzreports_lastfilestransmitted\bzstat_lastfile_transmitted.xml", "lastfile_transmitted", "kBitsPerSec_of_lastActualTransmission");
                var val = double.Parse(stringVal);

                if (param1 == "mbit")
                    val = val / 1000;

                return FormatNumberWithFixedDecimals(param2, val);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        #endregion

        private static double ComputeByteUnit(string byteUnitParam, long byteValue)
        {
            var value = (double)byteValue;
            if (byteUnitParam.Equals("KB", StringComparison.InvariantCultureIgnoreCase))
                value = value / 1024;
            else if (byteUnitParam.Equals("MB", StringComparison.InvariantCultureIgnoreCase))
                value = value / Math.Pow(1024, 2);
            else if (byteUnitParam.Equals("GB", StringComparison.InvariantCultureIgnoreCase))
                value = value / Math.Pow(1024, 3);
            else if (byteUnitParam.Equals("TB", StringComparison.InvariantCultureIgnoreCase))
                value = value / Math.Pow(1024, 4);
            return value;
        }

        private static string FormatNumberWithFixedDecimals(string decimalParam, double value)
        {
            decimal decVal = new decimal(value);
            int decimals;
            if (int.TryParse(decimalParam, out decimals))
            {
                decVal = Math.Round(decVal, decimals);
                if (decimals > 0)
                {
                    var formatString = "0" + CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                    for (int i = 0; i < decimals; i++)
                        formatString += "0";
                    return decVal.ToString(formatString);
                }
            }
            return decVal.ToString();
        }

        private static string GetCurrentTransferredFileName()
        {
            return GetAttributeValue("overviewstatus.xml", "bztransmit", "current_file");
        }

        private static string GetCurrentTransferredFilePath()
        {
            return GetAttributeValue("overviewstatus.xml", "bztransmit", "current_file_fullpath");
        }

        private static long GetNumberOfBytesBackedUp()
        {
            string byteString = GetAttributeValue(@"bzreports\bzstat_last_synchostinfo_serv.xml", "info", "report_bytes");
            return long.Parse(byteString);
        }

        private static long GetNumberOfBytesScheduled()
        {
            string byteString = GetAttributeValue(@"bzreports\bzstat_totalbackup.xml", "totals", "totnumbytesforbackup");
            return long.Parse(byteString);
        }

        private static string GetAttributeValue(string filename, string elementname, string attributename)
        {
            var doc = GetXmlDocument(filename);
            var el = doc.Root.Descendants(elementname).LastOrDefault();
            var val = el.Attribute(attributename).Value;
            return val;
        }

        private static long GetCurrentTransferredFileSize()
        {
            var currentFilePath = GetCurrentTransferredFilePath();

            if (string.IsNullOrEmpty(currentFilePath))
                return 0;

            var file = new FileInfo(currentFilePath);
            if (file.Exists)
                return file.Length;

            return 0;
        }

        private static long GetStorageInfo(string storageparameter)
        {
            long total = 0;
            var doc = GetXmlDocument("bzvolumes.xml");
            var elements = doc.Root.Descendants("bzvolume");
            foreach (var el in elements)
            {
                var attr = el.Attribute(storageparameter)?.Value;
                long val;
                if (long.TryParse(attr, out val))
                    total += val;
            }
            return total;
        }

        private static XDocument GetXmlDocument(string filename)
        {
            return FileCache.GetXmlFile(filename);
        }
    }
}
