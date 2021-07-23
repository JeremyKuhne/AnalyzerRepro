using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using TestCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpSourceGeneratorTest<
    AnalyzerLibrary.MemberGenerator,
    Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

namespace AnalyzerLibrary.UnitTests
{
    public class MemberGeneratorTests
    {
        [Fact]
        public async Task SimpleTest()
        {
            await new TestCS()
            {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                TestState =
                {
                    Sources =
                    {
                        @"using Library;
using System;

[assembly: ListMembers(typeof(Class))]

namespace TestLibrary
{
    public class Test
    {
        ClassMembers members;
    }
}
",
                    },
                    GeneratedSources =
                    {
                        (typeof(MemberGenerator), "__MemberGenerator.ListMembers.generated.cs", @"using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class ListMembersAttribute : Attribute
{
    public ListMembersAttribute(Type type) => Type = type;

    public Type Type { get; }
}
"),
                        (typeof(MemberGenerator), "__MemberList.Class.generated.cs", @"// Library.Class._privateField;Library.Class._internalField;Library.Class._protectedField;Library.Class.PublicField;Library.Class.PrivateMethod();Library.Class.InternalMethod();Library.Class.ProtectedMethod();Library.Class.PublicMethod();Library.Class.Class()

/// <summary>
/// <see cref=""T:Library.Class""/>
/// </summary>
internal enum ClassMembers
{
    /// <summary>
    /// <see cref=""F:Library.Class._privateField""/>
    /// </summary>
    _privateField,
    /// <summary>
    /// <see cref=""F:Library.Class._internalField""/>
    /// </summary>
    _internalField,
    /// <summary>
    /// <see cref=""F:Library.Class._protectedField""/>
    /// </summary>
    _protectedField,
    /// <summary>
    /// <see cref=""F:Library.Class.PublicField""/>
    /// </summary>
    PublicField,
    /// <summary>
    /// <see cref=""M:Library.Class.PrivateMethod""/>
    /// </summary>
    PrivateMethod,
    /// <summary>
    /// <see cref=""M:Library.Class.InternalMethod""/>
    /// </summary>
    InternalMethod,
    /// <summary>
    /// <see cref=""M:Library.Class.ProtectedMethod""/>
    /// </summary>
    ProtectedMethod,
    /// <summary>
    /// <see cref=""M:Library.Class.PublicMethod""/>
    /// </summary>
    PublicMethod,{|#0:|}
    /// <summary>
    /// <see cref=""M:Library.Class.#ctor""/>
    /// </summary>
    .ctor,
}
"),
                    },
                    ExpectedDiagnostics =
                    {
                        // AnalyzerLibrary\AnalyzerLibrary.MemberGenerator\__MemberList.Class.generated.cs(11,18): error CS1001: Identifier expected
                        DiagnosticResult.CompilerError("CS1001").WithLocation(0),
                    },
                    AdditionalProjects =
                    {
                        ["Library"] =
                        {
                            Sources =
                            {
                                @"using System;

namespace Library
{
    public class Class
    {
        private int _privateField;
        internal int _internalField;
        protected int _protectedField;
        public int PublicField;

        private void PrivateMethod() { }
        internal void InternalMethod() { }
        protected void ProtectedMethod() { }
        public void PublicMethod() { }
    }
}
",
                            },
                        },
                    },
                    AdditionalProjectReferences =
                    {
                        "Library",
                    },
                }
            }.RunAsync();
        }
    }
}
