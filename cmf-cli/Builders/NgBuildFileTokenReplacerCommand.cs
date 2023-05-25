using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Builders;

internal class NgBuildFileTokenReplacerCommand : IBuildCommand
        {
            private readonly IFileSystem fileSystem;
            private readonly List<string> apps;
            private readonly CmfPackage cmfPackage;

            public NgBuildFileTokenReplacerCommand(IFileSystem fileSystem, List<string> apps, CmfPackage cmfPackage)
            {
                this.fileSystem = fileSystem;
                this.apps = apps;
                this.cmfPackage = cmfPackage;
            }
            public string DisplayName
            {
                get { return "ng build file replacer"; }
                set { throw new NotImplementedException();  }
            }
            public bool Test
            {
                get { return false; } 
                set { } 
            }
            public Task Exec()
            {
                foreach (var app in apps)
                {
                    // place tokens in assets/config.json
                    var configJsonPath = this.fileSystem.FileInfo.New($"dist/{app}/assets/config.json");
                    if (configJsonPath.Exists)
                    {
                        Log.Debug($"Placing tokens in config.json at {configJsonPath.FullName}");
                        var configJsonContent = configJsonPath.ReadToString();
                        // TODO: replace app ref if we're packaging an app with the app name
                        // configJsonContent = Regex.Replace(configJsonContent, @"""ref"":\s*""[^""]*""", "\"ref\": \"$(APPLICATION_REF)\"");
                        configJsonContent = Regex.Replace(configJsonContent, @"""name"":\s*""[^""]*""", "\"name\": \"$(TENANT_NAME)\"");
                        configJsonContent = Regex.Replace(configJsonContent, @"""enableSsl"":\s*[^,]*,", "\"enableSsl\": $(APPLICATION_PUBLIC_HTTP_TLS_ENABLED),");
                        configJsonContent = Regex.Replace(configJsonContent, @"""address"":\s*""[^""]*""", "\"address\": \"$(APPLICATION_PUBLIC_HTTP_ADDRESS)\"");
                        configJsonContent = Regex.Replace(configJsonContent, @"""port"":\s*[^,]*,", "\"port\": $(APPLICATION_PUBLIC_HTTP_PORT),");
                        configJsonContent = Regex.Replace(configJsonContent, @"""environmentName"":\s*""[^""]*""", "\"environmentName\": \"$(SYSTEM_NAME)\"");
                        configJsonContent = Regex.Replace(configJsonContent, @"""defaultDomain"":\s*""[^""]*""", "\"defaultDomain\": \"$(SECURITY_PORTAL_STRATEGY_LOCAL_AD_DEFAULT_DOMAIN)\"");
                        configJsonContent = Regex.Replace(configJsonContent, @"""version"":\s*""[^""]*""", $"\"version\": \"{ExecutionContext.Instance.ProjectConfig.MESVersion}\",\n\"customizationVersion\": \"{cmfPackage.Version}\"");
                        this.fileSystem.File.WriteAllText(configJsonPath.FullName, configJsonContent);
                    }
                    else
                    {
                        Log.Warning($"Couldn't find config.json at {configJsonPath.FullName}!");
                    }
                    
                    
                    if (ExecutionContext.Instance.ProjectConfig.RepositoryType == RepositoryType.App)
                    {
                        // place app name in index.html
                        // TODO get app name from cmfapp.json
                        var appName = ExecutionContext.Instance.ProjectConfig.ProjectName;
                        var indexPath = this.fileSystem.FileInfo.New($"dist/{app}/index.html");
                        if (indexPath.Exists)
                        {
                            Log.Debug($"Setting App Name '{appName}' as title in {indexPath.FullName}");
                            var indexContent = indexPath.ReadToString();
                            indexContent = Regex.Replace(indexContent, @"<title>.*</title>", $"<title>{appName}</title>");
                            this.fileSystem.File.WriteAllText(indexPath.FullName, indexContent);
                        }
                        else
                        {
                            Log.Warning($"Couldn't find index.html at {indexPath.FullName}!");
                        }
                    }
                    
                    // fix trailing slash in ngsw.json
                    var ngswPath = this.fileSystem.FileInfo.New($"dist/{app}/ngsw.json");
                    if (ngswPath.Exists)
                    {
                        Log.Debug($"Placing tokens in ngsw.json at {ngswPath.FullName}");
                        var ngswContent = ngswPath.ReadToString(); //  '\${APPLICATION_BASE_HREF}/', '${APPLICATION_BASE_HREF}';
                        ngswContent = Regex.Replace(ngswContent, @"\$\(APPLICATION_BASE_HREF\)/", "$(APPLICATION_BASE_HREF)");
                        this.fileSystem.File.WriteAllText(ngswPath.FullName, ngswContent);
                    }
                    else
                    {
                        Log.Warning($"Couldn't find ngsw.json at {ngswPath.FullName}!");
                    }
                }

                return null;
            }
        }