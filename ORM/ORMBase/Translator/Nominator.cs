using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace StoresHub.DataAccess.ORMBase.Translator
{
    class Nominator : ExpressionVisitor
    {
        Func<Expression, bool> fnCanBeEvaluated;
        HashSet<Expression> candidates;
        bool cannotBeEvaluated;

        internal Nominator(Func<Expression, bool> fnCanBeEvaluated)
        {
            this.fnCanBeEvaluated = fnCanBeEvaluated;
        }

        internal HashSet<Expression> Nominate(Expression expression)
        {
            candidates = new HashSet<Expression>();
            Visit(expression);

            return this.candidates;
        }

        protected override Expression Visit(Expression expression)
        {
            if (expression != null)
            {
                bool saveCannotBeEvaluated = cannotBeEvaluated;
                cannotBeEvaluated = false;
                base.Visit(expression);

                if (!this.cannotBeEvaluated)
                {
                    if (this.fnCanBeEvaluated(expression))
                        this.candidates.Add(expression);
                    else
                        this.cannotBeEvaluated = true;
                }

                this.cannotBeEvaluated |= saveCannotBeEvaluated;
            }

            return expression;
        }
    }
}
