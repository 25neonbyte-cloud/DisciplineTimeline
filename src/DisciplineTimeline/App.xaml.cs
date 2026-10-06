using System.Windows;
using DisciplineTimeline.Services;

namespace DisciplineTimeline;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var database = new DatabaseService();
            await database.InitializeAsync();

            var window = new MainWindow(database);
            MainWindow = window;
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Falha ao inicializar o Discipline Timeline.\n\n{ex.Message}",
                "Discipline Timeline",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(1);
        }
    }
}
