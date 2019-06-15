// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Model representing the response content of a request to get available Azure Resource Groups.
    /// </summary>
    public class ResourceGroupListResult : AzureValueCollectionResponse<ResourceGroup>
    {
    }
}
