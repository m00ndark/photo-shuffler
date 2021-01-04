namespace PhotoShuffler.Model
{
	internal class Configuration
	{
		public string ConfigurationPath { get; set; }
		public string[] SourcePaths { get; set; }
		public string DestinationPath { get; set; }
		public string[] FileExtensions { get; set; }
		public string[] ExcludeFolders { get; set; }
	}
}
