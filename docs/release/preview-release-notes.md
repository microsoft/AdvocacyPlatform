# Advocacy Platform Preview Release Notes

The Advocacy Platform project performs continuous integration and releases to a pre-release version tag. The following specifies the accumulative changes for all preview release leading up to a stable release.

## Advocacy Platform Preview Release

### Jun 24, 2019
#### Breaking Changes:
 - Renamed model property "date" in response body for ExtractInfo to "dates"
 - Model property "dates" (see above) is now an array of DateInfo objects.

#### Features:
 - Response for a request to ExtractInfo will additionally set *dateRejected* flag and return status code *MissingEntities* if more than one datetime, date/time pair, date, or time entities are extracted.