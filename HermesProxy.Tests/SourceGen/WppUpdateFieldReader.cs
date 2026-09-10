using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HermesProxy.Tests.SourceGen;

/// <summary>
/// One field read by a WPP <c>ReadUpdate*Data</c> method. <see cref="Parent"/> is the innermost
/// enclosing <c>changesMask[...]</c> other than the field's own, or -1; block leaders are left in and
/// normalised by the caller.
/// </summary>
internal sealed record WppField(string Name, int Bit, bool PerElement, int Count, int Parent, string Reader);

/// <summary>
/// Reads the update-field layout out of WowPacketParser's generated <c>UpdateFieldsHandler*.cs</c>
/// by walking its syntax: nested <c>if (changesMask[N])</c> blocks give the bit and parent,
/// <c>for (var i = 0; i &lt; N; ++i)</c> with <c>changesMask[B + i]</c> gives a per-element array, and the
/// <c>packet.ReadX(...)</c> call on the right of <c>data.Field = ...</c> gives the wire type.
/// </summary>
internal static class WppUpdateFieldReader
{
    public static Dictionary<string, Dictionary<string, WppField>> ReadEmbedded(string logicalName)
    {
        using var stream = typeof(WppUpdateFieldReader).Assembly.GetManifestResourceStream(logicalName)
            ?? throw new InvalidOperationException($"Embedded resource '{logicalName}' not found.");
        using var reader = new StreamReader(stream);
        return ReadUpdateSections(reader.ReadToEnd());
    }

    /// <summary>Section name (e.g. "Unit" for ReadUpdateUnitData) to its fields by name.</summary>
    public static Dictionary<string, Dictionary<string, WppField>> ReadUpdateSections(string source)
    {
        var root = CSharpSyntaxTree.ParseText(source).GetRoot();
        var sections = new Dictionary<string, Dictionary<string, WppField>>();
        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            string name = method.Identifier.Text;
            if (method.Body is null || !name.StartsWith("ReadUpdate", StringComparison.Ordinal)
                || !name.EndsWith("Data", StringComparison.Ordinal))
                continue;

            var fields = new Dictionary<string, WppField>();
            Walk(method.Body, [], 0, fields);
            sections.TryAdd(name["ReadUpdate".Length..^"Data".Length], fields);
        }
        return sections;
    }

    private static void Walk(SyntaxNode node, List<(int Bit, bool PerElement)> masks, int loopCount,
        Dictionary<string, WppField> fields)
    {
        foreach (var child in node.ChildNodes())
        {
            switch (child)
            {
                case IfStatementSyntax ifStatement:
                    Walk(ifStatement.Statement,
                        TryMask(ifStatement.Condition, out var bit, out var perElement) ? [.. masks, (bit, perElement)] : masks,
                        loopCount, fields);
                    if (ifStatement.Else is not null)
                        Walk(ifStatement.Else, masks, loopCount, fields);
                    break;
                case ForStatementSyntax forStatement:
                    int count = forStatement.Condition is BinaryExpressionSyntax { Right: LiteralExpressionSyntax limit }
                        ? (int)limit.Token.Value! : -1;
                    Walk(forStatement.Statement, masks, count, fields);
                    break;
                case ExpressionStatementSyntax statement:
                    Record(statement.Expression, masks, loopCount, fields);
                    break;
                default:
                    Walk(child, masks, loopCount, fields);
                    break;
            }
        }
    }

    private static bool TryMask(ExpressionSyntax condition, out int bit, out bool perElement)
    {
        bit = -1;
        perElement = false;
        if (condition is not ElementAccessExpressionSyntax
            { Expression: IdentifierNameSyntax { Identifier.Text: "changesMask" } } access)
            return false;

        switch (access.ArgumentList.Arguments[0].Expression)
        {
            case LiteralExpressionSyntax literal:
                bit = (int)literal.Token.Value!;
                return true;
            case BinaryExpressionSyntax { Left: LiteralExpressionSyntax firstElement }:
                bit = (int)firstElement.Token.Value!;
                perElement = true;
                return true;
            default:
                return false;
        }
    }

    private static void Record(ExpressionSyntax expression, List<(int Bit, bool PerElement)> masks, int loopCount,
        Dictionary<string, WppField> fields)
    {
        string? field = null;
        string reader = "";
        switch (expression)
        {
            // data.X = packet.ReadInt64(...), data.X[i] = ..., data.X = ReadUpdateUnitChannel(...)
            case AssignmentExpressionSyntax assignment:
                field = FieldOf(assignment.Left);
                reader = assignment.Right is InvocationExpressionSyntax call ? ReaderOf(call) : assignment.Right.Kind().ToString();
                break;
            // data.X.ReadUpdateMask(packet) — a dynamic field's mask
            case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax member }:
                field = FieldOf(member.Expression);
                reader = member.Name.Identifier.Text;
                break;
        }

        // A dynamic field is visited twice (mask pass, then values); the first visit carries its bit.
        if (field is null || masks.Count == 0 || fields.ContainsKey(field))
            return;

        var own = masks[^1];
        int parent = masks.Count >= 2 ? masks[^2].Bit : -1;
        fields[field] = new WppField(field, own.Bit, own.PerElement, own.PerElement ? loopCount : 0, parent, reader);
    }

    private static string? FieldOf(ExpressionSyntax expression) => expression switch
    {
        MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "data" } } member
            => member.Name.Identifier.Text,
        ElementAccessExpressionSyntax element => FieldOf(element.Expression),
        _ => null,
    };

    private static string ReaderOf(InvocationExpressionSyntax call) => call.Expression switch
    {
        MemberAccessExpressionSyntax { Name.Identifier.Text: "ReadBits" }
            => $"ReadBits({call.ArgumentList.Arguments.FirstOrDefault()})",
        MemberAccessExpressionSyntax member => member.Name.Identifier.Text,
        IdentifierNameSyntax identifier => identifier.Identifier.Text,
        var other => other.ToString(),
    };
}
