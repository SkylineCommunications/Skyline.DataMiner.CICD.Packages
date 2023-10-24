namespace Skyline.DataMiner.CICD.DMApp.Common
{
    using System;
    using System.IO;

    /// <summary>
    /// Represents an app package file name.
    /// </summary>
    public class DMAppFileName
    {
        private readonly string _fileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DMAppFileName"/> class.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="fileName"/> does not end with '.dmapp'.</exception>
        /// <exception cref="ArgumentException"><paramref name="fileName"/> does have a directory structure.</exception>
        /// <exception cref="ArgumentException"><paramref name="fileName"/> is empty or whitespace.</exception>
        public DMAppFileName(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (String.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("Value can not be empty or whitespace.", nameof(fileName));
            }

            if (!fileName.EndsWith(".dmapp"))
            {
                throw new ArgumentException("The specified file name must end with '.dmapp'.", nameof(fileName));
            }

            if (Path.GetDirectoryName(fileName) != String.Empty)
            {
                throw new ArgumentException("The specified file name can not have a directory structure.", nameof(fileName));
            }

            _fileName = fileName;
        }

        /// <summary>
        /// Creates a filename form the specified package name and tag.
        /// </summary>
        /// <param name="packageName">The package name.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>The corresponding app package file name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="packageName"/> or <paramref name="tag"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="packageName"/> or <paramref name="tag"/> is empty or whitespace.</exception>
        public static DMAppFileName FromTag(string packageName, string tag)
        {
            if (packageName == null)
            {
                throw new ArgumentNullException(nameof(packageName));
            }

            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            if (String.IsNullOrWhiteSpace(packageName))
            {
                throw new ArgumentException("Value can not be empty or whitespace.", nameof(packageName));
            }

            if (String.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentException("Value can not be empty or whitespace.", nameof(tag));
            }

            string packageFileName = packageName + " " + DMAppVersion.FromProtocolVersion(tag) + ".dmapp";

            return new DMAppFileName(packageFileName);
        }

        /// <summary>
        /// Creates a filename form the specified package name, branch and build number.
        /// </summary>
        /// <param name="packageName">The package name.</param>
        /// <param name="branch">The branch.</param>
        /// <param name="buildNumber">The build number.</param>
        /// <returns>The corresponding app package file name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="packageName"/> or <paramref name="branch"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="packageName"/> or <paramref name="branch"/> is empty or whitespace.</exception>
        /// <exception cref="ArgumentException"><paramref name="buildNumber"/> is lower than zero.</exception>
        public static DMAppFileName FromBuildNumber(string packageName, string branch, int buildNumber)
        {
            if (buildNumber < 0)
            {
                throw new ArgumentException("Value can not be lower than zero.", nameof(buildNumber));
            }

            return FromBuildNumber(packageName, branch, (uint)buildNumber);
        }

        /// <summary>
        /// Creates a filename form the specified package name, branch and build number.
        /// </summary>
        /// <param name="packageName">The package name.</param>
        /// <param name="branch">The branch.</param>
        /// <param name="buildNumber">The build number.</param>
        /// <returns>The corresponding app package file name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="packageName"/> or <paramref name="branch"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="packageName"/> or <paramref name="branch"/> is empty or whitespace.</exception>
        /// <exception cref="ArgumentException"><paramref name="buildNumber"/> is lower than zero.</exception>
        public static DMAppFileName FromBuildNumber(string packageName, string branch, uint buildNumber)
        {
            if (packageName == null)
            {
                throw new ArgumentNullException(nameof(packageName));
            }

            if (branch == null)
            {
                throw new ArgumentNullException(nameof(branch));
            }

            if (String.IsNullOrWhiteSpace(packageName))
            {
                throw new ArgumentException("Value can not be empty or whitespace.", nameof(packageName));
            }

            if (String.IsNullOrWhiteSpace(branch))
            {
                throw new ArgumentException("Value can not be empty or whitespace.", nameof(branch));
            }

            string packageFileName = packageName + " " + branch + "_B" + buildNumber + ".dmapp";

            return new DMAppFileName(packageFileName);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return _fileName;
        }
    }
}
