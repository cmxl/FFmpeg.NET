using FFmpeg.NET.Services;
using FFmpeg.NET.Tests.Fixtures;
using System;
using System.IO;
using System.Linq;
using FFmpeg.NET.Extensions;
using Microsoft.DotNet.PlatformAbstractions;
using Xunit;

namespace FFmpeg.NET.Tests
{
    public class StringExtensionsTest
    {
        [Fact]
        public void Should_Get_FileExists_True_On_This_File()
        {
            // Arrange
            var testFolder = GetTestFolder();
            var thisFile = Path.Combine(testFolder, nameof(StringExtensionsTest) + ".cs");

            // Act
            var fileExists = File.Exists(thisFile);

            // Assert
            Assert.True(fileExists);
        }

        [Fact]
        public void Should_Get_FullPath_To_Existing_File()
        {
            // Arrange
            var testFolder = GetTestFolder();
            var thisFile = Path.Combine(testFolder, nameof(StringExtensionsTest) + ".cs");

            // Act
            var fileExists = thisFile.TryGetFullPath(out var fullPath);
            var fileExistsVerified = File.Exists(fullPath);

            // Assert
            Assert.True(fileExists);
            Assert.True(fileExistsVerified);
        }

        [Fact]
        public void Should_Not_Get_FullPath_To_NonExisting_File()
        {
            // Arrange
            var testFolder = GetTestFolder();
            var bogusFile = Path.Combine(testFolder, "bogusFile.txt");

            // Act
            var fileExists = bogusFile.TryGetFullPath(out var fullPath);
            var fileExistsVerified = File.Exists(fullPath);

            // Assert
            Assert.False(fileExists);
            Assert.False(fileExistsVerified);
        }

        [Fact]
        public void Should_Get_FullPath_When_Directory_Is_In_Path()
        {
            // Arrange
            var testFolder = GetTestFolder();
            var testFileName = "existsInPathEnvFile.deleteMe";
            var testFileFullPath = Path.Combine(testFolder, testFileName);
            var pathVariable = System.Environment.GetEnvironmentVariable("PATH");
            var newPathVariable = pathVariable + $";{testFolder}";
            var target = EnvironmentVariableTarget.Process;

            Environment.SetEnvironmentVariable("PATH", newPathVariable, target);
            using (File.Create(testFileFullPath)) { }

            // Act
            var fileExists = testFileName.TryGetFullPath(out var fullPath);
            var fileExistsVerified = File.Exists(fullPath);

            // Cleanup
            Environment.SetEnvironmentVariable("PATH", pathVariable, target);
            File.Delete(testFileFullPath);

            // Assert
            Assert.True(fileExists);
            Assert.True(fileExistsVerified);
        }

        [Fact]
        public void Should_Not_Get_FullPath_When_Directory_Is__In_Path_But_File_Is_Not()
        {
            // Arrange
            var testFolder = GetTestFolder();
            var testFileName = "notInPathEnvFile.txt";
            var orgPathVariable = Environment.GetEnvironmentVariable("PATH");
            var newPathVariable = orgPathVariable + $";{testFolder}";
            var target = EnvironmentVariableTarget.Process;

            Environment.SetEnvironmentVariable("PATH", newPathVariable, target);

            // Act
            var fileExists = testFileName.TryGetFullPath(out var fullPath);
            var fileExistsVerified = File.Exists(fullPath);

            // Cleanup
            Environment.SetEnvironmentVariable("PATH", orgPathVariable, target);

            // Assert
            Assert.False(fileExists);
            Assert.False(fileExistsVerified);
        }

        private static string GetTestFolder()
        {
            var startupPath = ApplicationEnvironment.ApplicationBasePath;
            var pathItems = startupPath.Split(Path.DirectorySeparatorChar);
            var pos = pathItems.Reverse().ToList().FindIndex(x => string.Equals("bin", x));
            var projectPath = string.Join(Path.DirectorySeparatorChar.ToString(), pathItems.Take(pathItems.Length - pos - 1));
            return projectPath;
        }
    }
}