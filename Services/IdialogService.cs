using System.Threading.Tasks;

public interface IDialogService
{
    Task<string?> ShowInputDialogAsync(string title, string message);
}