// -----------------------------------------------------------------------
// <copyright file="FrameworkDetetctor.cs" company="Tier 3">
// Copyright © 2012 Tier 3 Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;

namespace IronFoundry.VcapClient
{
    public static class FrameworkDetetctor
    {
        /*
            vmc runtimes
            +--------------+---------------------------+-------------+
            | Name         | Description               | Version     |
            +--------------+---------------------------+-------------+
            | java         | Java 6                    | 1.6         |
            | ruby18       | Ruby 1.8.7                | 1.8.7       |
            | ruby19       | Ruby 1.9.2                | 1.9.2p180   |
            | erlangR14B02 | Erlang R14B02             | R14B02      |
            | aspdotnet40  | ASP.NET 4.0 (4.0.30319.1) | 4.0.30319.1 |
            | python2      | Python 2.6.5              | 2.6.5       |
            | node         | Node.js                   | 0.4.12      |
            | node06       | Node.js                   | 0.6.8       |
            | php          | PHP 5                     | 5.3         |
            +--------------+---------------------------+-------------+
         * 
         * Preliminary framework / runtime detection
         */
        private static readonly IDictionary<string, DetetectedFramework> Frameworks =
            new Dictionary<string, DetetectedFramework> 
            {
              { "ASP.NET 4.0",      new DetetectedFramework { Framework = "aspdotnet", Runtime = "aspdotnet40", DefaultMemoryMB = 64 } },
              { "Django",           new DetetectedFramework { Framework = "django", Runtime = "python2", DefaultMemoryMB = 128 } },
              { "Erlang/OTP Rebar", new DetetectedFramework { Framework = "otp_rebar", Runtime = "erlangR14B02", DefaultMemoryMB = 64 } },
              { "Grails",           new DetetectedFramework { Framework = "grails", Runtime = "java", DefaultMemoryMB = 512 } },
              { "JavaWeb",          new DetetectedFramework { Framework = "java_web", Runtime = "java", DefaultMemoryMB = 512 } },
              { "Lift",             new DetetectedFramework { Framework = "lift", Runtime = "java", DefaultMemoryMB = 512 } },
              { "Node",             new DetetectedFramework { Framework = "node", Runtime = "node", DefaultMemoryMB = 64 } }, // TODO Runtime
              { "PHP",              new DetetectedFramework { Framework = "php", Runtime = "php", DefaultMemoryMB = 128 } },
              { "Rack",             new DetetectedFramework { Framework = "rack", Runtime = "ruby19", DefaultMemoryMB = 128 } }, // TODO Runtime
              { "Rails",            new DetetectedFramework { Framework = "rails3", Runtime = "ruby19", DefaultMemoryMB = 256 } },
              { "Sinatra",          new DetetectedFramework { Framework = "sinatra", Runtime = "ruby19", DefaultMemoryMB = 64 } }, // TODO Runtime
              { "Spring",           new DetetectedFramework { Framework = "spring", Runtime = "java", DefaultMemoryMB = 512 } },
              { "Standalone",       new DetetectedFramework { Framework = "standalone", DefaultMemoryMB = 64 } },
              { "WSGI",             new DetetectedFramework { Framework = "wsgi", Runtime = "python2", DefaultMemoryMB = 64 } },
            };

        private static readonly Regex GrailsJarRegex = new Regex(@"WEB-INF[/\\]lib[/\\]grails-web.*\.jar", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex LiftJarRegex = new Regex(@"WEB-INF\/lib\/lift-webkit.*\.jar", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex SpringDirRegex = new Regex(@"WEB-INF\/classes\/org\/springframework", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex SpringJarRegex1 = new Regex(@"WEB-INF\/lib\/spring-core.*\.jar", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex SpringJarRegex2 = new Regex(@"WEB-INF\/lib\/org\.springframework\.core.*\.jar", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex SinatraRegex = new Regex(@"^\s*require[\s\(]*['""]sinatra['""]", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static DetetectedFramework Detect(DirectoryInfo path)
        {
            if (AppFileExists(path, @"config\environment.rb"))
            {
                return Frameworks["Rails"];
            }

            if (AppFileExists(path, "config.ru"))
            {
                return Frameworks["Rack"];
            }

            var firstWarFilePath = DirGlob(path, "*.war").FirstOrDefault();
            if (null != firstWarFilePath && firstWarFilePath.Exists)
            {
                return DetectFrameworkFromWar(firstWarFilePath, path);
            }

            if (AppFileExists(path, @"WEB-INF/web.xml"))
            {
                return DetectFrameworkFromPath(path);
            }

            var rubyFiles = DirGlob(path, "*.rb");
            var fileInfos = rubyFiles as List<FileInfo> ?? rubyFiles.ToList();
            if (false == fileInfos.IsNullOrEmpty())
            {
                if (fileInfos.Select(
                    rubyFile => File.ReadAllText(rubyFile.FullName)).
                    Any(text => SinatraRegex.IsMatch(text)))
                {
                    return Frameworks["Sinatra"];
                }
            }

            if (AppFileExists(path, "server.js") || AppFileExists(path, "app.js") ||
                AppFileExists(path, "index.js") || AppFileExists(path, "main.js"))
            {
                return Frameworks["Node"];
            }

            if (AppFileExists(path, "Web.config"))
            {
                return Frameworks["ASP.NET 4.0"];
            }

            var phpFiles = DirGlob(path, "*.php");
            if (false == phpFiles.IsNullOrEmpty())
            {
                return Frameworks["PHP"];
            }

            var erlangPath = new DirectoryInfo(Path.Combine(path.FullName, "releases"));
            var erlRelFiles = DirGlob(erlangPath, "*.rel", true);
            var erlBootFiles = DirGlob(erlangPath, "*.boot", true);
            if (false == erlRelFiles.IsNullOrEmpty() || false == erlBootFiles.IsNullOrEmpty())
            {
                return Frameworks["Erlang/OTP Rebar"];
            }

            if (AppFileExists(path, "manage.py") || AppFileExists(path, "settings.py"))
            {
                return Frameworks["Django"];
            }

            return AppFileExists(path, "wsgi.py") ? Frameworks["WSGI"] : Frameworks["Standalone"];
        }

        private static DetetectedFramework DetectFrameworkFromPath(DirectoryInfo appPath)
        {
            return DetectFrameworkFromWar(null, appPath);
        }

        private static DetetectedFramework DetectFrameworkFromWar(FileSystemInfo warFile = null, FileSystemInfo appPath = null)
        {
            IEnumerable<string> contents;
            if (null == warFile && appPath != null)
            {
                contents = Directory.EnumerateFiles(appPath.FullName, "*", SearchOption.AllDirectories);
            }
            else if (warFile != null)
            {
                    var zf = new ZipFile(warFile.FullName);
                    var zipContents = (from ZipEntry entry in zf select entry.Name).ToList();
                    contents = zipContents;
            }
            else
            {
                // Code paths enabled a possible null null which left an exception possibility. Thus
                // I've added this argument.  <-- Possibly re-write this. For now just placed this here.
                throw new ArgumentNullException("Contents are null.");
            }

            var enumerable = contents as List<string> ?? contents.ToList();
            if (false == enumerable.IsNullOrEmpty())
            {
                foreach (var name in enumerable)
                {
                    if (GrailsJarRegex.IsMatch(name))
                    {
                        return Frameworks["Grails"];
                    }
                    if (LiftJarRegex.IsMatch(name))
                    {
                        return Frameworks["Lift"];
                    }
                    if (SpringDirRegex.IsMatch(name) ||
                        SpringJarRegex1.IsMatch(name) ||
                        SpringJarRegex2.IsMatch(name))
                    {
                        return Frameworks["Spring"];
                    }
                }
            }

            return Frameworks["JavaWeb"];
        }

        private static IEnumerable<FileInfo> DirGlob(FileSystemInfo path, string glob, bool recursive = false)
        {
            IEnumerable<FileInfo> rv = null;

            if (path.Exists)
            {
                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                rv = Directory.GetFiles(path.FullName, glob, searchOption).Select(f => new FileInfo(f));
            }

            return rv;
        }

        private static bool AppFileExists(FileSystemInfo path, string relativeFilePath)
        {
            var pathToFile = Path.Combine(path.FullName, relativeFilePath);
            return File.Exists(pathToFile);
        }
    }
}