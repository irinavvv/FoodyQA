using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using FoodyExamPrep.Models;

namespace FoodyExamPrep
{
    [TestFixture]
    public class Tests
    {
        private RestClient client;

        private const string BaseUrl = "http://144.91.123.158:81/";
        private const string UserName = "irina321";
        private const string Password = "123456";
        private static string foodId;
       
        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(UserName, Password);

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }
        private string GetJwtToken(string userName, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("api/User/Authentication", Method.Post);
            request.AddJsonBody(new { userName, password });
            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                using var content = JsonDocument.Parse(response.Content);
                var token = content.RootElement.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}");
            }
        }

        [Order(1)]
        [Test]
        public void CreateFoody_WithRequiredFields_ShouldReturnSuccess()
        {
            var foodCreated = new FoodDTO
            {
                Name = "Test Food",
                Description = "Test Description",
                Url = ""
            };
            var request = new RestRequest("api/Food/Create", Method.Post);
            request.AddJsonBody(foodCreated);

            var response = this.client.Execute(request);

            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(createResponse, Is.Not.Null);
            Assert.That(createResponse!.FoodId, Is.Not.Null.And.Not.Empty);
            foodId = createResponse.FoodId;
        }
        [Order(2)]
        [Test]
        public void Edit_TitleOfCreatedFood_ShouldReturnSuccess()
        {
            Assert.That(foodId, Is.Not.Null.And.Not.Empty);

            var newTitle = "Edited Food Title";
            var request = new RestRequest($"api/Food/Edit/{foodId}", Method.Patch);

            request.AddJsonBody(new object[]
            {
                new
                {
                    path = "/name",
                    op = "replace",
                    value = newTitle
                }
            });

            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(editResponse, Is.Not.Null);
            Assert.That(editResponse!.Msg, Is.EqualTo("Successfully edited"));
        }

        [Order(3)]
        [Test]
        public void GetAllFoods_ShouldListAllFoods()
        { 
            var request = new RestRequest("api/Food/All", Method.Get);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.Not.Null.And.Not.Empty);
        }
        [Order(4)]
        [Test]
        public void Delete_CreatedFood_ShouldReturnSuccess()
        {
            Assert.That(foodId, Is.Not.Null.And.Not.Empty);
            var request = new RestRequest($"api/Food/Delete/{foodId}", Method.Delete);
            var response = this.client.Execute(request);
            var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deleteResponse, Is.Not.Null);
            Assert.That(deleteResponse!.Msg, Is.EqualTo("Deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateFood_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var foodCreated = new FoodDTO
            {
                Name = "",
                Description = "",
                Url = ""
            };
            var request = new RestRequest("api/Food/Create", Method.Post);
            request.AddJsonBody(foodCreated);

            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }

        [Order(6)]
        [Test]
        public void Edit_NonExistentFood_ShouldReturnNotFound()
        {
            var nonExistentFoodId = "non-existent-id";
            var newTitle = "Edited Food Title";
            var request = new RestRequest($"api/Food/Edit/{nonExistentFoodId}", Method.Patch);
            request.AddJsonBody(new object[]
            {
                new
                {
                    path = "/name",
                    op = "replace",
                    value = newTitle
                }
            });
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Order(7)]
        [Test]
        public void Delete_NonExistentFood_ShouldReturnNotFound()
        {
            var nonExistentFoodId = "non-existent-id";
            var request = new RestRequest($"api/Food/Delete/{nonExistentFoodId}", Method.Delete);
            var response = this.client.Execute(request);
            var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(deleteResponse!.Msg, Is.EqualTo("Unable to delete this food revue!"));
        }
        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}