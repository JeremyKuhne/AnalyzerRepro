using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AnalyzerLibrary
{
    [Generator]
    public class MemberGenerator : ISourceGenerator
    {
        private const string AttributeSource =
@"using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class ListMembersAttribute : Attribute
{
    public ListMembersAttribute(Type type) => Type = type;

    public Type Type { get; }
}
";

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (SyntaxReceiver)context.SyntaxContextReceiver;
            foreach (var type in receiver.Types)
            {
                context.AddSource(
                    $"__MemberList.{type.TypeInfo.Type.Name}.generated",
                    SourceText.From(GenerateSource(type), Encoding.UTF8));
            }
        }

        private static string GenerateSource((TypeInfo TypeInfo, string Members) type)
        {
            StringBuilder builder = new(1000);
            builder.Append("// ");
            builder.AppendLine(type.Members);
            builder.AppendLine();
            builder.AppendLine($"/// <summary>");
            builder.AppendLine($"/// <see cref=\"{type.TypeInfo.Type.GetDocumentationCommentId()}\"/>");
            builder.AppendLine($"/// </summary>");
            builder.Append("internal enum ");
            builder.Append(type.TypeInfo.Type.Name);
            builder.AppendLine("Members");
            builder.AppendLine("{");
            foreach (var member in type.TypeInfo.Type.GetMembers())
            {
                builder.AppendLine($"    /// <summary>");
                builder.AppendLine($"    /// <see cref=\"{member.GetDocumentationCommentId()}\"/>");
                builder.AppendLine($"    /// </summary>");
                builder.Append("    ");
                builder.Append(member.Name);
                builder.AppendLine(",");
            }
            builder.AppendLine("}");
            return builder.ToString();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization((pi) => pi.AddSource("__MemberGenerator.ListMembers.generated", AttributeSource));

#if false
            // Attaching like this makes the internal members no longer visible.
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        private class SyntaxReceiver : ISyntaxContextReceiver
        {
            public HashSet<(TypeInfo TypeInfo, string Members)> Types = new();

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is not CompilationUnitSyntax compilation)
                {
                    return;
                }

                foreach (var attrbuteList in compilation.AttributeLists)
                {
                    foreach (var attribute in attrbuteList.Attributes)
                    {
                        var semanticModel = context.SemanticModel; //.Compilation.GetSemanticModel(context.SemanticModel.SyntaxTree, ignoreAccessibility: true);
                        var attributeType = semanticModel.GetTypeInfo(attribute).Type;
                        if (attributeType is not null
                            && attributeType.ContainingNamespace.IsGlobalNamespace
                            && attributeType.Name is "ListMembersAttribute")
                        {
                            var targetType = semanticModel.GetTypeInfo(((TypeOfExpressionSyntax)attribute.ArgumentList!.Arguments[0].Expression).Type);
                            Types.Add((targetType, string.Join(";", targetType.Type.GetMembers())));
                        }
                    }
                }
            }
        }
    }
}
