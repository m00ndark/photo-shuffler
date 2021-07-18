using System;
using System.Collections.Generic;
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
			Configuration config = configurationFileContent.Deserialize<Configuration>();
			config.Sanitize();

			if (!config.Jobs.Any())
				throw new ArgumentException("No jobs provided in configuration");

			foreach (Configuration.Job job in config.Jobs)
			{
				if (!job.SourcePaths.Any())
					throw new ArgumentException("No source path provided in configuration");

				if (string.IsNullOrWhiteSpace(job.DestinationPath))
					throw new ArgumentException("No destination path provided in configuration");

				if (!job.FileExtensions.Any())
					throw new ArgumentException("No file extension provided in configuration");
			}

			config.Path = configurationPath;

			return config;
		}

		private static ShufflePlan CreatePlan(Configuration config)
		{
			ShufflePlan shufflePlan = new ShufflePlan(config);

			Console.WriteLine("Scanning files..");

			(Configuration.Job Job, string filePath)[] jobFilePaths = config.Jobs
				.Select(job => new
					{
						Job = job,
						FileExtensionPattern = string.Join("|", job.FileExtensions.Select(ext => ext.Replace(".", @"\.")))
					})
				.SelectMany(x => x.Job.SourcePaths
					.SelectMany(sourcePath => Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories))
					.Where(filePath => Regex.IsMatch(filePath, $"({x.FileExtensionPattern})$"))
					.Select(filePath => new { Only = Path.GetDirectoryName(filePath).Split('\\'), Full = filePath })
					.Where(path => !x.Job.ExcludeFolders.Any(excludeFolder => path.Only.Any(part => Regex.IsMatch(part, excludeFolder))))
					.Select(path => path.Full)
					.OrderBy(filePath => filePath), (x, filePath) => (x.Job, filePath))
				.ToArray();

			foreach ((Configuration.Job job, string filePath) in jobFilePaths)
			{
				try
				{
					if (filePath.TryGetFileNameDateTime(out DateTime dateTime) || filePath.TryGetMetadataTagDateTime(out dateTime))
					{
						shufflePlan.Add(filePath, dateTime, job);
					}
					else
					{
						shufflePlan.AddInvalid(filePath, "Unable to determine file date");
					}
				}
				catch (Exception ex)
				{
					shufflePlan.AddInvalid(filePath, $"{ex.GetType().Name}: {ex.Message}");
				}

				if (shufflePlan.Files.Count % 10 == 0 || shufflePlan.Files.Count == jobFilePaths.Length)
				{
					Console.CursorLeft = 0;
					Console.Write($"Processed {shufflePlan.Files.Count * 100.0 / jobFilePaths.Length:0.00}% ({shufflePlan.Files.Count} / {jobFilePaths.Length})");
				}
			}

			Console.WriteLine();

			int invalidCount = shufflePlan.Files.Count(x => !x.Valid);
			if (invalidCount > 0)
			{
				Console.WriteLine($"Detected {invalidCount} invalid files");
			}

			if (shufflePlan.Files.Any())
			{
				string planOutputPath = Path.Combine(Path.GetDirectoryName(config.Path), $"plan-{DateTime.Now:yyyyMMdd-HHmmss}.json");
				Console.WriteLine($"Writing plan to {planOutputPath} ..");
				File.WriteAllText(planOutputPath, shufflePlan.Serialize(), Encoding.UTF8);
			}

			return shufflePlan;
		}

		private static void Shuffle(ShufflePlan shufflePlan)
		{
			if (!shufflePlan.Files.Any())
			{
				Console.WriteLine("No matching files found");
				Console.WriteLine("Done");
				return;
			}

			Console.WriteLine("Shuffling files..");

			int movedCount = 0, errorCount = 0;
			foreach (FileData fileData in shufflePlan.Files)
			{
				try
				{
					string destinationPath = Path.GetDirectoryName(fileData.DestinationFilePath);
					Directory.CreateDirectory(destinationPath);

					File.Move(fileData.SourceFilePath, fileData.DestinationFilePath);
					movedCount++;

					Console.CursorLeft = 0;
					Console.Write($"Moved {movedCount * 100.0 / shufflePlan.Files.Count:0.00}% ({movedCount} / {shufflePlan.Files.Count})");
				}
				catch (Exception ex)
				{
					errorCount++;
					string relativeFilePath = string.Concat(fileData.SourceFilePath.SkipWhile((ch, i) => fileData.DestinationFilePath[i] == ch));
					int cursorPos = Console.CursorTop;
					Console.WriteLine();
					Console.WriteLine($"Failed to move {relativeFilePath}: {ex.Message}");
					Console.CursorTop = cursorPos;
				}
			}

			Console.CursorTop += errorCount;
			Console.WriteLine();
			Console.WriteLine("Done");
		}
	}
}
