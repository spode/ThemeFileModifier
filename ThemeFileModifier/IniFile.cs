using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

// Credits: Danny Beckett, Leo https://stackoverflow.com/a/14906422, https://stackoverflow.com/questions/217902/reading-writing-an-ini-file
namespace ThemeFileModifier
{
    class IniFile   // revision 11
    {
        string Path;
        string EXE = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern uint GetPrivateProfileSectionNames(IntPtr lpszReturnBuffer,
           uint nSize, string lpFileName);

        [DllImport("kernel32.dll")]
        static extern uint GetPrivateProfileSection(string lpAppName, IntPtr lpszReturnBuffer,
           uint nSize, string lpFileName);

        //[DllImport("kernel32", CharSet = CharSet.Unicode)]
        //static extern int GetPrivateProfileSectionNames(
        //   StringBuilder RetVal,
        //   int Size,
        //   string FilePath
        //);

        public IniFile(string IniPath = null)
        {
            Path = new FileInfo(IniPath ?? EXE + ".ini").FullName;
        }

        public string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 255, Path);
            return RetVal.ToString();
        }

        public void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? EXE, Key, Value, Path);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? EXE);
        }

        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? EXE);
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }

        //public List<string> GetSectionHeaders()
        //{
        //    var RetVal = new byte[2048];
        //    GetPrivateProfileSectionNames(RetVal, RetVal.Length, Path);
        //    var sectionsString = RetVal.ToString();
        //    // split string to list
        //    // for section in list
        //    // get data from section with https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprofilesectiona
        //    // loop sections and data and write to other file

        //    Console.WriteLine(System.Text.Encoding.Default.GetString(RetVal));

        //    return new List<string>();
        //}


        public string[] GetSectionData(string section)
        {
            uint MAX_BUFFER = 32767;
            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER);
            uint bytesReturned = GetPrivateProfileSection(section, pReturnedString, MAX_BUFFER, Path);
            if (bytesReturned == 0)
            {
                Marshal.FreeCoTaskMem(pReturnedString);
                return null;
            }
            string local = Marshal.PtrToStringAnsi(pReturnedString, (int)bytesReturned).ToString();
            Marshal.FreeCoTaskMem(pReturnedString);
            //use of Substring below removes terminating null for split
            return local.Substring(0, local.Length - 1).Split('\0');
        }

        //https://pinvoke.net/default.aspx/kernel32.GetPrivateProfileSectionNames
        public string[] GetSectionNames()
        {
            uint MAX_BUFFER = 32767;
            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER);
            uint bytesReturned = GetPrivateProfileSectionNames(pReturnedString, MAX_BUFFER, Path);
            if (bytesReturned == 0)
                return null;
            string ret = Marshal.PtrToStringAnsi(pReturnedString, (int)(bytesReturned * 2));
            Marshal.FreeCoTaskMem(pReturnedString);

            // convert ASCII string to Unicode string
            byte[] bytes = Encoding.ASCII.GetBytes(ret);
            string local = Encoding.Unicode.GetString(bytes);

            //use of Substring below removes terminating null for split
            return local.Substring(0, local.Length - 1).Split('\0');
        }
    }
}