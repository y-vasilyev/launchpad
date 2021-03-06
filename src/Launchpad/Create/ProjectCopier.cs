using System;
using System.IO;
using System.Linq;

namespace Launchpad.Create
{
    internal class ProjectCopier
    {
        private static readonly string[] ignoredDirectories = {"bin", "obj", ".vs"};
        private const string templateProjectName = "ProjectTemplate";
        private const string templateProjectMacro = "%project%";
        private const string templateServiceMacro = "%service%";

        private readonly CreateOptions createOptions;
        private readonly string sourceDirectory;
        private readonly string targetDirectory;

        public ProjectCopier(CreateOptions createOptions)
        {
            this.createOptions = createOptions;
            createOptions.ServiceName = FixNamespace(createOptions.ServiceName);
            sourceDirectory = Helpers.PatchDirectoryName(Path.Combine("templates", createOptions.Template));
            targetDirectory = Path.Combine(createOptions.Output, createOptions.ServiceName);
        }

        public void Execute()
        {
            var sourceDir = new DirectoryInfo(sourceDirectory);
            var targetDir = new DirectoryInfo(targetDirectory);
            if (targetDir.Exists && targetDir.EnumerateFileSystemInfos().Any())
                throw new InvalidOperationException($"Directory already exists and not empty");
            if (!targetDir.Exists)
                targetDir.Create();
            CopyDirectory(sourceDir, targetDir);
        }

        private static string FixNamespace(string projectName)
        {
            projectName = projectName.Substring(0, 1).ToUpperInvariant() + projectName.Substring(1);
            return new string(projectName.SkipWhile(c => c >= '0' && c <= '9').Where(c => c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c == '.' || c >= '0' && c <= '9').ToArray());
        }

        private void CopyDirectory(DirectoryInfo sourceDir, DirectoryInfo targetDir)
        {
            foreach (var subDir in sourceDir.GetDirectories().Where(d => !ignoredDirectories.Contains(d.Name, StringComparer.OrdinalIgnoreCase)))
                CopyDirectory(subDir, targetDir.CreateSubdirectory(GetTargetDirectoryName(subDir.Name)));
            foreach (var file in sourceDir.GetFiles())
                CopyFile(file, targetDir);
        }

        private string GetTargetDirectoryName(string subDirName)
        {
            if (string.Equals(subDirName, templateProjectName, StringComparison.OrdinalIgnoreCase))
                return createOptions.ServiceName;
            return subDirName;
        }

        private void CopyFile(FileInfo file, DirectoryInfo targetDir)
        {
            using (var reader = file.OpenText())
            using (var writer = new StreamWriter(Path.Combine(targetDir.FullName, GetTargetFileName(file.Name))))
            {
                var content = reader.ReadToEnd();
                var patchedContent = content.Replace(templateProjectName, createOptions.ServiceName);
                patchedContent = patchedContent.Replace(templateProjectMacro, createOptions.ProjectName);
                patchedContent = patchedContent.Replace(templateServiceMacro, createOptions.ServiceName);
                writer.Write(patchedContent);
            }
        }

        private string GetTargetFileName(string fileName)
        {
            return fileName.Replace(templateProjectName, createOptions.ServiceName, StringComparison.OrdinalIgnoreCase);
        }
    }
}