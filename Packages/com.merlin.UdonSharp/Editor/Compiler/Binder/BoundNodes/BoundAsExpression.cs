using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using UdonSharp.Compiler.Assembly;
using UdonSharp.Compiler.Emit;
using UdonSharp.Compiler.Symbols;
using UdonSharp.Internal;

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
            // User classes are stored as object[] with a type-ID header. Runtime GetType() always
            // returns typeof(object[]), so identity must be checked via that header instead.
            if (TargetType is ImportedUdonSharpTypeSymbol && !TargetType.IsArray && !TargetType.IsEnum)
                return EmitUserClassAs(context);

            return EmitExternAs(context);
        }

        private Value EmitExternAs(EmitContext context)
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

        private Value EmitUserClassAs(EmitContext context)
        {
            Value sourceValue = context.EmitValue(SourceExpression);
            Value returnValue = context.GetReturnValue(TargetType);

            TypeSymbol objectType = context.GetTypeSymbol(SpecialType.System_Object);
            TypeSymbol objectArrayType = objectType.MakeArrayType(context);
            TypeSymbol typeType = context.GetTypeSymbol(typeof(Type));
            TypeSymbol ulongType = context.GetTypeSymbol(SpecialType.System_UInt64);
            TypeSymbol intType = context.GetTypeSymbol(SpecialType.System_Int32);

            JumpLabel assignNullLabel = context.Module.CreateLabel();
            JumpLabel exitLabel = context.Module.CreateLabel();

            BoundExpression boxedSourceExpression = new BoundCastExpression(SyntaxNode,
                BoundAccessExpression.BindAccess(sourceValue), objectType, true);

            BoundExpression nullObjectExpression = new BoundConstantExpression(
                new ConstantValue<object>(null), objectType, SyntaxNode);

            MethodSymbol objectInequality = new ExternSynthesizedOperatorSymbol(
                BuiltinOperatorType.Inequality, objectType, context);

            Value sourceNotNullValue = context.EmitValue(BoundInvocationExpression.CreateBoundInvocation(
                context, SyntaxNode, objectInequality, null,
                new[] { boxedSourceExpression, nullObjectExpression }));
            context.Module.AddJumpIfFalse(assignNullLabel, sourceNotNullValue);

            // Confirm the runtime value is an object[] before reading the type-ID header.
            MethodSymbol getTypeMethod = objectType.GetMembers<MethodSymbol>("GetType", context)
                .First(method => method.Parameters.Length == 0);

            BoundExpression runtimeTypeExpression = BoundInvocationExpression.CreateBoundInvocation(
                context, SyntaxNode, getTypeMethod, boxedSourceExpression, Array.Empty<BoundExpression>());

            BoundExpression objectArrayTypeExpression = new BoundConstantExpression(
                typeof(object[]), typeType, SyntaxNode);

            MethodSymbol objectEquality = new ExternSynthesizedOperatorSymbol(
                BuiltinOperatorType.Equality, objectType, context);

            Value isObjectArrayValue = context.EmitValue(BoundInvocationExpression.CreateBoundInvocation(
                context, SyntaxNode, objectEquality, null,
                new[]
                {
                    new BoundCastExpression(SyntaxNode, runtimeTypeExpression, objectType, true),
                    new BoundCastExpression(SyntaxNode, objectArrayTypeExpression, objectType, true)
                }));
            context.Module.AddJumpIfFalse(assignNullLabel, isObjectArrayValue);

            Value objectArrayValue = context.CastValue(sourceValue, objectArrayType);

            BoundExpression headerIndexExpression = BoundAccessExpression.BindAccess(
                context.GetConstantValue(intType, ImportedUdonSharpTypeSymbol.HEADER_TYPE_ID_INDEX));

            Value headerValue = context.EmitValue(BoundAccessExpression.BindElementAccess(
                context, SyntaxNode,
                BoundAccessExpression.BindAccess(objectArrayValue),
                new[] { headerIndexExpression }));

            Value headerULongValue = context.CastValue(headerValue, ulongType);

            ulong expectedTypeId = (ulong)UdonSharpInternalUtility.GetTypeID(TypeSymbol.GetFullTypeName(TargetType.RoslynSymbol));
            BoundExpression expectedTypeIdExpression = BoundAccessExpression.BindAccess(
                context.GetConstantValue(ulongType, expectedTypeId));

            MethodSymbol ulongEquality = new ExternSynthesizedOperatorSymbol(
                BuiltinOperatorType.Equality, ulongType, context);

            Value typeIdMatchesValue = context.EmitValue(BoundInvocationExpression.CreateBoundInvocation(
                context, SyntaxNode, ulongEquality, null,
                new[]
                {
                    BoundAccessExpression.BindAccess(headerULongValue),
                    expectedTypeIdExpression
                }));
            context.Module.AddJumpIfFalse(assignNullLabel, typeIdMatchesValue);

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
