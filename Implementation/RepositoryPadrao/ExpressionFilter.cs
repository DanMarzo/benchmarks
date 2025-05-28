using System.Linq.Expressions;

namespace RepositoryPadrao;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

public class DapperQueryBuilder<T>
{
    public static (string WhereClause, Dictionary<string, object> Parameters) Build(Expression<Func<T, bool>> expression)
    {
        var visitor = new DapperExpressionVisitor();
        visitor.Visit(expression);
        return (visitor.WhereClause, visitor.Parameters);
    }

    private class DapperExpressionVisitor : ExpressionVisitor
    {
        public string WhereClause { get; private set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; } = new Dictionary<string, object>();
        private int _parameterCount = 0;
        private Expression _currentBinaryLeft;
        private bool _isNotOperator = false;

        public override Expression Visit(Expression node)
        {
            if (node == null) return null;

            switch (node.NodeType)
            {
                case ExpressionType.Lambda:
                    var lambda = (LambdaExpression)node;
                    Visit(lambda.Body);
                    break;

                case ExpressionType.Not:
                    _isNotOperator = true;
                    Visit(((UnaryExpression)node).Operand);
                    _isNotOperator = false;
                    break;

                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    var binary = (BinaryExpression)node;
                    //WhereClause += "(";
                    _currentBinaryLeft = binary.Left;
                    Visit(binary.Left);
                    WhereClause += GetOperator(binary.NodeType);
                    _currentBinaryLeft = null;
                    Visit(binary.Right);
                    //WhereClause += ")";
                    break;

                case ExpressionType.MemberAccess:
                    var member = (MemberExpression)node;
                    HandleMemberExpression(member);
                    break;

                case ExpressionType.Constant:
                    var constant = (ConstantExpression)node;
                    AddParameter(constant.Value);
                    break;

                default:
                    throw new NotSupportedException($"Node type {node.NodeType} not supported");
            }

            return node;
        }

        private void HandleMemberExpression(MemberExpression member)
        {
            if (member.Expression?.NodeType == ExpressionType.Parameter)
            {
                WhereClause += member.Member.Name;

                // Se for booleano e não estiver no lado direito de uma comparação
                if (member.Type == typeof(bool) && _currentBinaryLeft != member)
                {
                    WhereClause += _isNotOperator ? " = 0" : " = 1";
                }
            }
            else
            {
                var value = GetValue(member);
                AddParameter(value);
            }
        }

        private object GetValue(MemberExpression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            return getterLambda.Compile()();
        }

        private void AddParameter(object value)
        {
            var paramName = $"p{_parameterCount++}";
            Parameters.Add(paramName, value);
            WhereClause += $"@{paramName}";
        }

        private string GetOperator(ExpressionType nodeType)
        {
            return nodeType switch
            {
                ExpressionType.Equal => " = ",
                ExpressionType.NotEqual => " <> ",
                ExpressionType.GreaterThan => " > ",
                ExpressionType.GreaterThanOrEqual => " >= ",
                ExpressionType.LessThan => " < ",
                ExpressionType.LessThanOrEqual => " <= ",
                ExpressionType.AndAlso => " AND ",
                ExpressionType.OrElse => " OR ",
                _ => throw new NotSupportedException($"Operator {nodeType} not supported")
            };
        }
    }
}