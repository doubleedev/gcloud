using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace StoresHub.DataAccess.ORMBase.Translator
{
    class SubtreeEvaluator : ExpressionVisitor
    {
        HashSet<Expression> candidates;

        internal SubtreeEvaluator(HashSet<Expression> candidates)
        {
            this.candidates = candidates;
        }

        internal Expression Eval(Expression exp)
        {
            return this.Visit(exp);
        }

        protected override Expression Visit(Expression exp)
        {
            if (exp == null)
                return null;

            if (candidates.Contains(exp))
                return Evaluate(exp);

            return base.Visit(exp);
        }

        private Expression Evaluate(Expression e)
        {
            if (e.NodeType == ExpressionType.Constant)
                return e;

            LambdaExpression lambda = Expression.Lambda(e);
            Delegate fn = lambda.Compile();

            return Expression.Constant(fn.DynamicInvoke(null), e.Type);
        }

    }
}
