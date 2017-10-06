using System;
using System.Reflection;

namespace NuGetOffline
{
    /// <summary>
    /// An implementation of <see cref="IAssemblyNameFinder"/> that uses the reflection-only load context
    /// </summary>
    internal class ReflectionOnlyAssemblyNameFinder : IAssemblyNameFinder
    {
        private readonly string _assemblyName;
        private readonly string _typeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionOnlyAssemblyNameFinder"/> class.
        /// </summary>
        public ReflectionOnlyAssemblyNameFinder()
        {
            _assemblyName = typeof(MsbuildFileBuilder).Assembly.GetName().ToString();
            _typeName = typeof(AssemblyLoadHelper).FullName;
        }

        /// <inheritdoc/>
        public AssemblyName GetAssemblyName(byte[] bytes)
        {
            var domain = AppDomain.CreateDomain(Guid.NewGuid().ToString());

            try
            {
                var service = (AssemblyLoadHelper)domain.CreateInstanceAndUnwrap(_assemblyName, _typeName);

                return service.GetAssemblyName(bytes);
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        private class AssemblyLoadHelper : MarshalByRefObject
        {
            public AssemblyName GetAssemblyName(byte[] bytes)
            {
                return Assembly.ReflectionOnlyLoad(bytes).GetName();
            }
        }
    }
}
