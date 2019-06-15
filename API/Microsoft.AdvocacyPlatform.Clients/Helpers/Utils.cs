// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security;
    using System.Text;
    using Microsoft.AdvocacyPlatform.Contracts;

    /// <summary>
    /// Utility helper class.
    /// </summary>
    public static class Utils
    {
        private static IEnumerable<string> _monthNames = Enumerable.Range(1, 12).Select(i => DateTimeFormatInfo.CurrentInfo.GetMonthName(i));
        private static IEnumerable<string> _monthNamesLower = Enumerable.Range(1, 12).Select(i => DateTimeFormatInfo.CurrentInfo.GetMonthName(i).ToLowerInvariant());
        private static Dictionary<string, string> _yearValues = new Dictionary<string, string>()
        {
            { "two thousand sixteen", "2016" },
            { "two thousand seventeen", "2017" },
            { "two thousand eighteen", "2018" },
            { "two thousand nineteen", "2019" },
            { "two thousand twenty one", "2021" },
            { "two thousand twenty two", "2022" },
            { "two thousand twenty three", "2023" },
            { "two thousand twenty four", "2024" },
            { "two thousand twenty five", "2025" },
            { "two thousand twenty six", "2026" },
            { "two thousand twenty seven", "2027" },
            { "two thousand twenty eight", "2028" },
            { "two thousand twenty nine", "2029" },
            { "two thousand twenty", "2020" },
            { "two thousand thirty", "2030" },
            { "two thousand and sixteen", "2016" },
            { "two thousand and seventeen", "2017" },
            { "two thousand and eighteen", "2018" },
            { "two thousand and nineteen", "2019" },
            { "two thousand and twenty one", "2021" },
            { "two thousand and twenty two", "2022" },
            { "two thousand and twenty three", "2023" },
            { "two thousand and twenty four", "2024" },
            { "two thousand and twenty five", "2025" },
            { "two thousand and twenty six", "2026" },
            { "two thousand and twenty seven", "2027" },
            { "two thousand and twenty eight", "2028" },
            { "two thousand and twenty nine", "2029" },
            { "two thousand and twenty", "2020" },
            { "two thousand and thirty", "2030" },
        };

        private static Dictionary<string, string> _ordinals = new Dictionary<string, string>()
        {
            { "thirty first", "31st" },
            { "thirtieth", "30th" },
            { "twenty ninth", "29th" },
            { "twenty eigth", "28th" },
            { "twenty seventh", "27th" },
            { "twenty sixth", "26th" },
            { "twenty fifth", "25th" },
            { "twenty fourth", "24th" },
            { "twenty third", "23rd" },
            { "twenty second", "22nd" },
            { "twenty first", "21st" },
            { "twentieth", "20th" },
            { "nineteenth", "19th" },
            { "eighteenth", "18th" },
            { "seventeenth", "17th" },
            { "sixteenth", "16th" },
            { "fifteenth", "15th" },
            { "fourteenth", "14th" },
            { "thirteenth", "13th" },
            { "twelfth", "12th" },
            { "eleventh", "11th" },
            { "tenth", "10th" },
            { "nineth", "9th" },
            { "eighth", "8th" },
            { "seventh", "7th" },
            { "sixth", "6th" },
            { "fifth", "5th" },
            { "fourth", "4th" },
            { "third", "3rd" },
            { "second", "2nd" },
            { "first", "1st" },
        };

        private static Dictionary<string, string> _numbers = new Dictionary<string, string>()
        {
            { "thirty one", "31" },
            { "thirty", "30" },
            { "twenty nine", "29" },
            { "twenty eight", "28" },
            { "twenty seven", "27" },
            { "twenty six", "26" },
            { "twenty five", "25" },
            { "twenty four", "24" },
            { "twenty three", "23" },
            { "twenty two", "22" },
            { "twenty one", "21" },
            { "twenty", "20" },
            { "nineteen", "19" },
            { "nine teen", "19" },
            { "eighteen", "18" },
            { "eight teen", "18" },
            { "seventeen", "17" },
            { "seven teen", "17" },
            { "sixteen", "16" },
            { "six teen", "16" },
            { "fifteen", "15" },
            { "fourteen", "14" },
            { "four teen", "14" },
            { "thirteen", "13" },
            { "twelve", "12" },
            { "eleven", "11" },
            { "ten", "10" },
            { "nine", "9" },
            { "eight", "8" },
            { "seven", "7" },
            { "six", "6" },
            { "five", "5" },
            { "four", "4" },
            { "three", "3" },
            { "two", "2" },
            { "one", "1" },
            { "zero", "0" },
            { "oh", "0" },
        };

        private static string[] _hours =
        {
            "one",
            "two",
            "three",
            "four",
            "five",
            "six",
            "seven",
            "eight",
            "nine",
            "ten",
            "eleven",
            "twelve",
        };

        private static string[] _minutes =
        {
            "fifteen",
            "thirty",
            "forty five",
        };

        /// <summary>
        /// Mapping of number homonyms.
        /// </summary>
        private static Dictionary<string, string> _homonyms = new Dictionary<string, string>()
        {
            { "won", "one" },
            { "too", "two" },
            { "to", "two" },
            { "tree", "three" },
            { "for", "four" },
            { "ate", "eight" },
            { "fort", "fourth" },
            { "forth", "fourth" },
            { "fit", "fifth" },
            { "tent", "tenth" },
        };

        /// <summary>
        /// Mapping of full state names to abbreviations.
        /// </summary>
        private static Dictionary<string, string> _stateAbbreviations = new Dictionary<string, string>()
        {
            { "Alabama", "AL" },
            { "Alaska", "AK" },
            { "Arizona", "AZ" },
            { "Arkansas", "AR" },
            { "California", "CA" },
            { "Colorado", "CO" },
            { "Connecticut", "CT" },
            { "Delaware", "DE" },
            { "Florida", "FL" },
            { "Georgia", "GA" },
            { "Hawaii", "HI" },
            { "Idaho", "ID" },
            { "Illinois", "IL" },
            { "Indiana", "IN" },
            { "Iowa", "IA" },
            { "Kansas", "KS" },
            { "Kentucky", "KY" },
            { "Louisiana", "LA" },
            { "Maine", "ME" },
            { "Maryland", "MD" },
            { "Massachusetts", "MA" },
            { "Michigan", "MI" },
            { "Minnesota", "MN" },
            { "Mississippi", "MS" },
            { "Missouri", "MO" },
            { "Montana", "MT" },
            { "Nebraska", "NE" },
            { "Nevada", "NV" },
            { "New Hampshire", "NH" },
            { "New Jersey", "NJ" },
            { "New Mexico", "NM" },
            { "New York", "NY" },
            { "North Carolina", "NC" },
            { "North Dakota", "ND" },
            { "Ohio", "OH" },
            { "Oklahoma", "OK" },
            { "Oregon", "OR" },
            { "Pennsylvania", "PA" },
            { "Rhode Island", "RI" },
            { "South Carolina", "SC" },
            { "South Dakota", "SD" },
            { "Tennessee", "TN" },
            { "Texas", "TX" },
            { "Utah", "UT" },
            { "Vermont", "VT" },
            { "Virginia", "VA" },
            { "Washington", "WA" },
            { "Washington DC", "DC" },
            { "West Virginia", "WV" },
            { "Wisconsin", "WI" },
            { "Wyoming", "WY" },
        };

        /// <summary>
        /// Mapping of full state names in lower case to abbreviations.
        /// </summary>
        private static Dictionary<string, string> _stateAbbreviationsLowercase = StateAbbreviations
            .ToDictionary(k => k.Key.ToLowerInvariant(), v => v.Value);

        /// <summary>
        /// Collection of date time formats for parsing datetimes.
        /// </summary>
        private static string[] _dateTimeFormats = new string[]
        {
            "MMMM d'th' yyyy 'at' h tt",
            "MMMM d'th at' h tt",
            "MMMM ',' yyyy 'at' h tt",
            "MMMM',' d'th' yyyy'.at' h tt",
            "MMMM',' d'th' yyyy'. at' h tt",
            "MMMM d ',' yyyy 'at' h:m tt",
        };

        /// <summary>
        /// Internal dictionary mapping datetime utterances to datetime values.
        /// </summary>
        private static Dictionary<string, string> _dtime;

        /// <summary>
        /// Gets the collection of month names.
        /// </summary>
        public static IEnumerable<string> MonthNames => _monthNames;

        /// <summary>
        /// Gets the collection of month names in lower case.
        /// </summary>
        public static IEnumerable<string> MonthNamesLower => _monthNamesLower;

        /// <summary>
        /// Gets the mapping of year utterances to numerical year values.
        /// </summary>
        public static Dictionary<string, string> YearValues => _yearValues;

        /// <summary>
        /// Gets the mapping of ordinal utterances to shortened alphanumeric ordinal values.
        /// </summary>
        public static Dictionary<string, string> Ordinals => _ordinals;

        /// <summary>
        /// Gets the mapping of number utterances to numerical values.
        /// </summary>
        public static Dictionary<string, string> Numbers => _numbers;

        /// <summary>
        /// Gets the collection of hour utterances.
        /// </summary>
        public static string[] Hours => _hours;

        /// <summary>
        /// Gets the collection of minute utterances.
        /// </summary>
        public static string[] Minutes => _minutes;

        /// <summary>
        /// Gets the _dtime dictionary.
        /// </summary>
        public static Dictionary<string, string> DTime
        {
            get
            {
                // TODO: Make thread-safe
                if (_dtime != null)
                {
                    return _dtime;
                }

                _dtime = new Dictionary<string, string>();

                for (int hour = 1; hour <= 12; hour++)
                {
                    for (int minute = 15; minute <= 45; minute += 15)
                    {
                        _dtime.Add($"{Hours[hour - 1]} {Minutes[(minute / 15) - 1]}", $"{hour}:{minute}");
                    }
                }

                return _dtime;
            }
        }

        /// <summary>
        /// Gets the mapping of number homonyms.
        /// </summary>
        public static Dictionary<string, string> Homonyms => _homonyms;

        /// <summary>
        /// Gets the mapping of full state names to abbreviations.
        /// </summary>.
        public static Dictionary<string, string> StateAbbreviations => _stateAbbreviations;

        /// <summary>
        /// Gets the mapping of full state names in lower case to abbreviations.
        /// </summary>
        public static Dictionary<string, string> StateAbbreviationsLowercase => _stateAbbreviationsLowercase;

        /// <summary>
        /// Gets the collection of date time formats for parsing datetimes.
        /// </summary>
        public static string[] DateTimeFormats => _dateTimeFormats;

        /// <summary>
        /// Transforms year utterance to numerical year values
        ///
        /// Examples:
        /// 'two thousand seventeen' -> '2017'
        /// 'two thousand and seventeen' -> '2017'
        ///
        /// Up to 2030.
        /// </summary>
        /// <param name="value">Segment with utterances.</param>
        /// <returns>Segment with utterances transformed to the appropriate numerical year values.</returns>
        public static string YearsToDigits(string value)
        {
            StringBuilder valueBuilder = new StringBuilder(value);

            foreach (KeyValuePair<string, string> kvp in YearValues)
            {
                valueBuilder = valueBuilder.Replace(kvp.Key, $", {kvp.Value}");
            }

            return valueBuilder.ToString();
        }

        /// <summary>
        /// Transforms ordinal utterance to shortened alphanumerical ordinal values
        ///
        /// 'third' -> '3rd'
        ///
        /// Up to 31st(intended for dates).
        /// </summary>
        /// <param name="value">Segment with utterances.</param>
        /// <returns>Segment with utterances transformed to the appropriate shortened alphanumerical ordinal values.</returns>
        public static string OrdinalsToOrdinals(string value)
        {
            StringBuilder valueBuilder = new StringBuilder(value);

            foreach (KeyValuePair<string, string> kvp in Ordinals)
            {
                valueBuilder = valueBuilder.Replace(kvp.Key, kvp.Value);
            }

            return valueBuilder.ToString();
        }

        /// <summary>
        /// Transforms number utterance to numerical values
        ///
        /// Examples:
        /// 'four' -> '4'
        /// 'fourteen' -> '14'
        /// 'four teen' -> '14'
        ///
        /// Up to 31 (intended for dates, hours, and digits in zipcodes).
        /// </summary>
        /// <param name="value">Segment with utterances.</param>
        /// <returns>Segment with utterances transformed to the appropriate numerical values.</returns>
        public static string WordnumsToNums(string value)
        {
            StringBuilder valueBuilder = new StringBuilder(value);

            foreach (KeyValuePair<string, string> kvp in Numbers)
            {
                valueBuilder = valueBuilder.Replace(kvp.Key, kvp.Value);
            }

            return valueBuilder.ToString();
        }

        /// <summary>
        /// Transforms time utterances to time value
        ///
        /// Example:
        /// 'one forty five' -> '1:45'
        ///
        /// Only considers 15, 30, 45 for now.
        /// </summary>
        /// <param name="value">Segment with utterance.</param>
        /// <returns>Segment with utterances transformed to the appropriate time values.</returns>
        public static string HourWithMinuteToTime(string value)
        {
            StringBuilder valueBuilder = new StringBuilder(value);

            foreach (KeyValuePair<string, string> kvp in DTime)
            {
                valueBuilder = valueBuilder.Replace(kvp.Key, kvp.Value);
            }

            return valueBuilder.ToString();
        }

        /// <summary>
        /// Replaces numerical homonyms with the appropriate numerical word
        ///
        /// Trying to catch examples where a number was transcribed as a homonym
        /// of that number.
        ///
        /// Example:
        /// 'for' -> 'four'.
        /// </summary>
        /// <param name="value">Segment with utterances.</param>
        /// <returns>Segment with utterances transformed to the appropriate numerical words.</returns>
        public static string ReplaceHomonyms(string value)
        {
            StringBuilder valueBuilder = new StringBuilder();

            foreach (string valuePart in value.Split(new char[] { ' ' }))
            {
                valueBuilder.Append($"{(Homonyms.ContainsKey(valuePart) ? Homonyms[valuePart] : valuePart)} ");
            }

            return valueBuilder
                .ToString()
                .Trim();
        }

        /// <summary>
        /// Builds a full connection string with access key included and returns as a <see cref="Microsoft.AdvocacyPlatform.Contracts.Secret"/>.
        /// </summary>
        /// <param name="storageConnectionString">The data store connection string.</param>
        /// <param name="storageAccessKey">The access key for the data store.</param>
        /// <returns>The full connection string as a <see cref="Microsoft.AdvocacyPlatform.Contracts.Secret"/>.</returns>
        public static Secret GetFullStorageConnectionString(string storageConnectionString, Secret storageAccessKey)
        {
            StringBuilder connectionStringBuilder = new StringBuilder(storageConnectionString);

            if (!storageConnectionString.EndsWith(";"))
            {
                connectionStringBuilder = connectionStringBuilder.Append(";");
            }

            connectionStringBuilder.Append("SharedAccessSignature=");

            SecureString securedString = new SecureString();

            foreach (char character in connectionStringBuilder.ToString())
            {
                securedString.AppendChar(character);
            }

            foreach (char character in storageAccessKey.Value)
            {
                securedString.AppendChar(character);
            }

            securedString.MakeReadOnly();

            return new Secret(storageAccessKey.Identifier, securedString);
        }
    }
}
