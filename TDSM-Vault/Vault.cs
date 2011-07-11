using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text.RegularExpressions;
using Terraria_Server;
using Envoy.TDSM_Passport;

namespace Envoy.TDSM_Vault
{
    public class VaultFactory
    {
        private static Vault vault;

        // get the shared singleton instance of Vault
        public static Vault getVault()
        {
            if (vault == null) {
                vault = new Vault();
            }
            return vault;
        }
    }

    public class Vault
    {
        private string PLUGIN_FOLDER = Statics.PluginPath + Path.DirectorySeparatorChar + "Vault";
        private string databaseFileName;
        private SQLiteDatabase database;

        public void store(VaultObject vaultObject)
        {
            try {
                String tableName = tableNameFromPluginName(vaultObject.getPluginName());
                createTable(tableName);

                Dictionary<string, string > dataDictionary = vaultObject.getDataDictionary();
                int id = this.getVaultObjectId(vaultObject);
                if (id == -1) {
                    System.Console.WriteLine("insert");
                    DateTime now = System.DateTime.Now;
                    dataDictionary["created"] = now.ToString();
                    dataDictionary["updated"] = now.ToString();
                    database.Insert(tableName, dataDictionary);
                } else {
                    System.Console.WriteLine("update");
                    DateTime now = System.DateTime.Now;
                    dataDictionary["updated"] = now.ToString();
                    String idString = tableName + ".id = " + id;
                    database.Update(tableName, dataDictionary, idString);
                }
                vaultObject.setId(this.getVaultObjectId(vaultObject));
            } catch (Exception e) {
                log(e.Message);
            } 
         
        }

        public int getVaultObjectId(VaultObject vaultObject)
        {
            int id = -1;
            try {
                String tableName = tableNameFromPluginName(vaultObject.getPluginName());
                String query = "select id \"id\"";
                query += " from " + tableName;
                query += " where objectName = \"" + vaultObject.getObjectName() + "\"";

                // use passport info if present
                if (vaultObject.getPassport() != null) {
                    String username = vaultObject.getPassport().getUser().username;
                    query += " and passportUsername = \"" + username + "\"";
                } else {
                    query += " and passportUsername = \"\"";
                }
                debug(query);

                DataTable results = database.GetDataTable(query);
                // there should only ever be one copy of an object in the database
                DataRow row = results.Rows[0];
                id = Int32.Parse(row["id"].ToString());
            } catch (Exception e) {
                id = -1;
            }
            return id;
        }
     
        public void getVaultObject(VaultObject vaultObject)
        {
            try {
                String tableName = tableNameFromPluginName(vaultObject.getPluginName());
                String query = "select id \"id\",";
                query += "objectName \"objectName\", xml \"xml\"";
                query += " from " + tableName;
                query += " where objectName = \"" + vaultObject.getObjectName() + "\"";
    
                // use passport info if present
                if (vaultObject.getPassport() != null) {
                    String username = vaultObject.getPassport().getUser().username;
                    query += " and passportUsername = \"" + username + "\"";
                } else {
                    query += " and passportUsername = \"\"";
                }

                debug(query);

                DataTable results = database.GetDataTable(query);
                // there should only ever be one copy of an object in the database
                DataRow row = results.Rows[0];
                vaultObject.fromDataRow(row);
            } catch (Exception e) {
                throw new VaultObjectNotFoundException();
            }
        }

        //
        // PRIVATE and INTERNAL
        //

        internal Vault()
        {
            databaseFileName = PLUGIN_FOLDER + Path.DirectorySeparatorChar + "vault.db";
            debug("database file location:" + databaseFileName);

            Dictionary<string, string > options = new Dictionary<string, string>();
            options["Data Source"] = databaseFileName;
            options["New"] = "True";
            database = new SQLiteDatabase(options);

            setup();            
        }

        private String tableNameFromPluginName(String pluginName)
        {
            String tableName = Regex.Replace(pluginName, @"[^\w\.-]", "");
            tableName = tableName.ToLower();
            if (tableName.Length > 16) {
                tableName = tableName.Substring(0, 15);
            }

            return tableName;
        }

        private SQLiteConnection getConnection()
        {
            SQLiteConnection con = new SQLiteConnection("Data Source=" + databaseFileName + ";New=True;");
            return con;
        }

        private void createTable(String tableName)
        {
            SQLiteConnection con = null;
            try {
                // create table if need be
                debug("creating table");
                con = this.getConnection();
                con.Open();
                SQLiteCommand command = con.CreateCommand();

                string createTable = "CREATE TABLE " + tableName + "(id INTEGER PRIMARY KEY,";
                createTable += " objectName TEXT NOT NULL, passportUsername TEXT,";
                createTable += " xml TEXT, created TEXT, updated TEXT);";
                command.CommandText = createTable;
                command.ExecuteNonQuery();
            } catch (Exception e) {
                debug("table exists, skipped");
            } finally {
                con.Close();
            }
        }

        private void setup()
        {
            // create folder
            createDirectory(PLUGIN_FOLDER);

            // setup any properties
            setupProperties();
        }

        private void createDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
            }
        }

        private void setupProperties()
        {
        }

        private void debug(String message)
        {
            #if (DEBUG)
            System.Console.WriteLine(message);
            #endif
        }

        private void log(String message)
        {
            System.Console.WriteLine(message);
        }

    }

    //
    // EXCEPTIONS
    //

    public class VaultObjectNotFoundException : Exception
    {
    }

}