using System.Windows.Controls;
using System.Windows.Input;

namespace vrchat_osc;

public partial class OneStringTextBox
{
    public OneStringTextBox(int index, string text, TextChangedEventHandler  handler)
    {
        InitializeComponent();

        InputBox.TextChanged += handler;
        InputBox.Text = text;
        Counter.Text = index.ToString();
    }

    public string GetText()
    {
        return !string.IsNullOrEmpty(InputBox.Text) ? InputBox.Text : "";
    }

    public void SetText(string text)
    {
        InputBox.Text = string.IsNullOrEmpty(text) ? "" : text;
    }
}