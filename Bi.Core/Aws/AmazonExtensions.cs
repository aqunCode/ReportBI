using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Bi.Core.Aws
{
    /// <summary>
    /// 亚马逊服务扩展类
    /// </summary>
    public static class AmazonExtensions
    {
        #region 注入Amazon
        /// <summary>
        /// 注入Amazon配置及服务接口
        /// </summary>
        /// <param name="this">IServiceCollection</param>
        /// <param name="configuration">appsettings配置</param>
        /// <param name="defaultSection">Amazon配置默认Section</param>
        /// <param name="useOfficialDependencyInjection">是否使用官方依赖注入，默认：true</param>
        /// <param name="lifetime">生命周期，默认：Singleton</param>
        /// <returns></returns>
        public static IServiceCollection AddAmazon(
            this IServiceCollection @this,
            IConfiguration configuration,
            string defaultSection = "AWS",
            bool useOfficialDependencyInjection = true,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            //判断是否使用官方依赖注入
            if (useOfficialDependencyInjection)
            {
                return @this.AddDefaultAWSOptions(configuration.GetAWSOptions(defaultSection))
                            .AddAWSService<IAmazonS3>(lifetime);
            }
            else
            {
                var accessKeyId = configuration.GetValue<string>("AWS:AccessKeyId");
                var secretAccessKey = configuration.GetValue<string>("AWS:SecretAccessKey");
                var amazonS3Config = configuration.GetSection("AWS").Get<AmazonS3Config>();

                return lifetime switch
                {
                    ServiceLifetime.Singleton => @this.AddSingleton<IAmazonS3>(x =>
                        new AmazonS3Client(accessKeyId, secretAccessKey, amazonS3Config)),
                    ServiceLifetime.Transient => @this.AddTransient<IAmazonS3>(x =>
                        new AmazonS3Client(accessKeyId, secretAccessKey, amazonS3Config)),
                    ServiceLifetime.Scoped => @this.AddScoped<IAmazonS3>(x =>
                        new AmazonS3Client(accessKeyId, secretAccessKey, amazonS3Config)),
                    _ => throw new ArgumentException($"Unkonwn {nameof(lifetime)}"),
                };
            }
        }
        #endregion

        #region TransferUtility
        /// <summary>
        /// 转换为TransferUtility
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static TransferUtility ToTransferUtility(this IAmazonS3 client)
        {
            return new TransferUtility(client);
        }
        #endregion

        #region 存储桶
        /// <summary>
        /// 校验存储通是否存在
        /// </summary>
        /// <param name="client">The Amazon S3 client object.</param>
        /// <param name="bucketName">The name of the bucket</param>
        /// <returns></returns>
        public static async Task<bool> BucketExistAsync(
            this IAmazonS3 client,
            string bucketName)
        {
            return await AmazonS3Util.DoesS3BucketExistV2Async(client, bucketName);
        }

        /// <summary>
        /// 创建存储桶(Bucket)
        /// </summary>
        /// <param name="client">The client object used to connect to Amazon S3.</param>
        /// <param name="bucketName">The name of the bucket to create.</param>
        public static async Task<PutBucketResponse> CreateBucketAsync(
            this IAmazonS3 client,
            string bucketName)
        {
            var putBucketRequest = new PutBucketRequest
            {
                BucketName = bucketName,
                UseClientRegion = true
            };

            return await client.PutBucketAsync(putBucketRequest);
        }

        /// <summary>
        /// 删除存储桶(Bucket)
        /// </summary>
        /// <param name="client">The Amazon S3 client object.</param>
        /// <param name="bucketName">The name of the bucket to be deleted.</param>
        public static async Task<DeleteBucketResponse> DeleteBucketAsync(
            this IAmazonS3 client,
            string bucketName)
        {
            return await client.DeleteBucketAsync(bucketName);
        }
        #endregion

        #region 上传
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="client">An initialized Amazon S3 client object.</param>
        /// <param name="bucketName">The name of the S3 bucket to upload the
        /// file to.</param>
        /// <param name="objectName">The destination file name.</param>
        /// <param name="filePath">The full path, including file name, to the
        /// file to upload. This doesn't necessarily have to be the same as the
        /// name of the destination file.</param>
        public static async Task<PutObjectResponse> UploadObjectAsync(
            this IAmazonS3 client,
            string bucketName,
            string objectName,
            string filePath)
        {
            return await client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = bucketName,
                Key = objectName,
                FilePath = filePath
            });
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="client"></param>
        /// <param name="filePath"></param>
        /// <param name="bucketName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task UploadAsync(
            this IAmazonS3 client,
            string filePath,
            string bucketName,
            string key = null)
        {
            using var transferUtility = new TransferUtility(client);

            if (!string.IsNullOrEmpty(key))
                await transferUtility.UploadAsync(filePath, bucketName, key);
            else
                await transferUtility.UploadAsync(filePath, bucketName);
        }

        /// <summary>
        /// 上传文件目录
        /// </summary>
        /// <param name="client"></param>
        /// <param name="directory"></param>
        /// <param name="bucketName"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static async Task UploadDirectoryAsync(
            this IAmazonS3 client,
            string directory,
            string bucketName,
            string searchPattern = null,
            SearchOption searchOption = default)
        {
            using var transferUtility = new TransferUtility(client);

            if (!string.IsNullOrEmpty(searchPattern))
                await transferUtility.UploadDirectoryAsync(directory, bucketName, searchPattern, searchOption);
            else
                await transferUtility.UploadDirectoryAsync(directory, bucketName);
        }
        #endregion

        #region 下载
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="client"></param>
        /// <param name="filePath"></param>
        /// <param name="bucketName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task DownloadAsync(
            this IAmazonS3 client,
            string filePath,
            string bucketName,
            string key)
        {
            using var transferUtility = new TransferUtility(client);

            await transferUtility.DownloadAsync(filePath, bucketName, key);
        }

        /// <summary>
        /// 下载文件目录
        /// </summary>
        /// <param name="client"></param>
        /// <param name="bucketName"></param>
        /// <param name="s3Directory"></param>
        /// <param name="localDirectory"></param>
        /// <returns></returns>
        public static async Task DownloadDirectoryAsync(
            this IAmazonS3 client,
            string bucketName,
            string s3Directory,
            string localDirectory)
        {
            using var transferUtility = new TransferUtility(client);

            await transferUtility.DownloadDirectoryAsync(bucketName, s3Directory, localDirectory);
        }
        #endregion
    }
}
