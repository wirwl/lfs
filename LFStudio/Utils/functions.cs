using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security.AccessControl;
using System.Security.Principal;

namespace lf2dat
{
    public static class functions
    {


        //Standart lf password: SiuHungIsAGoodBearBecauseHeIsVeryGood
        public static String DatFileToPlainText(String filepath, String password)
        {            
            FileStream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[fileStream.Length];
            fileStream.Read(buffer, 0, buffer.Length);
            StringBuilder decryptedtext = new StringBuilder((int)fileStream.Length);
            fileStream.Close();
            int index = 12;
            for (int i = 123; i < buffer.Length; i++)///////////////
            {              
                byte b1 = (byte)(buffer[i] - (byte)password[index]);
                decryptedtext.Append((char)b1);
                index++;
                if (index == password.Length) index = 0;
            }
            return decryptedtext.ToString();
        }
        public static void PlainTextToDatFile(string text, string filepath, string password, string First123 = "")
        {
            byte[] dat = new byte[123 + text.Length];
            for (int i = 0; i < 123; i++) dat[i] = 0;
            for (int i = 0; i < First123.Length; i++)
            {
                if (i > 123) continue;
                dat[i] = (byte)First123[i];
            }
            int index = 12;
            for (int i = 0; i < text.Length; i++)
            {         
                byte b1 = (byte)((byte)text[i] + (byte)password[index]);
                dat[i + 123] = b1;
                index++;
                if (index == password.Length) index = 0;
            }            
            FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write);            
            fs.Write(dat, 0, 123 + text.Length);
            fs.Close();
        }        
    /*    public static void AddFileSecurity(string fileName, string account, FileSystemRights rights, AccessControlType controlType)
        {
            FileSecurity fSecurity = File.GetAccessControl(fileName);
            fSecurity.AddAccessRule(new FileSystemAccessRule(account, rights, controlType));            
            File.SetAccessControl(fileName, fSecurity);
        }       
        public static void RemoveFileSecurity(string fileName, string account, FileSystemRights rights, AccessControlType controlType)
        {
            FileSecurity fSecurity = File.GetAccessControl(fileName);
            fSecurity.RemoveAccessRule(new FileSystemAccessRule(account, rights, controlType));
            File.SetAccessControl(fileName, fSecurity);

        }*/
    }
}
