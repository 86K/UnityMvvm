

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fusion.Mvvm
{
    public interface IExpressionPathFinder
    {
        List<Path> FindPaths(LambdaExpression expression);
        
    }
}
