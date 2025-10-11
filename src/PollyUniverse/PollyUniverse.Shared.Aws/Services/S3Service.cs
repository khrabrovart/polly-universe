using System.Net;
using Amazon.S3;
using Amazon.S3.Model;

namespace PollyUniverse.Shared.Aws.Services;

public interface IS3Service
{
    Task<bool> Download(string bucketName, string objectKey, string filePath);

    Task<bool> UploadFile(string bucketName, string objectKey, string filePath);

    Task<bool> UploadData(string bucketName, string objectKey, byte[] data);
}

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;

    public S3Service(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    public async Task<bool> Download(string bucketName, string objectKey, string filePath)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey,

        };

        try
        {
            using var response = await _s3Client.GetObjectAsync(request);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            await response.WriteResponseStreamToFileAsync(filePath, false, CancellationToken.None);

            return true;
        }
        catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<bool> UploadFile(string bucketName, string objectKey, string filePath)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            FilePath = filePath
        };

        var response = await _s3Client.PutObjectAsync(request);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> UploadData(string bucketName, string objectKey, byte[] data)
    {
        await using var memoryStream = new MemoryStream(data);

        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            InputStream = memoryStream
        };

        var response = await _s3Client.PutObjectAsync(request);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }
}
