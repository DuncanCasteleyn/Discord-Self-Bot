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
along with Foobar.  If not, see<http://www.gnu.org/licenses/>.
*/

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Discord;
using Discord.WebSocket;
using DiscordSelfBot.Properties;

namespace DiscordSelfBot
{
    public partial class LoginForm : Form
    {
        /// <summary>
        /// Intializes the form
        /// </summary>
        public LoginForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method is called when the connect button is pressed, if the connection was successfull the application will move itself to the background.
        /// </summary>
        /// <param name="sender">the object the called this method</param>
        /// <param name="e">event arguments</param>
        private void connectButton_Click(object sender, System.EventArgs e)
        {
            connectButton.Enabled = false;
            this.Update();
            SelfBot selfBot = new SelfBot(tokenBox.Text, this);
            DiscordSocketClient discordClient = selfBot.Start();
            uint count = 0;
            connectButton.TextAlign = ContentAlignment.MiddleLeft;
            do
            {
                connectButton.Text = Resources.LoginForm_connectButton_Click_Connecting_waitState1;
                this.Update();
                Thread.Sleep(333);
                connectButton.Text = Resources.LoginForm_connectButton_Click_Connecting_waitState2;
                this.Update();
                Thread.Sleep(333);
                connectButton.Text = Resources.LoginForm_connectButton_Click_Connecting_waitState3;
                this.Update();
                Thread.Sleep(334);
                count++;
            } while (discordClient.ConnectionState == ConnectionState.Connecting && count < 15);
            if (discordClient.ConnectionState == ConnectionState.Connected)
            {
                this.Hide();
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(500);
                MessageBox.Show(Resources.LoginForm_loginOk, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                selfBot.Stop();
                connectButton.Text = Resources.LoginForm_connectButton_Click_Connect;
                connectButton.TextAlign = ContentAlignment.MiddleCenter;
                connectButton.Enabled = true;
                MessageBox.Show(Resources.LoginForm_loginNotOk, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// This method closes the application and cleans up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            notifyIcon.Visible = false;
            this.Dispose();
        }

        /// <summary>
        /// Exceptions from everything that is run from this form should be thrown here so the user is informed.
        /// </summary>
        /// <param name="e">An exception to notify the user about.</param>
        internal void ExceptionNotifier(Exception e)
        {
            MessageBox.Show(e.GetType().Name + @": " + e.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
}
