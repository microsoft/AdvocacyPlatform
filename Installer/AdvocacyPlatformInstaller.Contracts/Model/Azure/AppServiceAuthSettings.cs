// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Model representing an Azure App Service's authentication settings.
    /// </summary>
    public class AppServiceAuthSettings : AzureResourceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppServiceAuthSettings"/> class.
        /// </summary>
        public AppServiceAuthSettings()
        {
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets or sets the enabled authentication setting.
        /// </summary>
        [JsonIgnore]
        public object Enabled
        {
            get => SafeGetKey("enabled");
            set => SafeSetKey("enabled", value);
        }

        /// <summary>
        /// Gets or sets the runtimeVersion authentication setting.
        /// </summary>
        [JsonIgnore]
        public object RuntimeVersion
        {
            get => SafeGetKey("runtimeVersion");
            set => SafeSetKey("runtimeVersion", value);
        }

        /// <summary>
        /// Gets or sets the unauthenticatedClientAction authentication setting.
        /// </summary>
        [JsonIgnore]
        public object UnauthenticatedClientAction
        {
            get => SafeGetKey("unauthenticatedClientAction");
            set => SafeSetKey("unauthenticatedClientAction", value);
        }

        /// <summary>
        /// Gets or sets the tokenStoreEnabled authentication setting.
        /// </summary>
        [JsonIgnore]
        public object TokenStoreEnabled
        {
            get => SafeGetKey("tokenStoreEnabled");
            set => SafeSetKey("tokenStoreEnabled", value);
        }

        /// <summary>
        /// Gets or sets the allowedExternalRedirectUrls authentication setting.
        /// </summary>
        [JsonIgnore]
        public object AllowedExternalRedirectUrls
        {
            get => SafeGetKey("allowedExternalRedirectUrls");
            set => SafeSetKey("allowedExternalRedirectUrls", value);
        }

        /// <summary>
        /// Gets or sets the defaultProvider authentication setting.
        /// </summary>
        [JsonIgnore]
        public object DefaultProvider
        {
            get => SafeGetKey("defaultProvider");
            set => SafeSetKey("defaultProvider", value);
        }

        /// <summary>
        /// Gets or sets the clientId authentication setting.
        /// </summary>
        [JsonIgnore]
        public object ClientId
        {
            get => SafeGetKey("clientId");
            set => SafeSetKey("clientId", value);
        }

        /// <summary>
        /// Gets or sets the clientSecret authentication setting.
        /// </summary>
        [JsonIgnore]
        public object ClientSecret
        {
            get => SafeGetKey("clientSecret");
            set => SafeSetKey("clientSecret", value);
        }

        /// <summary>
        /// Gets or sets the clientSecretCertificateThumbprint authentication setting.
        /// </summary>
        [JsonIgnore]
        public object ClientSecretCertificateThumbprint
        {
            get => SafeGetKey("clientSecretCertificateThumbprint");
            set => SafeSetKey("clientSecretCertificateThumbprint", value);
        }

        /// <summary>
        /// Gets or sets the issuer authentication setting.
        /// </summary>
        [JsonIgnore]
        public object Issuer
        {
            get => SafeGetKey("issuer");
            set => SafeSetKey("issuer", value);
        }

        /// <summary>
        /// Gets or sets the additionalLoginParams authentication setting.
        /// </summary>
        [JsonIgnore]
        public object AdditionalLoginParams
        {
            get => SafeGetKey("additionalLoginParams");
            set => SafeSetKey("additionalLoginParams", value);
        }

        /// <summary>
        /// Gets or sets the isAadAutoProvisioned authentication setting.
        /// </summary>
        [JsonIgnore]
        public object IsAadAutoProvisioned
        {
            get => SafeGetKey("isAadAutoProvisioned");
            set => SafeSetKey("isAadAutoProvisioned", value);
        }

        /// <summary>
        /// Gets or sets the googleClientId authentication setting.
        /// </summary>
        [JsonIgnore]
        public object GoogleClientId
        {
            get => SafeGetKey("googleClientId");
            set => SafeSetKey("googleClientId", value);
        }

        /// <summary>
        /// Gets or sets the googleClientSecret authentication setting.
        /// </summary>
        [JsonIgnore]
        public object GoogleClientSecret
        {
            get => SafeGetKey("googleClientSecret");
            set => SafeSetKey("googleClientSecret", value);
        }

        /// <summary>
        /// Gets or sets the googleOAuthScopes authentication setting.
        /// </summary>
        [JsonIgnore]
        public object GoogleOAuthScopes
        {
            get => SafeGetKey("googleOAuthScopes");
            set => SafeSetKey("googleOAuthScopes", value);
        }

        /// <summary>
        /// Gets or sets the facebookAppId authentication setting.
        /// </summary>
        [JsonIgnore]
        public object FacebookAppId
        {
            get => SafeGetKey("facebookAppId");
            set => SafeSetKey("facebookAppId", value);
        }

        /// <summary>
        /// Gets or sets the facebookAppSecret authentication setting.
        /// </summary>
        [JsonIgnore]
        public object FacebookAppSecret
        {
            get => SafeGetKey("facebookAppSecret");
            set => SafeSetKey("facebookAppSecret", value);
        }

        /// <summary>
        /// Gets or sets the facebookOAuthScopes authentication setting.
        /// </summary>
        [JsonIgnore]
        public object FacebookOAuthScopes
        {
            get => SafeGetKey("facebookOAuthScopes");
            set => SafeSetKey("facebookOAuthScopes", value);
        }

        /// <summary>
        /// Gets or sets the twitterConsumerKey authentication setting.
        /// </summary>
        [JsonIgnore]
        public object TwitterConsumerKey
        {
            get => SafeGetKey("twitterConsumerKey");
            set => SafeSetKey("twitterConsumerKey", value);
        }

        /// <summary>
        /// Gets or sets the twitterConsumerSecret authentication setting.
        /// </summary>
        [JsonIgnore]
        public object TwitterConsumerSecret
        {
            get => SafeGetKey("twitterConsumerSecret");
            set => SafeSetKey("twitterConsumerSecret", value);
        }

        /// <summary>
        /// Gets or sets the microsoftAccountClientId authentication setting.
        /// </summary>
        [JsonIgnore]
        public object MicrosoftAccountClientId
        {
            get => SafeGetKey("microsoftAccountClientId");
            set => SafeSetKey("microsoftAccountClientId", value);
        }

        /// <summary>
        /// Gets or sets the microsoftAccountClientSecret authentication setting.
        /// </summary>
        [JsonIgnore]
        public object MicrosoftAccountClientSecret
        {
            get => SafeGetKey("microsoftAccountClientSecret");
            set => SafeSetKey("microsoftAccountClientSecret", value);
        }

        /// <summary>
        /// Gets or sets the microsoftAccountOAuthScopes authentication setting.
        /// </summary>
        [JsonIgnore]
        public object MicrosoftAccountOAuthScopes
        {
            get => SafeGetKey("microsoftAccountOAuthScopes");
            set => SafeSetKey("microsoftAccountOAuthScopes", value);
        }

        /// <summary>
        /// Gets or sets the allowedAudiences authentication setting.
        /// </summary>
        [JsonIgnore]
        public object AllowedAudiences
        {
            get => SafeGetKey("allowedAudiences");
            set => SafeSetKey("allowedAudiences", value);
        }

        /// <summary>
        /// Gets or sets the dictionary of authentication settings.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// Safely sets a key in the authentication settings dictionary.
        /// </summary>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The value to set.</param>
        public void SafeSetKey(string key, object value)
        {
            if (Properties.ContainsKey(key))
            {
                Properties[key] = value;
            }
            else
            {
                Properties.Add(key, value);
            }
        }

        /// <summary>
        /// Safely gets a value for a key in the authentication settings dictionary.
        /// </summary>
        /// <param name="key">The key to get the value for.</param>
        /// <returns>The value if the key exists or null if it does not.</returns>
        public object SafeGetKey(string key)
        {
            if (Properties.ContainsKey(key))
            {
                return Properties[key];
            }

            return null;
        }
    }
}
