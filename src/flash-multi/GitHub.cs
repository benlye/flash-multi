// -------------------------------------------------------------------------------
// <copyright file="GitHub.cs" company="Ben Lye">
// Copyright 2019 Ben Lye
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
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using Newtonsoft.Json;

    /// <summary>
    /// Class for interacting with the GitHub API.
    /// </summary>
    internal class GitHub
    {
        /// <summary>
        /// The base URL for the GitHub API.
        /// </summary>
        private static readonly string ApiBaseUrl = "https://api.github.com/repos/pascallanger/DIY-Multiprotocol-TX-Module/";

        /// <summary>
        /// Gets or sets a collection of the recent release tags.
        /// Used to cache the details of the releases so that we don't have to call the API every time we need them.
        /// </summary>
        private static Collection<Release> Releases { get; set; }

        /// <summary>
        /// Calls the Github API and returns the response.
        /// </summary>
        /// <param name="url">The API URL to call.</param>
        /// <returns>An <see cref="ApiResponse"/> object containing the response.</returns>
        public static ApiResponse GetApiRespose(string url)
        {
            // Create the response object to return
            ApiResponse response = new ApiResponse
            {
                Url = url,
                Success = false,
            };

            Debug.WriteLine($"Invoking GitHub API call to {url}.");

            try
            {
                // Set TLS1.2, as required by the Github API
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Create the WebRequest
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);

                // Set the UserAgent, as required by the Github API
                webRequest.UserAgent = $"flash-multi-{Assembly.GetExecutingAssembly().GetName().Version.ToString()}";

                // Disable keepalive so we don't hold a connection open
                webRequest.KeepAlive = false;

                // Get the response and read it
                using (HttpWebResponse myResponse = (HttpWebResponse)webRequest.GetResponse())
                using (StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8))
                {
                    Debug.WriteLine($"GitHub API responded with {myResponse.StatusCode.ToString()}.");

                    // Set the response code
                    response.ResponseCode = myResponse.StatusCode.ToString();

                    // Read all of the output from the StreamReader
                    response.Response = sr.ReadToEnd();

                    Debug.WriteLine($"{myResponse.ContentLength} bytes read from API.");

                    // Check the headers
                    string apiRatelimit = myResponse.Headers["X-RateLimit-Limit"];
                    string apiRateLimitRemaining = myResponse.Headers["X-RateLimit-Remaining"];
                    string apiRateLimitReset = myResponse.Headers["X-RateLimit-Reset"];

                    if (int.TryParse(apiRateLimitReset, out int limitReset))
                    {
                        DateTime apiLimitReset = FromUnixTime(limitReset);
                        Debug.WriteLine($"{apiRateLimitRemaining} API calls remaining until {apiLimitReset}.");
                    }

                    // Success
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error invoking API request.\r\n{ex.Message}");
                response.Error = ex.Message;
            }

            // Return the reponse
            return response;
        }

        /// <summary>
        /// Use the GitHub API to get a collection of all the releases.
        /// </summary>
        /// <returns>A collection of <see cref="Release"/> objects.</returns>
        public static Collection<Release> GetReleases()
        {
            // Try the cache first
            if (Releases != null)
            {
                Debug.WriteLine($"Returning {Releases.Count} cached releases.");
                return Releases;
            }

            Debug.WriteLine("Calling GitHub API to fetch releases");

            // Variable to return the releases
            Collection<Release> releases = new Collection<Release>();

            // The URL of the releases API
            string releasesUrl = ApiBaseUrl + "releases";

            // Call the API and get the response
            ApiResponse response = GetApiRespose(releasesUrl);

            // Deserialize the JSON response if the API call succeeded
            if (response.Success)
            {
                releases = JsonConvert.DeserializeObject<Collection<Release>>(response.Response);
            }

            // Cache the answer for future queries
            Releases = releases;

            // Return the deserialized collection of releases
            return releases;
        }

        /// <summary>
        /// Converts a UNIX-style epoch timestamp to a DateTime.
        /// Used to convert the times in GitHub API responses.
        /// </summary>
        /// <param name="unixTime">Epoch time to convert.</param>
        /// <returns>A <see cref="DateTime"/> containing the converted timestamp.</returns>
        public static DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        /// <summary>
        /// Contains a response from a GitHub API call.
        /// </summary>
        public class ApiResponse
        {
            /// <summary>
            /// Gets or sets the API url used for the request.
            /// </summary>
            public string Url { get; set; }

            /// <summary>
            ///  Gets or sets a value indicating whether the API call was successful.
            /// </summary>
            public bool Success { get; set; }

            /// <summary>
            /// Gets or sets the HTTP reponse code returned by the API.
            /// </summary>
            public string ResponseCode { get; set; }

            /// <summary>
            /// Gets or sets the response returned by the API.
            /// </summary>
            public string Response { get; set; }

            /// <summary>
            /// Gets or sets an error message.
            /// </summary>
            public string Error { get; set; }

            /// <summary>
            /// Gets or sets an API message.
            /// </summary>
            public string Message { get; set; }
        }

        /// <summary>
        /// Class to store the details of a GitHub release, as retrieved from the Git API.
        /// See https://developer.github.com/v3/repos/releases/ for details.
        /// See https://api.github.com/repos/pascallanger/DIY-Multiprotocol-TX-Module/releases/18408222 for an example.
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        public class Release
        {
            /// <summary>
            /// Gets or sets the API URL for the release.
            /// </summary>
            [JsonProperty("url")]
            public string Url { get; set; }

            /// <summary>
            /// Gets or sets the HTML URL for the release.
            /// </summary>
            [JsonProperty("html_url")]
            public string HtmlUrl { get; set; }

            /// <summary>
            /// Gets or sets the release ID.
            /// </summary>
            [JsonProperty("id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the tag name of the release.
            /// </summary>
            [JsonProperty("tag_name")]
            public string TagName { get; set; }

            /// <summary>
            /// Gets or sets the commitish value of the release tag.
            /// </summary>
            [JsonProperty("target_commitish")]
            public string TargetCommitish { get; set; }

            /// <summary>
            /// Gets or sets the name of the release.
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether or not this is a draft release.
            /// </summary>
            [JsonProperty("draft")]
            public bool Draft { get; set; }

            /// <summary>
            /// Gets or sets the creation date of the release.
            /// </summary>
            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }

            /// <summary>
            /// Gets or sets the publish date of the release.
            /// </summary>
            [JsonProperty("published_at")]
            public DateTime PublishedAt { get; set; }
        }
    }
}
