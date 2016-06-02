using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NHotkey.Wpf;
using TrelloNet;


namespace QuickTrelloAdd
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private System.Windows.Forms.NotifyIcon notifyIcon = null;
        private TrelloNet.Board currentBoard = null;
        private List<TrelloNet.Board> boards = null;
        private TrelloNet.Trello trello = null;
        App()
        {
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            trello = new Trello(QuickTrelloAdd.Properties.Resources.API_KEY);
            if (QuickTrelloAdd.Properties.Settings.Default.TrelloAuthToken == "")
            {
                var uri = trello.GetAuthorizationUrl("QuickTrelloAdd", Scope.ReadWriteAccount, Expiration.Never);
                var authform = new AuthWindow();
                authform.AuthURI = uri.ToString();
                authform.ShowDialog();
                if (authform.DialogResult == false || authform.AuthToken == null || authform.AuthToken.Trim() == "")
                {
                    Current.Shutdown();
                    return;
                }
                QuickTrelloAdd.Properties.Settings.Default.TrelloAuthToken = authform.AuthToken.Trim();
                QuickTrelloAdd.Properties.Settings.Default.Save();
            }
            trello.Authorize(QuickTrelloAdd.Properties.Settings.Default.TrelloAuthToken);
            boards = trello.Boards.ForMe(BoardFilter.Open).ToList();

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Click += new EventHandler(notifyIcon_Click);
            notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
            notifyIcon.Icon = QuickTrelloAdd.Properties.Resources.trello_mark_blue;
            notifyIcon.Visible = true;
            List<System.Windows.Forms.MenuItem> items = new List<System.Windows.Forms.MenuItem>();
            foreach (var board in boards)
            {
                var mi = new System.Windows.Forms.MenuItem(board.Name, new EventHandler(onBoardClick));
                mi.Tag = board;
                if (board.Id == QuickTrelloAdd.Properties.Settings.Default.BoardId)
                {
                    currentBoard = board;
                    mi.Checked = true;
                }
                items.Add(mi);
            }
            if (currentBoard == null)
            { 
                currentBoard = boards.First();
                items.First().Checked = true;
            }

            items.Add(new System.Windows.Forms.MenuItem("-"));
            items.Add(new System.Windows.Forms.MenuItem("Create task", new EventHandler(showMainTrello)));
            items.Add(new System.Windows.Forms.MenuItem("-"));
            items.Add(new System.Windows.Forms.MenuItem("Quit", new EventHandler(onQuit)));
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(items.ToArray());
            try
            {
                HotkeyManager.Current.AddOrReplace("Increment", System.Windows.Input.Key.Add, System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Alt, new EventHandler<NHotkey.HotkeyEventArgs>(showMainTrello));
            }
            catch (NHotkey.HotkeyAlreadyRegisteredException )
            {

               }
        }

        private void onQuit(object sender, EventArgs e)
        {
            Current.Shutdown();
        }

        private void showMainTrello(object sender, EventArgs e)
        {
            var main = new MainWindow();
            main.boards = boards;
            main.currentBoard = currentBoard;
            main.trello = trello;
            main.ShowDialog();
            currentBoard = main.currentBoard;
        }

        private void onBoardClick(object sender, EventArgs e)
        {
            foreach (System.Windows.Forms.MenuItem mi in notifyIcon.ContextMenu.MenuItems)
                mi.Checked = false;
            var mis = sender as System.Windows.Forms.MenuItem;
            currentBoard = mis.Tag as TrelloNet.Board;
            QuickTrelloAdd.Properties.Settings.Default.BoardId = currentBoard.Id;
            QuickTrelloAdd.Properties.Settings.Default.Save();
            notifyIcon.ContextMenu.MenuItems[mis.Index].Checked = true;
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            notifyIcon_Click(sender, e);
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
          //  throw new NotImplementedException();
        }
    }
}
