namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Flags returned by ExtractInfo.
    /// </summary>
    public static class ExtractInfoFlag
    {
        /// <summary>
        /// Flag specifying the extracted date was rejected as not meeting the minimum DateTime value threshold.
        /// </summary>
        public const string DateRejected = "dateRejected";
    }
}
