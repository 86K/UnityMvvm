

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fusion.Mvvm
{
    public class ExpressionPathFinder : IExpressionPathFinder
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(ExpressionPathFinder));

        public List<Path> FindPaths(LambdaExpression expression)
        {
            PathExpressionVisitor visitor = new PathExpressionVisitor();
            visitor.Visit(expression);
            return visitor.Paths;
        }
    }
}
