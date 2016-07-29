namespace CustomBindingDemo.BaseModels
{
    public interface IRoutedRequest<out TRoute>
    {
        TRoute Route { get; }
    }
}