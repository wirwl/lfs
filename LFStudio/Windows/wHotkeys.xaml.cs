using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LFStudio.wnd
{
    /// <summary>
    /// Interaction logic for wHotkeys.xaml
    /// </summary>
    public partial class wHotkeys : Window
    {
        public wHotkeys()
        {
            InitializeComponent();
            Owner = G.mainWindow;
            teMain.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            teMain.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            teMain.AppendText("TEXT EDITOR:");
            teMain.AppendText(Environment.NewLine);
            teMain.AppendText("Ctrl + '+' - Increase number");
            teMain.AppendText(Environment.NewLine);
            teMain.AppendText("Ctrl + '-' - Decrease number.");
            teMain.AppendText(Environment.NewLine);
            teMain.AppendText("Ctrl + Enter - Open file by cursor.");
            teMain.AppendText(Environment.NewLine);
            teMain.AppendText("Ctrl+Space - Show autocompletion window");
            teMain.AppendText(Environment.NewLine);
            teMain.AppendText("Ctrl+G - Go to line number");
            teMain.AppendText(Environment.NewLine);
            teMain.AppendText(Environment.NewLine);

            teMain.AppendText("COMBOBOXES:");
            teMain.AppendText(Environment.NewLine);
            teMain.AppendText("F9 - Focus to combobox that consist frame numbers");
            teMain.AppendText(Environment.NewLine);
            teMain.AppendText("F10 - Focus to combobox that consist frame captions");
            teMain.AppendText(Environment.NewLine);
            teMain.AppendText("F11 - Focus to combobox that consist list of region captions");
            teMain.AppendText(Environment.NewLine);
            teMain.AppendText("Esc - Change focus from combobox to text editor");
            teMain.AppendText(Environment.NewLine);
            teMain.AppendText("Enter - Show next combobox item");
            teMain.AppendText(Environment.NewLine);
            teMain.AppendText(Environment.NewLine);

            teMain.AppendText("APPLICATION:");
            teMain.AppendText(Environment.NewLine);
            teMain.AppendText("F5 - Run game.");
            teMain.AppendText(Environment.NewLine);
            teMain.AppendText("Ctrl + Tab - show Tools and Documents window.");
            teMain.AppendText(Environment.NewLine);
            teMain.AppendText(Environment.NewLine);

            teMain.AppendText("DEBUG OUTPUT:");
            teMain.AppendText(Environment.NewLine);
            teMain.AppendText("Ctrl + L - Clear All");
            teMain.AppendText(Environment.NewLine);
            teMain.AppendText("L - Delete current line");
        }
    }
}
