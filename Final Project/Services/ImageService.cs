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
        public readonly ItemTypeService _typeService;

        public ImageService(IConfiguration configuration, ItemService itemService, ItemTypeService typeService)
        {
            ApiKey = configuration.GetSection("FireBaseStorage").GetValue<string>("ApiKey");
            Bucket = configuration.GetSection("FireBaseStorage").GetValue<string>("Bucket");
            AuthEmail = configuration.GetSection("FireBaseStorage").GetValue<string>("AuthEmail");
            AuthPassword = configuration.GetSection("FireBaseStorage").GetValue<string>("AuthPassword");
            this._itemService = itemService;
            this._typeService = typeService;
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
            var ms = file.OpenReadStream();
            ms.Position = 0;

            var uploadImageTask = new FirebaseStorage(
                Bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(_authorized.FirebaseToken),
                    ThrowOnCancel = true
                })
                .Child("product")
                .Child(_itemObject.Name)
                .PutAsync(ms);
            _itemObject.Image = await uploadImageTask;
            await _itemService.UpdateAsync(_itemObject.Id, _itemObject);
        }

        public async Task uploadTypeImage(string id, IFormFile file)
        {
            try
            {
                var _typeObject = await _typeService.GetAsync(id);
                if (_typeObject == null)
                {
                    throw new HttpReturnException(HttpStatusCode.NotFound, "Type doesn't exist");
                }
                var _firebaseAuth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                var _authorized = await _firebaseAuth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);
                var ms = file.OpenReadStream();
                ms.Position = 0;

                var uploadTypeImageTask = new FirebaseStorage(
                    Bucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(_authorized.FirebaseToken),
                        ThrowOnCancel = true
                    })
                    .Child("type")
                    .Child(_typeObject.Name)
                    .PutAsync(ms);
                _typeObject.Image = await uploadTypeImageTask;
                await _typeService.UpdateAsync(_typeObject.Id, _typeObject);
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> deleteImage(string id)
        {
            try
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
                    .Child(_itemObject.Name)
                    .DeleteAsync();
                _itemObject.Image = null;
                await _itemService.UpdateAsync(_itemObject.Id, _itemObject);
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> deleteTypeImage(string id)
        {
            try
            {
                var _itemTypeObject = await _typeService.GetAsync(id);
                if (_itemTypeObject == null)
                {
                    throw new HttpReturnException(HttpStatusCode.NotFound, "Type doesn't exist");
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
                    .Child("type")
                    .Child(_itemTypeObject.Name)
                    .DeleteAsync();
                _itemTypeObject.Image = null;
                await _typeService.UpdateAsync(_itemTypeObject.Id, _itemTypeObject);
                return true;
            }
            catch { return false; }
        }
    }
}
