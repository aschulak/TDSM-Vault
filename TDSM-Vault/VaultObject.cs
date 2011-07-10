using System;
using System.Collections.Generic;
using System.Data;
using Envoy.TDSM_Passport;

namespace Envoy.TDSM_Vault
{
    public class VaultObject
    {

        private int id;
        private string pluginName;
        private string objectName;
        private Passport passport;

        public VaultObject(String pluginName)
        {
            this.pluginName = pluginName;
            this.id = -1;
            this.objectName = GetType().ToString();
            this.passport = null;
        }

        public string getObjectName()
        {
            return objectName;
        }

        public string getPluginName()
        {
            return pluginName;
        }

        public int getId()
        {
            return id;
        }

        public Passport getPassport()
        {
            return passport;
        }

        public void setPassport(Passport passport)
        {
            this.passport = passport;
        }

        //
        // API
        //
     
        public virtual string toXml()
        {
            throw new NotImplementedException();         
        }
     
        public virtual void fromXml(string xml)
        {
            throw new NotImplementedException();
        }
     
                     
        //
        // PRIVATE and INTERNAL
        //

        internal VaultObject()
        {
         
        }

        internal void setId(int id)
        {
            this.id = id;    
        }

        internal Dictionary<string, string> getDataDictionary()
        {
            Dictionary<string, string > dataDictionary = new Dictionary<string, string>();           
            dataDictionary["objectName"] = getObjectName();
            if (passport != null) {
                dataDictionary["passportUsername"] = passport.getUser().username;
            }
            dataDictionary["xml"] = toXml();
            return dataDictionary;
        }

        internal void fromDataRow(DataRow dataRow)
        {
            id = Int32.Parse(dataRow["id"].ToString());
            string xml = dataRow["xml"].ToString();
            System.Console.WriteLine("xml:" + xml);
            fromXml(xml);
        }

    }
 
}