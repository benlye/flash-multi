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
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class FirmwareDownloader : Form
    {
        public FirmwareDownloader(FlashMulti flashMulti)
        {
            this.InitializeComponent();

            this.PopulateReleaseSelector();

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

        private void releaseSelector_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
