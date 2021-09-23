using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplication_MT4North.Resources;
using WebApplication_MT4North.Infrastructure;
using WebApplication_MT4North.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using WebApplication_MT4North.Models;

namespace WebApplication_MT4North.IntegrationTests
{
    [TestClass]
    public class ActivityControllerTests
    {

        private LoginRequest user1Credentials = new LoginRequest
        {
            Email = "user1@localhost",
            Password = "P@ssw0rd1!"
        };

        private LoginRequest user2Credentials = new LoginRequest
        {
            Email = "user2@localhost",
            Password = "P@ssw0rd1!"
        };

        private LoginRequest user3Credentials = new LoginRequest
        {
            Email = "user3@localhost",
            Password = "P@ssw0rd1!"
        };

        private LoginRequest user4Credentials = new LoginRequest
        {
            Email = "user4@localhost",
            Password = "P@ssw0rd1!"
        };

        private ApplicationUser user1;
        private ApplicationUser user2;
        private ApplicationUser user3;
        private ApplicationUser user4;

        private string authUser1;
        private string authUser2;
        private string authUser3;
        private string authUser4;

        private readonly TestHostFixture _testHostFixture = new TestHostFixture();
        private HttpClient _httpClient;
        private IServiceProvider _serviceProvider;

        [TestInitialize]
        public async Task SetUpAsync()
        {
            _httpClient = _testHostFixture.Client;
            _serviceProvider = _testHostFixture.ServiceProvider;

            // login users
            authUser1 = await loginUserAsync(user1Credentials);
            authUser2 = await loginUserAsync(user2Credentials);
            authUser3 = await loginUserAsync(user3Credentials);
            authUser4 = await loginUserAsync(user4Credentials);

            // fetch user
            user1 = await getUserAsync(authUser1);
            user2 = await getUserAsync(authUser2);
            user3 = await getUserAsync(authUser3);
            user4 = await getUserAsync(authUser4);
        }

        private async Task<string> loginUserAsync(LoginRequest userCredentials)
        {
            // Login users
            var loginResponse = await _httpClient.PostAsync("api/account/login",
                new StringContent(JsonSerializer.Serialize(userCredentials), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResult>(loginResponseContent);
            return loginResult.AccessToken;
        }

        private async Task<ApplicationUser> getUserAsync(string authUser)
        {
            // Autherize
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authUser);
            // Login users
            var getUserResponse = await _httpClient.GetAsync("api/account/user");

            Assert.AreEqual(HttpStatusCode.OK, getUserResponse.StatusCode);

            var userContent = await getUserResponse.Content.ReadAsStringAsync();
            var userResult = JsonSerializer.Deserialize<ApplicationUser>(userContent);
            return userResult;
        }

        [TestMethod]
        public async Task x01_CanGetProjectActivities()
        {
            // Autherize
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authUser1);

            var projectId = 1006;
            // Get project-activities with id projectId
            var getUrl = "api/activities/project/" + projectId;
            // Call to get the activities for the project
            var getResponse = await _httpClient.GetAsync(getUrl);
            // Did we fail as expected with 403 Forbidden
            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
            // Can we parse a list of activities from the content of the body?
            var content = await getResponse.Content.ReadAsStringAsync();
            var activities = JsonSerializer.Deserialize<List<Activity>>(content);

            Assert.IsNotNull(activities);
        }

        [TestMethod]
        public async Task x02_CanNotGetProjectActivitiesForbidden()
        {
            // Autherize
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authUser3);

            var projectId = 1006;
            // Get project-activities with id projectId
            var getUrl = "api/activities/project/" + projectId;
            // Call to get the activities for the project
            var getResponse = await _httpClient.GetAsync(getUrl);
            // Did we fail as expected with 403 Forbidden
            Assert.AreEqual(HttpStatusCode.Forbidden, getResponse.StatusCode);
        }

        [TestMethod]
        public async Task x03_CanNotGetProjectActivitiesNotFound()
        {
            // Autherize
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authUser3);

            var projectId = Int32.MaxValue;
            // Get project-activities with id projectId
            var getUrl = "api/activities/project/" + projectId;
            // Call to get the activities for the project
            var getResponse = await _httpClient.GetAsync(getUrl);
            // Did we fail as expected with 404 NotFound
            Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        //
        //
        // * Can get project activities
        // * Can NOT get project activities (permission)
        // * Can NOT get project activities (project doesnt exist)
        // Can create activity (base)
        // Can create activity (custom)
        // Can NOT create activity (permission)
        // Can NOT create activity (BadRequest)
        // Can get activity
        // Can NOT get activity (activcity not found)
        // Can edit activity (values)
        //
        // WTFs!
        // * If changing customactivityid to another existing customactivity? Zombie customactivities? 
        // * If changing customactivityid to another nonexisting customactivity? 
        // * 
        // * 
        //
        // Can NOT edit activity (missformed json / BadRequest, timestamps syntax, ... )
        // Can NOT edit activity (permission)
        // Can NOT edit activity (activcity not found)
        // Can edit custom-activity
        // Can NOT edit custom-activity (permission)
        // Can NOT delete activity (permission)
        // Can NOT delete activity (activity not found)
        // Can delete activity
        // 
        //
    }
}