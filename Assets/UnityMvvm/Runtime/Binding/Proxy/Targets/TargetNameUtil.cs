namespace Fusion.Mvvm
{
    public static class TargetNameUtil
    {
        public static bool IsCollection(string targetName)
        {
            return targetName.IndexOf('[') >= 0;
        }
    }
}
