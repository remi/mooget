using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using MooGet;

namespace MooGet.Specs.Core {

	[TestFixture]
	public class UnpackedNupkgSpec : Spec {

		// UnpackedNupkg implements IUnpackedNupkg which gives it nearly all of the methods we need to satisfy this spec.
		IUnpackedPackage nunit, fluent, foo;

		[SetUp]
		public void Before() {
			nunit  = new UnpackedNupkg(PathToContent("packages/NUnit.2.5.7.10213"));
			fluent = new UnpackedNupkg(PathToContent("packages/FluentNHibernate.1.1.0.694"));
			foo    = new UnpackedNupkg(PathToContent("more_unpacked_packages/Foo"));
		}

		[Test]
		public void has_a_Path() {
			nunit.Path.ShouldEqual(PathToContent("packages/NUnit.2.5.7.10213"));
			fluent.Path.ShouldEqual(PathToContent("packages/FluentNHibernate.1.1.0.694"));
		}

		[Test]
		public void can_check_if_Exists() {
			nunit.Exists().ShouldBeTrue();
			fluent.Exists().ShouldBeTrue();
			new UnpackedNupkg("/i/dont/exist").Exists.ShouldBeFalse();
		}

		[Test]
		public void Details() {
			nunit.Id.ShouldEqual("NUnit");
			nunit.Version.ToString().ShouldEqual("2.5.7.10213");
			nunit.Details.AuthorsText.ShouldEqual("Charlie Poole");

			(nunit as UnpackedNupkg).Nuspec.ShouldHaveProperties(new {
				Id          = "NUnit",
				VersionText = "2.5.7.10213",
				AuthorsText = "Charlie Poole"
			});

			fluent.Id.ShouldEqual("FluentNHibernate");
			fluent.Version.ToString().ShouldEqual("1.1.0.694");
			fluent.Details.AuthorsText.ShouldEqual("James Gregory");

			(fluent as UnpackedNupkg).Nuspec.ShouldHaveProperties(new {
				Id          = "FluentNHibernate",
				VersionText = "1.1.0.694",
				AuthorsText = "James Gregory"
			});
		}

		[Test]
		public void Id() {
			nunit.Id.ShouldEqual("NUnit");
			fluent.Id.ShouldEqual("FluentNHibernate");
		}

		[Test]
		public void Version() {
			nunit.Version.ToString().ShouldEqual("2.5.7.10213");
			fluent.Version.ToString().ShouldEqual("1.1.0.694");
		}

		[Test]
		public void Files() {
			fluent.Files.Select(path => path.Replace(fluent.Path, "")).ToList().ShouldEqual(new List<string>{
				"/FluentNHibernate.nuspec", 
				"/[Content_Types].xml", 
				"/_rels/.rels", 
				"/lib/FluentNHibernate.XML", 
				"/lib/FluentNHibernate.dll", 
				"/lib/FluentNHibernate.pdb", 
				"/package/services/metadata/core-properties/902c256232984c97952ba394905bddfe.psmdcp"
			});
			nunit.Files.Count.ShouldEqual(57);
			var nunitFiles = nunit.Files.Select(path => path.Replace(nunit.Path, "")).ToList();
			nunitFiles.ShouldContain("/Logo.ico");
			nunitFiles.ShouldContain("/NUnit.nuspec");
			nunitFiles.ShouldContain("/Content/NUnitSampleTests.cs.pp");
			nunitFiles.ShouldContain("/Tools/NUnitTests.config");
			nunitFiles.ShouldContain("/Tools/nunit-agent-x86.exe");
			nunitFiles.ShouldContain("/Tools/lib/fit.dll");
			nunitFiles.ShouldContain("/Tools/lib/Skipped.png");
			nunitFiles.ShouldContain("/lib/nunit.framework.dll");
			nunitFiles.ShouldContain("/lib/pnunit.framework.dll");
		}

/*
Foo/
|-- cOnTeNt
|   |-- file.html
|   `-- subdir
|       `-- hi.there
|-- LiB
|   |-- Another.Global.DLL
|   |-- global_1.dll
|   |-- global_1.XmL
|   |-- nEt20
|   |   |-- Hi.DlL
|   |   `-- TherE.dLl
|   `-- Net35
|       |-- fooooo.dll
|       `-- fooooo.XML
|-- sRc
|   |-- hi.cs
|   `-- more
|       `-- FooFile.cs
`-- tOolS
    `-- this
        `-- that
            |-- hi.exe
            `-- neato.bat

*/

		[Test]
		public void LibrariesDirectory() {
			(foo    as UnpackedNupkg).LibrariesDirectory.ShouldEqual(Path.Combine(foo.Path, "LiB"));
			(nunit  as UnpackedNupkg).LibrariesDirectory.ShouldEqual(Path.Combine(nunit.Path, "lib"));
			(fluent as UnpackedNupkg).LibrariesDirectory.ShouldEqual(Path.Combine(fluent.Path, "lib"));
		}

		[Test]
		public void LibraryFrameworkNames() {
			foo.LibraryFrameworkNames.ShouldEqual(new List<FrameworkName>{ new FrameworkName(".NETFramework", "3.5"), new FrameworkName(".NETFramework", "2.0") });
			nunit.LibraryFrameworkNames.Should(Be.Empty);
			fluent.LibraryFrameworkNames.Should(Be.Empty);
		}

		[Test]
		public void ToolsDirectory() {
			(foo    as UnpackedNupkg).ToolsDirectory.ShouldEqual(Path.Combine(foo.Path, "tOolS"));
			(nunit  as UnpackedNupkg).ToolsDirectory.ShouldEqual(Path.Combine(nunit.Path, "Tools"));
			(fluent as UnpackedNupkg).ToolsDirectory.Should(Be.Null);
		}

		[Test]
		public void ContentDirectory() {
			(foo    as UnpackedNupkg).ContentDirectory.ShouldEqual(Path.Combine(foo.Path, "cOnTeNt"));
			(nunit  as UnpackedNupkg).ContentDirectory.ShouldEqual(Path.Combine(nunit.Path, "Content"));
			(fluent as UnpackedNupkg).ContentDirectory.Should(Be.Null);
		}

		[Test]
		public void SourceDirectory() {
			(foo    as UnpackedNupkg).SourceDirectory.ShouldEqual(Path.Combine(foo.Path, "sRc"));
			(nunit  as UnpackedNupkg).SourceDirectory.Should(Be.Null);
			(fluent as UnpackedNupkg).SourceDirectory.Should(Be.Null);
		}

		[Test]
		public void Libraries() {
			fluent.Libraries.Count.ShouldEqual(1);
			fluent.Libraries.First().Should(Be.StringEnding("/FluentNHibernate.dll"));

			nunit.Libraries.Count.ShouldEqual(3);
			nunit.Libraries.AsFiles().Select(f => f.Name()).ToList().ShouldEqual(new List<string>{ "nunit.framework.dll", "nunit.mocks.dll", "pnunit.framework.dll" });

			foo.Libraries.AsFiles().Select(f => f.Name()).ToList().ShouldEqual(new List<string>{ "Another.Global.DLL", "global_1.dll", "fooooo.dll", "Hi.DlL", "TherE.dLl" });
		}

		[Test]
		public void GlobalLibraries() {
			foo.GlobalLibraries.AsFiles().Select(f => f.Name()).ToList().ShouldEqual(new List<string>{ "Another.Global.DLL", "global_1.dll" });
		}

		[Test]
		public void just_NET20_Libraries() {
			(fluent as UnpackedNupkg).LibraryDirectoryFor("net20").Should(Be.Null);
			(fluent as UnpackedNupkg).JustLibrariesFor("net20").Should(Be.Empty);
			fluent.LibrariesFor("net20").AsFiles().Select(f => f.Name()).ToList().ShouldEqual(new List<string>{ "FluentNHibernate.dll" });

			(foo as UnpackedNupkg).LibraryDirectoryFor("net20").ShouldEqual(Path.Combine(foo.Path, "LiB/nEt20"));
			(foo as UnpackedNupkg).JustLibrariesFor("net20").AsFiles().Select(f => f.Name()).ToList().ShouldEqual(new List<string>{ "Hi.DlL", "TherE.dLl" });
			foo.LibrariesFor("net20").AsFiles().Select(f => f.Name()).ToList().ShouldEqual(new List<string>{ "Another.Global.DLL", "global_1.dll", "Hi.DlL", "TherE.dLl" });
		}

		[Test]
		public void just_NET35_Libraries() {
			(foo as UnpackedNupkg).LibraryDirectoryFor("NET35").ShouldEqual(Path.Combine(foo.Path, "LiB/Net35"));
			(foo as UnpackedNupkg).JustLibrariesFor("NET35").AsFiles().Select(f => f.Name()).ToList().ShouldEqual(new List<string>{ "fooooo.dll" });
			foo.LibrariesFor("NET35").AsFiles().Select(f => f.Name()).ToList().ShouldEqual(new List<string>{ "Another.Global.DLL", "global_1.dll", "fooooo.dll" });
		}

		[Test]
		public void Tools() {
			fluent.Tools.AsFiles().Select(f => f.Name()).ToList().Should(Be.Empty);
			nunit.Tools.AsFiles().Select(f => f.Name()).ToList().ShouldEqual(new List<string>{ 
				"nunit-agent-x86.exe", "nunit-agent.exe", "nunit-console-x86.exe", 
				"nunit-console.exe", "nunit-x86.exe", "nunit.exe",
				"pnunit-agent.exe", "pnunit-launcher.exe", "runFile.exe"
			});
			foo.Tools.AsFiles().Select(f => f.Name()).ToList().ShouldEqual(new List<string>{ "hi.exe" });
		}

		[Test]
		public void Content() {
			fluent.Content.AsFiles().Select(f => f.Name()).ToList().Should(Be.Empty);
			nunit.Content.AsFiles().Select(f => f.Name()).ToList().ShouldEqual(new List<string>{ "NUnitSampleTests.cs.pp" });
			foo.Content.AsFiles().Select(f => f.Name()).ToList().ShouldEqual(new List<string>{ "file.html", "hi.there" });
		}

		[Test]
		public void SourceFiles() {
			fluent.SourceFiles.AsFiles().Select(f => f.Name()).ToList().Should(Be.Empty);
			nunit.SourceFiles.AsFiles().Select(f => f.Name()).ToList().Should(Be.Empty);
			foo.SourceFiles.AsFiles().Select(f => f.Name()).ToList().ShouldEqual(new List<string>{ "hi.cs", "FooFile.cs" });
		}

		[Test]
		public void CreateNupkg() {
			PathToTemp("MarkdownSharp.1.13.0.0.nupkg").AsFile().Exists().Should(Be.False);

			var unpacked = new UnpackedNupkg(PathToContent("packages", "MarkdownSharp.1.13.0.0"));
			var nupkg    = unpacked.CreateNupkg(PathToTemp(""));

			nupkg.Path.ShouldEqual(PathToTemp("MarkdownSharp-1.13.0.0.nupkg"));
			PathToTemp("MarkdownSharp-1.13.0.0.nupkg").AsFile().Exists().Should(Be.True);
			nupkg.Files.ShouldEqual(new List<string>{ "MarkdownSharp.nuspec", "lib/35/MarkdownSharp.dll", "lib/35/MarkdownSharp.pdb", "lib/35/MarkdownSharp.xml" });
		}
	}
}
