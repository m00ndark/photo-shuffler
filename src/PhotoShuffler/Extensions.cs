using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.FileType;
using MetadataExtractor.Formats.QuickTime;
using MetadataDirectory = MetadataExtractor.Directory;

namespace PhotoShuffler
{
	internal static class Extensions
	{
		public static bool TryGetFileNameDateTime(this string imageFilePath, out DateTime dateTime)
		{
			dateTime = DateTime.MinValue;

			string imageFileName = Path.GetFileNameWithoutExtension(imageFilePath);
			Match imageFileNameMatch = Regex.Match(imageFileName, @"^(\d{8}_\d{6})(\b|_).*$");

			if (!imageFileNameMatch.Success)
				return false;

			string washedImageFileName = imageFileNameMatch.Groups[1].Value;

			return DateTime.TryParseExact(washedImageFileName, "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
		}

		public static bool TryGetMetadataTagDateTime(this string imageFilePath, out DateTime dateTime)
		{
			dateTime = DateTime.MinValue;

			IReadOnlyList<MetadataDirectory> directories = ImageMetadataReader.ReadMetadata(imageFilePath);

			FileTypeDirectory fileTypeDirectory = directories?
				.OfType<FileTypeDirectory>()
				.FirstOrDefault();

			if (fileTypeDirectory == null)
				return false;

			if (fileTypeDirectory.GetDescription(FileTypeDirectory.TagDetectedFileTypeName) == "JPEG")
			{
				ExifSubIfdDirectory subIfdDirectory = directories
					.OfType<ExifSubIfdDirectory>()
					.FirstOrDefault();

				return subIfdDirectory != null
					&& subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out dateTime);
			}

			if (fileTypeDirectory.GetDescription(FileTypeDirectory.TagDetectedFileTypeName) == "QuickTime")
			{
				QuickTimeMovieHeaderDirectory quickTimeMovieHeaderDirectory = directories
					.OfType<QuickTimeMovieHeaderDirectory>()
					.FirstOrDefault();

				return quickTimeMovieHeaderDirectory != null
					&& quickTimeMovieHeaderDirectory.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagCreated, out dateTime);
			}

			return false;
		}

		public static void PrintAllMetadata(this string imageFilePath)
		{
			foreach (var directory in ImageMetadataReader.ReadMetadata(imageFilePath))
			{
				foreach (var tag in directory.Tags)
				{
					Console.WriteLine($"{directory.Name} - {tag.Name} = {tag.Description}");
				}
			}
		}
	}
}
