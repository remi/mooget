using System;
using System.Linq;
using MooGet;
using NUnit.Framework;

namespace MooGet.Specs {

	[TestFixture]
	public class ReadingNuspecSpec : MooGetSpec {
	
		[Test]
		public void NUnit_example() {
			var package = Package.FromSpec(PathToContent("packages", "NUnit.2.5.7.10213", "NUnit.nuspec"));

			package.Id.ShouldEqual("NUnit");
			package.VersionString.ShouldEqual("2.5.7.10213");
			package.Description.ShouldEqual("NUnit is a unit-testing framework for all .Net languages. Initially ported from JUnit, the current production release, version 2.5, is the sixth major release of this xUnit based unit testing tool for Microsoft .NET.");
			package.Authors.Count.ShouldEqual(1);
			package.Authors.First().ShouldEqual("Charlie Poole");
			package.Language.ShouldEqual("en-US");
			package.RequireLicenseAcceptance.ShouldBeFalse();
			package.Created.ShouldEqual(DateTime.Parse("2010-10-25T22:55:39.6602+00:00"));
			package.Modified.ShouldEqual(DateTime.Parse("2010-10-25T22:55:39.6602+00:00"));
			package.LicenseUrl.Should(Be.Null);
			package.Dependencies.Should(Be.Empty);
		}

		[Test]
		public void FluentNHibernate_example() {
			var package = Package.FromSpec(PathToContent("packages", "FluentNHibernate.1.1.0.694", "FluentNHibernate.nuspec"));

			package.Id.ShouldEqual("FluentNHibernate");
			package.VersionString.ShouldEqual("1.1.0.694");
			package.Description.ShouldEqual("Fluent, XML-less, compile safe, automated,  convention-based mappings for NHibernate.  Get your fluent on.");
			package.Authors.Count.ShouldEqual(1);
			package.Authors.First().ShouldEqual("James Gregory");
			package.Language.ShouldEqual("en-US");
			package.RequireLicenseAcceptance.ShouldBeFalse();
			package.Created.ShouldEqual(DateTime.Parse("2010-10-25T22:55:28.92+00:00"));
			package.Modified.ShouldEqual(DateTime.Parse("2010-10-25T22:55:28.921+00:00"));
			package.LicenseUrl.ShouldEqual("http://github.com/jagregory/fluent-nhibernate/raw/master/LICENSE.txt");
			package.Dependencies.Count.ShouldEqual(1);
			package.Dependencies.First().Id.ShouldEqual("NHibernate.Core");
			package.Dependencies.First().VersionString.ShouldEqual("2.1.2.4000");
			package.Dependencies.First().MinVersionString.Should(Be.Null);
			package.Dependencies.First().MaxVersionString.Should(Be.Null);
		}

		[Test][Ignore]
		public void Ninject_example() {}

		[Test][Ignore]
		public void MarkdownSharp_example() {}

		// TODO find or make examples with:
		// summary
		// tags
		// iconUrl
		// projectUrl
		// dependencies
		// files specified
		// owners
		// <author> and <owner> should be able to have an email= attribute?  could be useful for permissions when pushing/etc
	}
}