using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;

namespace Tools365.Technology.DbConnectionStringGenerator;

public sealed partial class DbConnectionStringGeneratorWindow : Window
{
    private static readonly int[] DefaultPorts = [1433, 5432, 3306, 0];

    public DbConnectionStringGeneratorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(680, 900));
        ProviderComboBox.SelectionChanged += ProviderComboBox_SelectionChanged;
        AuthenticationComboBox.SelectionChanged += AuthenticationComboBox_SelectionChanged;
        UpdateProviderFields();
        ServerBox.TextChanged += SettingsChanged;
        DatabaseBox.TextChanged += SettingsChanged;
        UsernameBox.TextChanged += SettingsChanged;
        SqliteFileBox.TextChanged += SettingsChanged;
        PasswordBox.PasswordChanged += SettingsChanged;
        PortBox.ValueChanged += SettingsChanged;
        PoolingSwitch.Toggled += SettingsChanged;
    }

    private void ProviderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateProviderFields();
        ClearResult();
    }

    private void AuthenticationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CredentialsPanel.Visibility = AuthenticationComboBox.SelectedIndex == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
        ClearResult();
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetProvider(out var provider))
        {
            ShowValidation("Choose a supported database provider.");
            return;
        }

        var result = ConnectionStringFactory.Build(
            provider,
            ServerBox.Text,
            PortBox.Value,
            DatabaseBox.Text,
            UsernameBox.Text,
            PasswordBox.Password,
            SqliteFileBox.Text,
            provider == DatabaseProvider.SqlServer && AuthenticationComboBox.SelectedIndex == 1,
            PoolingSwitch.IsOn);

        if (!result.IsValid)
        {
            ShowValidation(result.ErrorMessage);
            return;
        }

        ValidationBar.IsOpen = false;
        ConnectionStringBox.Text = result.ConnectionString;
        CopyButton.IsEnabled = true;
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        var package = new DataPackage();
        package.SetText(ConnectionStringBox.Text);
        Clipboard.SetContent(package);
    }

    private void UpdateProviderFields()
    {
        if (!TryGetProvider(out var provider))
        {
            return;
        }

        var isSqlite = provider == DatabaseProvider.Sqlite;
        var isSqlServer = provider == DatabaseProvider.SqlServer;
        ServerFields.Visibility = isSqlite ? Visibility.Collapsed : Visibility.Visible;
        DatabasePanel.Visibility = isSqlite ? Visibility.Collapsed : Visibility.Visible;
        SqliteFilePanel.Visibility = isSqlite ? Visibility.Visible : Visibility.Collapsed;
        AuthenticationPanel.Visibility = isSqlite ? Visibility.Collapsed : Visibility.Visible;
        AuthenticationLabel.Visibility = isSqlServer ? Visibility.Visible : Visibility.Collapsed;
        AuthenticationComboBox.Visibility = isSqlServer ? Visibility.Visible : Visibility.Collapsed;
        ServerLabel.Text = provider switch
        {
            DatabaseProvider.PostgreSql => "Host",
            DatabaseProvider.SqlServer => "Server",
            _ => "Server"
        };

        if (isSqlite)
        {
            return;
        }

        PortBox.Value = DefaultPorts[(int)provider];

        if (!isSqlServer)
        {
            AuthenticationComboBox.SelectedIndex = 0;
        }

        CredentialsPanel.Visibility = AuthenticationComboBox.SelectedIndex == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void ClearResult()
    {
        ConnectionStringBox.Text = string.Empty;
        CopyButton.IsEnabled = false;
        ValidationBar.IsOpen = false;
    }

    private void SettingsChanged(object sender, TextChangedEventArgs e) => ClearResult();

    private void SettingsChanged(object sender, RoutedEventArgs e) => ClearResult();

    private void SettingsChanged(NumberBox sender, NumberBoxValueChangedEventArgs args) => ClearResult();

    private void ShowValidation(string message)
    {
        ConnectionStringBox.Text = string.Empty;
        CopyButton.IsEnabled = false;
        ValidationBar.Title = "Check the connection settings";
        ValidationBar.Message = message;
        ValidationBar.IsOpen = true;
    }

    private bool TryGetProvider(out DatabaseProvider provider)
    {
        provider = ProviderComboBox.SelectedIndex switch
        {
            0 => DatabaseProvider.SqlServer,
            1 => DatabaseProvider.PostgreSql,
            2 => DatabaseProvider.MySql,
            3 => DatabaseProvider.Sqlite,
            _ => (DatabaseProvider)(-1)
        };

        return Enum.IsDefined(provider);
    }
}
