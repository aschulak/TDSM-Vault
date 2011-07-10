using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Envoy.TDSM_Vault
{
    public class Properties : PropertiesFile
    {
        public Properties(String propertiesPath) : base(propertiesPath)
        {
        }
     
        public void pushData()
        {
            setSaveTimeMillis(saveTimeMillis());
        }

        public int saveTimeMillis()
        {
            string saveTimeMillis = base.getValue("saveTimeMillis");
            if (saveTimeMillis == null || saveTimeMillis.Trim().Length < 0) {
                return 300000; // 5 min
            } else {
                return Int32.Parse(saveTimeMillis);
            }
        }

        public void setSaveTimeMillis(int saveTimeMillis)
        {
            base.setValue("saveTimeMillis", saveTimeMillis.ToString());
        }

    }

}