// -------------------------------------------------------------------------------
// <copyright file="FirmwareDownloader.cs" company="Ben Lye">
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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class FirmwareDownloader : Form
    {
        private static Collection<GitHub.Asset> ReleaseAssets { get; set; }

        public FirmwareDownloader(FlashMulti flashMulti)
        {
            this.InitializeComponent();
            this.PopulateReleaseSelector();
            this.UpdateReleaseInfo();
            this.GetReleaseAssets();
            this.UpdateReleaseFiles();

            this.moduleTypeSelector.SelectedIndex = 0;
            this.radioTypeSelector.SelectedIndex = 0;
            this.channelOrderSelector.SelectedIndex = 0;
            this.telemetryInversionSelector.SelectedIndex = 0;
        }

        /// <summary>
        /// Populates the GitHub Release Selector drop-down menu.
        /// </summary>
        private void PopulateReleaseSelector()
        {
            // Get the list of releases from GitHub
            Collection<GitHub.Release> releases = GitHub.GetReleases();

            // A list we will populate and then bind to the control
            List<ReleaseSelectorItem> items = new List<ReleaseSelectorItem>();

            // We only want releases after this date - this was when releases were first built with all the options
            DateTime date = new DateTime(2019, 09, 1, 0, 0, 0);

            // Go through the list of releases we got back from GitHub
            foreach (GitHub.Release release in releases)
            {
                // Only add releases which are newer than our filter
                if (release.PublishedAt >= date)
                {
                    // Create the item
                    ReleaseSelectorItem item = new ReleaseSelectorItem
                    {
                        TagName = release.TagName,
                        DisplayName = release.TagName,
                    };

                    // Add the item to the collection
                    items.Add(item);
                }
            }

            // Bind the list to the control
            this.releaseSelector.DataSource = items;
            this.releaseSelector.DisplayMember = "DisplayName";
            this.releaseSelector.ValueMember = "TagName";

            // Select the first item in the list
            this.releaseSelector.SelectedItem = 0;
        }

        private void UpdateReleaseInfo()
        {
            string selectedRelease = this.releaseSelector.SelectedValue.ToString();
            Debug.WriteLine($"{selectedRelease} is selected");
            string releaseDate = "Unknown";
            foreach (GitHub.Release release in GitHub.GetReleases())
            {
                if (release.TagName == selectedRelease)
                {
                    releaseDate = release.PublishedAt.ToShortDateString();
                }
            }

            this.releaseDate.Text = releaseDate;
        }

        private void GetReleaseAssets()
        {
            ReleaseAssets = GitHub.GetReleaseAssets(this.releaseSelector.SelectedValue.ToString());
        }

        private void UpdateReleaseFiles()
        {
            this.firmwareFileSelector.Items.Clear();
            if (ReleaseAssets != null)
            {
                foreach (GitHub.Asset asset in ReleaseAssets)
                {
                    if (asset.Name.EndsWith(".bin"))
                    {
                        this.firmwareFileSelector.Items.Add(asset.Name);
                    }
                }
            }
        }

        private string ComputeNominalFileName()
        {
            string unknown = "[unknown]";
            string boardType = unknown;
            string radioType = unknown;
            string channelOrder = unknown;

            string releaseTag = this.releaseSelector.SelectedValue.ToString();

            if (this.moduleTypeSelector.SelectedItem != null)
            {
                boardType = this.moduleTypeSelector.SelectedItem.ToString() == "STM32" ? "stm" : this.moduleTypeSelector.SelectedItem.ToString() == "ATmega328p" ? "avr" : this.moduleTypeSelector.SelectedItem.ToString() == "OrangeRX" ? "orx" : unknown;
            }

            if (this.radioTypeSelector.SelectedItem != null)
            {
                radioType = this.radioTypeSelector.SelectedItem.ToString() == "PPM" ? "ppm" : this.radioTypeSelector.SelectedItem.ToString() == "OpenTX" ? "opentx" : this.radioTypeSelector.SelectedItem.ToString() == "erSkyTX" ? "erskytx" : unknown;
            }

            if (this.channelOrderSelector.SelectedItem != null)
            {
                channelOrder = this.channelOrderSelector.SelectedItem.ToString() == "AETR" ? "aetr" : this.channelOrderSelector.SelectedItem.ToString() == "TAER" ? "taer" : this.channelOrderSelector.SelectedItem.ToString() == "RETA" ? "reta" : unknown;
            }

            string telemetryInversion = this.telemetryInversionSelector.SelectedItem.ToString() == "Enabled" ? "inv" : this.telemetryInversionSelector.SelectedItem.ToString() == "Enabled" ? "noinv" : unknown;

            string nominalFileName = $"multi-{boardType}-{radioType}-{channelOrder}-{telemetryInversion}-{releaseTag}.bin";

            Debug.WriteLine(nominalFileName);

            this.firmwareFileSelector.SelectedItem = nominalFileName;

            if (this.firmwareFileSelector.SelectedItem != null && this.firmwareFileSelector.SelectedItem.ToString() != nominalFileName)
            {
                this.firmwareFileSelector.SelectedItem = null;
            }

            return nominalFileName;
        }

        private void releaseSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateReleaseInfo();
            this.GetReleaseAssets();
            this.UpdateReleaseFiles();
        }

        private void moduleTypeSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComputeNominalFileName();
        }

        private void multiTelemetrySelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (radioTypeSelector.SelectedItem != null && radioTypeSelector.SelectedItem.ToString() == "PPM")
            {
                telemetryInversionSelector.SelectedItem = "Disabled";
                telemetryInversionSelector.Enabled = false;
                inversionLabel.Enabled = false;
            }
            else
            {
                telemetryInversionSelector.Enabled = true;
                telemetryInversionSelector.SelectedItem = "*";
                inversionLabel.Enabled = true;
            }
            ComputeNominalFileName();
        }

        private void channelOrderSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComputeNominalFileName();
        }

        private void telemetryInversionSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComputeNominalFileName();
        }

        private void firmwareFileSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*
            Regex fileNameParts = new Regex(@"^multi-([A-z]{3})-([A-z]+)-([A-z]+)-([A-z]+).*$");
            if (firmwareFileSelector.SelectedItem != null)
            {
                Match match = fileNameParts.Match(firmwareFileSelector.SelectedItem.ToString());
                if (match.Success)
                {
                    moduleTypeSelector.SelectedItem = match.Groups[1].Value == "stm" ? "STM32" : "*";
                    radioTypeSelector.SelectedItem = match.Groups[2].Value == "opentx" ? "OpenTX" : match.Groups[2].Value == "erskytx" ? "erSkyTX" : match.Groups[2].Value == "ppm" ? "PPM" : "[Any]";
                    channelOrderSelector.SelectedItem = match.Groups[3].Value == "aetr" ? "AETR" : match.Groups[3].Value == "taer" ? "TAER" : match.Groups[3].Value == "reta" ? "RETA" : "[Any]";
                    telemetryInversionSelector.SelectedItem = match.Groups[4].Value == "inv" ? "Enabled" : match.Groups[4].Value == "noinv" ? "Disabled" : "[Any]";
                }
            }
            */
        }

        /// <summary>
        /// Opens a URL in the default browser.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        public void OpenLink(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Handles the Github release notes link being clicked.
        /// </summary>
        private void ReleaseNotesLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.OpenLink("https://github.com/pascallanger/DIY-Multiprotocol-TX-Module/releases/tag/" + this.releaseSelector.SelectedValue.ToString());
        }

        /// <summary>
        /// Class for the items in the GitHub release selector dropdown listbox.
        /// </summary>
        public class ReleaseSelectorItem
        {
            /// <summary>
            /// Gets or sets the GitHub tag name.
            /// Used as the value of the item in the list.
            /// </summary>
            public string TagName { get; set; }

            /// <summary>
            /// Gets or sets the display name for the list item.
            /// </summary>
            public string DisplayName { get; set; }
        }

    }
}
