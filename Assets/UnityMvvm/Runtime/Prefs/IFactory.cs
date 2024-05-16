

namespace Fusion.Mvvm
{
    /// <summary>
    /// IFactory
    /// </summary>
    public interface IFactory
    {
        /// <summary>
        /// Create an instance of the preferences.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Preferences Create(string name);
    }
}
