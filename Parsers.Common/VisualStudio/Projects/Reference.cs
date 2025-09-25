namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.CICD.FileSystem;

    /// <summary>
    /// Represents a reference in a Visual Studio project.
    /// </summary>
    public struct Reference : IEquatable<Reference>
    {
        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="Reference"/> structure with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        public Reference(string name)
        {
            _fileSystem = FileSystem.Instance;

            Name = name;
            HintPath = null;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Reference"/> structure with the specified name and hint path.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="hintPath">The hint path.</param>
        public Reference(string name, string hintPath) : this(name)
        {
            HintPath = hintPath;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the hint path.
        /// </summary>
        /// <value>The hint path.</value>
        public string HintPath { get; private set; }

        /// <summary>
        /// Determines whether the two specified objects are equal.
        /// </summary>
        /// <param name="term1">The first value to compare.</param>
        /// <param name="term2">The second value to compare.</param>
        /// <returns><c>true</c> if the operands are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Reference term1, Reference term2)
        {
            return term1.Equals(term2);
        }

        /// <summary>
        /// Determines whether the two specified objects are not equal.
        /// </summary>
        /// <param name="term1">The first object to compare.</param>
        /// <param name="term2">The second object to compare.</param>
        /// <returns><c>false</c> if the operands are equal; otherwise, <c>true</c>.</returns>
        public static bool operator !=(Reference term1, Reference term2)
        {
            return !term1.Equals(term2);
        }

        /// <summary>
        /// Retrieves the DLL name.
        /// </summary>
        /// <returns>The DLL name.</returns>
        public string GetDllName()
        {
            string dll = null;

            if (!String.IsNullOrWhiteSpace(HintPath))
            {
                dll = _fileSystem.Path.GetFileName(HintPath);
            }
            else if (!String.IsNullOrWhiteSpace(Name))
            {
                dll = Name.Split(',')[0]?.Trim();
            }
            else
            {
                // Do nothing.
            }

            if (!String.IsNullOrWhiteSpace(dll) &&
                !dll.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                dll += ".dll";
            }

            return dll;
        }

        /// <summary>
        /// Returns a value indicating whether the current <see cref="ProjectReference"/> object is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare with the current <see cref="ProjectReference"/> object, or <see langword="null"/>.</param>
        /// <returns><c>true</c> if the current <see cref="ProjectReference"/> object and obj are both <see cref="ProjectReference"/> objects, and every component of the current <see cref="ProjectReference"/> object matches the corresponding component of obj; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is Reference reference &&
                   Name == reference.Name &&
                   HintPath == reference.HintPath;
        }

        /// <summary>
        /// Returns a value indicating whether the current <see cref="Reference"/> object and a specified Version object represent the same value.
        /// </summary>
        /// <param name="other">A Version object to compare to the current <see cref="Reference"/> object, or null.</param>
        /// <returns><c>true</c> if every component of the current <see cref="Reference"/> object matches the corresponding component of the obj parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(Reference other)
        {
            return Name == other.Name && HintPath == other.HintPath;
        }

        /// <summary>
        /// Returns a hash code for the current <see cref="Reference"/> object.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            int hashCode = 2134248345;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(HintPath);
            return hashCode;
        }
    }
}