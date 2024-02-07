using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;

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
        private string dbID = "spring2024team4";
        private string dbPass = "spring2024team4";
        private string dbName = "spring2024team4";
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        ///call this method anywhere that you need the connection string!
        ////////////////////////////////////////////////////////////////////////
        private string getConString()
        {
            return "SERVER=107.180.1.16; PORT=3306; DATABASE=" + dbName + "; UID=" + dbID + "; PASSWORD=" + dbPass;
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
                MySqlConnection con = new MySqlConnection(getConString());
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
            string sqlConnectString = getConString();
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
        public void RequestAccount(string uid, string pass)
        {
            string sqlConnectString = getConString();
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
            }
            catch (Exception e)
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

                using (MySqlConnection con = new MySqlConnection(getConString()))
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

                using (MySqlConnection con = new MySqlConnection(getConString()))
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

        // Method to check if a user can swipe
        public bool CanUserSwipe(string userid)
        {
            try
            {
                string sqlSelect = "SELECT swipes, lastSwipeDate FROM Users WHERE userid = @userid";
                using (MySqlConnection con = new MySqlConnection(getConString()))
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
                using (MySqlConnection con = new MySqlConnection(getConString()))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand(sqlUpdate, con))
                    {
                        cmd.Parameters.AddWithValue("@userid", HttpUtility.UrlDecode(userid));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                // Handle exceptions (e.g., log the error)
            }
        }



        // Pull topics dynamically from the database
        [WebMethod(EnableSession = true)]
        public List<string> GetTopics()
        {
            List<string> topics = new List<string>();
            string connectionString = getConString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sqlQuery = "SELECT TopicName FROM Topics";

                using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                {
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            topics.Add(reader.GetString("TopicName"));
                        }
                    }
                }
            }

            return topics;
        }

        // pull questions dynamically from the DB. Changing this to use topicId instead of TopicName is a good idea.
        [WebMethod(EnableSession = true)]
        public List<string> GetQuestions(string topicName)
        {
            List<string> questions = new List<string>();
            string connectionString = getConString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                //SQL Query to Join 
                string sqlQuery = @"
            SELECT q.Question 
            FROM Questions q
            JOIN Topics t ON q.idTopics = t.idTopics
            WHERE t.TopicName = @TopicName";

                using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@TopicName", HttpUtility.UrlDecode(topicName));
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            questions.Add(reader.GetString("Question"));
                        }
                    }
                }
            }

            return questions;
        }
    }
}
