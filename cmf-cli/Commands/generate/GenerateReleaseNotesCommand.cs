using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.IO.Abstractions;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Cmf.CLI.Commands.Generate
{
    /// <summary>
    ///     GenerateReleaseNotesCommand
    /// </summary>
    [CmfCommand("releasenotes", Id = "generate_releasenotes", ParentId = "generate")]
    public class GenerateReleaseNotesCommand : BaseCommand
    {
        /// <summary>
        ///     Configure command
        /// </summary>
        /// <param name="cmd">
        ///     Command
        /// </param>
        public override void Configure(Command cmd)
        {
            cmd.AddOption(
                new Option<string>(
                    aliases: new[] { "--date" },
                    getDefaultValue: () => DateTime.Now.ToString("yyyy/MM/dd"),
                    description: "Release Notes date."
                )
            );

            cmd.AddOption(
                new Option<string>(
                    aliases: new[] { "--includes" },
                    getDefaultValue: () => "-",
                    description: "List of previous package versions included in current package (separated by \",\" comma)."
                )
            );

            cmd.AddOption(
                new Option<string>(
                    aliases: new[] { "--customDependsOnVersion" },
                    getDefaultValue: () => "-",
                    description: "Package version in which current Package depends on."
                )
            );

            cmd.AddOption(
                new Option<string>(
                    aliases: new[] { "--tfsProject" },
                    getDefaultValue: () => ExecutionContext.Instance.ProjectConfig.ProjectName,
                    description: "Tfs Project Name."
                )
            );

            cmd.AddOption(
                new Option<string>(
                    aliases: new[] { "--tfsProjectTeam" },
                    description: "Tfs Project Team Name."
                )
                { IsRequired = true }
            );

            cmd.AddOption(
                new Option<bool>(
                    aliases: new[] { "--removeHtmlTags" },
                    getDefaultValue: () => true,
                    description: "Remove Html Tags."
                )
            );

            cmd.AddOption(
                new Option<bool>(
                    aliases: new[] { "--onlyResolvedOrClosed" },
                    getDefaultValue: () => false,
                    description: "Get only Work Items with State \"Resolved\" or \"Closed\"."
                )
            );

            cmd.Handler = CommandHandler.Create(Execute);
        }

        /// <summary>
        ///     Executes the command
        /// </summary>
        public void Execute(string date = null, string includes = "-", string customDependsOnVersion = "-", string tfsProject = null, string tfsProjectTeam = null, bool removeHtmlTags = true, bool onlyResolvedOrClosed = false)
        {
            #region Input Date

            // If no Date was inputed in parameter (when executing trough `cmf menu`)
            if (string.IsNullOrEmpty(date))
            {
                int yearInput = GenericUtilities.ReadIntValueFromConsole(prompt: "Year (empty to use current):", allowEmptyValueInput: true);
                int year = yearInput == 0 ? DateTime.Now.Year : yearInput;

                int monthInput = GenericUtilities.ReadIntValueFromConsole(prompt: "Month (empty to use current):", minValue: 1, maxValue: 12, allowEmptyValueInput: true);
                int month = monthInput == 0 ? DateTime.Now.Month : monthInput;

                int dayInput = GenericUtilities.ReadIntValueFromConsole(prompt: "Day (empty to use current):", minValue: 1, maxValue: DateTime.DaysInMonth(year, month), allowEmptyValueInput: true);
                int day = dayInput == 0 ? DateTime.Now.Day : dayInput;
                date = new DateTime(year, month, day).ToString("yyyy/MM/dd");
            }

            if (!DateTime.TryParse(date, out DateTime __))
            {
                Log.Error("Invalid input date. Try again.");
                return;
            }

            #endregion Input Date

            #region Validate/Set Input data

            tfsProject = string.IsNullOrEmpty(tfsProject) ? ExecutionContext.Instance.ProjectConfig.ProjectName : tfsProject;

            if (string.IsNullOrEmpty(tfsProjectTeam))
            {
                tfsProjectTeam = GenericUtilities.ReadValueFromConsole<string>(prompt: "Tfs Project Team:");
            }

            #endregion Validate/Set Input data

            #region Get WI and Generate Release Notes file

            CmfPackage helpPackage = GenericUtilities.SelectPackage(fileSystem, packageType: PackageType.Help);
            IFileInfo helpPackageFileInfo = helpPackage.GetFileInfo();

            string srcPackage = Directory.GetDirectories($@"{helpPackageFileInfo.Directory}\src\packages")[0];
            string releaseNotesFolderPath = Array.Find(Directory.GetDirectories($@"{srcPackage}\assets"), dir => dir.Contains("releasenotes"));
            if (string.IsNullOrEmpty(releaseNotesFolderPath))
            {
                Log.Error("Release Notes folder not found.");
            }
            string releaseNotesTemplateFile = $@"{releaseNotesFolderPath}\packagetemplate";

            string query = $"SELECT [System.Id], [System.Title], [System.WorkItemType], [System.State], [Project.ReleaseNotes] FROM workitems WHERE [System.TeamProject] = \"{tfsProject}\" AND [System.WorkItemType] IN (\"User Story\", \"Bug\") AND NOT [System.Tags] CONTAINS \"Internal\" AND NOT [System.Tags] CONTAINS \"Product\" AND NOT [System.Tags] CONTAINS \"T&M\" AND [Project.DocumentationImpact] CONTAINS \"{helpPackage.Version}\" {(onlyResolvedOrClosed ? "AND ([System.State] = \"Resolved\" OR [System.State] = \"Closed\")" : "")} ORDER BY [System.WorkItemType] DESC";
            StringContent body = new StringContent(JsonConvert.SerializeObject(new { query }), Encoding.UTF8, "application/json");

            using HttpClient client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });
            client.DefaultRequestHeaders.Add("ContentType", "application/json");
            client.DefaultRequestHeaders.Add("Accept", "application/json;api-version=4.1;excludeUrls=true");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

            string uri = $"{CoreConstants.TfsServerUrl}/{HttpUtility.UrlPathEncode(tfsProject)}/{HttpUtility.UrlPathEncode(tfsProjectTeam)}/_apis/wit/wiql?timePrecision=true&$top=50";

            // TODO: FIX returning bad response (might be related with server?)
            /*
             * when using a random Tfs Project Team, it gets an error response body with a readable message
             * when using a correct Tfs Project Team, it gets an unreadable response with strange characters (responseContent = "\u001f�\b\0\0\0\0\0\u0004\0��\a`\u001cI�%&/m�{\u007fJ�J��t�\b�`\u0013$ؐ...)
             */
            HttpResponseMessage response = client.PostAsync(uri, body).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;
            dynamic responseObject = JsonConvert.DeserializeObject(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new CliException(responseObject["message"].Value);
            }

            string columns = string.Empty;
            string workItemIds = string.Empty;

            foreach (dynamic column in responseObject.columns)
            {
                if (!string.IsNullOrEmpty(columns))
                    columns += ",";
                columns += column.referenceName;
            }

            foreach (dynamic column in responseObject.workItems)
            {
                if (!string.IsNullOrEmpty(workItemIds))
                    workItemIds += ",";
                workItemIds += column.id;
            }

            string releaseNotesContent = string.Empty;
            if (!string.IsNullOrEmpty(workItemIds))
            {
                string workItemsUri = $"{CoreConstants.TfsServerUrl}/_apis/wit/workItems?ids={workItemIds}&fields={columns}";
                string workItemsResponseContent = client.GetAsync(workItemsUri).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
                dynamic workItems = JsonConvert.DeserializeObject(workItemsResponseContent);

                releaseNotesContent += "## User Stories/Bugs\n\n";
                releaseNotesContent += "| Title        | Notes            |\n";
                releaseNotesContent += "| :----------- | :--------------- |\n";

                foreach (dynamic column in responseObject.workItems)
                {
                    string workItemId = column.id.ToString();
                    dynamic item = null;
                    foreach (dynamic wi in workItems.value)
                    {
                        if (wi.id.ToString() == workItemId)
                        {
                            item = wi.fields;
                            break;
                        }
                    }
                    if (item == null)
                        throw new CliException("Item not found");

                    string releasenotes = item.Project.ReleaseNotes ?? "-";
                    if (removeHtmlTags)
                    {
                        releasenotes = Regex.Replace(releasenotes, "<[^>]*?>", "");
                    }

                    string idSection = $"**{item.id}**";
                    bool isBug = item.System.WorkItemType != "User Story";
                    if (isBug)
                    {
                        idSection = "<span style='color:red'>" + idSection + "</span>";
                    }
                    releaseNotesContent += $"| {idSection} {item.System.Title} | {releasenotes} |\n";
                }
            }

            string packageTemplateContent = File.ReadAllText(releaseNotesTemplateFile, Encoding.UTF8)
                .Replace("@PackageId@", helpPackage.PackageName)
                .Replace("@SprintNumber@", Regex.Replace(helpPackage.Version, "Sprint", ""))
                .Replace("@PackageVersion@", helpPackage.Version)
                .Replace("@ExpectedReleaseDate@", date)
                .Replace("@PackageDeliverablesIncluded@", includes)
                .Replace("@MESVersion@", ExecutionContext.Instance.ProjectConfig.MESVersion.ToString())
                .Replace("@CustomDependsOn@", customDependsOnVersion)
                .Replace("@UserStories@", releaseNotesContent);

            string outputFileName = releaseNotesTemplateFile.Replace("packagetemplate", $"{date.Replace("/", "")}-{helpPackage.Version.Replace(".", "_")}.md");
            File.WriteAllText(outputFileName, packageTemplateContent, Encoding.UTF8);

            #endregion Get WI and Generate Release Notes file

            new GenerateDocumentationCommand().Execute();
        }
    }
}