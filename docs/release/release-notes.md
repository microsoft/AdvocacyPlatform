# Advocacy Platform Release Notes

Prior release tag notes are also listed here
for completeness.

## Advocacy Platform Releases

### Jun 24, 2019
#### Breaking Changes:
 - Renamed model property "date" in response body for ExtractInfo to "dates"
 - Model property "dates" (see above) is now an array of DateInfo objects.

#### Features:
 - Response for a request to ExtractInfo will additionally set *dateRejected* flag and return status code *MissingEntities* if more than one datetime, date/time pair, date, or time entities are extracted.

### Apr 29, 2019

#### Features:
 - Initial test release to latest-stable tag

#### Known Issues/Limitations:
 - N/A