using System.IO;
using System.Reflection;

namespace LibraryApplication.Services
{
    public static class EmbeddedResourceHelper
    {
        /// <summary>
        /// Reads an embedded resource file as a string.
        /// </summary>
        /// <param name="resourceName">The fully qualified resource name.</param>
        /// <returns>The content of the embedded resource as a string.</returns>
        public static string GetEmbeddedResource(string resourceName)
        {
            // Get the assembly containing the resource
            var assembly = Assembly.GetExecutingAssembly();

            // Attempt to find and load the embedded resource
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

            // Read the resource content
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}