// -------------------------------------------------------------------------------
// <copyright file="UpdateCheck.cs" company="Ben Lye">
// Copyright 2020 Ben Lye
//
// This file is part of Flash Multi.
//
// Flash Multi is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free Software
// Foundation, either version 3 of the License, or(at your option) any later
// version.
//
// Flash Multi is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with
// Flash Multi. If not, see http://www.gnu.org/licenses/.
// </copyright>
// -------------------------------------------------------------------------------

namespace Flash_Multi
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    /// <summary>
    /// Class for performing application update checks.
    /// </summary>
    internal class UpdateCheck
    {
        /// <summary>
        /// Gets a value indicating whether or not the Github version check succeeded.
        /// </summary>
        public bool CheckSuccess { get; private set; }

        /// <summary>
        /// Gets a <see cref="Version"/> containing the version of the latest release.
        /// </summary>
        public Version LatestVersion { get; private set; }

        /// <summary>
        /// Gets a string containing the URL of the latest release.
        /// </summary>
        public string ReleaseUrl { get; private set; }

        /// <summary>
        /// Prompt the user if a newer version is available.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> class.</param>
        public static async void DoCheck(FlashMulti flashMulti)
        {
            // Get check for the latest version on Github
            UpdateCheck check = new UpdateCheck();
            await Task.Run(() => { check = GetLatestVersion(); });

            // If the check completed successfully
            if (check.CheckSuccess)
            {
                // Get the current version
                Version currentVersion = Version.Parse(Application.ProductVersion);

                // Get the version available on Github
                Version latestVersion = check.LatestVersion;

                // Check if the current version is older than the latest Github release version
                if (currentVersion.CompareTo(latestVersion) == -1)
                {
                    // A newer version is available to show the user a prompt
                    Debug.WriteLine($"App version is older than latest version: {currentVersion} < {latestVersion}");
                    DialogResult showUpdate = MessageBox.Show(
                        $"A newer version of Flash Multi is available.\n\nYou have Flash Multi v{currentVersion.ToString()} and Flash Multi v{latestVersion.ToString()} is available.\n\nSee the latest release on Github?",
                        "Flash Multi Update Available",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    // Show the Github release page for the new version if the user clicked Yes
                    if (showUpdate == DialogResult.Yes)
                    {
                        flashMulti.OpenLink(check.ReleaseUrl);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the latest release tag from Github.
        /// </summary>
        /// <returns>Returns an <see cref="UpdateCheck"/>.</returns>
        private static UpdateCheck GetLatestVersion()
        {
            // Somewhere to store the results
            UpdateCheck results = new UpdateCheck
            {
                CheckSuccess = false,
            };

            try
            {
                // The API URL to check for the latest release
                // Returns a JSON payload containing all the details of the latest release
                string releasesUrl = "https://api.github.com/repos/benlye/flash-multi/releases/latest";

                // Set TLS1.2, as required by the Github API
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Create the WebRequest
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(releasesUrl);

                // Set the UserAgent, as required by the Github API
                webRequest.UserAgent = $"flash-multi-{Application.ProductVersion}";

                // Disable keepalive so we don't hold a connection open
                webRequest.KeepAlive = false;

                // Get the response and read it
                using (WebResponse myResponse = webRequest.GetResponse())
                using (StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8))
                {
                    // Read all of the output from the StreamReader
                    string result = sr.ReadToEnd();

                    // Parse the release tag out of the JSON
                    // This contains the version string
                    int tagStart = result.IndexOf("\"tag_name\":");
                    int tagEnd = result.IndexOf(",", tagStart);
                    string tagLine = result.Substring(tagStart, tagEnd - tagStart);
                    string tag = tagLine.Split('"')[3];

                    // Add the release tag to the results
                    results.LatestVersion = Version.Parse(tag);

                    // Parse the release URL out of the JSON
                    // This is the URL of the Github page containing details of the latest release
                    int urlStart = result.IndexOf("\"html_url\":");
                    int urlEnd = result.IndexOf(",", urlStart);
                    string urlLine = result.Substring(urlStart, urlEnd - urlStart);
                    string url = urlLine.Split('"')[3];

                    // Add the release URL to the results
                    results.ReleaseUrl = url;
                }

                // Define a regular expression to test the version number looks how we expect it to
                Regex versionRegex = new Regex(@"\d+\.\d+\.\d+");

                // Check that the URL and version number are as we expect
                if (results.ReleaseUrl.StartsWith("https://github.com/benlye/flash-multi/releases/") && versionRegex.Match(results.LatestVersion.ToString()).Success)
                {
                    // All looks good; the check succeeded
                    Debug.WriteLine($"Update check succeeded. Latest version is {results.LatestVersion}");
                    results.CheckSuccess = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting latest version: {ex.Message}");
            }

            // Return the results
            return results;
        }
    }
}
