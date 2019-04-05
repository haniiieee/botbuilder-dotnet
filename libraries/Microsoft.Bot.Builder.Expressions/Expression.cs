﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Bot.Builder.Expressions
{
    /// <summary>
    /// Type expected from evalating an expression.
    /// </summary>
    public enum ReturnType
    {
        /// <summary>
        /// True or false boolean value.
        /// </summary>
        Boolean,

        /// <summary>
        /// Numerical value like int, float, double, ...
        /// </summary>
        Number,

        /// <summary>
        /// Any value is possible.
        /// </summary>
        Object,

        /// <summary>
        /// String value.
        /// </summary>
        String
    }

    /// <summary>
    /// An expression which can be analyzed or evaluated to produce a value.
    /// </summary>
    /// <remarks>
    /// This provides an open-ended wrapper that supports a number of built-in functions and can also be extended at runtime.  
    /// It also supports validation of the correctness of an expression and evaluation that should be exception free.
    /// </remarks>
    public class Expression
    {
        /// <summary>
        /// Expression constructor.
        /// </summary>
        /// <param name="type">Type of expression from <see cref="ExpressionType"/>.</param>
        /// <param name="evaluator">Information about how to validate and evaluate expression.</param>
        /// <param name="children">Child expressions.</param>
        public Expression(string type, ExpressionEvaluator evaluator = null, params Expression[] children)
        {
            Type = type;
            Evaluator = evaluator ?? BuiltInFunctions.Lookup(type);
            Children = children;
        }

        /// <summary>
        /// Type of expression.
        /// </summary>
        public string Type { get; }

        public ExpressionEvaluator Evaluator { get; }

        /// <summary>
        /// Children expressions.
        /// </summary>
        public Expression[] Children { get; set; }

        /// <summary>
        /// Expected result of evaluating expression.
        /// </summary>
        public ReturnType ReturnType => Evaluator.ReturnType;

        /// <summary>
        /// Validate immediate expression.
        /// </summary>
        public void Validate() => Evaluator.ValidateExpression(this);

        /// <summary>
        /// Recursively validate the expression tree.
        /// </summary>
        public void ValidateTree()
        {
            Validate();
            foreach (var child in Children)
            {
                child.ValidateTree();
            }
        }

        /// <summary>
        /// Evaluate the expression.
        /// </summary>
        /// <param name="state">
        /// Global state to evaluate accessor expressions against.  Can be <see cref="IDictionary{String}{Object}"/>, <see cref="IDictionary"/> otherwise reflection is used to access property and then indexer.
        /// </param>
        /// <returns>Computed value and an error string.  If the string is non-null, then there was an evaluation error.</returns>
        public (object value, string error) TryEvaluate(object state)
            => Evaluator.TryEvaluate(this, state);

        public override string ToString()
        {
            var builder = new StringBuilder();
            // Special support for memory paths
            if (Type == ExpressionType.Accessor)
            {
                var prop = (Children[0] as Constant).Value;
                if (Children.Count() == 1)
                {
                    builder.Append(prop);
                }
                else
                {
                    builder.Append(Children[1].ToString());
                    builder.Append('.');
                    builder.Append(prop);
                }
            }
            else if (Type == ExpressionType.Element)
            {
                builder.Append(Children[0].ToString());
                builder.Append('[');
                builder.Append(Children[1].ToString());
                builder.Append(']');
            }
            else
            {
                var infix = Type.Length > 0 && !char.IsLetter(Type[0]) && Children.Count() >= 2;
                if (!infix)
                {
                    builder.Append(Type);
                }
                builder.Append('(');
                var first = true;
                foreach (var child in Children)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        if (infix)
                        {
                            builder.Append(' ');
                            builder.Append(Type);
                            builder.Append(' ');
                        }
                        else
                        {
                            builder.Append(", ");
                        }
                    }

                    builder.Append(child.ToString());
                }
                builder.Append(')');
            }
            return builder.ToString();
        }

        /// <summary>
        /// Make an expression and validate it.
        /// </summary>
        /// <param name="type">Type of expression from <see cref="ExpressionType"/>.</param>
        /// <param name="evaluator">Information about how to validate and evaluate expression.</param>
        /// <param name="children">Child expressions.</param>
        /// <returns>New expression.</returns>
        public static Expression MakeExpression(string type, ExpressionEvaluator evaluator = null, params Expression[] children)
        {
            var expr = new Expression(type, evaluator, children);
            expr.Validate();
            return expr;
        }

        /// <summary>
        /// Construct an expression from a <see cref="EvaluateExpressionDelegate"/>.
        /// </summary>
        /// <param name="function">Function to create an expression from.</param>
        /// <returns></returns>
        public static Expression LambaExpression(EvaluateExpressionDelegate function)
            => new Expression(ExpressionType.Lambda, new ExpressionEvaluator(function));

        /// <summary>
        /// Construct an expression from a lamba expression over the state.
        /// </summary>
        /// <remarks>Exceptions will be caught and surfaced as an error string.</remarks>
        /// <param name="function">Lambda expression to evaluate.</param>
        /// <returns>New expression.</returns>
        public static Expression Lambda(Func<object, object> function)
            => new Expression(ExpressionType.Lambda,
                new ExpressionEvaluator((expression, state) =>
                {
                    object value = null;
                    string error = null;
                    try
                    {
                        value = function(state);
                    }
                    catch (Exception e)
                    {
                        error = e.Message;
                    }
                    return (value, error);
                }));

        /// <summary>
        /// Construct and validate an And expression.
        /// </summary>
        /// <param name="children">Child clauses.</param>
        /// <returns>New expression.</returns>
        public static Expression AndExpression(params Expression[] children)
            => Expression.MakeExpression(ExpressionType.And, null, children);

        /// <summary>
        /// Construct and validate an Or expression.
        /// </summary>
        /// <param name="children">Child clauses.</param>
        /// <returns>New expression.</returns>
        public static Expression OrExpression(params Expression[] children)
            => Expression.MakeExpression(ExpressionType.Or, null, children);

        /// <summary>
        /// Construct and validate a Not expression.
        /// </summary>
        /// <param name="children">Child clauses.</param>
        /// <returns>New expression.</returns>
        public static Expression NotExpression(Expression child)
            => Expression.MakeExpression(ExpressionType.Not, null, child);

        /// <summary>
        /// Construct a constant expression.
        /// </summary>
        /// <param name="value">Constant value.</param>
        /// <returns>New expression.</returns>
        public static Expression ConstantExpression(object value)
            => new Constant(value);

        /// <summary>
        /// Construct and validate a property accessor.
        /// </summary>
        /// <param name="property">Property to lookup.</param>
        /// <param name="instance">Expression to get instance that contains property or null for global state.</param>
        /// <returns>New expression.</returns>
        public static Expression Accessor(string property, Expression instance = null)
            => instance == null
            ? MakeExpression(ExpressionType.Accessor, null, ConstantExpression(property))
            : MakeExpression(ExpressionType.Accessor, null, ConstantExpression(property), instance);
    }
}
