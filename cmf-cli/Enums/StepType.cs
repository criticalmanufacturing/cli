namespace Cmf.Common.Cli.Enums
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
        /// Compiles IoT Repository Indexer
        /// </summary>
        GenerateRepositoryIndex = 10
    }
}