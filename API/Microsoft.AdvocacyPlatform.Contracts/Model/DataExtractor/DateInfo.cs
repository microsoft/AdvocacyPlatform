// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Date and time information extracted from a transcription.
    /// </summary>
    public class DateInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateInfo"/> class.
        /// </summary>
        public DateInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateInfo"/> class.
        /// </summary>
        /// <param name="year">The year to set.</param>
        /// <param name="month">The month to set.</param>
        /// <param name="day">The day to set.</param>
        /// <param name="hour">The hour to set.</param>
        /// <param name="minute">The minute to set.</param>
        public DateInfo(int year, int month, int day, int hour, int minute)
        {
            this.Year = year;
            this.Month = month;
            this.Day = day;
            this.Hour = hour;
            this.Minute = minute;
        }

        /// <summary>
        /// Gets or sets the four digit year value.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the single or two digit month value.
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Gets or sets the single or two digit day value.
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// Gets or sets the single or two digit hour value.
        /// </summary>
        public int Hour { get; set; }

        /// <summary>
        /// Gets or sets the single or two digit minute value.
        /// </summary>
        public int Minute { get; set; }

        /// <summary>
        /// Gets or sets the full datetime value.
        /// </summary>
        public DateTime? FullDate { get; set; }

        /// <summary>
        /// Returns the full date as a string.
        /// </summary>
        /// <returns>The full date.</returns>
        public override string ToString()
        {
            return this.FullDate.ToString();
        }
    }
}
