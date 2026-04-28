using System.Windows;
using System.Windows.Input;

namespace vrchat_osc;

public partial class NotificationWindow
{
    private Action OnOkButtonClick { get; }
    private Action OnCancelButtonClick { get; }

    public NotificationWindow(string title, string message, Action ok, Action cancel)
    {
        InitializeComponent();
        TitleTextBlock.Text = title;
        DescTextField.Text = message;
        OnOkButtonClick = ok;
        OnCancelButtonClick = cancel;
    }

    private void TopBar_MouseDown(object sender, MouseButtonEventArgs e) => this.DragMove();

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        OnOkButtonClick.Invoke();
        Close();
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        OnCancelButtonClick.Invoke();
        Close();
    }
}