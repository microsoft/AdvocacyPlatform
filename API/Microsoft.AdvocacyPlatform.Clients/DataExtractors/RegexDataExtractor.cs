// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// IDataExtractor implementation for extracting information using regular expressions.
    /// </summary>
    public class RegexDataExtractor : IDataExtractor
    {
        /// <summary>
        /// Extracts data from a transcription.
        /// </summary>
        /// <param name="transcript">The transcript to extract data from.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning the data extracted from the transcription.</returns>
        public Task<TranscriptionData> ExtractAsync(string transcript, ILogger log)
        {
            return Task.Run(() =>
            {
                TranscriptionData data = new TranscriptionData();

                data.Transcription = transcript;
                data.Date = ExtractDateTime(transcript, log);
                data.Location = ExtractLocation(transcript, log);
                data.Person = ExtractPerson(transcript, log);

                return data;
            });
        }

        /// <summary>
        /// Creates digits from number words.
        ///
        /// Examples:
        /// 'april thirteenth two thousand sixteen at two thirty PM'
        /// -> 'april 13th, 2016 at 2:30 PM'
        ///
        /// 'april thirteen two thousand sixteen at two thirty PM'
        /// -> 'april 13, 2016, at 2:30 PM'
        ///
        /// 'april thirteenth two thousand and sixteen at two thirty PM'
        /// -> 'april 13th, 2016 at 2:30 PM'
        ///
        /// 'april thirteenth two thousand sixteen at two PM'
        /// -> 'april 13th, 2016, at 2 PM'
        ///
        /// 'april two thousand sixteen at two thirty PM'
        /// -> 'april, 2016 at 2:30 PM'.
        /// </summary>
        /// <param name="dateText">The text to transform.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>The transformed text.</returns>
        public string CreateDigitsForDateParsing(string dateText, ILogger log)
        {
            log.LogInformation("Attempting to create digits for date parsing...");
            return Utils.WordnumsToNums(
                Utils.HourWithMinuteToTime(
                    Utils.OrdinalsToOrdinals(
                        Utils.YearsToDigits(
                            dateText))));
        }

        /// <summary>
        /// Extracts the date and time information using multiple strategies.
        ///
        /// If extract_date_time_base doesn't succeed, try again after having changed words to digits
        /// and replaced homonyms.
        /// </summary>
        /// <param name="dateText">The text to extract date and time from.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>The extracted date and time information.</returns>
        public DateInfo ExtractDateTime(string dateText, ILogger log)
        {
            return ExtractDateTimeBase(dateText, log) ??
                        ExtractDateTimeBase(dateText, log, true) ??
                            ExtractDateTimeBase(Utils.ReplaceHomonyms(dateText), log, true) ??
                                new DateInfo();
        }

        /// <summary>
        /// Base function for extracting date and time information.
        ///
        /// Example:
        ///     s = 'blah blah thirty one may st new york new york on april third,
        ///      two thousand seventeen at one thirty PM blah blah'
        /// returns
        /// dict('year': 2017,
        ///      'month': 4,
        ///      'day': 3,
        ///      'hour': 13,
        ///      'minute': 30)
        ///
        /// minute default to 0 if none found.
        /// All other keys default to None.
        ///
        /// Loops through possible dates, returns as soon as dparser succeeds in
        /// parsing date (dates are ordered by length)
        ///
        /// The parser seems to always return something, e.g.
        /// 'on march 2021 at 4:30 pm' -->
        /// {'year': 2021, 'month': 3, 'day': 2, 'hour': 16, 'minute': 30}
        /// even though 'day' should be None.
        ///
        /// We get around this problem by adding two different defaults and checking if the
        /// dict returned are the same.
        /// </summary>
        /// <param name="dateText">The text to extract the date and time from.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <param name="wordsToNumbers">Specifies if number words should be transformed to numbers.</param>
        /// <returns>The extracted date and time information.</returns>
        public DateInfo ExtractDateTimeBase(string dateText, ILogger log, bool wordsToNumbers = false)
        {
            IEnumerable<string> possibleDateTimes = FindPossibleDateTimes(dateText, wordsToNumbers, log).ToList();

            DateInfo dateInfo = new DateInfo();

            foreach (string dateTime in possibleDateTimes)
            {
                // TODO: Revisit the logic here
                DateTime dateOut;

                if (DateTime.TryParseExact(dateTime, Utils.DateTimeFormats, null, DateTimeStyles.AssumeLocal, out dateOut))
                {
                    dateInfo.FullDate = dateOut;
                    dateInfo.Year = dateOut.Year;
                    dateInfo.Month = dateOut.Month;
                    dateInfo.Day = dateOut.Day;
                    dateInfo.Hour = dateOut.Hour;
                    dateInfo.Minute = dateOut.Minute;

                    return dateInfo;
                }
            }

            return null;
        }

        // TODO: Implement logic

        /// <summary>
        /// Extracts location information from text.
        /// </summary>
        /// <param name="text">The text to extract location information from.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>The extracted location information.</returns>
        public LocationInfo ExtractLocation(string text, ILogger log)
        {
            return null;
        }

        // TODO: Implement logic

        /// <summary>
        /// Extracts person information from text.
        /// </summary>
        /// <param name="text">The text to extract location information from.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>The extracted person information.</returns>
        public PersonInfo ExtractPerson(string text, ILogger log)
        {
            return null;
        }

        /// <summary>
        /// Finds segments containing potential (partial or full) datetime values.
        ///
        /// Example:
        ///        s = 'blah blah thirty one may st new york new york on april third,
        ///         two thousand seventeen at one thirty PM blah blah'
        ///    returns
        ///    list('april 3rd, 2017 at 1:30 p.m.',
        ///         'may st new york new york on april 3rd, 2017 at 1:30 PM')
        ///
        ///    The returned list is ordered by length.
        ///    Duplicates are removed.
        /// </summary>
        /// <param name="dateText">The text to find potential datetime values from.</param>
        /// <param name="wordsToNumbers">Specifies if number words should be transformed into numbers.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A list of potential segments containing datetime information.</returns>
        public IEnumerable<string> FindPossibleDateTimes(string dateText, bool wordsToNumbers, ILogger log)
        {
            StringBuilder dateTextBuilder;

            if (wordsToNumbers)
            {
                dateTextBuilder = new StringBuilder(CreateDigitsForDateParsing(dateText, log).ToLowerInvariant());
            }
            else
            {
                dateTextBuilder = new StringBuilder(dateText.ToLowerInvariant());
            }

            Regex regEx = GetRegExForDateParsing(log);

            MatchCollection dateTimeMatches = regEx.Matches(dateTextBuilder.ToString());
            List<Tuple<int, string>> returnMatches = new List<Tuple<int, string>>();

            foreach (Match match in dateTimeMatches)
            {
                foreach (Group matchGroup in match.Groups)
                {
                    returnMatches.Add(new Tuple<int, string>(matchGroup.Length, matchGroup.Value));
                }
            }

            return returnMatches
                .OrderByDescending(x => x.Item1)
                .Select(x => x.Item2
                    .Replace("a.m.", "am", StringComparison.OrdinalIgnoreCase)
                    .Replace("p.m.", "pm", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Finds segments containing potential location values.
        /// </summary>
        /// <param name="text">The text to find potential datetime values from.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A list of potential segments containing location information.</returns>
        public IEnumerable<LocationInfo> FindPossibleLocations(string text, ILogger log)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Basically matches strings beginning with a month, and ending in AM or PM
        ///        In between the month and AM/PM, matching is non-greedy.
        ///        Uses lookahead with grouping to find overlapping matches.
        ///        Example:
        ///    s = '31 may st new york new york on april 3rd, 2017 at 1:30 p.m.'
        ///
        ///        The re will catch both:
        ///        'may st new york new york on april 3rd, 2017 at 1:30 p.m.'
        ///
        ///        and
        ///            'april 3rd, 2017 at 1:30 p.m.'.
        /// </summary>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>The regular expression to use for matching.</returns>
        public Regex GetRegExForDateParsing(ILogger log)
        {
            string monthsOr = string.Join("|", Utils.MonthNamesLower);

            return new Regex($"(?=((?:{monthsOr})(,| ).* (?:a\\.m\\.|p\\.m\\.|am|pm)))");
        }

        /// <summary>
        /// Returns regular expression for location parsing.
        /// </summary>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>The regular expression to use for matching.</returns>
        public Regex GetRegExForLocationParsing(ILogger log)
        {
            throw new NotImplementedException();
        }
    }
}
