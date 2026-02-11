using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cmf.CLI.Utilities;
using Microsoft.TemplateEngine.Core;

namespace Cmf.CLI.Core.Objects
{
    public class RelatedPackageCollection : List<RelatedPackage>
    {
        public void Load(CmfPackage anchorPackage)
        {
            
            ForEach(relatedPackage =>
            {
                var fileSystem = (ExecutionContext.Instance ?? throw new InvalidOperationException("ExecutionContext not initialized")).FileSystem;
                var cmfpackageJsonFile = fileSystem.FileInfo.New(Path.Join(anchorPackage.GetFileInfo().Directory?.FullName, relatedPackage.Path, Constants.CoreConstants.CmfPackageFileName));
                relatedPackage.CmfPackage = new(cmfpackageJsonFile);
                relatedPackage.CmfPackage.Peek();
                if (!ExecutionContext.RelatedPackagesCache.Contains(relatedPackage))
                {
                    ExecutionContext.RelatedPackagesCache.Add(relatedPackage);
                    relatedPackage.CmfPackage = CmfPackage.Load(cmfpackageJsonFile, anchorPackage.IsToSetDefaultValues);
                }
            });
        }

        public new bool Contains(RelatedPackage relatedPackage)
        {
            return this.HasAny(x => x.CmfPackage?.PackageName.IgnoreCaseEquals(relatedPackage?.CmfPackage?.PackageName) == true);
        }
    }
}