using Final_Project.Models;
using Final_Project.Utils.Resources.Exceptions;
using Firebase.Auth;
using Firebase.Storage;
using Minio;
using Minio.AspNetCore;
using System.Net;

namespace Final_Project.Services
{
    public class ImageService
    {
        private static string ApiKey;
        private static string Bucket;
        private static string AuthEmail;
        private static string AuthPassword;
        public readonly ItemService _itemService;

        public ImageService(IConfiguration configuration, ItemService itemService)
        {
            ApiKey = configuration.GetSection("FireBaseStorage").GetValue<string>("ApiKey");
            Bucket = configuration.GetSection("FireBaseStorage").GetValue<string>("Bucket");
            AuthEmail = configuration.GetSection("FireBaseStorage").GetValue<string>("AuthEmail");
            AuthPassword = configuration.GetSection("FireBaseStorage").GetValue<string>("AuthPassword");
            this._itemService = itemService;
        }
        public async Task uploadImage(string id, IFormFile file)
        {
            var _itemObject = await _itemService.GetAsync(id);
            if (_itemObject == null)
            {
                throw new HttpReturnException(HttpStatusCode.NotFound, "Item doesn't exist");
            }
            var _firebaseAuth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var _authorized = await _firebaseAuth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

            var uploadImageTask = new FirebaseStorage(
                Bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(_authorized.FirebaseToken),
                    ThrowOnCancel = true
                })
                .Child("product")
                .Child(_itemObject.ItemName)
                .PutAsync(file.OpenReadStream());
            _itemObject.Image = await uploadImageTask;
            await _itemService.UpdateAsync(_itemObject.Id, _itemObject);
        }
    }
}
