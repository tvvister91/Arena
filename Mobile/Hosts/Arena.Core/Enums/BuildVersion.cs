namespace Arena.Core.Enums
{
    /// <summary>
    /// The possible build scenarios. Use for settings constants.
    /// </summary>
    public enum BuildVersionEnum
    {
        /// <summary>
        /// A local build in Debug mode.
        /// </summary>
        Debug,
        /// <summary>
        /// A local build in Release mode, or a build server build from the
        /// `develop` branch.
        /// </summary>
        Dev,
        /// <summary>
        /// A build server build from the `master` branch.
        /// </summary>
        QA,
        /// <summary>
        /// A build server build from the `uat` branch.
        /// </summary>
        UAT,
        /// <summary>
        /// A build server build from the `release` branch.
        /// </summary>
        Release,
        /// <summary>
        /// A build server build from the `demo` branch.
        /// </summary>
        Demo
    }
}
