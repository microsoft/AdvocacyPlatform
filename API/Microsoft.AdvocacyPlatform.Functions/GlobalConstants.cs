// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Global constants.
    /// </summary>
    public static class GlobalConstants
    {
        // File Names

        /// <summary>
        /// The local file name of the JSON settings file used for development testing.
        /// </summary>
        public const string LocalJsonSettingsFileName = "local.settings.json";

        /// <summary>
        /// The file name of the JSON settings file used for configuration testing.
        /// </summary>
        public const string JsonAppSettingsFileName = "appsettings.json";

        // App Settings Names

        /// <summary>
        /// The name of the Twilio Account SSID secret name app setting.
        /// </summary>
        public const string TwilioAccountSidSecretNameAppSettingName = "twilioAccountSidSecretName";

        /// <summary>
        /// The name of the Twilio auth token secret name app setting.
        /// </summary>
        public const string TwilioAuthTokenSecretNameAppSettingName = "twilioAuthTokenSecretName";

        /// <summary>
        /// The name of the authority app setting.
        /// </summary>
        public const string AuthorityAppSettingName = "authority";
    }
}
