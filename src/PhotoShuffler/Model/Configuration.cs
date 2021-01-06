using System.Linq;

namespace PhotoShuffler.Model
{
	internal class Configuration
	{
		public string Path { get; set; }
		public Job[] Jobs { get; set; }

		public void Sanitize()
		{
			Jobs = Jobs.Where(job => job != null).ToArray();

			foreach (Job job in Jobs)
			{
				job.Sanitize();
			}
		}

		internal class Job
		{
			public string[] SourcePaths { get; set; }
			public string DestinationPath { get; set; }
			public string[] FileExtensions { get; set; }
			public string[] ExcludeFolders { get; set; }

			public void Sanitize()
			{
				SourcePaths = SourcePaths.Where(sourcePath => !string.IsNullOrWhiteSpace(sourcePath)).ToArray();
				FileExtensions = FileExtensions.Where(fileExtension => !string.IsNullOrWhiteSpace(fileExtension)).ToArray();
				ExcludeFolders = ExcludeFolders.Where(excludeFolder => !string.IsNullOrWhiteSpace(excludeFolder)).ToArray();
			}
		}
	}
}
