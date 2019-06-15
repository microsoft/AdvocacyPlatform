// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    /// Base model for function requests.
    /// </summary>
    public abstract class FunctionRequestBase
    {
        /// <summary>
        /// Parses the request's body content.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the body content as.</typeparam>
        /// <param name="requestBody">The request's body content.</param>
        /// <returns>The deserialized body content.</returns>
        public static T Parse<T>(string requestBody)
            where T : FunctionRequestBase
        {
            T request = JsonConvert.DeserializeObject<T>(requestBody);

            if (request == null)
            {
                throw new MalformedRequestBodyException<CommonErrorCode>(CommonErrorCode.MalformedRequestBody, "Request body is malformed.");
            }

            request.Validate();

            return request;
        }

        /// <summary>
        /// Validates the request.
        /// </summary>
        public virtual void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
