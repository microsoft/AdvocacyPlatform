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
    /// Interface for REST clients interacting with Dynamics 365 CRM APIs.
    /// </summary>
    public interface IDynamicsCrmClient : ILoggedClient
    {
        /// <summary>
        /// Gets the user id of the current user.
        /// </summary>
        /// <returns>The user id of the current user.</returns>
        Task<string> GetUserIdAsync();

        /// <summary>
        /// Gets available solutions.
        /// </summary>
        /// <returns>A list of available solutions.</returns>
        Task<DynamicsCrmValueResponse<DynamicsCrmSolution>> GetSolutionsAsync();

        /// <summary>
        /// Gets a solution.
        /// </summary>
        /// <param name="solutionUniqueName">The unique name of the solution.</param>
        /// <returns>The solution.</returns>
        Task<DynamicsCrmSolution> GetSolutionAsync(string solutionUniqueName);

        /// <summary>
        /// Imports a solution.
        /// </summary>
        /// <param name="solutionFilePath">Path to the solution ZIP archive.</param>
        /// <param name="isHoldingSolution">Specifies if the solution is a holding solution.</param>
        /// <returns>The response content as a string.</returns>
        Task<string> ImportSolutionAsync(string solutionFilePath, bool isHoldingSolution);

        /// <summary>
        /// Imports entities.
        /// </summary>
        /// <param name="schemaXml">The name of the schema file.</param>
        /// <param name="dataXml">The name of the data file.</param>
        /// <param name="areFilePaths">Specifies if schemaXml and dataXml are file paths (true) or archive file names (false).</param>
        /// <returns>An asynchronous task.</returns>
        Task ImportEntitiesAsync(string schemaXml, string dataXml, bool areFilePaths = false);

        /// <summary>
        /// Export an unmanaged solution.
        /// </summary>
        /// <param name="solutionUniqueName">The unique name of the solution.</param>
        /// <param name="exportFilePath">Path to where to export the solution ZIP archive to.</param>
        /// <returns>The response content as a string.</returns>
        Task<string> ExportSolutionAsync(string solutionUniqueName, string exportFilePath);

        /// <summary>
        /// Updates an existing solution.
        /// </summary>
        /// <param name="solutionUniqueName">The unique name of the solution.</param>
        /// <param name="solutionFilePath">Path to the solution ZIP archive.</param>
        /// <returns>Information regarding the updated solution.</returns>
        Task<DynamicsCrmSolution> UpdateSolutionAsync(string solutionUniqueName, string solutionFilePath);

        /// <summary>
        /// Deletes a solution.
        /// </summary>
        /// <param name="solutionUniqueName">The unique name of the solution.</param>
        /// <returns>Information regarding the deleted solution.</returns>
        Task<DynamicsCrmSolution> DeleteSolutionAsync(string solutionUniqueName);
    }
}
