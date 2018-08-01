using System;
using Cake.Core;
using Cake.Core.Annotations;

namespace CodeFileSanity
{
    public static class CakeAlias
    {
        [CakeMethodAlias]
        public static void ValidateCodeSanity(this ICakeContext context, ValidateCodeSanitySettings settings) {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            CodeSanityValidator.hasAppveyor = settings.IsAppveyorBuild;

            CodeSanityValidator.checkDirectory(".");

            if (CodeSanityValidator.hasErrors)
                throw new CakeException("Code sanity validation failed.");
        }

    }
}