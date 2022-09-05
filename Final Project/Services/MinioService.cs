/*using Final_Project.Models;
using Minio;
using Minio.AspNetCore;

namespace Final_Project.Services
{
    public class MinioService
    {
        private readonly MinioClient _minioClient;
        private readonly string imageStorage = "itemimage";
        private readonly string minioServer = "localhost:9000";
        public MinioService()
        {
            _minioClient = new MinioClient();
            _minioClient.WithEndpoint("localhost:9000");
            _minioClient.WithCredentials("NK5pi9N2U0VxTUfP", "Ep3pJOfWagdPqOflrmd9OWVxSNxdssgO");            
            _minioClient.Build();
        }
        public async Task<string> uploadImage(string fileName, IFormFile file)
        {
            fileName = fileName + file.ContentType.Replace("image/", ".");

            if (await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(imageStorage)))
            {
                PutObjectArgs args = new PutObjectArgs();
                args.WithObject(fileName);
                args.WithStreamData(file.OpenReadStream());
                args.WithBucket(imageStorage);
                args.WithObjectSize(file.OpenReadStream().Length);
                args.WithContentType(file.ContentType);
                await _minioClient.PutObjectAsync(args);
            }
            return $"{minioServer}/{imageStorage}/{fileName}";
        }
    }
}*/
