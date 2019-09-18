// -------------------------------------------------------------------------------
// <copyright file="RunCommand.cs" company="Ben Lye">
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
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Class for running commands.
    /// </summary>
    internal class RunCommand
    {
        /// <summary>
        /// Runs a command with arguments.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> class.</param>
        /// <param name="command">The command to run.</param>
        /// <param name="args">Arguments for the command.</param>
        /// <returns>The exit code returned by the command.</returns>
        public static int Run(FlashMulti flashMulti, string command, string args)
        {
            Debug.WriteLine("\n" + command + " " + args + "\n");
            flashMulti.AppendVerbose(command + " " + args + "\r\n");

            // Check if the file exists
            if (!File.Exists(command))
            {
                flashMulti.AppendVerbose(string.Format("{0} does not exist", command));
                return -1;
            }

            Process myProcess = new Process();

            // Process process = new Process();
            myProcess.StartInfo.FileName = command;
            myProcess.StartInfo.Arguments = args;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.StartInfo.RedirectStandardError = true;
            myProcess.StartInfo.CreateNoWindow = true;

            // Hande error output asynchronously
            myProcess.ErrorDataReceived += new DataReceivedEventHandler(flashMulti.OutputHandler);

            // Start process and handlers
            myProcess.Start();

            // Read the error output asynchronously, handle it by line
            myProcess.BeginErrorReadLine();

            // Read the standard output synchronously, handle it character-by-character
            while (!myProcess.StandardOutput.EndOfStream)
            {
                var data = myProcess.StandardOutput.Read();
                flashMulti.CharOutputHandler((char)data);
            }

            // Loop until the process finishes
            myProcess.WaitForExit();

            // Return the exit code from the process
            return myProcess.ExitCode;
        }
    }
}
