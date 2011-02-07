using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MooGet {

	/// <summary>Represents a directory.  Any class that has a public string Path property can be an IDirectory</summary>
	/// <remarks>Why?  So you can use the IDirectoryExtensions with your IDirectory class to Copy(), Move(), Delete(), etc the Directory.</remarks>
	public interface IDirectory {
		string Path { get; set; }
	}

	/// <summary>Concrete implementation of IDirectory</summary>
	public class RealDirectory : IDirectory {
		public RealDirectory(){}
		public RealDirectory(string path){ Path = path; }
		public string Path { get; set; }
		public override string ToString(){ return Path; }
	}

	/// <summary>The actual methods for an IDirectory</summary>
	public static class IDirectoryExtensions {

		public static bool   Exists(this IDirectory dir) { return Directory.Exists(dir.Path); }
		public static string Name(this IDirectory dir)   { return Path.GetFileName(dir.Path); }
		public static void   Delete(this IDirectory dir) { Directory.Delete(dir.Path); }

		/// <summary>Creates this directory (if it doesn't exist)</summary>
		public static IDirectory Create(this IDirectory dir) {
			if (! dir.Exists())
				Directory.CreateDirectory(dir.Path);
			return dir;
		}

		/// <summary>
		/// Returns an IFile with the specified path relative to this IDirectory's Path. 
		/// It might not exist, but we don't return null!
		/// </summary>
		public static IFile GetFile(this IDirectory dir, string path) {
			return new RealFile(Path.Combine(dir.Path, path));
		}

		/// <summary>
		/// Returns an IDirectory with the specified path relative to this IDirectory's Path. 
		/// It might not exist, but we don't return null!
		/// </summary>
		public static IDirectory GetDirectory(this IDirectory dir, string path) {
			return new RealDirectory(Path.Combine(dir.Path, path));
		}

		/// <summary>Shortcut to GetDirectory</summary>
		public static IDirectory GetDir(this IDirectory dir, string path){ return dir.GetDirectory(path); }

		public static IDirectory Copy(this IDirectory dir, params string[] destinationParts) {
			var destination = destinationParts.Combine();
			if (Directory.Exists(destination))
				dir.CopyToExactPath(Path.Combine(destination, dir.Name()));
			else
				dir.CopyToExactPath(destination);
			return dir;
		}

		public static string Relative(this IDirectory dir, string fullPath) {
			return fullPath.Replace(dir.Path, "");
		}

		public static IDirectory CopyToExactPath(this IDirectory dir, string exactPath) {
			Console.WriteLine("CopyToExactPath {0} --> {1}", dir.Path, exactPath);
			// make the directories
			//dir.Directories().ForEach(fullDir => Console.WriteLine("create: " + Path.Combine(exactPath, dir.Relative(fullDir.Path))));
			dir.Dirs().ForEach(subdir => Console.WriteLine(subdir.Path));
			// copy the files

			dir.Path = exactPath;
			return dir;
		}

		public static IDirectory Move(this IDirectory dir, params string[] destinationParts) {
			var destination = destinationParts.Combine();
			if (Directory.Exists(destination)) {
				// move INTO the existing directory
				var newPath = Path.Combine(destination, dir.Name());
				Directory.Move(dir.Path, newPath);
				dir.Path = newPath;
			} else {
				// move to the exact destination
				Directory.Move(dir.Path, destination);
				dir.Path = destination;
			}
			return dir;
		}

		/// <summary>Returns all of the files in this directory (as as list of IFile)</summary>
		public static List<IFile> Files(this IDirectory dir) {
			return Directory.GetFiles(dir.Path, "*", SearchOption.AllDirectories).Select(path => new RealFile(path) as IFile).ToList();
		}

		/// <summary>Returns all of the directories in this directory (as as list of IDirectory)</summary>
		public static List<IDirectory> Directories(this IDirectory dir) {
			return Directory.GetDirectories(dir.Path, "*", SearchOption.AllDirectories).Select(path => new RealDirectory(path) as IDirectory).ToList();
		}

		/// <summary>Shortcut for Directories()</summary>
		public static List<IDirectory> Dirs(this IDirectory dir) { return dir.Directories(); }

		public static List<IFile> Search(this IDirectory dir, string matcher) {
			return dir.Search(matcher, true);
		}

		public static List<IFile> Search(this IDirectory dir, string matcher, bool ignoreCase) {
			// We have to initially substitude ** out for something besides a single * because then we replace single *'s.
			matcher   = "^" + matcher.Replace("\\", "/").Replace("**", ".REAL_REGEX_STAR").Replace("*", @"[^\/]+").Replace("REAL_REGEX_STAR", "*") + "$";
			var regex = ignoreCase ? new Regex(matcher, RegexOptions.IgnoreCase) : new Regex(matcher);
			return dir.Search(regex);
		}

		public static List<IFile> Search(this IDirectory dir, Regex regex) {
			return dir.Files().Where(file => {
				var relative = file.Path.Replace(dir.Path, "").TrimStart('\\').TrimStart('/');
				return regex.IsMatch(relative);
			}).ToList();
		}

		public static string ToString(this IDirectory dir) { return dir.Path; }
	}
}
