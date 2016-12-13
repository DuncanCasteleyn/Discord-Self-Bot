/*
This file is part of Discord self bot.

Discord self bot is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Discord self bot is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Discord self bot.  If not, see<http://www.gnu.org/licenses/>.
*/

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Data.SQLite;
using Discord;
using DiscordSelfBot.Properties;

namespace DiscordSelfBot
{
    internal sealed partial class LoginForm : Form
    {
        private bool _exceptionThrown;

        /// <summary>
        ///     Intializes the form
        /// </summary>
        public LoginForm()
        {
            InitializeComponent();
            try
            {
                using (var conn = new SQLiteConnection($@"Data Source={System.Environment.GetEnvironmentVariable("appdata")}\discord\Local Storage\https_discordapp.com_0.localstorage;Version=3;"))
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = @"SELECT key, value FROM ItemTable where key = ""token""";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tokenBox.Text = reader.GetString(reader.GetOrdinal("value")).Trim('"');
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show(Resources.LoginForm_Retrieving_token_failed, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        ///     This method is called when the connect button is pressed, if the connection was successfull the application will
        ///     move itself to the background.
        /// </summary>
        /// <param name="sender">the object the called this method</param>
        /// <param name="e">event arguments</param>
        private void connectButton_Click(object sender, EventArgs e)
        {
            connectButton.Enabled = false;
            Update();
            var selfBot = new SelfBot(tokenBox.Text, this);
            var discordClient = selfBot.Start();
            uint count = 0;
            connectButton.TextAlign = ContentAlignment.MiddleLeft;
            do
            {
                connectButton.Text = Resources.LoginForm_connectButton_Click_Connecting_waitState1;
                Update();
                Thread.Sleep(333);
                connectButton.Text = Resources.LoginForm_connectButton_Click_Connecting_waitState2;
                Update();
                Thread.Sleep(333);
                connectButton.Text = Resources.LoginForm_connectButton_Click_Connecting_waitState3;
                Update();
                Thread.Sleep(334);
                count++;
            } while ((count < 5) || ((discordClient.ConnectionState == ConnectionState.Connecting) && (count < 15)));
            //only check connectionstate after 5 seconds for slower computers/networks

            if (discordClient.ConnectionState == ConnectionState.Connected)
            {
                Hide();
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(500);
                MessageBox.Show(Resources.LoginForm_loginOk, Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else if (_exceptionThrown)
            {
                ConnectionFailed(selfBot);
                _exceptionThrown = false;
            }
            else
            {
                ConnectionFailed(selfBot);
                MessageBox.Show(Resources.LoginForm_loginNotOk, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        ///     Clean up the client that is given as parameter
        /// </summary>
        /// <param name="selfBot">Client to clean</param>
        private void ConnectionFailed(SelfBot selfBot)
        {
            selfBot.Dispose();
            connectButton.Text = Resources.LoginForm_connectButton_Click_Connect;
            connectButton.TextAlign = ContentAlignment.MiddleCenter;
            connectButton.Enabled = true;
        }

        /// <summary>
        ///     This method closes the application and cleans up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Dispose();
        }

        /// <summary>
        ///     Exceptions from everything that is run from this form should be thrown here so the user is informed.
        /// </summary>
        /// <param name="e">An exception to notify the user about.</param>
        public void ExceptionNotifier(Exception e)
        {
            _exceptionThrown = true;
            MessageBox.Show(e.GetType().Name + @": " + e.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
}