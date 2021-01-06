# Photo Shuffler
Photo Shuffler is an application that moves media files from a set of source paths to a dynamically described destination path. Can be used to move photos and videos uploaded from a mobile phone to a folder structure based on the date it was taken/shot.

## Configuration
Create a configuration file with JSON content according to the following example:
```json
{
  "Jobs": [
    {
      "SourcePaths": [
        "Z:\\photos\\_mobile\\Person_A\\100MEDIA",
        "Z:\\photos\\_mobile\\Person_A\\Camera"
      ],
      "DestinationPath": "Z:\\photos\\%date:yyyy%\\%date:yyyy-MM-dd% [mobile]\\%file:name%_A%file:ext%",
      "FileExtensions": [
        ".jpg",
        ".3gp",
        ".mp4"
      ],
      "ExcludeFolders": [
        "\\[Originals\\]"
      ]
    },
    {
      "SourcePaths": [
        "Z:\\photos\\_mobile\\Person_B\\Camera"
      ],
      "DestinationPath": "Z:\\photos\\%date:yyyy%\\%date:yyyy-MM-dd% [mobile]\\%file:name%_B%file:ext%",
      "FileExtensions": [
        ".jpg",
        ".3gp",
        ".mp4"
      ],
      "ExcludeFolders": [
        "\\[Originals\\]"
      ]
    }
  ]
}
```
The appilication will scan all source paths including subfolders for files with the specified file extensions, create and save a plan to move the files to dynamically generated destination paths, then execute the move according to the plan. The scanned folders will not include the specified exclude folders (regex patterns). The date and time of when the image was taken/video was shot is identified either by the format of the file name (starting with `yyyyMMdd_HHmmss`) or by EXIF or QuickTime metadata tags. The destination path can be dynamically described using certain tags as follows:
Tag | Replacement
--- | -----------
`%date:<format>%` | A date and time representation of when the image was taken/video was shot according to the specified standard or custom date and time format, see .NET's [Standard Date and Time Format Strings](https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings) and [Custom Date and Time Format Strings](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings)
`%file:name%` | The name of the media file, without the file extension
`%file:ext%` | The file extension of the media file, including the dot

## Generated plan
The generated plan will placed next to the configuration file and include a copy of the configuration used. The following is an example of a plan generated using the configuration above:
```json
{
  "Configuration": { ... },
  "Files": [
    {
      "SourceFilePath": "Z:\\photos\\_mobile\\Person_A\\Camera\\20201231_235718.jpg",
      "FileName": "20201231_235718.jpg",
      "FileDate": "2020-12-31T23:57:18",
      "Error": null,
      "Valid": true,
      "DestinationFilePath": "Z:\\photos\\2020\\2020-12-31 [mobile]\\20201231_235718_A.jpg"
    },
    {
      "SourceFilePath": "Z:\\photos\\_mobile\\Person_B\\Camera\\20210101_115104.jpg",
      "FileName": "20210101_115104.jpg",
      "FileDate": "2021-01-01T11:51:04",
      "Error": null,
      "Valid": true,
      "DestinationFilePath": "Z:\\photos\\2021\\2021-01-01 [mobile]\\20210101_115104_B.jpg"
    }
  ]
}
```
