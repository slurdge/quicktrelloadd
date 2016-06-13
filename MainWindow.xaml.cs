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
using Manatee.Trello;

namespace AddToTrello
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public IEnumerable<Board> boards;
        public Board currentBoard;

        Dictionary<Manatee.Trello.LabelColor, System.Windows.Media.Color> colors;

        public MainWindow()
        {
            colors = new Dictionary<Manatee.Trello.LabelColor, System.Windows.Media.Color>() {
                { Manatee.Trello.LabelColor.Green, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#61BD4F")},
                { Manatee.Trello.LabelColor.Yellow, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#F2D600")},
                { Manatee.Trello.LabelColor.Orange, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#FFAB4A")},
                { Manatee.Trello.LabelColor.Red, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#EB5A46")},
                { Manatee.Trello.LabelColor.Purple, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#C377E0")},
                { Manatee.Trello.LabelColor.Blue, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#0079BF")},
                { Manatee.Trello.LabelColor.Sky, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#00C2E0")},
                { Manatee.Trello.LabelColor.Lime, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#51E898")},
                { Manatee.Trello.LabelColor.Pink, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF80CE")},
                { Manatee.Trello.LabelColor.Black, (System.Windows.Media.Color)ColorConverter.ConvertFromString("#000000")}
            };

            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ListBoxItem selectItem = null;
            foreach (var board in boards.Where(x => (x.IsClosed == false)))
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
            this.LabelStackPanel.Children.Clear();
            int i = 0;
            foreach (var label in currentBoard.Labels.Where(l => l.Color != null).OrderBy((l => l.Color.GetValueOrDefault())))
            {
                var newBtn = new System.Windows.Controls.Primitives.ToggleButton();
                newBtn.Content = label.Name;
                newBtn.Name = "Button" + i.ToString();
                ++i;

                newBtn.Foreground = Brushes.White;
                newBtn.FontWeight = FontWeights.Bold;
                newBtn.Margin = new Thickness(0, 10, 0, 0);
                newBtn.Background = new SolidColorBrush(colors[label.Color.GetValueOrDefault()]);
                newBtn.Checked += new RoutedEventHandler(label_Checked);
                newBtn.Unchecked += new RoutedEventHandler(label_Unchecked);
                newBtn.Tag = label;
                newBtn.Style = (Style)this.Resources["FlatButtonStyle"];
                this.LabelStackPanel.Children.Add(newBtn);
            }
        }

        private void label_Unchecked(object sender, RoutedEventArgs e)
        {
            var t = sender as System.Windows.Controls.Primitives.ToggleButton;
            t.Content = ((Manatee.Trello.Label)t.Tag).Name;
        }

        private void label_Checked(object sender, RoutedEventArgs e)
        {
            var t = sender as System.Windows.Controls.Primitives.ToggleButton;
            t.Content = "✓ " + ((Manatee.Trello.Label)t.Tag).Name;
        }

        public void AddTask(Object sender, ExecutedRoutedEventArgs e)
        {
            this.Hide();
            if (this.TitleTextBox.Text.Length == 0)
                return;
            var board = (this.BoardsList.SelectedItem as ListBoxItem).Tag as Board;
            var lists = board.Lists;
            var newCard = lists.First().Cards.Add(this.TitleTextBox.Text);
            newCard.Description = DescTextBox.Text;
            foreach (System.Windows.Controls.Primitives.ToggleButton button in this.LabelStackPanel.Children)
            {
                if (button.IsChecked == true)
                {
                    newCard.Labels.Add(((Manatee.Trello.Label)button.Tag));
                }
            }
            this.Close();
        }

        private void BoardsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            var li = cb.SelectedItem as ListBoxItem;
            currentBoard = li.Tag as Board;
            refreshLabels();
        }

    }
}
