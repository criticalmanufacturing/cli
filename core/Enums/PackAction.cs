namespace Cmf.Common.Cli.Enums
{
    /// <summary>
    /// The action to apply to the content specified to be packed
    /// </summary>
    public enum PackAction
    {
        /// <summary>
        /// pack the source content into the package
        /// </summary>
        Pack,

        /// <summary>
        /// Use the source content to apply a transformation to another file
        /// This currently doesn't use the Target property, as it is handler dependent
        /// </summary>
        Transform,

        /// <summary>
        /// Use the source content to apply untar the file to a target destination
        /// This currently handler dependent (IoT Package)
        /// </summary>
        Untar
    }
}