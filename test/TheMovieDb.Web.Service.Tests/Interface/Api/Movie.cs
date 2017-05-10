using System.Net.Http;
using Xunit;
using System;
using Newtonsoft.Json.Linq;
using System.Text;

namespace TheMovieDb.Web.Service.Tests.Interface.Api
{
    public class Movie
    {
        public HttpClient client = new HttpClient();
        private string validApiKey = "60c2bdd54b7f8da973408e1660fa467e";
        private string invalidApiKey = "";
        private string username = "wmallett";
        private string password = "sofitest";
        private string token;
        private string sessionId;

        public Movie()
        {
            client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            try
            {
                var requestTokenResponse = client.GetAsync($"authentication/token/new?api_key={validApiKey}").Result;
                string content = requestTokenResponse.Content.ReadAsStringAsync().Result;
                dynamic requestToken = JObject.Parse(content);
                token = requestToken.request_token;
            }
            catch(Exception requestTokenCreateException)
            {
                Assert.True(false, "Your request token failed to generate: " + requestTokenCreateException.ToString());
            }

            try
            {
                var validateTokenResponse = client.GetAsync($"authentication/token/validate_with_login?api_key={validApiKey}&username={username}&password={password}&request_token={token}").Result;
            }
            catch (Exception requestTokenCreateException)
            {
                Assert.True(false, "Your request token failed to validate: " + requestTokenCreateException.ToString());
            }

            try
            {
                var createSessionResponse = client.GetAsync($"authentication/session/new?api_key={validApiKey}&request_token={token}").Result;
                string content = createSessionResponse.Content.ReadAsStringAsync().Result;
                dynamic session = JObject.Parse(content);
                sessionId = session.session_id;
            }
            catch (Exception createSessionException)
            {
                Assert.True(false, "Your session failed to create: " + createSessionException.ToString());
            }
        }

        [Fact]
        public void GetMovieDetails() 
        {
            /*
             * TEST DESCRIPTION: Verify fields that are returned in a GET movie details call.
             * PRE-TEST STEPS: Add a movie to TMDB and know the ID of that movie in the database.
             * TEST STEPS:
             * 1. Make the following GET call with a valid apiKey: https://api.themoviedb.org/3/movie/550?api_key=[valid_api_key]
             * VERIFY: A 200 is returned as the status code.
             * VERIFY: The following fields are returned in the response body: [insert list of fields here]
             */

            int movieId = 500;

            using (client)

            {
                HttpResponseMessage response = client.GetAsync($"movie/{movieId}?api_key={validApiKey}").Result;
                int statusCode = (int)response.StatusCode;
                string getContent = response.Content.ReadAsStringAsync().Result;
                dynamic returnedMovie = JObject.Parse(getContent);

                Assert.Equal(200, statusCode);
                Assert.True(getContent.Contains("adult"));
                Assert.True(getContent.Contains("backdrop_path"));
                Assert.True(getContent.Contains("belongs_to_collection"));
                Assert.True(getContent.Contains("budget"));
                Assert.True(getContent.Contains("genres"));
                Assert.True(getContent.Contains("homepage"));
                Assert.True(getContent.Contains("id"));
                Assert.True(getContent.Contains("imdb_id"));
                Assert.True(getContent.Contains("original_language"));
                Assert.True(getContent.Contains("original_title"));
                Assert.True(getContent.Contains("overview"));
                Assert.True(getContent.Contains("popularity"));
                Assert.True(getContent.Contains("poster_path"));
                Assert.True(getContent.Contains("production_companies"));
                Assert.True(getContent.Contains("production_countries"));
                Assert.True(getContent.Contains("release_date"));
                Assert.True(getContent.Contains("revenue"));
                Assert.True(getContent.Contains("runtime"));
                Assert.True(getContent.Contains("spoken_languages"));
                Assert.True(getContent.Contains("status"));
                Assert.True(getContent.Contains("tagline"));
                Assert.True(getContent.Contains("title"));
                Assert.True(getContent.Contains("video"));
                Assert.True(getContent.Contains("vote_average"));
                Assert.True(getContent.Contains("vote_count"));
            }
        }

        [Fact]
        public void GetMovieDetailsInvalidKey()
        {
            /*
             * TEST DESCRIPTION: Verifies the GET call for movie returns a 401 and no movie details with an invalid API key.
             * PRE-TEST STEPS: Add a movie to TMDB and know the ID of that movie in the database.
             * TEST STEPS:
             * 1. Make the following GET call with an invalid apiKey: https://api.themoviedb.org/3/movie/550?api_key=[invalid_api_key]
             * VERIFY: A 401 is returned as the status code.
             * VERIFY: The status message returned states an invalid API key was used and must be a valid one.
             */

            int movieId = 500;

            using (client)

            {
                HttpResponseMessage response = client.GetAsync($"movie/{movieId}?api_key={invalidApiKey}").Result;
                int statusCode = (int)response.StatusCode;
                string getContent = response.Content.ReadAsStringAsync().Result;
                dynamic returnedMovie = JObject.Parse(getContent);

                Assert.Equal(401, statusCode);
                Assert.Equal("Invalid API key: You must be granted a valid key.", returnedMovie.status_message.ToString());
            }
        }

        [Fact]
        public void PostMovieRating()
        {
            /*
             * TEST DESCRIPTION: Verifies a rating can be posted to a movie with a successful response.
             * PRE-TEST STEPS: Add a movie to TMDB and know the ID of that movie in the database. Have a valid sessionId.
             * TEST STEPS:
             * 1. Make the following POST call with an valid apiKey: https://api.themoviedb.org/3/movie/550/rating?api_key=[valid_api_key]&session_id=[session_id]
             * VERIFY: A 201 is returned as the status code.
             * VERIFY: The status message returned states the item was created successfully.
             */

            int movieId = 550;

            using (client)
            {
                dynamic postRatingBody = new JObject();
                postRatingBody.value = 10;

                HttpContent postBody = new StringContent(postRatingBody.ToString(), Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync($"movie/{movieId}/rating?api_key={validApiKey}&session_id={sessionId}", postBody).Result;
                int statusCode = (int)response.StatusCode;
                string content = response.Content.ReadAsStringAsync().Result;
                dynamic responseBody = JObject.Parse(content);

                Assert.Equal(201, statusCode);
                Assert.Equal("The item/record was updated successfully.", responseBody.status_message.ToString());

                //I WOULD ALSO ADD A GET CALL HERE TO GET THE MOVIE AND ENSURE MY RATING WAS ADDED, BUT I DO NOT SEE AN API THAT RETURNS RATING
            }
        }

        [Fact]
        public void GetMovieDetailOfIdThatDoesNotExist()
        {
            /*
             * TEST DESCRIPTION: Verifies when a resource (movie) that does not exist is attempted to be retried, a 404 is returned and a proper error message.
             * PRE-TEST STEPS:
             * TEST STEPS:
             * 1. Make the following GET call with an valid apiKey: https://api.themoviedb.org/3/movie/[id_that_does_not_exist]?api_key=[valid_api_key]
             * VERIFY: A 404 is returned as the status code.
             * VERIFY: The status message returned states the resource could not be found.
             */
        }

        [Fact]
        public void GetMovieDetailSpanish()
        {
            /*
             * TEST DESCRIPTION: Verifies the ability to GET movie details in spanish.
             * PRE-TEST STEPS:  Add a movie to TMDB and know the ID of that movie in the database.
             * TEST STEPS:
             * 1. Make the following GET call with an valid apiKey: https://api.themoviedb.org/3/movie/550?api_key=[valid_api_key]&language=esp
             * VERIFY: A 200 is returned as the status code.
             * VERIFY: The response body is returned in spanish.
             */

            //{
            //    response = client.GetAsync($"movie/{movieId}?api_key&language=esp").Result;
            //    statusCode = (int)response.StatusCode;
            //    getContent = response.Content.ReadAsStringAsync().Result;
            //    returnedMovie = JObject.Parse(getContent);

            //    Assert.True(returnedMovie.contains("spanish attribute name");
         }

        [Fact]
        public void DeleteRating()
        {
            /*
            * TEST DESCRIPTION: Verifies the ability to delete a movie rating
            * PRE-TEST STEPS:  Add a movie to TMDB and know the ID of that movie in the database. Add a rating to the movie.
            * TEST STEPS:
            * 1. Make the following DELETE call with an valid apiKey: https://api.themoviedb.org/3/movie/550/rating?api_key=[valid_api_key]&session_id=[session_id]
            * VERIFY: A 200 is returned as the status code.
            * VERIFY: The response body returns a message stating the item was deleted.
            */
        }

        [Fact]
        public void DeleteRatingForNonExistentMovie()
        {
            /*
            * TEST DESCRIPTION: Verifies a 200 is returned with a proper message when attempting to delete  a rating for a movie that does not exist.
            * PRE-TEST STEPS: 
            * TEST STEPS:
            * 1. Make the following DELETE call with an valid apiKey: https://api.themoviedb.org/3/movie/[id_that_does_not_exist]/rating?api_key=[valid_api_key]&session_id=[session_id]
            * VERIFY: A 200 is returned as the status code.
            * VERIFY: The response body returns a message stating the item was deleted.
            */
        }

        [Fact]
        public void PostRatingOnNonExistentMovie()
        {
            /*
            * TEST DESCRIPTION: Verifies a rating cannot be posted to a movie that does not exist. A 401 is returned with a proper error message.
            * PRE-TEST STEPS: Have a valid sessionId.
            * TEST STEPS:
            * 1. Make the following POST call with an valid apiKey: https://api.themoviedb.org/3/movie/[id_that_does_not_exist]/rating?api_key=[valid_api_key]&session_id=[session_id]
            * VERIFY: A 404 is returned as the status code.
            * VERIFY: The status message returned states the resource could not be found.
            */
        }

        [Fact]
        public void PostRatingInvalidRating()
        {
            /*
             * TEST DESCRIPTION: Verifies an invalid rating cannot be posted to a movie. A 400 is returned with a proper error message.
             * PRE-TEST STEPS: Add a movie to TMDB and know the ID of that movie in the database. Have a valid sessionId.
             * TEST STEPS:
             * 1. Make the following POST call with the following body with an valid apiKey: https://api.themoviedb.org/3/movie/550/rating?api_key=[valid_api_key]&session_id=[session_id]
             * {"value":5000}
             * VERIFY: A 400 is returned as the status code.
             * VERIFY: The status message returned states the value was not valid and must be a value less than or equal to 10.0
             */

            //dynamic postRatingBody = new JObject();
            //postRatingBody.value = 50000;

            //res = client.PostAsync("/movie", postBody).Result;
            //statusCode = (int)res.StatusCode;
            //string content = response.Content.ReadAsStringAsync().Result;
            //dynamic responseBody = JObject.Parse(content);

            //Assert.Equal(400, statusCode);
            //Assert.Equal("message", responseBody.status_message);
        }

        [Fact]
        public void PostRatingGuestSession()
        {
            /*
             * TEST DESCRIPTION: Verifies a rating can be posted to a movie with a successful response with a guest session id.
             * PRE-TEST STEPS: Add a movie to TMDB and know the ID of that movie in the database. Have a valid guest session id.
             * TEST STEPS:
             * 1. Make the following POST call with an valid apiKey and valid guest session id: https://api.themoviedb.org/3/movie/550/rating?api_key=[valid_api_key]&guest_session_id=[guest_session_id]
             * VERIFY: A 201 is returned as the status code.
             * VERIFY: The status message returned states the item was created successfully.
             */
        }

        [Fact]
        public void UpdateRating()
        {
            /*
             * TEST DESCRIPTION: Verifies when a rating is posted to a movie that already has a rating, the rating is updated.
             * PRE-TEST STEPS: Add a movie to TMDB and know the ID of that movie in the database. Add a rating to that movie. Have a valid sessionId.
             * TEST STEPS:
             * 1. Make the following POST call with an valid apiKey: https://api.themoviedb.org/3/movie/550/rating?api_key=[valid_api_key]&session_id=[session_id]
             * VERIFY: A 201 is returned as the status code.
             * VERIFY: The status message returned states the item was created successfully.
             */


            //dynamic postRatingBody = new JObject();
            //postRatingBody.value = 5;

            //res = client.PostAsync("/movie/#/rating", postBody).Result;

            //dynamic udpatedBody = new JObject();
            //udpatedBody.value = 8.5;

            //res = client.PostAsync("/movie/#/rating", postBody).Result;
            //statusCode = (int)res.StatusCode;
            //string content = response.Content.ReadAsStringAsync().Result;
            //dynamic responseBody = JObject.Parse(content);

            //Assert.Equal(201, statusCode);
            //Assert.Equal("message", responseBody.status_message);

            //resAfterUpdate = client.PostAsync("").Result;
            //string afterupdatecontent = resAfterUpdate.Content.ReadAsStringAsync().Result;
            //dynamic afterupdate = JObject.Parse(content);

            //Assert.Equal(updatedRating, returnedRating);
        }

        [Fact]
        public void ExpiredSessionPostRatingCall()
        {
            /*
             * TEST DESCRIPTION: Verifies a rating cannot be posted to a movie with an expired session id.
             * PRE-TEST STEPS: Add a movie to TMDB and know the ID of that movie in the database. Have a valid guest session id and allow it to expire.
             * TEST STEPS:
             * 1. Make the following POST call with an valid apiKey: https://api.themoviedb.org/3/movie/550/rating?api_key=[valid_api_key]&session_id=[expired_session_id]
             * VERIFY: A 401 is returned as the status code.
             * VERIFY: The status message returned states the authentication failed.
             */
        }
    }
}
