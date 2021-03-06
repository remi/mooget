using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using MooGet;
using NUnit.Framework;

namespace MooGet.Specs.CLI {

	[TestFixture]
	public class SourceCommandSpec : Spec {

		int defaultSourceCount   = 0; // how many sources you have by default when you use moo
		MooDir mooDir            = new MooDir(PathToTemp("home", ".moo"));
		DirectoryOfNupkg source1 = new DirectoryOfNupkg(PathToContent("packages")); // <--- relative
		DirectoryOfNupkg source2 = new DirectoryOfNupkg(PathToContent("more_packages"));

		[Test]
		public void should_be_setup_properly() {
			mooDir.Exists().Should(Be.False); // hasn't been initialized yet
			source1.Packages.Count.ShouldEqual(5);
			source2.Packages.Count.ShouldEqual(13);
		}

		[Test][Ignore]
		public void default_sources() {
			// ... test the default sources ...
		}

		[Test][Description("moo help source")]
		public void help() {
			moo("help source").ShouldContain("Usage: moo source");
		}

		[Test][Description("moo source add /home/me/packages")]
		public void add_without_name() {
			moo("source").ShouldNotContain(source1.Path);
			moo("source").ShouldNotContain(source2.Path);
			mooDir.Sources.Count.ShouldEqual(defaultSourceCount);

			moo("source add {0}", "../../content/packages"); // <--- add using relative path, to our working directory
			moo("source").ShouldContain(Path.GetFullPath(source1.Path)); // <-- the FULL path should be saved
			moo("source").ShouldNotContain(source2.Path);
			mooDir.Sources.Count.ShouldEqual(defaultSourceCount + 1);

			moo("source add {0}", source2.Path);
			moo("source").ShouldContain(source1.Path);
			moo("source").ShouldContain(source2.Path);
			mooDir.Sources.Select(s => s.Path).ToList().ShouldEqual(new List<string>{ PathToContent("packages"), PathToContent("more_packages") });
			mooDir.Sources.Select(s => s.GetType()).ToList().ShouldEqual(new List<Type>{ typeof(DirectoryOfNupkg), typeof(DirectoryOfNupkg) });
			mooDir.Sources.Count.ShouldEqual(defaultSourceCount + 2);
		}

		[Test][Description("moo source add awesome /home/me/packages")]
		public void add_with_name() {
			var output = moo("source");
			output.ShouldNotContain(source1.Path);
			output.ShouldNotContain("Awesome");
			output.ShouldNotContain(source2.Path);
			output.ShouldNotContain("Totally Cool");
			mooDir.Sources.Count.ShouldEqual(defaultSourceCount);

			moo("source add Awesome ../../content/packages"); // <--- add using relative path, to our working directory

			output = moo("source");
			output.ShouldContain(Path.GetFullPath(source1.Path)); // <-- the FULL path should be saved
			output.ShouldContain("Awesome");
			output.ShouldNotContain(source2.Path);
			output.ShouldNotContain("Totally Cool");
			mooDir.Sources.Count.ShouldEqual(defaultSourceCount + 1);

			moo("source add \"Totally Cool\" {0}", source2.Path);

			output = moo("source");
			output.ShouldContain(source1.Path);
			output.ShouldContain("Awesome");
			output.ShouldContain(source2.Path);
			output.ShouldContain("Totally Cool");
			mooDir.Sources.Select(s => s.Name).ToList().ShouldEqual(new List<string>{ "Awesome", "Totally Cool" });
			mooDir.Sources.Select(s => s.Path).ToList().ShouldEqual(new List<string>{ PathToContent("packages"), PathToContent("more_packages") });
			mooDir.Sources.Select(s => s.GetType()).ToList().ShouldEqual(new List<Type>{ typeof(DirectoryOfNupkg), typeof(DirectoryOfNupkg) });
			mooDir.Sources.Count.ShouldEqual(defaultSourceCount + 2);
		}

		[Test][Description("moo source rm ../../content/packages")][Ignore]
		public void rm_using_path() {
			moo("source add Awesome ../../content/packages"); // <--- add using relative path, to our working directory
			moo("source add \"Totally Cool\" {0}", source2.Path);
			mooDir.Sources.Select(s => s.Name).ToList().ShouldEqual(new List<string>{ "Awesome", "Totally Cool" });
			mooDir.Sources.Select(s => s.Path).ToList().ShouldEqual(new List<string>{ PathToContent("packages"), PathToContent("more_packages") });
			mooDir.Sources.Count.ShouldEqual(defaultSourceCount + 2);

			moo("source rm ../../content/packages"); // relative should turn into a full path, so you can remove ok ...

			mooDir.Sources.Count.ShouldEqual(defaultSourceCount + 1);
			mooDir.Sources.Select(s => s.Name).ToList().ShouldEqual(new List<string>{ "Totally Cool" });
			mooDir.Sources.Select(s => s.Path).ToList().ShouldEqual(new List<string>{ PathToContent("more_packages") });

			moo("source rm {0}", source2.Path);

			mooDir.Sources.Count.ShouldEqual(defaultSourceCount);
		}

		[Test][Description("moo source rm \"My Source\"")][Ignore]
		public void rm_using_name() {
			moo("source add Awesome ../../content/packages"); // <--- add using relative path, to our working directory
			moo("source add \"Totally Cool\" {0}", source2.Path);
			mooDir.Sources.Select(s => s.Name).ToList().ShouldEqual(new List<string>{ "Awesome", "Totally Cool" });
			mooDir.Sources.Select(s => s.Path).ToList().ShouldEqual(new List<string>{ PathToContent("packages"), PathToContent("more_packages") });
			mooDir.Sources.Count.ShouldEqual(defaultSourceCount + 2);

			moo("source rm Awesome");

			mooDir.Sources.Count.ShouldEqual(defaultSourceCount + 1);
			mooDir.Sources.Select(s => s.Name).ToList().ShouldEqual(new List<string>{ "Totally Cool" });
			mooDir.Sources.Select(s => s.Path).ToList().ShouldEqual(new List<string>{ PathToContent("more_packages") });

			moo("source rm \"Totally Cool\"", source2.Path);

			mooDir.Sources.Count.ShouldEqual(defaultSourceCount);
		}

		[Test][Description("moo source add http://whatever")][Ignore]
		public void add_nuget_http_source() {
			moo("source add http://whatever").ShouldContain("?");
		}

		[Test][Description("moo source add \"Official NuGet\" http://packages.nuget.org/v1/FeedService.svc")][Ignore]
		public void add_nuget_http_source_with_name() {
		}

		// Once we have default sources, we need to make sure that they can be removed ...
		[Test][Ignore]
		public void removing_default_sources() {
		}

		// TODO sub commands for clearing or updating the cache (when we add caching)
	}
}
