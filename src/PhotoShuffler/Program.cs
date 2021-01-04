using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PhotoShuffler.Model;

namespace PhotoShuffler
{
	internal class Program
	{
		internal static void Main(string[] args)
		{
			try
			{
				string configurationPath = args.FirstOrDefault();
				Configuration config = LoadConfiguration(configurationPath);

				ShufflePlan shufflePlan = CreatePlan(config);

				Shuffle(shufflePlan);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ERROR: {ex.Message}");
			}
		}

		private static Configuration LoadConfiguration(string configurationPath)
		{
			if (string.IsNullOrWhiteSpace(configurationPath))
				throw new ArgumentException("No configuration path provided");

			if (!File.Exists(configurationPath))
				throw new ArgumentException("Configuration file does not exist");

			string configurationFileContent = File.ReadAllText(configurationPath);

			Configuration config = JsonConvert.DeserializeObject<Configuration>(configurationFileContent);

			if (!config.SourcePaths.Any(path => !string.IsNullOrWhiteSpace(path)))
				throw new ArgumentException("No source path provided in configuration");

			if (string.IsNullOrWhiteSpace(config.DestinationPath))
				throw new ArgumentException("No destination path provided in configuration");

			if (!config.FileExtensions.Any(ext => !string.IsNullOrWhiteSpace(ext)))
				throw new ArgumentException("No file extension provided in configuration");

			config.ConfigurationPath = configurationPath;

			return config;
		}

		private static ShufflePlan CreatePlan(Configuration config)
		{
			ShufflePlan shufflePlan = new ShufflePlan(config);
			string fileExtensionPattern = string.Join("|", config.FileExtensions.Select(ext => ext.Replace(".", @"\.")));

			string[] filePaths = config.SourcePaths
				.SelectMany(sourcePath => Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories))
				.Where(filePath => Regex.IsMatch(filePath, $"({fileExtensionPattern})$"))
				.Select(filePath => new { Only = Path.GetDirectoryName(filePath), Full = filePath })
				.Where(path => !config.ExcludeFolders.Any(excludeFolder => path.Only.Contains(excludeFolder, StringComparison.InvariantCultureIgnoreCase)))
				.Select(path => path.Full)
				.OrderBy(filePath => filePath)
				.ToArray();

			foreach (string filePath in filePaths)
			{
				try
				{
					if (!filePath.TryGetFileNameDateTime(out DateTime fileNameDateTime))
					{
						shufflePlan.AddInvalid(filePath, "Bad file name");
					}
					else if (filePath.TryGetExifTagDateTime(out DateTime tagDateTime) && fileNameDateTime.Matches(tagDateTime))
					{
						shufflePlan.Add(filePath, tagDateTime);
					}
					else
					{
						shufflePlan.Add(filePath, fileNameDateTime);
					}
				}
				catch (Exception ex)
				{
					shufflePlan.AddInvalid(filePath, ex.Message);
				}

				if (shufflePlan.Files.Count % 10 == 0)
				{
					Console.CursorLeft = 0;
					Console.Write($"Processed {shufflePlan.Files.Count * 100.0 / filePaths.Length:0.00}% ({shufflePlan.Files.Count} / {filePaths.Length})");
				}
			}

			string planOutputPath = Path.Combine(Path.GetDirectoryName(config.ConfigurationPath), $"plan-{DateTime.Now:yyyyMMdd-HHmmss}.json");
			File.WriteAllText(planOutputPath, JsonConvert.SerializeObject(shufflePlan), Encoding.UTF8);

			return shufflePlan;
		}

		private static void Shuffle(ShufflePlan shufflePlan)
		{

		}
	}
}
