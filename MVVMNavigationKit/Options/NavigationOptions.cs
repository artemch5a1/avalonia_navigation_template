
namespace MvvmNavigationKit.Options
{
    public class NavigationOptions
    {
        public int MaxSizeHistory { get; set; } = int.MaxValue;

        public Dictionary<Type, Type> KeyViewModel { get; set; } = new();
    }
}
