using System;
using SQLite;
using Windows.Storage;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PassProtect
{
    class DataAccess
    {
        //declaring layout of list for account information
        public class AccountList
        {
            [SQLite.PrimaryKey, SQLite.AutoIncrement]
            public string ID { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Notes { get; set; }
        }

        //function to initialize and create database and table if it does not already exist
        public static async Task InitializeDatabase(string key)
        {
            StorageFolder localfolder = ApplicationData.Current.LocalFolder;
            try
            {
                StorageFile databaseFile = await localfolder.GetFileAsync("core");
            }
            catch
            {
                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "core");
                var dboptions = new SQLiteConnectionString(dbpath, true, key: key);
                var connection = new SQLiteConnection(dboptions);
                string tableCommand = "CREATE TABLE IF NOT " +
                "EXISTS AccountTable (ID INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "name NVARCHAR(128) NULL, " + "email NVARCHAR(128) NULL, " +
                "username NVARCHAR(128) NULL, " + "password NVARCHAR(128) NULL, " +
                "notes NVARCHAR(1024) NULL)";
                var command = connection.CreateCommand(tableCommand);
                command.ExecuteNonQuery();
                string formattingCommand = "INSERT INTO AccountTable VALUES (1, null, null, null, null, null);";
                command = connection.CreateCommand(formattingCommand);
                command.ExecuteNonQuery();
            }
            return;
        }

        //function to add a row of data into the database
        public static void AddData(string key, string name, string email, string username, string password, string notes)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "core");
            var dboptions = new SQLiteConnectionString(dbpath, true, key: key);
            var connection = new SQLiteConnection(dboptions);
            string insertCommand = "INSERT INTO AccountTable VALUES (?, ?, ?, ?, ?, ?);";
            var tableCommand = connection.CreateCommand(insertCommand, null, name, email, username, password, notes);
            tableCommand.ExecuteNonQuery();
        }

        //function to update rows in the database based on id
        public static void UpdateData(string key, int id, string name, string email, string username, string password, string notes)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "core");
            var dboptions = new SQLiteConnectionString(dbpath, true, key: key);
            var connection = new SQLiteConnection(dboptions);
            string updateCommand = "UPDATE AccountTable SET name = '" + name + "', email = '" + email + "', username = '" + username + "', password = '" + password + "', notes = '" + notes + "' WHERE ID = " + id + ";";
            var tableCommand = connection.CreateCommand(updateCommand);
            tableCommand.ExecuteNonQuery();
        }

        //function to delete a row from the database
        public static void DeleteData(string key, int id)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "core");
            var dboptions = new SQLiteConnectionString(dbpath, true, key: key);
            var connection = new SQLiteConnection(dboptions);
            string deleteCommand = "DELETE FROM AccountTable WHERE ID = " + id + ";";
            var tableCommand = connection.CreateCommand(deleteCommand);
            tableCommand.ExecuteNonQuery();
        }

        //function to read the information of one specific row.
        public static List<AccountList> GetAccountData(string key)
        {
            List<AccountList> account = new List<AccountList>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "core");
            var dboptions = new SQLiteConnectionString(dbpath, true, key: key);
            var connection = new SQLiteConnection(dboptions);
            string selectCommand = "SELECT * FROM AccountTable";
            var accountDataCommand = connection.CreateCommand(selectCommand);
            var query = accountDataCommand.ExecuteQuery<AccountList>();

            foreach (var readaccount in query)
            {
                account.Add(new AccountList() { ID = readaccount.ID, Name = readaccount.Name, Email = readaccount.Email, Username = readaccount.Username, Password = readaccount.Password, Notes = readaccount.Notes });
            }

            return account;
        }

        //function to update DB password
        public static void changeDBPassword(string key, string newkey)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "core");
            var dboptions = new SQLiteConnectionString(dbpath, true, key: key);
            var connection = new SQLiteConnection(dboptions);
            connection.Execute("PRAGMA rekey = '" + newkey + "';");
        }
    }
}
