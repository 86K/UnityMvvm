using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fusion.Mvvm
{
    public class ExpressionPathFinder : IExpressionPathFinder
    {
        public List<Path> FindPaths(LambdaExpression expression)
        {
            PathExpressionVisitor visitor = new PathExpressionVisitor();
            visitor.Visit(expression);
            return visitor.Paths;
        }
    }
}
