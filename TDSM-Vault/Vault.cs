using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Terraria_Server;
using Envoy.TDSM_Passport;

namespace Envoy.TDSM_Vault
{
    public class SampleObject : VaultObject
    {
        public string foo = "bar";

        public SampleObject(String pluginName) : base(pluginName)
        {
             
        }

        public SampleObject() : base("SamplePlugin")
        {
             
        }

        public override string toXml ()
        {
            return foo;  
        }
     
        public override void fromXml (string xml)
        {
            this.foo = xml;
        }
    }
 
    public class Vault
    {
        private string PLUGIN_FOLDER = Statics.PluginPath + Path.DirectorySeparatorChar + "Vault";
        private const string TABLE_NAME = "vault";
        private string databaseFileName;
        private SQLiteDatabase database;

        static void Main (string[] args)
        {
            System.Console.WriteLine("start");

            Vault vault = new Vault();
            SampleObject o = new SampleObject();
            vault.store(o);

            o.foo = "pepe";
            vault.store(o);         

            SampleObject vo = new SampleObject();
            vault.getVaultObject(vo);           
            System.Console.WriteLine(vo.getId());
            System.Console.WriteLine(vo.getPluginName());
            System.Console.WriteLine(vo.getObjectName());
            System.Console.WriteLine(vo.foo);           

            PassportManager pm = PassportManagerFactory.getPassportManager();


            System.Console.WriteLine("done");
        }

        public Vault()
        {
            databaseFileName = PLUGIN_FOLDER + "/vault.db";
            database = new SQLiteDatabase(databaseFileName);
            setup();            
        }
     
        public void store (VaultObject vaultObject)
        {
            try {
                Dictionary<string, string > dataDictionary = vaultObject.getDataDictionary();
                int id = this.getVaultObjectId(vaultObject);
                if (id == -1) {
                    System.Console.WriteLine("insert");
                    DateTime now = System.DateTime.Now;
                    dataDictionary["created"] = now.ToString();
                    dataDictionary["updated"] = now.ToString();
                    database.Insert(TABLE_NAME, dataDictionary);
                } else {
                    System.Console.WriteLine("update");
                    DateTime now = System.DateTime.Now;
                    dataDictionary["updated"] = now.ToString();
                    String idString = TABLE_NAME + ".id = " + id;
                    database.Update(TABLE_NAME, dataDictionary, idString);
                }
                vaultObject.setId(this.getVaultObjectId(vaultObject));                             
            } catch (Exception e) {
                System.Console.WriteLine(e.Message);    
            } 
         
        }

        public int getVaultObjectId (VaultObject vaultObject)
        {
            int id = -1;
            try {
                String query = "select id \"id\"";
                query += " from " + TABLE_NAME;
                query += " where pluginName = \"" + vaultObject.getPluginName() + "\"";
                query += " and objectName = \"" + vaultObject.getObjectName() + "\"";

                // use passport info if present
                if (vaultObject.getPassport() != null) {
                    String username = vaultObject.getPassport().getUser().username;
                    query += " and passportUsername = \"" + username + "\"";
                }
                System.Console.WriteLine(query);
             
                DataTable results = database.GetDataTable(query);
             
                DataRow row = results.Rows[0];              
                id = Int32.Parse(row["id"].ToString());
            } catch (Exception e) {
                id = -1;
            }
            return id;
        }
     
        public void getVaultObject (VaultObject vaultObject)
        {
         
            String query = "select id \"id\", pluginName \"pluginName\",";
            query += "objectName \"objectName\", xml \"xml\"";
            query += " from " + TABLE_NAME;
            query += " where pluginName = \"" + vaultObject.getPluginName() + "\"";
            query += " and objectName = \"" + vaultObject.getObjectName() + "\"";

            // use passport info if present
            if (vaultObject.getPassport() != null) {
                String username = vaultObject.getPassport().getUser().username;
                query += " and passportUsername = \"" + username + "\"";
            }

            System.Console.WriteLine(query);    
         
            DataTable results = database.GetDataTable(query);
         
            DataRow row = results.Rows[0];              
            vaultObject.fromDataRow(row);
        }
     
        private SQLiteConnection getConnection ()
        {
            SQLiteConnection con = new SQLiteConnection("Data Source=" + databaseFileName);
            return con;
        }

        private void setup ()
        {
            SQLiteConnection con = null;
            try {
                // create folder
                createDirectory(PLUGIN_FOLDER);

                // setup any properties
                setupProperties();

                // create table if need be
                System.Console.WriteLine("Creating table");
                con = this.getConnection();
                con.Open();         
                SQLiteCommand command = con.CreateCommand();
             
                string createTable = "CREATE TABLE " + TABLE_NAME + "(id INTEGER PRIMARY KEY,";
                createTable += " pluginName TEXT NOT NULL, objectName TEXT NOT NULL, passportUsername TEXT,";
                createTable += " xml TEXT, created TEXT, updated TEXT);";
                command.CommandText = createTable;
                command.ExecuteNonQuery();
            } catch (Exception e) {
                System.Console.WriteLine("table exists");
            } finally {              
                con.Close();    
            }
        }

        private void createDirectory (string dirPath)
        {
            if (!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
            }
        }

        private void setupProperties ()
        {
            //properties = new Properties(PLUGIN_FOLDER + Path.DirectorySeparatorChar + "vault.properties");
            //properties.Load();
            //properties.pushData(); //Creates default values if needed.
            //properties.Save();
        }

    }
 
}