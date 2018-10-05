﻿/*
 * Developer: Ramtin Jokar [ Ramtinak@live.com ] [ My Telegram Account: https://t.me/ramtinak ]
 * 
 * Github source: https://github.com/ramtinak/InstagramApiSharp
 * Nuget package: https://www.nuget.org/packages/InstagramApiSharp
 * 
 * IRANIAN DEVELOPERS
 */
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using InstagramApiSharp.Logger;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using InstagramApiSharp.Helpers;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using InstagramApiSharp.Converters;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Classes.Models;
using System.Net;
using InstagramApiSharp.Converters.Json;
using InstagramApiSharp.Enums;
using InstagramApiSharp.Classes.ResponseWrappers.Business;
using InstagramApiSharp.Classes.Models.Business;

namespace InstagramApiSharp.API.Processors
{
    internal class BusinessProcessor : IBusinessProcessor
    {
        #region Properties and constructor
        private readonly AndroidDevice _deviceInfo;
        private readonly IHttpRequestProcessor _httpRequestProcessor;
        private readonly IInstaLogger _logger;
        private readonly UserSessionData _user;
        private readonly UserAuthValidate _userAuthValidate;
        private readonly InstaApi _instaApi;
        private readonly HttpHelper _httpHelper;
        public BusinessProcessor(AndroidDevice deviceInfo, UserSessionData user,
            IHttpRequestProcessor httpRequestProcessor, IInstaLogger logger,
            UserAuthValidate userAuthValidate, InstaApi instaApi, HttpHelper httpHelper)
        {
            _deviceInfo = deviceInfo;
            _user = user; 
            _httpRequestProcessor = httpRequestProcessor;
            _logger = logger;
            _userAuthValidate = userAuthValidate;
            _instaApi = instaApi;
            _httpHelper = httpHelper;
        }
        #endregion Properties and constructor
        /// <summary>
        ///     Get statistics of current account
        /// </summary>
        public async Task<IResult<InstaStatistics>> GetStatisticsAsync()
        {
            UserAuthValidator.Validate(_userAuthValidate);
            try
            {
                var instaUri = UriCreator.GetGraphStatisticsUri(InstaApiConstants.ACCEPT_LANGUAGE);
                var queryParamsData = new JObject
                {
                    {"access_token", ""},
                    {"id", _user.LoggedInUser.Pk.ToString()}
                };
                var variables = new JObject
                {
                    {"query_params", queryParamsData},
                    {"timezone", InstaApiConstants.TIMEZONE}
                };
                var data = new Dictionary<string, string>
                {
                    {"access_token", "undefined"},
                    {"fb_api_caller_class", "RelayModern"},
                    {"variables", variables.ToString(Formatting.None)},
                    {"doc_id", "1618080801573402"}
                };
                var request =
                    _httpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<InstaStatistics>(response, json);
                var obj = JsonConvert.DeserializeObject<InstaStatisticsRootResponse>(json);
                return Result.Success(ConvertersFabric.Instance.GetStatisticsConverter(obj).Convert());
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<InstaStatistics>(exception);
            }
        }
        /// <summary>
        ///     Get media insights
        /// </summary>
        /// <param name="mediaPk">Media PK (<see cref="InstaMedia.Pk"/>)</param>
        public async Task<IResult<InstaMediaInsights>> GetMediaInsightsAsync(string mediaPk)
        {
            UserAuthValidator.Validate(_userAuthValidate);
            try
            {
                var instaUri = UriCreator.GetMediaSingleInsightsUri(mediaPk);
                var request =
                    _httpHelper.GetDefaultRequest(HttpMethod.Get, instaUri, _deviceInfo);
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<InstaMediaInsights>(response, json);
                var obj = JsonConvert.DeserializeObject<InstaMediaInsightsContainer>(json);
                return Result.Success(obj.MediaOrganicInsights);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<InstaMediaInsights>(exception);
            }
        }
        /// <summary>
        ///     Get full media insights
        /// </summary>
        /// <param name="mediaId">Media id (<see cref="InstaMedia.InstaIdentifier"/>)</param>
        public async Task<IResult<InstaFullMediaInsights>> GetFullMediaInsightsAsync(string mediaId)
        {
            UserAuthValidator.Validate(_userAuthValidate);
            try
            {
                var instaUri = UriCreator.GetGraphStatisticsUri(InstaApiConstants.ACCEPT_LANGUAGE, InstaInsightSurfaceType.Post);

                var queryParamsData = new JObject
                {
                    {"access_token", ""},
                    {"id", mediaId}
                };
                var variables = new JObject
                {
                    {"query_params", queryParamsData}
                };
                var data = new Dictionary<string, string>
                {
                    {"access_token", "undefined"},
                    {"fb_api_caller_class", "RelayModern"},
                    {"variables", variables.ToString(Formatting.None)},
                    {"doc_id", "1527362987318283"}
                };
                var request =
                    _httpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                
                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<InstaFullMediaInsights>(response, json);
                var obj = JsonConvert.DeserializeObject<InstaFullMediaInsightsRootResponse>(json);
                return Result.Success(ConvertersFabric.Instance.GetFullMediaInsightsConverter(obj.Data.Media).Convert());
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<InstaFullMediaInsights>(exception);
            }
        }
        
        #region Direct threads
        /// <summary>
        ///     Star direct thread
        /// </summary>
        /// <param name="threadId">Thread id</param>
        public async Task<IResult<bool>> StarDirectThreadAsync(string threadId)
        {
            UserAuthValidator.Validate(_userAuthValidate);
            try
            {
                var instaUri = UriCreator.GetStarThreadUri(threadId);

                var data = new Dictionary<string, string>
                {
                    {"thread_label", "1"},
                    {"_csrftoken", _user.CsrfToken},
                    {"_uuid", _deviceInfo.DeviceGuid.ToString()}
                };
                var request =
                    _httpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<bool>(response, json);
                var obj = JsonConvert.DeserializeObject<InstaDefault>(json);
                return obj.Status.ToLower() == "ok" ? Result.Success(true) : Result.UnExpectedResponse<bool>(response, json);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<bool>(exception);
            }
        }

        /// <summary>
        ///     Unstar direct thread
        /// </summary>
        /// <param name="threadId">Thread id</param>
        public async Task<IResult<bool>> UnStarDirectThreadAsync(string threadId)
        {
            UserAuthValidator.Validate(_userAuthValidate);
            try
            {
                var instaUri = UriCreator.GetUnStarThreadUri(threadId);

                var data = new Dictionary<string, string>
                {
                    {"_csrftoken", _user.CsrfToken},
                    {"_uuid", _deviceInfo.DeviceGuid.ToString()}
                };
                var request =
                    _httpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<bool>(response, json);
                var obj = JsonConvert.DeserializeObject<InstaDefault>(json);
                return obj.Status.ToLower() == "ok" ? Result.Success(true) : Result.UnExpectedResponse<bool>(response, json);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<bool>(exception);
            }
        }
        #endregion Direct threads

        /// <summary>
        ///     Get promotable media feeds
        /// </summary>
        public async Task<IResult<InstaMediaList>> GetPromotableMediaFeedsAsync()
        {
            var mediaList = new InstaMediaList();
            try
            {
                var instaUri = UriCreator.GetPromotableMediaFeedsUri();
                var request = _httpHelper.GetDefaultRequest(HttpMethod.Get, instaUri, _deviceInfo);
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<InstaMediaList>(response, json);
                var mediaResponse = JsonConvert.DeserializeObject<InstaMediaListResponse>(json,
                    new InstaMediaListDataConverter());

                mediaList = ConvertersFabric.Instance.GetMediaListConverter(mediaResponse).Convert();
                mediaList.PageSize = mediaResponse.ResultsCount;
                return Result.Success(mediaList);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail(exception, mediaList);
            }
        }

        /// <summary>
        ///     Get business get buttons (partners)
        /// </summary>
        public async Task<IResult<InstaBusinessPartnersList>> GetBusinessButtonsAsync()
        {
            UserAuthValidator.Validate(_userAuthValidate);
            try
            {
                var data = new JObject();
                var dataStr = _httpHelper.GetSignature(data);
                var instaUri = UriCreator.GetBusinessInstantExperienceUri(dataStr);

                var request = _httpHelper.GetDefaultRequest(HttpMethod.Get, instaUri, _deviceInfo);
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<InstaBusinessPartnersList>(response, json);

                var obj = JsonConvert.DeserializeObject<InstaBusinessPartnerContainer>(json);
                var partners = new InstaBusinessPartnersList();

                foreach (var p in obj.Partners)
                    partners.Add(p);

                return Result.Success(partners);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<InstaBusinessPartnersList>(exception);
            }
        }

        /// <summary>
        ///     Validate an uri for an button(instagram partner)
        ///     <para>Note: Use <see cref="IBusinessProcessor.GetBusinessButtonsAsync"/> to get business buttons(instagram partner) list!</para>
        /// </summary>
        /// <param name="desirePartner">Desire partner (Use <see cref="IBusinessProcessor.GetBusinessButtonsAsync"/> to get business buttons(instagram partner) list!)</param>
        /// <param name="uri">Uri to check</param>
        public async Task<IResult<bool>> ValidateUrlAsync(InstaBusinessPartner desirePartner, Uri uri)
        {
            UserAuthValidator.Validate(_userAuthValidate);
            try
            {
                if(desirePartner?.AppId == null)
                    return Result.Fail<bool>("Desire partner cannot be null");
                if (uri == null)
                    return Result.Fail<bool>("Uri cannot be null");

                var instaUri = UriCreator.GetBusinessValidateUrlUri();

                var data = new JObject
                {
                    {"app_id", desirePartner.AppId},
                    {"_csrftoken", _user.CsrfToken},
                    {"url", uri.ToString()},
                    {"_uid", _user.LoggedInUser.Pk.ToString()},
                    {"_uuid", _deviceInfo.DeviceGuid.ToString()}
                };
                var request =
                    _httpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<InstaBusinessValidateUrl>(json);
                return obj.IsValid ? Result.Success(true) : Result.Fail<bool>(obj.ErrorMessage);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<bool>(exception);
            }
        }
        /// <summary>
        ///     Remove button from your business account
        /// </summary>
        public async Task<IResult<bool>> RemoveBusinessButtonAsync()
        {
            UserAuthValidator.Validate(_userAuthValidate);
            try
            {
                var instaUri = UriCreator.GetUpdateBusinessInfoUri();

                var data = new JObject
                {
                    {"is_call_to_action_enabled","0"},
                    {"_csrftoken", _user.CsrfToken},
                    {"_uid", _user.LoggedInUser.Pk.ToString()},
                    {"_uuid", _deviceInfo.DeviceGuid.ToString()}
                };

                var request =
                    _httpHelper.GetSignedRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<bool>(response, json);

                var obj = JsonConvert.DeserializeObject<InstaDefault>(json);
                return obj.Status.ToLower() == "ok" ? Result.Success(true) : Result.Fail<bool>(obj.Message);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<bool>(exception);
            }
        }
        /// <summary>
        ///     Get suggested categories 
        /// </summary>
        public async Task<IResult<InstaBusinessSugesstedCategoryList>> GetSuggestedCategoriesAsync()
        {
            UserAuthValidator.Validate(_userAuthValidate);
            try
            {
                var instaUri = UriCreator.GetBusinessGraphQLUri();

                var zero = new JObject
                {
                    {"page_name", _user.UserName.ToLower()},
                    {"num_result", "5"}
                };
                var queryParams = new JObject
                {
                    {"0", zero}
                };
                var data = new Dictionary<string, string>
                {
                    {"query_id", "706774002864790"},
                    {"locale", InstaApiConstants.ACCEPT_LANGUAGE.Replace("-", "_")},
                    {"vc_policy", "ads_viewer_context_policy"},
                    {"signed_body", $"{_httpHelper._apiVersion.SignatureKey}."},
                    {InstaApiConstants.HEADER_IG_SIGNATURE_KEY_VERSION, InstaApiConstants.IG_SIGNATURE_KEY_VERSION},
                    {"strip_nulls", "true"},
                    {"strip_defaults", "true"},
                    {"query_params", queryParams.ToString(Formatting.None)},
                };
                var request =
                    _httpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<InstaBusinessSugesstedCategoryList>(response, json);

                var obj = JsonConvert.DeserializeObject<InstaBusinessSugesstedCategoryList>(json, new InstaBusinessSuggestedCategoryDataConverter());
                return Result.Success(obj);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<InstaBusinessSugesstedCategoryList>(exception);
            }
        }
        /// <summary>
        ///     Get all categories 
        /// </summary>
        public async Task<IResult<InstaBusinessCategoryList>> GetCategoriesAsync()
        {
            UserAuthValidator.Validate(_userAuthValidate);
            try
            {
                var instaUri = UriCreator.GetBusinessGraphQLUri();

                var queryParams = new JObject
                {
                    {"0", "-1"}
                };
                var data = new Dictionary<string, string>
                {
                    {"query_id", "425892567746558"},
                    {"locale", InstaApiConstants.ACCEPT_LANGUAGE.Replace("-", "_")},
                    {"vc_policy", "ads_viewer_context_policy"},
                    {"signed_body", $"{_httpHelper._apiVersion.SignatureKey}."},
                    {InstaApiConstants.HEADER_IG_SIGNATURE_KEY_VERSION, InstaApiConstants.IG_SIGNATURE_KEY_VERSION},
                    {"strip_nulls", "true"},
                    {"strip_defaults", "true"},
                    {"query_params", queryParams.ToString(Formatting.None)},
                };
                var request =
                    _httpHelper.GetDefaultRequest(HttpMethod.Post, instaUri, _deviceInfo, data);
                var response = await _httpRequestProcessor.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.UnExpectedResponse<InstaBusinessCategoryList>(response, json);

                var obj = JsonConvert.DeserializeObject<InstaBusinessCategoryList>(json, new InstaBusinessCategoryDataConverter());
                return Result.Success(obj);
            }
            catch (Exception exception)
            {
                _logger?.LogException(exception);
                return Result.Fail<InstaBusinessCategoryList>(exception);
            }
        }

    }
}