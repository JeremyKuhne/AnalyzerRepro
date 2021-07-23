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
internal enum ClassMembers
{
    _privateField,
    _internalField,
    _protectedField,
    PublicField,
    PrivateMethod,
    InternalMethod,
    ProtectedMethod,
    PublicMethod,{|#0:|}
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
