using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace StoresHub.DataAccess.ORMBase.Translator
{
    internal class QueryTranslator : ExpressionVisitor
    {
        StringBuilder builder;

        internal QueryTranslator()
        {
        }

        internal string Translate<T>(Expression expression)
        {
            builder = new StringBuilder();
            expression = Evaluator.PartialEval(expression);
            Visit(expression);
            return builder.ToString().Replace("= NULL", "IS NULL");
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
                e = ((UnaryExpression)e).Operand;

            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable))
            {
                if (m.Method.Name == "Where")
                {
                    builder.Append("SELECT * FROM (");
                    Visit(m.Arguments[0]);
                    builder.Append(") AS T WHERE ");
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    Visit(lambda.Body);

                    return m;
                }
            }

            if (m.Method.DeclaringType == typeof(string))
            {
                string left = m.Object.ToString();

                switch (m.Method.Name)
                {
                    case "Contains":
                        builder.Append(left);
                        builder.Append(" LIKE('%");
                        builder.Append(m.Arguments[0].ToString().Replace("\"", ""));
                        builder.Append("%') ");

                        return m;
                    case "Equals":
                        builder.Append(left);
                        builder.Append("='");
                        builder.Append(m.Arguments[0].ToString().Replace("\"", ""));
                        builder.Append("' ");

                        return m;
                    case "StartsWith":
                        builder.Append(left);
                        builder.Append(" LIKE('");
                        builder.Append(m.Arguments[0].ToString().Replace("\"", ""));
                        builder.Append("%') ");

                        return m;
                    case "EndsWith":
                        builder.Append(left);
                        builder.Append(" LIKE('%");
                        builder.Append(m.Arguments[0].ToString().Replace("\"", ""));
                        builder.Append("') ");

                        return m;

                    case "ToLower":
                        builder.Append("LOWER(");
                        builder.Append(left);
                        builder.Append(") ");

                        return m;
                    default:
                        throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
                }
            }

            return m;
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    builder.Append(" NOT ");
                    Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported");
            }

            return u;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            builder.Append("(");

            switch (b.NodeType)
            {
                case ExpressionType.ArrayIndex:
                    Visit(b.Left);
                    builder.Append("[");
                    Visit(b.Right);
                    builder.Append("] ");
                    break;
                case ExpressionType.Coalesce:
                    builder.Append(" COALESCE(");
                    Visit(b.Left);
                    builder.Append(", ");
                    Visit(b.Right);
                    builder.Append(") ");
                    break;
                case ExpressionType.LeftShift:
                    // From: https://www.sqlservercentral.com/scripts/integer-left-and-right-shift
                    builder.Append(" SELECT y = CAST(");
                    Visit(b.Left);
                    builder.Append(" * p.pow AS BINARY(4)) AS INT) FROM ( SELECT pow = POWER(CAST(2 AS BIGINT), ");
                    Visit(b.Right);
                    builder.Append(" & 0x1F)) p");
                    break;
                case ExpressionType.Power:
                    builder.Append(" POWER( ");
                    Visit(b.Left);
                    builder.Append(", ");
                    Visit(b.Right);
                    builder.Append(") ");
                    break;
                case ExpressionType.RightShift:
                    // From: https://www.sqlservercentral.com/scripts/integer-left-and-right-shift
                    builder.Append(" SELECT y = CASE WHEN ");
                    Visit(b.Left);
                    builder.Append(" >= 0 THEN CAST(");
                    Visit(b.Left);
                    builder.Append(" / p.pow AS INT) ELSE CAST(~(~");
                    Visit(b.Left);
                    builder.Append(" / p.pow) AS INT) END FROM ( SELECT pow = POWER(CAST(2 AS BIGINT), ");
                    Visit(b.Right);
                    builder.Append(" & 0x1F)) p");
                    break;
                default:
                    Visit(b.Left);

                    switch (b.NodeType)
                    {
                        case ExpressionType.Add:
                        case ExpressionType.AddChecked:
                            builder.Append(" + ");
                            break;
                        case ExpressionType.And:
                        case ExpressionType.AndAlso:
                            builder.Append(" AND ");
                            break;
                        case ExpressionType.Divide:
                            builder.Append(" / ");
                            break;
                        case ExpressionType.Equal:
                            builder.Append(" = ");
                            break;
                        case ExpressionType.ExclusiveOr:
                            builder.Append(" ^ ");
                            break;
                        case ExpressionType.GreaterThan:
                            builder.Append(" > ");
                            break;
                        case ExpressionType.GreaterThanOrEqual:
                            builder.Append(" >= ");
                            break;
                        case ExpressionType.LessThan:
                            builder.Append(" < ");
                            break;
                        case ExpressionType.LessThanOrEqual:
                            builder.Append(" <= ");
                            break;
                        case ExpressionType.Modulo:
                            builder.Append(" % ");
                            break;
                        case ExpressionType.Multiply:
                        case ExpressionType.MultiplyChecked:
                            builder.Append(" * ");
                            break;
                        case ExpressionType.NotEqual:
                            builder.Append(" != ");
                            break;
                        case ExpressionType.Or:
                        case ExpressionType.OrElse:
                            builder.Append(" OR ");
                            break;
                        case ExpressionType.Subtract:
                        case ExpressionType.SubtractChecked:
                            builder.Append(" - ");
                            break;
                        default:
                            throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported");
                    }

                    Visit(b.Right);
                    break;
            }

            builder.Append(")");

            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;

            if (q != null)
            {
                // assume constant nodes w/ IQueryables are table references
                builder.Append("SELECT * FROM ");
                builder.Append(q.ElementType.Name);
            }
            else if (c.Value == null)
                builder.Append("NULL");
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        builder.Append(((bool)c.Value) ? 1 : 0);
                        break;
                    case TypeCode.String:
                        builder.Append("'");
                        builder.Append(c.Value);
                        builder.Append("'");
                        break;
                    case TypeCode.DateTime:
                        builder.Append("'");
                        builder.Append(((DateTime)c.Value).ToString("yyyy-MM-dd HH:mm"));
                        builder.Append("'");
                        break;
                    case TypeCode.Object:
                        throw new NotSupportedException($"The constant for '{c.Value}' is not supported");
                    default:
                        builder.Append(c.Value);
                        break;
                }
            }

            return c;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                builder.Append($"runSql.{m.Member.Name}");

                return m;
            }

            throw new NotSupportedException($"The member '{m.Member.Name}' is not supported");
        }
    }
}
