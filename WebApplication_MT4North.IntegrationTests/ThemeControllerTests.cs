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
    public class ThemeControllerTests
    {
        private readonly TestHostFixture _testHostFixture = new TestHostFixture();
        private HttpClient _httpClient;
        private IServiceProvider _serviceProvider;

        private LoginRequest userCredentials = new LoginRequest
        {
            Email = "user1@localhost",
            Password = "P@ssw0rd1!"
        };

        private LoginRequest adminCredentials = new LoginRequest
        {
            Email = "admin@localhost",
            Password = "P@ssw0rd1!"
        };

        private ApplicationUser user;
        private ApplicationUser admin;

        private string authUser;
        private string authAdmin;

        private static Theme _newTheme;

        [TestInitialize]
        public async Task SetUpAsync()
        {
            _httpClient = _testHostFixture.Client;
            _serviceProvider = _testHostFixture.ServiceProvider;

            // login users
            authAdmin = await loginUserAsync(adminCredentials);
            authUser  = await loginUserAsync(userCredentials);

            // fetch user
            admin = await getUserAsync(authAdmin);
            user  = await getUserAsync(authUser);
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

        private async Task<List<Theme>> getThemesAsync(HttpStatusCode expectedHttpStatusCode)
        {
            var getResponse = await _httpClient.GetAsync("api/Themes");

            Assert.AreEqual(expectedHttpStatusCode, getResponse.StatusCode);

            var content = await getResponse.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<Theme>>(content);
            return result;
        }

        private async Task<Theme> getThemeAsync(int id, HttpStatusCode expectedHttpStatusCode)
        {
            
            var getResponse = await _httpClient.GetAsync("api/Themes/"+id);

            Assert.AreEqual(expectedHttpStatusCode, getResponse.StatusCode);

            var content = await getResponse.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Theme>(content);
            return result;
        }

        private async Task<Theme> CreateTheme(HttpStatusCode expectedHttpStatus)
        {
            var theme = new Theme
            {
                Name = "TestCase Theme Controller",
                Description = "Theme created from test ThemeControllerTests"
            };

            var response = await _httpClient.PostAsync("api/Themes/",
                new StringContent(JsonSerializer.Serialize(theme), Encoding.UTF8, MediaTypeNames.Application.Json));
            Assert.AreEqual(expectedHttpStatus, response.StatusCode);
            if (expectedHttpStatus == HttpStatusCode.Created)
            {
                // Get created theme from body
                var postResponseBody = await response.Content.ReadAsStringAsync();
                var new_theme = JsonSerializer.Deserialize<Theme>(postResponseBody);
                // Check the result
                Assert.IsNotNull(new_theme);
                Assert.AreEqual(theme.Name, new_theme.Name);
                Assert.AreEqual(theme.Description, new_theme.Description);
                // return the new theme
                return new_theme;
            }
            // return null if we expected to fail
            return null;
        }

        private async Task<Theme> putThemeAsync(Theme theme, HttpStatusCode expectedHttpStatus)
        {
            var response = await _httpClient.PutAsync("api/Themes/"+theme.ThemeId,
                new StringContent(JsonSerializer.Serialize(theme), Encoding.UTF8, MediaTypeNames.Application.Json));
            Assert.AreEqual(expectedHttpStatus, response.StatusCode);
            if (expectedHttpStatus == HttpStatusCode.OK)
            {
                var putResponseBody = await response.Content.ReadAsStringAsync();
                var updated_theme = JsonSerializer.Deserialize<Theme>(putResponseBody);
                return updated_theme;
            }
            return null;
        }

        private async 
        Task
delThemeAsync(Theme theme, HttpStatusCode expectedHttpStatus)
        {
            var response = await _httpClient.DeleteAsync("api/Themes/" + theme.ThemeId);
            Assert.AreEqual(expectedHttpStatus, response.StatusCode);
        }

        [TestMethod]
        public async Task AA_ShouldBeAbleToGetThemes()
        {
            //Both admin and user should be able to get activties
            // Autherize Admin
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authAdmin);
            // Get for admin
            var themes1 = await getThemesAsync(HttpStatusCode.OK);
            Assert.IsNotNull(themes1);
            Assert.IsTrue(themes1.Count > 0);
            // Autherize User
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authUser);
            // Get for user
            var themes2 = await getThemesAsync(HttpStatusCode.OK);
            Assert.IsNotNull(themes2);
            Assert.IsTrue(themes2.Count > 0);
            // Compare results
            Assert.AreEqual(themes1.Count, themes2.Count);
            //CollectionAssert.AreEquivalent(themes1, themes2);
        }

        [TestMethod]
        public async Task AB_ShouldBeAbleToGetSingleTheme()
        {
            //Both admin and user should be able to get activties
            // Autherize Admin
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authAdmin);
            // Get for admin
            var themes1 = await getThemesAsync(HttpStatusCode.OK);
            Assert.IsNotNull(themes1);
            Assert.IsTrue(themes1.Count > 0);
            var theme1 = await getThemeAsync(themes1[0].ThemeId, HttpStatusCode.OK);
            // Autherize User
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authUser);
            // Get for user
            var themes2 = await getThemesAsync(HttpStatusCode.OK);
            Assert.IsNotNull(themes2);
            Assert.IsTrue(themes2.Count > 0);
            var theme2 = await getThemeAsync(themes2[0].ThemeId, HttpStatusCode.OK);
            // Compare results
            Assert.AreEqual(themes1.Count, themes2.Count);
            Assert.AreEqual(theme1.ThemeId, theme2.ThemeId);
            Assert.AreEqual(theme1.Name, theme2.Name);
            Assert.AreEqual(theme1.Description, theme2.Description);
        }

        [TestMethod]
        public async Task AC_ShouldBeAbleToCreateTheme()
        {
            // Autherize Admin
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authAdmin);
            var theme = await CreateTheme(HttpStatusCode.Created);
            Assert.IsNotNull(theme);
            _newTheme = theme;
        }

        [TestMethod]
        public async Task AD_ShouldNotBeAbleToCreateTheme()
        {
            // Autherize User
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authUser);
            var theme = await CreateTheme(HttpStatusCode.Forbidden);
            Assert.IsNull(theme);
        }

        [TestMethod]
        public async Task AE_ShouldBeAbleToEditTheme()
        {
            // Autherize Admin
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authAdmin);
            // Get themes
            var themes1 = await getThemesAsync(HttpStatusCode.OK);
            Assert.IsNotNull(themes1);
            Assert.IsTrue(themes1.Count > 0);
            // Select one theme
            var theme1 = await getThemeAsync(themes1[0].ThemeId, HttpStatusCode.OK);
            // Edit theme
            theme1.Description = "Changed description of theme created from test ThemeControllerTests";
            // Save the changes
            var theme2 = await putThemeAsync(theme1, HttpStatusCode.OK);
            Assert.AreEqual(theme1.ThemeId, theme2.ThemeId);
            Assert.AreEqual(theme1.Name, theme2.Name);
            Assert.AreEqual(theme1.Description, theme2.Description);
        }

        [TestMethod]
        public async Task AF_ShouldNotBeAbleToEditTheme()
        {
            // Autherize User
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authUser);
            // Get themes
            var themes1 = await getThemesAsync(HttpStatusCode.OK);
            Assert.IsNotNull(themes1);
            Assert.IsTrue(themes1.Count > 0);
            // Select one theme
            var theme1 = await getThemeAsync(themes1[0].ThemeId, HttpStatusCode.OK);
            // Edit theme
            theme1.Description = "Changed description of theme created from test ThemeControllerTests";
            // Save the changes
            var theme2 = await putThemeAsync(theme1, HttpStatusCode.Forbidden);
            Assert.IsNull(theme2);
        }

        [TestMethod]
        public async Task AG_ShouldNotBeAbleToDeleteTheme()
        {
            // Autherize User
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authUser);
            Assert.IsNotNull(_newTheme);
            await delThemeAsync(_newTheme, HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task AH_ShouldBeAbleToDeleteTheme()
        {
            // Autherize Admin
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authAdmin);
            Assert.IsNotNull(_newTheme);
            await delThemeAsync(_newTheme, HttpStatusCode.OK);
        }

    }
}