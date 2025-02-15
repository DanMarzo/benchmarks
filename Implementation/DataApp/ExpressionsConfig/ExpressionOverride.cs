using System.Linq.Expressions;
using System.Text;

namespace DataApp.ExpressionsConfig;

internal class ExpressionOverride : ExpressionVisitor
{

    public Expression Expression { get; set; }
    private StringBuilder Query = new StringBuilder();
    private Dictionary<string, object> Parameters = new Dictionary<string, object>();

    public ExpressionOverride(Expression expression)
    {
        Expression = expression;
    }

    public ExpressionValues GenerateWhere(Expression expression)
    {
        this.Query.Clear();
        this.Parameters.Clear();
        this.Query.Append("WHERE ");
        Visit(expression);
        var result = new ExpressionValues(this.Query.ToString(), this.Parameters);
        return result;
    }

    public ExpressionValues GenerateWhere()
    {
        this.Query.Clear();
        this.Parameters.Clear();
        this.Query.Append("WHERE ");
        Visit(this.Expression);
        var result = new ExpressionValues(this.Query.ToString(), this.Parameters);
        return result;
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        MemberExpression? memberExpression = node.Operand as MemberExpression;
        if (memberExpression is null) throw new Exception("Nome nao acessivel");


        bool value = node.NodeType switch
        {
            ExpressionType.Not => false,
            ExpressionType.Equal => true,
            _ => throw new Exception("Excessao inicial (Unary)"),
        };

        this.Parameters.Add(memberExpression.Member.Name, value);
        this.Query.Append($"{memberExpression.Member.Name} = @{memberExpression.Member.Name}");
        return node;
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        MemberExpression? expressionLeft = Visit(node.Left) as MemberExpression;
        ConstantExpression? expressionRight = Visit(node.Right) as ConstantExpression;
        if (expressionLeft is null || expressionRight is null)
            return node;

        var nameProp = expressionLeft.Member.Name;

        var operatorType = node.NodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            _ => throw new NotSupportedException($"Operador '{node.NodeType}' não suportado.")
        };
        this.Parameters.Add(nameProp, expressionRight.Value);
        this.Query.Append($"{nameProp} {operatorType} @{nameProp}");
        return node;
    }
}

internal record ExpressionValues(string Where, Dictionary<string, object> Parameters);