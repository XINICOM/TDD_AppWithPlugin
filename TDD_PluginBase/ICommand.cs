namespace TDD_PluginBase
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        int Execute(IAppContent content);
    }
}
