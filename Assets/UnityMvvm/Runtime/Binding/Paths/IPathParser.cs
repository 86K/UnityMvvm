using System.Linq.Expressions;

namespace Fusion.Mvvm
{
    public interface IPathParser
    {
        /// <summary>
        /// Parser object instance path.eg:vm => vm.User.Username
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        Path Parse(LambdaExpression expression);

        /// <summary>
        /// Parser text path.eg:User.Username
        /// </summary>
        /// <param name="pathText"></param>
        /// <returns></returns>
        Path Parse(string pathText);

        /// <summary>
        /// Parser target name.eg:vm => vm.User
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        string ParseMemberName(LambdaExpression expression);
    }
}
