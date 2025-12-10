using System;
using System.IO;
using System.Linq;

namespace CourseProject
{
    public static class PathHelper
    {
        public static string GetProjectRoot()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            var dir = new DirectoryInfo(currentDir);

            while (dir != null)
            {
                if (dir.GetFiles("*.csproj").Any() || dir.GetFiles("*.sln").Any())
                    return dir.FullName;

                dir = dir.Parent;
            }

            return AppDomain.CurrentDomain.BaseDirectory; 
        }
    }
}