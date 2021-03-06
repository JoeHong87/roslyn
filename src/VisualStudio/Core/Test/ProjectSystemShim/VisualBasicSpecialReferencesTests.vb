' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.VisualStudio.LanguageServices.VisualBasic.ProjectSystemShim.Interop
Imports Roslyn.Test.Utilities
Imports Microsoft.VisualStudio.LanguageServices.UnitTests.ProjectSystemShim.Framework
Imports Microsoft.VisualStudio.LanguageServices.UnitTests.ProjectSystemShim.VisualBasicHelpers

Namespace Microsoft.VisualStudio.LanguageServices.UnitTests.ProjectSystemShim
    Public Class VisualBasicSpecialReferencesTests
        <Fact()>
        <Trait(Traits.Feature, Traits.Features.ProjectSystemShims)>
        Sub ProjectIncludesReferencesToMscorlibSystemAndMicrosoftVisualBasic()
            Using environment = New TestEnvironment()
                Dim project = CreateVisualBasicProject(environment, "Test")

                project.SetCompilerOptions(CreateMinimalCompilerOptions(project))

                Dim workspaceProject = environment.Workspace.CurrentSolution.Projects.Single()

                Assert.True(workspaceProject.HasMetadataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("Microsoft.VisualBasic.dll")))
                Assert.True(workspaceProject.HasMetadataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("mscorlib.dll")))
                Assert.True(workspaceProject.HasMetadataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("System.dll")))

                project.Disconnect()
            End Using
        End Sub

        <Fact()>
        <Trait(Traits.Feature, Traits.Features.ProjectSystemShims)>
        Sub ProjectWithoutStandardLibsDoesNotReferenceSystem()
            Using environment = New TestEnvironment()
                Dim project = CreateVisualBasicProject(environment, "Test")
                Dim options = CreateMinimalCompilerOptions(project)

                options.bNoStandardLibs = True
                project.SetCompilerOptions(options)

                Dim workspaceProject = environment.Workspace.CurrentSolution.Projects.Single()

                Assert.True(workspaceProject.HasMetadataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("Microsoft.VisualBasic.dll")))
                Assert.True(workspaceProject.HasMetadataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("mscorlib.dll")))
                Assert.False(workspaceProject.HasMetadataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("System.dll")))

                project.Disconnect()
            End Using
        End Sub

        <Fact()>
        <Trait(Traits.Feature, Traits.Features.ProjectSystemShims)>
        Sub ProjectWithoutVisualBasicRuntimeDoesNotReferenceMicrosoftVisualBasic()
            Using environment = New TestEnvironment()
                Dim project = CreateVisualBasicProject(environment, "Test")
                Dim options = CreateMinimalCompilerOptions(project)

                options.vbRuntimeKind = VBRuntimeKind.NoRuntime
                project.SetCompilerOptions(options)

                Dim workspaceProject = environment.Workspace.CurrentSolution.Projects.Single()

                Assert.False(workspaceProject.HasMetadataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("Microsoft.VisualBasic.dll")))
                Assert.True(workspaceProject.HasMetadataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("mscorlib.dll")))
                Assert.True(workspaceProject.HasMetadataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("System.dll")))

                project.Disconnect()
            End Using
        End Sub

        <Fact()>
        <Trait(Traits.Feature, Traits.Features.ProjectSystemShims)>
        <WorkItem(860964)>
        Sub AddingReferenceToMicrosoftVisualBasicBeforeSettingOptionsShouldNotCrash()
            Using environment = New TestEnvironment()
                Dim project = CreateVisualBasicProject(environment, "Test")

                project.AddMetaDataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("Microsoft.VisualBasic.dll"), bAssembly:=True)

                Dim workspaceProject = environment.Workspace.CurrentSolution.Projects.Single()
                Assert.True(workspaceProject.HasMetadataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("Microsoft.VisualBasic.dll")))

                Dim options = CreateMinimalCompilerOptions(project)
                project.SetCompilerOptions(options)

                ' It should still reference the VB runtime
                workspaceProject = environment.Workspace.CurrentSolution.Projects.Single()
                Assert.True(workspaceProject.HasMetadataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("Microsoft.VisualBasic.dll")))

                ' Now, remove the VB runtime reference
                options.vbRuntimeKind = VBRuntimeKind.NoRuntime
                project.SetCompilerOptions(options)

                ' It should still reference the VB runtime
                workspaceProject = environment.Workspace.CurrentSolution.Projects.Single()
                Assert.True(workspaceProject.HasMetadataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("Microsoft.VisualBasic.dll")))

                project.Disconnect()
            End Using
        End Sub


        <Fact()>
        <Trait(Traits.Feature, Traits.Features.ProjectSystemShims)>
        <WorkItem(860964)>
        Sub AddingReferenceToMicrosoftVisualBasicAfterSettingOptionsShouldNotCrash()
            Using environment = New TestEnvironment()
                Dim project = CreateVisualBasicProject(environment, "Test")

                project.SetCompilerOptions(CreateMinimalCompilerOptions(project))

                ' We already should be referencing the VB runtime at this point
                Dim workspaceProject = environment.Workspace.CurrentSolution.Projects.Single()
                Assert.True(workspaceProject.HasMetadataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("Microsoft.VisualBasic.dll")))

                project.AddMetaDataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("Microsoft.VisualBasic.dll"), bAssembly:=True)

                ' It still should be referencing it
                workspaceProject = environment.Workspace.CurrentSolution.Projects.Single()
                Assert.True(workspaceProject.HasMetadataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("Microsoft.VisualBasic.dll")))

                project.RemoveMetaDataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("Microsoft.VisualBasic.dll"))

                ' It still should be referencing it since we're implicitly adding it as a part of the options
                workspaceProject = environment.Workspace.CurrentSolution.Projects.Single()
                Assert.True(workspaceProject.HasMetadataReference(MockCompilerHost.FullFrameworkCompilerHost.GetWellKnownDllName("Microsoft.VisualBasic.dll")))

                project.Disconnect()
            End Using
        End Sub
    End Class
End Namespace
