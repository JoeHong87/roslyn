' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Navigation
Imports Microsoft.VisualStudio.Language.CallHierarchy

Namespace Microsoft.CodeAnalysis.Editor.UnitTests.CallHierarchy
    Public Class CallHierarchyTests
        <Fact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub TestScopes()
            Dim input =
<Workspace>
    <Project Language="C#" AssemblyName="Assembly1" CommonReferences="true">
        <Document>
namespace C
{
    public class CC
    {
        public int GetFive() { return 5; }
    }
}
        </Document>
        <Document>
using C;
namespace G
{
    public Class G
    {
        public void G()
        {
            CC c = new CC();
            c.GetFive();
        }
    }
}

        </Document>
    </Project>
    <Project Language="C#" AssemblyName="Assembly2" CommonReferences="true">
        <ProjectReference>Assembly1</ProjectReference>
        <Document>
using C;
public class D
{
    void bar()
    {
        var c = new C.CC();
        var d = c.Ge$$tFive();
    }
}
        </Document>
        <Document>
using C;
public class DSSS
{
    void bar()
    {
        var c = new C.CC();
        var d = c.GetFive();
    }
}
        </Document>
    </Project>
</Workspace>

            Dim testState = New CallHierarchyTestState(input)
            Dim root = testState.GetRoot()
            testState.VerifyResult(root, String.Format(EditorFeaturesResources.CallsTo, "GetFive"), {"DSSS.bar()", "D.bar()", "G.G.G()"}, CallHierarchySearchScope.EntireSolution)
            Dim documents = testState.GetDocuments({"Test3.cs", "Test4.cs"})
            testState.VerifyResult(root, String.Format(EditorFeaturesResources.CallsTo, "GetFive"), {"DSSS.bar()", "D.bar()"}, CallHierarchySearchScope.CurrentProject)
            documents = testState.GetDocuments({"Test3.cs"})
            testState.VerifyResult(root, String.Format(EditorFeaturesResources.CallsTo, "GetFive"), {"D.bar()"}, CallHierarchySearchScope.CurrentDocument, documents)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub VBMethod()
            Dim input =
<Workspace>
    <Project Language="Visual Basic" AssemblyName="Assembly1" CommonReferences="true">
        <Document>
Class C
    Sub Foo()
        Fo$$o()
    End Sub
End Class
            </Document>
    </Project>
</Workspace>

            Dim testState = New CallHierarchyTestState(input)
            Dim root = testState.GetRoot()
            testState.VerifyResult(root, String.Format(EditorFeaturesResources.CallsTo, "Foo"), {"C.Foo()"})
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub VBInterface()
            Dim input =
<Workspace>
    <Project Language="Visual Basic" AssemblyName="Assembly1" CommonReferences="true">
        <Document>
Class C
    Implements I
    Sub Foo() Implements I.Foo
        Foo()
    End Sub
End Class

Interface I
    Sub F$$oo()
End Interface
            </Document>
    </Project>
</Workspace>

            Dim testState = New CallHierarchyTestState(input)
            Dim root = testState.GetRoot()
            testState.VerifyResult(root, String.Format(EditorFeaturesResources.ImplementsArg, "Foo"), {"C.Foo()"})
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub TestInterfaceScopes()
            Dim input =
<Workspace>
    <Project Language="C#" AssemblyName="Assembly1" CommonReferences="true">
        <Document>
namespace C
{
    public interface I
    {
        void fo$$o();
    }

    public class C : I
    {
        void foo() { }
    }
}
        </Document>
        <Document>
using C;
namespace G
{
    public Class G : I
    {
        public void foo()
        {
        }
    }
}

        </Document>
    </Project>
    <Project Language="C#" AssemblyName="Assembly2" CommonReferences="true">
        <ProjectReference>Assembly1</ProjectReference>
        <Document>
using C;
public class D : I
{
    public void foo()
    {
    }
}
        </Document>
    </Project>
</Workspace>

            Dim testState = New CallHierarchyTestState(input)
            Dim root = testState.GetRoot()
            testState.VerifyResult(root, String.Format(EditorFeaturesResources.ImplementsArg, "foo"), {"D.foo()", "G.G.foo()", "C.C.foo()"}, CallHierarchySearchScope.EntireSolution)
            Dim documents = testState.GetDocuments({"Test1.cs", "Test2.cs"})
            testState.VerifyResult(root, String.Format(EditorFeaturesResources.ImplementsArg, "foo"), {"G.G.foo()", "C.C.foo()"}, CallHierarchySearchScope.CurrentProject, documents)
            documents = testState.GetDocuments({"Test1.cs"})
            testState.VerifyResult(root, String.Format(EditorFeaturesResources.ImplementsArg, "foo"), {"C.C.foo()"}, CallHierarchySearchScope.CurrentDocument, documents)
        End Sub

        <WorkItem(981869)>
        <Fact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub CallHierarchyCrossProjectNavigation()
            Dim input =
<Workspace>
    <Project Language="C#" AssemblyName="Assembly1" CommonReferences="true">
        <Document>
public interface IChangeSignatureOptionsService
{
    bool GetChangeS$$ignatureOptions();
}
        </Document>
    </Project>
    <Project Language="C#" AssemblyName="Assembly2" CommonReferences="true">
        <ProjectReference>Assembly1</ProjectReference>
        <Document>
using System;

class CSharpIt : IChangeSignatureOptionsService
{
    public bool GetChangeSignatureOptions()
    {
        throw new NotImplementedException();
    }
}
        </Document>
    </Project>
</Workspace>

            Dim testState = New CallHierarchyTestState(input)
            Dim root = testState.GetRoot()
            testState.SearchRoot(root,
                                 String.Format(EditorFeaturesResources.ImplementsArg, "GetChangeSignatureOptions"),
                                 Sub(c)
                                     Assert.Equal("Assembly2", c.Project.Name)
                                 End Sub,
                                 CallHierarchySearchScope.EntireSolution)
        End Sub

        <WorkItem(844613)>
        <Fact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub MustInheritMethodInclusionToOverrides()
            Dim input =
<Workspace>
    <Project Language="Visual Basic" AssemblyName="Assembly1" CommonReferences="true">
        <Document>
Public MustInherit Class Base
    Public MustOverride Sub $$M()
End Class

Public Class Derived
    Inherits Base

    Public Overrides Sub M()
        Throw New NotImplementedException()
    End Sub
End Class
            </Document>
    </Project>
</Workspace>

            Dim testState = New CallHierarchyTestState(input)
            Dim root = testState.GetRoot()
            testState.VerifyResult(root, EditorFeaturesResources.Overrides, {"Derived.M()"})
        End Sub

        <WorkItem(1022864)>
        <Fact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub NavigateCrossProject()
            Dim input =
    <Workspace>
        <Project Language="C#" AssemblyName="Assembly1" CommonReferences="true">
            <Document>
public class C
{
    public virtual void fo$$o() { }
}
        </Document>
        </Project>
        <Project Language="C#" AssemblyName="Assembly2" CommonReferences="true">
            <ProjectReference>Assembly1</ProjectReference>
            <Document>
class D : C
{
    public override void foo() { }
}
        </Document>
        </Project>
    </Workspace>

            Dim testState = New CallHierarchyTestState(input, GetType(MockSymbolNavigationServiceProvider))
            Dim root = testState.GetRoot()
            testState.Navigate(root, EditorFeaturesResources.Overrides, "D.foo()")

            Dim mockNavigationService = DirectCast(testState.Workspace.Services.GetService(Of ISymbolNavigationService)(), MockSymbolNavigationServiceProvider.MockSymbolNavigationService)
            Assert.NotNull(mockNavigationService.Symbol)
            Assert.NotNull(mockNavigationService.Project)
        End Sub

        <WorkItem(1022864)>
        <Fact, Trait(Traits.Feature, Traits.Features.CallHierarchy)>
        Public Sub UseDocumentIdWhenNavigating()
            Dim input =
    <Workspace>
        <Project Language="C#" AssemblyName="Assembly1" CommonReferences="true">
            <Document>
namespace N
{
    class C
    {
        void F$$oo()
        {
        }
    }

    class G
    {
        void Main()
        {
            var c = new C();
            c.Foo();
        }
    }
}
        </Document>
        </Project>
    </Workspace>

            Dim testState = New CallHierarchyTestState(input, GetType(MockDocumentNavigationServiceProvider))
            Dim root = testState.GetRoot()
            testState.VerifyRoot(root, "N.C.Foo()", {String.Format(EditorFeaturesResources.CallsTo, "Foo")})
            testState.Navigate(root, String.Format(EditorFeaturesResources.CallsTo, "Foo"), "N.G.Main()")

            Dim navigationService = DirectCast(testState.Workspace.Services.GetService(Of IDocumentNavigationService)(), MockDocumentNavigationServiceProvider.MockDocumentNavigationService)
            Assert.NotEqual(navigationService.DocumentId, Nothing)
            Assert.NotEqual(navigationService.TextSpan, Nothing)
        End Sub
    End Class

End Namespace
