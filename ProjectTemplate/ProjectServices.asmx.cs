using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Services;

namespace ProjectTemplate
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]

    public class ProjectServices : System.Web.Services.WebService
    {
        ////////////////////////////////////////////////////////////////////////
        ///replace the values of these variables with your database credentials
        ////////////////////////////////////////////////////////////////////////
        private readonly string dbID = "spring2024team4";
        private readonly string dbPass = "spring2024team4";
        private readonly string dbName = "spring2024team4";
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        ///call this method anywhere that you need the connection string!
        ////////////////////////////////////////////////////////////////////////
        private string GetConString()
        {
            return "SERVER=107.180.1.16; PORT=3306; DATABASE=" + dbName + "; UID=" + dbID + "; PASSWORD=" + dbPass + "; CharSet=utf8mb4;";
        }
        ////////////////////////////////////////////////////////////////////////



        /////////////////////////////////////////////////////////////////////////
        //don't forget to include this decoration above each method that you want
        //to be exposed as a web service!
        [WebMethod(EnableSession = true)]
        /////////////////////////////////////////////////////////////////////////
        public string TestConnection()
        {
            try
            {

                string testQuery = "select * from Users";

                ////////////////////////////////////////////////////////////////////////
                ///here's an example of using the getConString method!
                ////////////////////////////////////////////////////////////////////////
                MySqlConnection con = new MySqlConnection(GetConString());
                ////////////////////////////////////////////////////////////////////////

                MySqlCommand cmd = new MySqlCommand(testQuery, con);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable table = new DataTable();
                adapter.Fill(table);
                return "Success!";
            }
            catch (Exception e)
            {
                return "Something went wrong, please check your credentials and db name and try again.  Error: " + e.Message;
            }
        }

        //LOG ON METHOD
        [WebMethod]
        public bool LogOn(string uid, string pass)
        {
            //LOGIC: pass the parameters into the database to see if an account
            //with these credentials exist.  If it does, then return true.  If
            //it doesn't, then return false

            //we return this flag to tell them if they logged in or not
            bool success = false;

            //our connection string comes from our web.config file like we talked about earlier
            string sqlConnectString = GetConString();
            //here's our query.  A basic select with nothing fancy.  Note the parameters that begin with @
            string sqlSelect = "SELECT id FROM Users WHERE userid=@idValue and pass=@passValue";

            //set up our connection object to be ready to use our connection string
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            //set up our command object to use our connection, and our query
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            //tell our command to replace the @parameters with real values
            //we decode them because they came to us via the web so they were encoded
            //for transmission (funky characters escaped, mostly)
            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(uid));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));

            //a data adapter acts like a bridge between our command object and 
            //the data we are trying to get back and put in a table object
            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            //here's the table we want to fill with the results from our query
            DataTable sqlDt = new DataTable();
            //here we go filling it!
            sqlDa.Fill(sqlDt);
            //check to see if any rows were returned.  If they were, it means it's 
            //a legit account
            if (sqlDt.Rows.Count > 0)
            {
                //flip our flag to true so we return a value that lets them know they're logged in

                success = true;
            }
            //return the result!
            return success;
        }

        [WebMethod(EnableSession = true)]
        public bool LogOff()
        {
            //if they log off, then we remove the session.  That way, if they access
            //again later they have to log back on in order for their ID to be back
            //in the session!
            Session.Abandon();
            return true;
        }

        [WebMethod(EnableSession = true)]
        public void RequestAccount(string uid, string pass)
        {
            string sqlConnectString = GetConString();
            //the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
            //does is tell mySql server to return the primary key of the last inserted row.
            string sqlSelect = "insert into Users (userid, pass) " +
                "values(@idValue, @passValue); SELECT LAST_INSERT_ID();";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(uid));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));


            //this time, we're not using a data adapter to fill a data table.  We're just
            //opening the connection, telling our command to "executescalar" which says basically
            //execute the query and just hand me back the number the query returns (the ID, remember?).
            //don't forget to close the connection!
            sqlConnection.Open();
            //we're using a try/catch so that if the query errors out we can handle it gracefully
            //by closing the connection and moving on
            try
            {
                int accountID = Convert.ToInt32(sqlCommand.ExecuteScalar());
                //here, you could use this accountID for additional queries regarding
                //the requested account.  Really this is just an example to show you
                //a query where you get the primary key of the inserted row back from
                //the database!

                UpdateTotalTicketCount(uid, 0);
            }
            catch (Exception)
            {
            }
            sqlConnection.Close();
        }

        [WebMethod]
        public string IncrementSwipes(string userid)
        {
            //This method increments the swipes value in the MySQL DB. Method should be invoked when a user makes a "yes" swipe.
            if (!CanUserSwipe(userid))
            {
                return "Swipe limit reached for today.";
            }

            try
            {
                // SQL UPDATE command to increment swipes value
                string sqlUpdate = "UPDATE Users SET swipes = swipes + 1 WHERE userid = @userid";
                string sqlUpdateSwipeDate = "UPDATE Users SET lastSwipeDate = CURDATE() WHERE userid = @userid";

                using (MySqlConnection con = new MySqlConnection(GetConString()))
                using (MySqlCommand cmdUpdateSwipeDate = new MySqlCommand(sqlUpdateSwipeDate, con))

                {
                    // Open connection
                    con.Open();

                    // Set up command with the SQL statement and connection
                    using (MySqlCommand cmd = new MySqlCommand(sqlUpdate, con))
                    {
                        // Add the userid parameter to the command
                        cmd.Parameters.AddWithValue("@userid", HttpUtility.UrlDecode(userid));
                        cmdUpdateSwipeDate.Parameters.AddWithValue("@userid", HttpUtility.UrlDecode(userid));


                        int rowsAffected = cmd.ExecuteNonQuery();

                        // Check if any rows were updated
                        if (rowsAffected > 0)
                        {
                            return "Swipes incremented successfully for userid: " + userid;
                        }
                        else
                        {
                            return "Userid not found or swipes already at maximum.";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // Return a message indicating the error
                return "An error occurred: " + e.Message;
            }
        }


        // GetSwipes method returns a user's daily swipes as an integer. Returns -1 if error is encountered.

        [WebMethod]
        public int GetSwipes(string userid)
        {
            try
            {
                // SQL SELECT
                string sqlSelect = "SELECT swipes FROM Users WHERE userid = @userid";

                using (MySqlConnection con = new MySqlConnection(GetConString()))
                {
                    con.Open();

                    using (MySqlCommand cmd = new MySqlCommand(sqlSelect, con))
                    {
                        cmd.Parameters.AddWithValue("@userid", HttpUtility.UrlDecode(userid));

                        object result = cmd.ExecuteScalar();

                        // If result is not null, convert and return the swipes value as integer
                        if (result != null)
                        {
                            return Convert.ToInt32(result);
                        }
                        else
                        {
                            // Userid not found
                            return -1;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        // Method to check if a user can swipe limits to 3 a day
        public bool CanUserSwipe(string userid)
        {
            try
            {
                string sqlSelect = "SELECT swipes, lastSwipeDate FROM Users WHERE userid = @userid";
                using (MySqlConnection con = new MySqlConnection(GetConString()))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand(sqlSelect, con))
                    {
                        cmd.Parameters.AddWithValue("@userid", HttpUtility.UrlDecode(userid));
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int swipes = reader.GetInt32(0);
                                // Check for DBNull to handle NULL lastSwipeDate values
                                DateTime? lastSwipeDate = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1);

                                // If the last swipe date is null or not today, reset swipes and lastSwipeDate.
                                if (!lastSwipeDate.HasValue || lastSwipeDate.Value.Date != DateTime.Now.Date)
                                {
                                    ResetSwipes(userid); // This will set the lastSwipeDate to today and swipes to 0.
                                    return true; // Since we reset, the user can swipe.
                                }

                                // Now we can simply check if the swipes are less than 3 to allow a new swipe.
                                return swipes < 3;
                            }
                            else
                            {
                                return false; // User not found.
                            }
                        }
                    }
                }
            }
            catch
            {
                return false; // In case of error, block swipe.
            }
        }



        // Method to reset swipes count to 0 and update lastSwipeDate to today.
        private void ResetSwipes(string userid)
        {
            try
            {
                string sqlUpdate = "UPDATE Users SET swipes = 0, lastSwipeDate = CURDATE() WHERE userid = @userid";
                using (MySqlConnection con = new MySqlConnection(GetConString()))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand(sqlUpdate, con))
                    {
                        cmd.Parameters.AddWithValue("@userid", HttpUtility.UrlDecode(userid));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                // Handle exceptions (e.g., log the error)
            }
        }






        // Pull topics and TopicID dynamically from the database
        [WebMethod(EnableSession = true)]
        public List<Topic> GetTopics()
        {
            List<Topic> topics = new List<Topic>();
            string connectionString = GetConString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sqlQuery = "SELECT idTopics, TopicName FROM Topics";

                using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                {
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Create a new Topic object for each row and add it to the list
                            Topic topic = new Topic
                            {
                                Id = reader.GetInt32("idTopics"),
                                Name = reader.GetString("TopicName")
                            };
                            topics.Add(topic);
                        }
                    }
                }
            }

            return topics;
        }

        // Topic class
        public class Topic
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }


        // pull questions dynamically from the DB. Changing this to use topicId instead of TopicName is a good idea.
        [WebMethod(EnableSession = true)]
        public List<Question> GetQuestions(int topicId)
        {
            List<Question> questions = new List<Question>();
            string connectionString = GetConString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                //SQL Query
                string sqlQuery = @"
                        SELECT q.idQuestions, q.Question 
                        FROM Questions q
                        WHERE q.idTopics = @TopicId";

                using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@TopicId", topicId);
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            Question question = new Question
                            {
                                Id = reader.GetInt32("idQuestions"),
                                Text = reader.GetString("Question")
                            };
                            questions.Add(question);
                        }
                    }
                }
            }

            return questions;
        }

        //Question class
        public class Question
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }



        // Gets daily amount of suggestions/entries user has made


        [WebMethod(EnableSession = true)]
        public int GetDailySuggestionCountByUser(string userid)
        {
            int suggestionCount = 0;
            string sqlConnectString = GetConString();
            // SQL query to get the count and the date of the latest suggestion by a user
            string sqlSelect = @"
        SELECT COUNT(*), MAX(Timestamp) FROM UserSuggestions 
        WHERE UserId = (SELECT id FROM Users WHERE userid = @userid) 
        AND DATE(Timestamp) = CURDATE()";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@userid", HttpUtility.UrlDecode(userid));

            try
            {
                sqlConnection.Open();
                using (MySqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        suggestionCount = reader.GetInt32(0);
                        DateTime? lastSuggestionDate = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1);

                        if (!lastSuggestionDate.HasValue || lastSuggestionDate.Value.Date != DateTime.Now.Date)
                        {
                            // Reset the count if the last suggestion was not made today
                            suggestionCount = 0;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                suggestionCount = -1; // Returning -1 to indicate an error
            }
            finally
            {
                sqlConnection?.Close();
            }
            return suggestionCount;
        }


        // Adds suggestion and limits daily entry to 3 per user


        [WebMethod(EnableSession = true)]
        public string AddUserSuggestion(string userid, string suggestionName, string suggestionText)
        {
            // Check if the user has already made 3 suggestions today
            int currentSuggestionCount = GetDailySuggestionCountByUser(userid);
            if (currentSuggestionCount < 0)
            {
                return "An error occurred while checking your suggestion count.";
            }
            else if (currentSuggestionCount >= 3)
            {
                return "You have reached the daily limit of 3 suggestions.";
            }
            else
            {
                string sqlConnectString = GetConString();
                // SQL command to insert a new suggestion
                string sqlInsert = @"
            INSERT INTO UserSuggestions (UserId, SuggestionName, SuggestionText, Timestamp, likes) 
            VALUES ((SELECT id FROM Users WHERE userid = @userid), @suggestionName, @suggestionText, NOW(), 0)";

                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlInsert, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@userid", HttpUtility.UrlDecode(userid));
                sqlCommand.Parameters.AddWithValue("@suggestionName", HttpUtility.UrlDecode(suggestionName));
                sqlCommand.Parameters.AddWithValue("@suggestionText", HttpUtility.UrlDecode(suggestionText));

                try
                {
                    sqlConnection.Open();
                    int rowsAffected = sqlCommand.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return "Thank you for your suggestion!";
                    }
                    else
                    {
                        return "Failed to add the suggestion. Please try again.";
                    }
                }
                catch (Exception e)
                {
                    return "An error occurred: " + e.Message;
                }
                finally
                {
                    sqlConnection?.Close();
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public int GetSuggestionCountByUser(string userid)
        {
            int suggestionCount = 0;
            string sqlConnectString = GetConString();
            // SQL query to count the number of suggestions made by a user
            string sqlSelect = "SELECT COUNT(*) FROM UserSuggestions WHERE UserId = (SELECT id FROM Users WHERE userid = @userid)";

            // Set up our connection object to use our connection string
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            // Set up our command object to use our connection, and our query
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            // Tell our command to replace the @parameter with the actual user id value
            sqlCommand.Parameters.AddWithValue("@userid", HttpUtility.UrlDecode(userid));

            // Open the connection and execute the query
            try
            {
                sqlConnection.Open();
                // ExecuteScalar returns the first column of the first row in the resultset
                suggestionCount = Convert.ToInt32(sqlCommand.ExecuteScalar());
            }
            catch (Exception e)
            {
                // Handle any errors that may occur
                Console.WriteLine("An error occurred: " + e.Message);
                suggestionCount = -1; // Returning -1 to indicate an error
            }
            finally
            {
                // Always close the connection when done
                sqlConnection?.Close();
            }
            // Return the count of suggestions
            return suggestionCount;
        }


        //Update the amount of tickets a person has to the SQL table
        private void UpdateTotalTicketCount(string uid, int ticketCount)
        {
            //inserts new user information into table
            string sqlUpdate = "insert into Tickets (userid, tickets) values(@idValue, @ticketsValue); SELECT LAST_INSERT_ID();";
            string sqlConnectString = GetConString();

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlUpdate, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(uid));
            sqlCommand.Parameters.AddWithValue("@ticketsValue", ticketCount);

            sqlConnection.Open();

            try
            {
                int accountID = Convert.ToInt32(sqlCommand.ExecuteScalar());

            }
            catch (Exception)
            {
            }
            sqlConnection.Close();
        }


        // Able to see ticket count for each user
        [WebMethod(EnableSession = true)]
        public int TotalTicketCount(string uid)
        {

            int ticketCount = 0;
            string sqlConnectString = GetConString();

            //Uses SELECT to look throught the tickets table to pull total tickets of user
            string sqlSelect = "SELECT tickets FROM Tickets WHERE userid = @userid";

            using (MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString))
            {
                using (MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@userid", HttpUtility.UrlDecode(uid));

                    try
                    {
                        sqlConnection.Open();

                        ticketCount = Convert.ToInt32(sqlCommand.ExecuteScalar());
                    }
                    catch (Exception e)
                    {
                        // Handle any errors that may occur
                        Console.WriteLine("An error occurred: " + e.Message);
                        ticketCount = -1; // Returning -1 to indicate an error
                    }

                }
            }
            return ticketCount;
        }


        //Function to submit suggestions to SQL DB
        [WebMethod(EnableSession = true)]
        public void SubmitSuggestion(string uid, int questionId, int topicId, string suggestionText)
        {

            string sqlConnectString = GetConString();

            // parameterized SQL query 

            string sqlInsert = @"
        INSERT INTO Suggestions (userid, questionid, topicid, Suggestiontext)
        VALUES (@userid, @questionid, @topicid, @Suggestiontext);";

            using (MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString))
            {
                using (MySqlCommand sqlCommand = new MySqlCommand(sqlInsert, sqlConnection))
                {
                    // Add parameters to avoid SQL injection
                    sqlCommand.Parameters.AddWithValue("@userid", uid);
                    sqlCommand.Parameters.AddWithValue("@questionid", questionId);
                    sqlCommand.Parameters.AddWithValue("@topicid", topicId);
                    sqlCommand.Parameters.AddWithValue("@Suggestiontext", suggestionText);

                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();

                }
            }
        }
        [WebMethod(EnableSession = true)]
        public Suggestions[] GetSuggestions()
        {
            DataTable sqlDt = new DataTable("Suggestions");
            string connectionString = GetConString();

            string sqlSelect = @"
                SELECT t.TopicName, q.Question, s.Suggestiontext
                FROM Topics t 
                JOIN Suggestions s ON t.idTopics = s.topicid
                JOIN Questions q ON s.questionid = q.idQuestions;";

            MySqlConnection sqlConnection = new MySqlConnection(connectionString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Suggestions> suggestions = new List<Suggestions>();

            // Loop through each row in the DataTable and create Suggestions objects
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                suggestions.Add(new Suggestions
                {
                    TopicId = sqlDt.Rows[i]["TopicName"].ToString(),
                    QuestionId = sqlDt.Rows[i]["Question"].ToString(),
                    SuggestionText = sqlDt.Rows[i]["Suggestiontext"].ToString(),
                });
            }

            // Convert the list of suggestions to an array and return
            return suggestions.ToArray();
        }

        //enables user to delete data from Suggestions table


        public class Suggestions
        {
            public string TopicId { get; set; }
            public string QuestionId { get; set; }
            public string SuggestionText { get; set; }
        }

        // public int GetUserIDNumber(string userID)
        //{


        //}



        public int GetLikes(string userid)
        {
            string sqlConnectString = GetConString();

            string sqlUpdate = "SELECT SUM(s.likes) AS TotalLikes FROM Users u LEFT JOIN Suggestions s ON u.id = s.userid WHERE u.id = @userid;";
            int likesCount = 0;

            using (MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString))
            {
                using (MySqlCommand sqlCommand = new MySqlCommand(sqlUpdate, sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@userid", userid);

                    try
                    {
                        sqlConnection.Open();
                        var result = sqlCommand.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            likesCount = Convert.ToInt32(result);
                        }
                        Console.WriteLine(result);
                    }
                    catch (Exception ex)
                    {
                        likesCount = -1;
                        Console.WriteLine("Error: " + ex.Message);

                    }
                }
            }
            return likesCount;
        }


        [WebMethod(EnableSession = true)]
        public void LikenDislikeSuggestion(string Suggestiontext, bool isLike)
        {

            string sqlConnectString = GetConString();
            string columnToUpdate = isLike ? "likes" : "dislikes";
            string sqlUpdate = $"UPDATE Suggestions SET {columnToUpdate} = {columnToUpdate} + 1 WHERE Suggestiontext = @SuggestiontextValue";


            using (MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString))
            {
                using (MySqlCommand sqlCommand = new MySqlCommand(sqlUpdate, sqlConnection))
                {

                    sqlCommand.Parameters.AddWithValue("@SuggestiontextValue", Suggestiontext);

                    try
                    {
                        sqlConnection.Open();
                        sqlCommand.ExecuteNonQuery();

                        if (!isLike)
                        {
                            // Call a method to check the dislike count and remove if necessary
                            CheckAndRemoveSuggestionIfDisliked(Suggestiontext);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public string LeaderboardSuggestions()
        {
            string sqlConnectString = GetConString();

            using (MySqlConnection connection = new MySqlConnection(sqlConnectString))
            {
                string query = "SELECT Suggestiontext, likes FROM Suggestions";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    return ConvertDataTableToJson(table);
                }
            }
        }


        [WebMethod(EnableSession = true)]


        private string ConvertDataTableToJson(DataTable table)
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in table.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            return serializer.Serialize(rows);
        }



        [WebMethod(EnableSession = true)]
        private void CheckAndRemoveSuggestionIfDisliked(string Suggestiontext)
        {
            string sqlSelect = "SELECT dislikes FROM Suggestions WHERE Suggestiontext = @SuggestiontextValue";
            string sqlDelete = "DELETE FROM Suggestions WHERE Suggestiontext = @SuggestiontextValue";

            using (MySqlConnection sqlConnection = new MySqlConnection(GetConString()))
            {
                sqlConnection.Open();
                int dislikes = 0;

                // Get the current number of dislikes
                using (MySqlCommand cmdSelect = new MySqlCommand(sqlSelect, sqlConnection))
                {
                    cmdSelect.Parameters.AddWithValue("@SuggestiontextValue", Suggestiontext);
                    object result = cmdSelect.ExecuteScalar();
                    if (result != null)
                    {
                        dislikes = Convert.ToInt32(result);
                    }
                }

                // If dislikes reach 6, delete the suggestion
                if (dislikes >= 6)
                {
                    using (MySqlCommand cmdDelete = new MySqlCommand(sqlDelete, sqlConnection))
                    {
                        cmdDelete.Parameters.AddWithValue("@SuggestiontextValue", Suggestiontext);
                        cmdDelete.ExecuteNonQuery();
                    }
                }
            }
        }

    }
}