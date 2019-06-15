// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// INlpDataExtractor implementation for interacting with a LUIS endpoint.
    /// </summary>
    public class LuisDataExtractor : INlpDataExtractor
    {
        /// <summary>
        /// IHttpClientWrapper implementation to use for making REST calls.
        /// </summary>
        private IHttpClientWrapper _httpClient;

        /// <summary>
        /// Configuration for data extraction.
        /// </summary>
        private NlpDataExtractorConfiguration _config;
        private HashSet<string> _expectedEntities;

        /// <summary>
        /// Initializes the data extractor.
        /// </summary>
        /// <param name="config">Configuration for data extraction.</param>
        /// <param name="httpClient">IHttpClientWrapper implementation to use for making REST calls.</param>
        public void Initialize(NlpDataExtractorConfiguration config, IHttpClientWrapper httpClient)
        {
            _config = config;
            _httpClient = httpClient;

            _expectedEntities = new HashSet<string>()
            {
                _config.DateTimeEntityName,
                _config.DateEntityName,
                _config.TimeEntityName,
                _config.PersonEntityName,
                _config.LocationEntityName,
                _config.CityEntityName,
                _config.StateEntityName,
                _config.ZipcodeEntityName,
            };
        }

        /// <summary>
        /// Extracts data from a transcription.
        /// </summary>
        /// <param name="transcript">The transcript to extract data from.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning the data extracted from the transcription.</returns>
        public async Task<TranscriptionData> ExtractAsync(string transcript, ILogger log)
        {
            try
            {
                Uri requestUri = new Uri($"{_config.NlpEndpoint}&subscription-key={_config.NlpSubscriptionKey.Value}");

                byte[] buffer = Encoding.UTF8.GetBytes($"\"{transcript}\"");
                ByteArrayContent content = new ByteArrayContent(buffer);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(requestUri.AbsoluteUri, content, log);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    // TODO: Throw a better exception
                    throw new Exception("Request failed!");
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                LuisResponse luisResponse = JsonConvert.DeserializeObject<LuisResponse>(responseContent);

                TranscriptionData data = new TranscriptionData();

                data.EvaluatedTranscription = transcript;

                if (luisResponse.TopScoringIntent != null)
                {
                    data.Intent = luisResponse.TopScoringIntent.Intent;
                    data.IntentConfidence = luisResponse.TopScoringIntent.Score;
                }

                data.Date = ExtractDateTime(luisResponse, log);
                data.Location = ExtractLocation(luisResponse, log);
                data.Person = ExtractPerson(luisResponse, log);
                data.AdditionalData = EnumerateAdditionalEntities(luisResponse, log);

                return data;
            }
            catch (HttpRequestException ex)
            {
                throw new DataExtractorException("Exception encountered extracting data! See inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Extracts date and time information from the transcript.
        /// </summary>
        /// <param name="luisResponse">The transcript.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>The extracted date and time information.</returns>
        public DateInfo ExtractDateTime(LuisResponse luisResponse, ILogger log)
        {
            DateInfo dateInfo = null;

            if (luisResponse.Entities == null
                || luisResponse.Entities.Count == 0)
            {
                return dateInfo;
            }

            LuisEntity dateTimeEntity = luisResponse
                .Entities
                .Where(x => x.Type == _config.DateTimeEntityName)
                .FirstOrDefault();

            if (dateTimeEntity == null)
            {
                DateTime outDateTime;

                LuisEntity dateEntity = luisResponse
                    .Entities
                    .Where(x => x.Type == _config.DateEntityName)
                    .FirstOrDefault();

                LuisEntity timeEntity = luisResponse
                    .Entities
                    .Where(x => x.Type == _config.TimeEntityName)
                    .FirstOrDefault();

                StringBuilder dateTimeBuilder = new StringBuilder();

                if (dateEntity != null
                    && dateEntity.Resolution != null
                    && dateEntity.Resolution.Values.Count > 0)
                {
                    dateTimeBuilder.Append($"{dateEntity.Resolution.Values.First().Value} ");
                }

                if (timeEntity != null
                    && timeEntity.Resolution != null
                    && timeEntity.Resolution.Values.Count() > 0)
                {
                    dateTimeBuilder.Append(timeEntity.Resolution.Values.First().Value);
                }

                if (DateTime.TryParse(dateTimeBuilder.ToString(), out outDateTime))
                {
                    dateInfo = new DateInfo()
                    {
                        FullDate = outDateTime,
                        Year = outDateTime.Year,
                        Month = outDateTime.Month,
                        Day = outDateTime.Day,
                        Hour = outDateTime.Hour,
                        Minute = outDateTime.Minute,
                    };

                    if (dateEntity == null)
                    {
                        dateInfo.Year = DateTime.MinValue.Year;
                        dateInfo.Month = DateTime.MinValue.Month;
                        dateInfo.Day = DateTime.MinValue.Day;
                        dateInfo.FullDate = new DateTime(dateInfo.Year, dateInfo.Month, dateInfo.Day, dateInfo.Hour, dateInfo.Minute, 0);
                    }
                }
            }
            else
            {
                if (dateTimeEntity.Resolution != null
                    && dateTimeEntity.Resolution.Values.Count > 0)
                {
                    DateTime outDateTime;

                    if (DateTime.TryParse(dateTimeEntity.Resolution.Values.First().Value, CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out outDateTime))
                    {
                        dateInfo = new DateInfo()
                        {
                            FullDate = outDateTime,
                            Year = outDateTime.Year,
                            Month = outDateTime.Month,
                            Day = outDateTime.Day,
                            Hour = outDateTime.Hour,
                            Minute = outDateTime.Minute,
                        };
                    }
                }
            }

            return dateInfo;
        }

        /// <summary>
        /// Extracts location information from the transcript.
        /// </summary>
        /// <param name="luisResponse">The transcript.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>The extracted location information.</returns>
        public LocationInfo ExtractLocation(LuisResponse luisResponse, ILogger log)
        {
            LocationInfo locationInfo = null;

            if (luisResponse.Entities == null
                || luisResponse.Entities.Count == 0)
            {
                return locationInfo;
            }

            LuisEntity locationEntity = luisResponse
                .Entities
                .Where(x => x.Type == _config.LocationEntityName)
                .FirstOrDefault();

            if (locationEntity != null)
            {
                locationInfo = new LocationInfo()
                {
                    Location = locationEntity.Entity,
                };

                if (luisResponse.CompositeEntities != null
                    && luisResponse.CompositeEntities.Count > 0)
                {
                    LuisCompositeEntity locationCompositeEntity = luisResponse
                        .CompositeEntities
                        .Where(x => x.ParentType == _config.LocationEntityName)
                        .FirstOrDefault();

                    if (locationCompositeEntity != null
                        && locationCompositeEntity.Children != null
                        && locationCompositeEntity.Children.Count > 0)
                    {
                        LuisEntity cityEntity = locationCompositeEntity
                            .Children
                            .Where(x => x.Type == _config.CityEntityName)
                            .FirstOrDefault();

                        LuisEntity stateEntity = locationCompositeEntity
                            .Children
                            .Where(x => x.Type == _config.StateEntityName)
                            .FirstOrDefault();

                        LuisEntity zipCodeEntity = locationCompositeEntity
                            .Children
                            .Where(x => x.Type == _config.ZipcodeEntityName)
                            .FirstOrDefault();

                        if (cityEntity != null)
                        {
                            locationInfo.City = cityEntity.Value;
                        }

                        if (stateEntity != null)
                        {
                            locationInfo.State = stateEntity.Value;
                        }

                        if (zipCodeEntity != null)
                        {
                            locationInfo.Zipcode = zipCodeEntity.Value;
                        }
                    }
                }
            }

            return locationInfo;
        }

        /// <summary>
        /// Extracts person information from the transcript.
        /// </summary>
        /// <param name="luisResponse">The transcript.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>The extracted person information.</returns>
        public PersonInfo ExtractPerson(LuisResponse luisResponse, ILogger log)
        {
            PersonInfo personInfo = null;

            if (luisResponse.Entities == null
                || luisResponse.Entities.Count == 0)
            {
                return personInfo;
            }

            LuisEntity personEntity = luisResponse
                .Entities
                .Where(x => x.Type == _config.PersonEntityName)
                .FirstOrDefault();

            if (personEntity != null)
            {
                personInfo = new PersonInfo()
                {
                    Name = personEntity.Entity,
                    Type = luisResponse.TopScoringIntent != null
                                ? (_config.PersonIntentTypeMap.ContainsKey(luisResponse.TopScoringIntent.Intent) ? _config.PersonIntentTypeMap[luisResponse.TopScoringIntent.Intent] : "Unknown")
                                : "Unknown",
                };
            }

            return personInfo;
        }

        /// <summary>
        /// Enumerates and returns additional entities returned by LUIS.
        /// </summary>
        /// <param name="luisResponse">The response from LUIS.</param>
        /// <param name="log">A logger instance.</param>
        /// <returns>A dictionary representing the additional entities.</returns>
        public Dictionary<string, string> EnumerateAdditionalEntities(LuisResponse luisResponse, ILogger log)
        {
            if (luisResponse.Entities == null)
            {
                return null;
            }

            IList<LuisEntity> extraEntities = luisResponse
                .Entities
                .Where(x => !_expectedEntities.Contains(x.Type))
                .ToList();

            Dictionary<string, string> extraEntitiesDictionary = new Dictionary<string, string>();

            int keySuffixNumber = 1;

            foreach (LuisEntity entity in extraEntities)
            {
                string key = entity.Type;

                while (extraEntitiesDictionary.ContainsKey(key))
                {
                    key = $"{key}-{++keySuffixNumber}";
                }

                extraEntitiesDictionary.Add(key, entity.Entity);
            }

            return extraEntitiesDictionary;
        }
    }
}
