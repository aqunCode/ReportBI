using Bi.Core.Extensions;
using Bi.Core.Helpers;
using Bi.Core.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;

namespace Bi.Core.EasyFS
{
    public class EasyFSService : IEasyFSService
    {
        private readonly string _appId;
        private readonly string _appKey;
        private readonly string _serverUrl;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="appKey"></param>
        /// <param name="serverUrl"></param>
        public EasyFSService(string appId, string appKey, string serverUrl)
        {
            _appId = appId;
            _appKey = appKey;
            _serverUrl = serverUrl;
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="fileId">文件ID</param>
        /// <returns></returns>
        public ResponseResult<byte[]> Download(string fileId)
        {
            var retval = new ResponseResult<byte[]>();

            try
            {
                using var http = new HttpHelper(new HttpRequest
                {
                    Url = $"{_serverUrl}/easyfs/appfile/get?appId={_appId}&appKey={_appKey}&fileId={fileId}",
                    ResultType = ResultType.Byte
                });

                var res = http.GetResult();

                if (res.StatusCode == HttpStatusCode.OK && res.ResultByte?.Length > 0)
                {
                    retval.Code = ResponseCode.Ok;
                    retval.Result = res.ResultByte;
                    retval.Message = HttpUtility.UrlDecode(res.Header["EasyFSName"]);
                }
                else
                {
                    retval.Code = ResponseCode.Error;
                    retval.ErrorCode = (int)res.StatusCode;
                }
            }
            catch (Exception ex)
            {
                retval.Code = ResponseCode.InternalServerError;
                retval.ErrorCode = (int)ResponseCode.InternalServerError;
                retval.Message = ex.Message;
            }

            return retval;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public ResponseResult<string> UpLoad(AppFileInput input)
        {
            var retval = new ResponseResult<string>();

            try
            {
                using var http = new HttpHelper(new HttpRequest
                {
                    Url = $"{_serverUrl}/easyfs/appfile/upload",
                    HttpMethod = HttpMethod.Post,
                    PostDataType = PostDataType.File,
                    PostString = new Dictionary<string, object>
                    {
                        ["appId"] = _appId,
                        ["appKey"] = _appKey,
                        ["fileId"] = input.FileId,
                        ["fileIndex"] = input.FileIndex,
                        ["fileTotal"] = input.FileTotal,
                        ["fileName"] = input.FileName,
                        ["directory"] = input.Directory,
                    }.ToUrl(),
                    PostFileStream = input.FileData.OpenReadStream(),
                    PostFileStreamInfo = ("fileData", input.FileName ?? input.FileData.FileName),
                });

                var res = http.GetResult();
                if (res.StatusCode == HttpStatusCode.OK && res.ResultString.IsNotNullOrEmpty())
                {
                    var result = res.ResultString.ToObject<ResponseResult>();
                    if (result.Code == ResponseCode.Ok && result.Result.IsNotNullOrEmpty())
                    {
                        retval.Code = ResponseCode.Ok;
                        retval.Result = result.Result;
                        return retval;
                    }
                }

                retval.Code = ResponseCode.Error;
                retval.ErrorCode = (int)res.StatusCode;
                retval.Result = res.ResultString;
            }
            catch (Exception ex)
            {
                retval.Code = ResponseCode.InternalServerError;
                retval.ErrorCode = (int)ResponseCode.InternalServerError;
                retval.Message = ex.Message;
            }

            return retval;
        }
    }
}
