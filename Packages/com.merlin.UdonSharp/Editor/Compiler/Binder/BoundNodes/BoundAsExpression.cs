using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using UdonSharp.Compiler.Assembly;
using UdonSharp.Compiler.Emit;
using UdonSharp.Compiler.Symbols;

namespace UdonSharp.Compiler.Binder
{
    internal sealed class BoundAsExpression : BoundExpression
    {
        private TypeSymbol TargetType { get; }

        public override TypeSymbol ValueType => TargetType;

        public BoundAsExpression(SyntaxNode node, BoundExpression sourceExpression, TypeSymbol targetType)
            : base(node, sourceExpression)
        {
            TargetType = targetType;
        }

        public override Value EmitValue(EmitContext context)
        {
            Value sourceValue = context.EmitValue(SourceExpression);
            Value returnValue = context.GetReturnValue(TargetType);

            TypeSymbol objectType = context.GetTypeSymbol(SpecialType.System_Object);
            TypeSymbol typeType = context.GetTypeSymbol(typeof(Type));

            BoundExpression boxedSourceExpression = new BoundCastExpression(SyntaxNode,
                BoundAccessExpression.BindAccess(sourceValue), objectType, true);

            BoundExpression nullObjectExpression = new BoundConstantExpression(
                new ConstantValue<object>(null), objectType, SyntaxNode);

            MethodSymbol objectInequality = new ExternSynthesizedOperatorSymbol(
                BuiltinOperatorType.Inequality, objectType, context);

            BoundExpression sourceNotNullExpression = BoundInvocationExpression.CreateBoundInvocation(
                context, SyntaxNode, objectInequality, null,
                new[] { boxedSourceExpression, nullObjectExpression });

            JumpLabel assignNullLabel = context.Module.CreateLabel();
            JumpLabel exitLabel = context.Module.CreateLabel();

            Value sourceNotNullValue = context.EmitValue(sourceNotNullExpression);
            context.Module.AddJumpIfFalse(assignNullLabel, sourceNotNullValue);

            MethodSymbol getTypeMethod = objectType.GetMembers<MethodSymbol>("GetType", context)
                .First(method => method.Parameters.Length == 0);

            BoundExpression runtimeTypeExpression = BoundInvocationExpression.CreateBoundInvocation(
                context, SyntaxNode, getTypeMethod, boxedSourceExpression, Array.Empty<BoundExpression>());

            BoundExpression targetTypeExpression = new BoundConstantExpression(
                TargetType.UdonType.SystemType, typeType, SyntaxNode);

            MethodSymbol objectEquality = new ExternSynthesizedOperatorSymbol(
                BuiltinOperatorType.Equality, objectType, context);

            BoundExpression typeMatchesExpression = BoundInvocationExpression.CreateBoundInvocation(
                context, SyntaxNode, objectEquality, null,
                new[]
                {
                    new BoundCastExpression(SyntaxNode, runtimeTypeExpression, objectType, true),
                    new BoundCastExpression(SyntaxNode, targetTypeExpression, objectType, true)
                });

            Value typeMatchesValue = context.EmitValue(typeMatchesExpression);
            context.Module.AddJumpIfFalse(assignNullLabel, typeMatchesValue);

            context.EmitValueAssignment(returnValue, new BoundCastExpression(SyntaxNode,
                BoundAccessExpression.BindAccess(sourceValue), TargetType, true));
            context.Module.AddJump(exitLabel);

            context.Module.LabelJump(assignNullLabel);
            context.EmitValueAssignment(returnValue, new BoundConstantExpression((object)null, TargetType, SyntaxNode));

            context.Module.LabelJump(exitLabel);

            return returnValue;
        }
    }
}
