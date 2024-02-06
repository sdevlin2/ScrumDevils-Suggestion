﻿using System;
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
		private string getConString() {
			return "SERVER=107.180.1.16; PORT=3306; DATABASE=" + dbName+"; UID=" + dbID + "; PASSWORD=" + dbPass;
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
				return "Something went wrong, please check your credentials and db name and try again.  Error: "+e.Message;
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


        [WebMethod]
        public string IncrementSwipes(string userid)
        {
            //This method increments the swipes value in the MySQL DB

            try
            {
                // SQL UPDATE command to increment swipes value
                string sqlUpdate = "UPDATE Users SET swipes = swipes + 1 WHERE userid = @userid";

                using (MySqlConnection con = new MySqlConnection(getConString()))
                {
                    // Open connection
                    con.Open();

                    // Set up command with the SQL statement and connection
                    using (MySqlCommand cmd = new MySqlCommand(sqlUpdate, con))
                    {
                        // Add the userid parameter to the command
                        cmd.Parameters.AddWithValue("@userid", HttpUtility.UrlDecode(userid));

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


        //



        [WebMethod]
		public Person[] getBries(int userCount)
        {
			List<Person> people = new List<Person>();
			for (int i = 0; i < userCount; i++)
			{
				Person brie = new Person();
				brie.firstName = "Brie";
				brie.lastName = "Jaramillo";
				brie.phoneNumber = 1234567890;
				people.Add(brie);
			}

            return people.ToArray();
        }
	}
}
