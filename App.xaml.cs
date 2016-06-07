using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NHotkey.Wpf;

using Manatee.Trello;
using Manatee.Trello.ManateeJson;

namespace QuickTrelloAdd
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private System.Windows.Forms.NotifyIcon notifyIcon = null;

        private Manatee.Trello.Board currentBoard = null;
        private IEnumerable<Board> boards = null;

        App()
        {
        }

        protected string getAuthURL(string key, string name)
        {
            return string.Format("https://trello.com/1/authorize?key={0}&name={1}&expiration=never&response_type=token&scope=read,write", key, name);
        }

        protected bool showOptionWindow()
        {
            var uri = getAuthURL(QuickTrelloAdd.Properties.Resources.API_KEY, App.ResourceAssembly.GetName().Name);

            var authform = new OptionWindow();
            authform.AuthToken = QuickTrelloAdd.Properties.Settings.Default.TrelloAuthToken;
            authform.KeyboardShortcut = QuickTrelloAdd.Properties.Settings.Default.KeyboardShortcut;
            authform.AuthURI = uri.ToString();
            authform.ShowDialog();
            if (authform.DialogResult == false || authform.AuthToken == null || authform.AuthToken.Trim() == "")
            {
                return false;
            }
            QuickTrelloAdd.Properties.Settings.Default.KeyboardShortcut = authform.KeyboardShortcut;
            QuickTrelloAdd.Properties.Settings.Default.TrelloAuthToken = authform.AuthToken.Trim();
            QuickTrelloAdd.Properties.Settings.Default.Save();

            return true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var serializer = new ManateeSerializer();
            TrelloConfiguration.Serializer = serializer;
            TrelloConfiguration.Deserializer = serializer;
            TrelloConfiguration.JsonFactory = new ManateeFactory();
            TrelloConfiguration.RestClientProvider = new Manatee.Trello.RestSharp.RestSharpClientProvider();

            if (QuickTrelloAdd.Properties.Settings.Default.TrelloAuthToken == "")
            {
                if (!showOptionWindow())
                {
                    Current.Shutdown();
                    return;
                }
            }
            TrelloAuthorization.Default.AppKey = QuickTrelloAdd.Properties.Resources.API_KEY;
            TrelloAuthorization.Default.UserToken = QuickTrelloAdd.Properties.Settings.Default.TrelloAuthToken;

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = QuickTrelloAdd.Properties.Resources.trello_mark_blue;
            notifyIcon.Visible = true;
            List<System.Windows.Forms.MenuItem> items = new List<System.Windows.Forms.MenuItem>();

            boards = Me.Me.Boards.Where(x => (x.IsClosed == false));

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
            items.Add(new System.Windows.Forms.MenuItem("Options", new EventHandler(onOptions)));
            items.Add(new System.Windows.Forms.MenuItem("Quit", new EventHandler(onQuit)));
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(items.ToArray());
            registerHotkey();
        }

        private void registerHotkey()
        {
            var stringkeys = QuickTrelloAdd.Properties.Settings.Default.KeyboardShortcut.Split('+');
            System.Windows.Input.ModifierKeys mod = System.Windows.Input.ModifierKeys.None;
            System.Windows.Input.Key key = System.Windows.Input.Key.None;
            foreach (var stringkey in stringkeys)
            {
                System.Windows.Input.ModifierKeys currentModifier;
                var ismod = Enum.TryParse(stringkey, out currentModifier);
                if (ismod)
                    mod |= currentModifier;
                Enum.TryParse(stringkey, out key);
            }
            try
            {
                HotkeyManager.Current.AddOrReplace("AddTask", key, mod, new EventHandler<NHotkey.HotkeyEventArgs>(showMainTrello));
            }
            catch (NHotkey.HotkeyAlreadyRegisteredException)
            {
            }
        }

        private void onOptions(object sender, EventArgs e)
        {
            showOptionWindow();
            registerHotkey();
        }

        private void onQuit(object sender, EventArgs e)
        {
            Current.Shutdown();
        }

        private void refreshSelectedBoard()
        {
            foreach (var item in notifyIcon.ContextMenu.MenuItems)
            {
                var menuItem = item as System.Windows.Forms.MenuItem;
                if (menuItem.Tag == currentBoard)
                    menuItem.Checked = true;
                else
                    menuItem.Checked = false;
            }
            QuickTrelloAdd.Properties.Settings.Default.BoardId = currentBoard.Id;
            QuickTrelloAdd.Properties.Settings.Default.Save();
        }

        private void showMainTrello(object sender, EventArgs e)
        {
            
            var main = new MainWindow();
            main.boards = boards;
            main.currentBoard = currentBoard;
            var oldSelectedBoard = currentBoard;
            main.ShowDialog();
            currentBoard = main.currentBoard;
            if (oldSelectedBoard != currentBoard)
            {
                refreshSelectedBoard();
            }
        }

        private void onBoardClick(object sender, EventArgs e)
        {
            var mis = sender as System.Windows.Forms.MenuItem;
            currentBoard = mis.Tag as Board;
            refreshSelectedBoard();
        }

    }
}
