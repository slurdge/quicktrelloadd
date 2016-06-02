using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrelloNet;

namespace QuickTrelloAdd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public List<TrelloNet.Board> boards;
        public TrelloNet.Board currentBoard;
        public TrelloNet.Trello trello;

        Dictionary<TrelloNet.Color, System.Windows.Media.Color> colors;

        public MainWindow()
        {
            colors = new Dictionary<TrelloNet.Color, System.Windows.Media.Color>() {
                { TrelloNet.Color.Green, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#61BD4F")},
                { TrelloNet.Color.Yellow, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#F2D600")},
                { TrelloNet.Color.Orange, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#FFAB4A")},
                { TrelloNet.Color.Red, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#EB5A46")},
                { TrelloNet.Color.Purple, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#C377E0")},
                { TrelloNet.Color.Blue, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#0079BF")},
                { TrelloNet.Color.Sky, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#00C2E0")},
                { TrelloNet.Color.Lime, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#51E898")},
                { TrelloNet.Color.Pink, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF80CE")},
                { TrelloNet.Color.Black, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#000000")}
            };

            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ListBoxItem selectItem = null;
            foreach (var board in boards)
            {
                var item = new ListBoxItem();
                item.Content = board.Name;
                if (board.Id == currentBoard.Id)
                    selectItem = item;
                item.Tag = board;
                this.BoardsList.Items.Add(item);
            }
            this.BoardsList.SelectedItem = selectItem;
            this.TitleTextBox.Focus();

            refreshLabels();
        }

        private void refreshLabels()
        {
            this.LabelStackPanel.Children.RemoveRange(0, colors.Keys.Count);
            foreach (var color in colors.Keys)
            {
                var newBtn = new System.Windows.Controls.Primitives.ToggleButton();
                newBtn.Content = currentBoard.LabelNames[color];
                newBtn.Name = "Button" + color.ToString();
                

                newBtn.Margin = new Thickness(0, 10, 0, 0);
                newBtn.Background = new SolidColorBrush(colors[color]);
                newBtn.Checked += new RoutedEventHandler(label_Checked);
                newBtn.Unchecked += new RoutedEventHandler(label_Unchecked);
                newBtn.Tag = color;
                this.LabelStackPanel.Children.Add(newBtn);
            }
        }

        private void label_Unchecked(object sender, RoutedEventArgs e)
        {
            var t = sender as System.Windows.Controls.Primitives.ToggleButton;
            t.Content = currentBoard.LabelNames[(TrelloNet.Color) t.Tag];
        }

        private void label_Checked(object sender, RoutedEventArgs e)
        {
            var t = sender as System.Windows.Controls.Primitives.ToggleButton;
            t.Content = "✓ " + currentBoard.LabelNames[(TrelloNet.Color)t.Tag];
        }

        public void AddTask(Object sender, ExecutedRoutedEventArgs e)
        {
            this.Hide();
            if (this.TitleTextBox.Text.Length == 0)
                return;
            var lists = trello.Lists.ForBoard(new BoardId(((this.BoardsList.SelectedItem as ListBoxItem).Tag as TrelloNet.Board).GetBoardId()));
            var newCard = trello.Cards.Add(new NewCard(this.TitleTextBox.Text, lists.First()));
            newCard.Desc = DescTextBox.Text;
            foreach (System.Windows.Controls.Primitives.ToggleButton button in this.LabelStackPanel.Children)
            {
                if (button.IsChecked == true)
                {
                    trello.Cards.AddLabel(newCard, (TrelloNet.Color)button.Tag);
                }
            }
            this.Close();
        }

        private void BoardsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            var li = cb.SelectedItem as ListBoxItem;
            currentBoard = li.Tag as TrelloNet.Board;
            refreshLabels();
        }

    }
}
