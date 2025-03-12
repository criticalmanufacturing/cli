namespace Cmf.CLI.Core.Enums
{
    /// <summary>
    ///
    /// </summary>
    public enum StepType
    {
        /// <summary>
        /// The generic
        /// </summary>
        Generic = 0,

        /// <summary>
        /// The deploy files
        /// </summary>
        DeployFiles = 1,

        /// <summary>
        /// The transform file
        /// </summary>
        TransformFile = 2,

        /// <summary>
        /// The run SQL
        /// </summary>
        RunSql = 3,

        /// <summary>
        /// The run SQL
        /// </summary>
        DeployReports = 4,

        /// <summary>
        /// The process rules
        /// </summary>
        ProcessRules = 5,

        /// <summary>
        /// The master data
        /// </summary>
        MasterData = 6,

        /// <summary>
        /// The exported objects
        /// </summary>
        ExportedObjects = 7,

        /// <summary>
        /// The create integration entries
        /// </summary>
        CreateIntegrationEntries = 8,

        /// <summary>
        /// The enqueue SQL
        /// </summary>
        EnqueueSql = 9,

        /// <summary>
        /// Deploy ConnectIoT packages to Repository
        /// </summary>
        DeployRepositoryFiles = 10,

        /// <summary>
        /// Update the ConnectIoT Repository Index
        /// </summary>
        GenerateRepositoryIndex = 11,

        /// <summary>
        /// Generate Service History Id
        /// </summary>
        GenerateServiceHistoryId = 12,

        /// <summary>
        /// Tag a file. This is similar to DeployFiles with the tag option set
        /// </summary>
        TaggedFile = 13,

        /// <summary>
        /// Sync Automation Task libraries
        /// </summary>
        IoTAutomationTaskLibrariesSync = 14,

        /// <summary>
        /// Sync Automation Business Scenarios libraries
        /// </summary>
        AutomationBusinessScenariosSync = 15
    }
}