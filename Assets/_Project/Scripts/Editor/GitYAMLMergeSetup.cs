using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace XRStarterProject.Editor
{
    /// <summary>
    /// Checks on Unity startup if the UnityYAMLMerge tool is configured in git.
    /// Prompts the user to configure it if not set up.
    /// </summary>
    [InitializeOnLoad]
    public static class GitYAMLMergeSetup
    {
        private const string SkipCheckKey = "GitYAMLMergeSetup_SkipCheck";
        private const string LastCheckKey = "GitYAMLMergeSetup_LastCheck";
        private const int CheckIntervalDays = 7; // Re-check every 7 days even if skipped

        static GitYAMLMergeSetup()
        {
            // Delay the check to avoid blocking editor startup
            EditorApplication.delayCall += CheckGitMergeConfiguration;
        }

        private static void CheckGitMergeConfiguration()
        {
            // Skip if not in a git repository
            if (!IsGitRepository())
                return;

            // Check if user has chosen to skip (with periodic re-check)
            if (ShouldSkipCheck())
                return;

            // Check if unityyamlmerge is already configured
            if (IsYAMLMergeConfigured())
                return;

            // Show setup dialog
            ShowSetupDialog();
        }

        private static bool IsGitRepository()
        {
            string projectPath = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectPath))
                return false;

            string gitPath = Path.Combine(projectPath, ".git");
            return Directory.Exists(gitPath) || File.Exists(gitPath); // .git can be a file for worktrees
        }

        private static bool ShouldSkipCheck()
        {
            if (!EditorPrefs.GetBool(SkipCheckKey, false))
                return false;

            // Check if enough time has passed to re-prompt
            string lastCheckStr = EditorPrefs.GetString(LastCheckKey, "");
            if (DateTime.TryParse(lastCheckStr, out DateTime lastCheck))
            {
                if ((DateTime.Now - lastCheck).TotalDays < CheckIntervalDays)
                    return true;
            }

            // Reset skip flag after interval
            EditorPrefs.SetBool(SkipCheckKey, false);
            return false;
        }

        private static bool IsYAMLMergeConfigured()
        {
            try
            {
                string output = RunGitCommand("config --get merge.unityyamlmerge.driver");
                return !string.IsNullOrWhiteSpace(output);
            }
            catch
            {
                // If git command fails, assume not configured
                return false;
            }
        }

        private static void ShowSetupDialog()
        {
            int choice = EditorUtility.DisplayDialogComplex(
                "Git YAML Merge Not Configured",
                "The UnityYAMLMerge tool is not configured for this repository. " +
                "This tool helps resolve merge conflicts in Unity files (scenes, prefabs, etc.).\n\n" +
                "Would you like to configure it now?",
                "Configure Automatically",
                "Skip for Now",
                "Don't Ask Again (7 days)"
            );

            switch (choice)
            {
                case 0: // Configure
                    ConfigureYAMLMerge();
                    break;
                case 1: // Skip
                    // Do nothing, will ask again next startup
                    break;
                case 2: // Don't ask again
                    EditorPrefs.SetBool(SkipCheckKey, true);
                    EditorPrefs.SetString(LastCheckKey, DateTime.Now.ToString("o"));
                    break;
            }
        }

        private static void ConfigureYAMLMerge()
        {
            try
            {
                // Get the path to UnityYAMLMerge
                string yamlMergePath = GetYAMLMergePath();
                if (string.IsNullOrEmpty(yamlMergePath))
                {
                    EditorUtility.DisplayDialog(
                        "Configuration Failed",
                        "Could not find UnityYAMLMerge tool. Please configure manually.",
                        "OK"
                    );
                    return;
                }

                // Escape backslashes for git config
                string escapedPath = yamlMergePath.Replace("\\", "/");

                // Configure the merge driver
                string driverCommand = $"\"{escapedPath}\" merge -p %O %B %A %A";

                RunGitCommand($"config merge.unityyamlmerge.name \"Unity YAML Merge\"");
                RunGitCommand($"config merge.unityyamlmerge.driver \"{driverCommand}\"");
                RunGitCommand("config merge.unityyamlmerge.recursive binary");

                // Verify configuration
                if (IsYAMLMergeConfigured())
                {
                    EditorUtility.DisplayDialog(
                        "Configuration Successful",
                        "UnityYAMLMerge has been configured for this repository.\n\n" +
                        $"Tool path: {yamlMergePath}",
                        "OK"
                    );
                    Debug.Log($"[GitYAMLMergeSetup] Successfully configured UnityYAMLMerge: {yamlMergePath}");
                }
                else
                {
                    throw new Exception("Configuration verification failed");
                }
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog(
                    "Configuration Failed",
                    $"Failed to configure UnityYAMLMerge:\n{ex.Message}\n\n" +
                    "Please configure manually by running:\n" +
                    "git config --local include.path ../.gitconfig",
                    "OK"
                );
                Debug.LogError($"[GitYAMLMergeSetup] Failed to configure: {ex}");
            }
        }

        private static string GetYAMLMergePath()
        {
            // Get Unity Editor path and derive Tools path
            string editorPath = EditorApplication.applicationPath;
            string editorDir = Path.GetDirectoryName(editorPath);

            if (string.IsNullOrEmpty(editorDir))
                return null;

            // On Windows: Editor/Data/Tools/UnityYAMLMerge.exe
            // On macOS: Unity.app/Contents/Tools/UnityYAMLMerge
            string toolsPath;

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                toolsPath = Path.Combine(editorDir, "Data", "Tools", "UnityYAMLMerge.exe");
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                // Go up from Unity.app/Contents/MacOS to Unity.app/Contents/Tools
                string contentsDir = Path.GetDirectoryName(editorDir);
                toolsPath = Path.Combine(contentsDir, "Tools", "UnityYAMLMerge");
            }
            else
            {
                // Linux
                toolsPath = Path.Combine(editorDir, "Data", "Tools", "UnityYAMLMerge");
            }

            return File.Exists(toolsPath) ? toolsPath : null;
        }

        private static string RunGitCommand(string arguments)
        {
            string projectPath = Directory.GetParent(Application.dataPath)?.FullName;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = projectPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0 && !string.IsNullOrEmpty(error))
                {
                    // Exit code 1 with no error just means config not found
                    if (process.ExitCode == 1 && string.IsNullOrWhiteSpace(error))
                        return "";

                    throw new Exception($"Git command failed: {error}");
                }

                return output.Trim();
            }
        }

        /// <summary>
        /// Menu item to manually trigger the configuration check.
        /// </summary>
        [MenuItem("Tools/Git/Configure YAML Merge Tool")]
        public static void ManualConfigure()
        {
            // Reset skip flag so dialog shows
            EditorPrefs.SetBool(SkipCheckKey, false);

            if (IsYAMLMergeConfigured())
            {
                bool reconfigure = EditorUtility.DisplayDialog(
                    "Already Configured",
                    "UnityYAMLMerge is already configured for this repository.\n\n" +
                    "Would you like to reconfigure it?",
                    "Reconfigure",
                    "Cancel"
                );

                if (reconfigure)
                    ConfigureYAMLMerge();
            }
            else
            {
                ShowSetupDialog();
            }
        }

        /// <summary>
        /// Menu item to check the current configuration status.
        /// </summary>
        [MenuItem("Tools/Git/Check YAML Merge Status")]
        public static void CheckStatus()
        {
            bool isConfigured = IsYAMLMergeConfigured();
            string yamlMergePath = GetYAMLMergePath();

            string message = isConfigured
                ? $"UnityYAMLMerge is configured.\n\nTool location: {yamlMergePath ?? "Unknown"}"
                : "UnityYAMLMerge is NOT configured.\n\nUse 'Tools > Git > Configure YAML Merge Tool' to set it up.";

            EditorUtility.DisplayDialog(
                "YAML Merge Status",
                message,
                "OK"
            );
        }
    }
}
