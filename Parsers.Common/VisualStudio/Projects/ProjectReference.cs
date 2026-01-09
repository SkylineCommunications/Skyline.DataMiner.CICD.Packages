namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a project reference.
    /// </summary>
    public struct ProjectReference : IEquatable<ProjectReference>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectReference"/> structure with the specified name and path.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        public ProjectReference(string name, string path)
        {
            Path = path;
            Name = name;
            Guid = System.Guid.Empty.ToString();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectReference"/> structure with the specified name, path and GUID.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <param name="guid">The GUID.</param>
        public ProjectReference(string name, string path, string guid) : this(name, path)
        {
            Guid = guid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectReference"/> structure with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        internal ProjectReference(string name) : this(name, null)
        {
        }

        /// <summary>
        /// Gets the name of the project.
        /// </summary>
        /// <value>The name of the project.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the path of the project.
        /// </summary>
        /// <value>The path of the project.</value>
        public string Path { get; }

        /// <summary>
        /// Gets the GUID of the project reference.
        /// </summary>
        /// <value>The GUID of the project reference.</value>
        public string Guid { get; }
        
        /// <summary>
        /// Determines whether the two specified objects are equal.
        /// </summary>
        /// <param name="term1">The first value to compare.</param>
        /// <param name="term2">The second value to compare.</param>
        /// <returns><c>true</c> if the operands are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(ProjectReference term1, ProjectReference term2)
        {
            return term1.Equals(term2);
        }

        /// <summary>
        /// Determines whether the two specified objects are not equal.
        /// </summary>
        /// <param name="term1">The first object to compare.</param>
        /// <param name="term2">The second object to compare.</param>
        /// <returns><c>false</c> if the operands are equal; otherwise, <c>true</c>.</returns>
        public static bool operator !=(ProjectReference term1, ProjectReference term2)
        {
            return !term1.Equals(term2);
        }

        /// <summary>
        /// Returns a value indicating whether the current <see cref="ProjectReference"/> object is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare with the current <see cref="ProjectReference"/> object, or <see langword="null"/>.</param>
        /// <returns><c>true</c> if the current <see cref="ProjectReference"/> object and obj are both <see cref="ProjectReference"/> objects, and every component of the current <see cref="ProjectReference"/> object matches the corresponding component of obj; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is ProjectReference reference &&
                   Name == reference.Name &&
                   Path == reference.Path &&
                   Guid == reference.Guid;
        }

        /// <summary>
        /// Returns a value indicating whether the current <see cref="ProjectReference"/> object and a specified Version object represent the same value.
        /// </summary>
        /// <param name="other">A Version object to compare to the current <see cref="ProjectReference"/> object, or null.</param>
        /// <returns><c>true</c> if every component of the current <see cref="ProjectReference"/> object matches the corresponding component of the obj parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(ProjectReference other)
        {
            return Name == other.Name && Path == other.Path && Guid == other.Guid;
        }

        /// <summary>
        /// Returns a hash code for the current <see cref="ProjectReference"/> object.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            int hashCode = -1635288508;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Path);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Guid);
            return hashCode;
        }
    }
}
