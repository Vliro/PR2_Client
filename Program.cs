using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Runtime.InteropServices;

namespace PR2_Client
{
    static class Program
    {   
        private static string encryptionIV = "JmM5KnkqNXA9MVVOeC9Ucg==";
        private static String encryptionKEY = "VUovam5GKndSMHFSSy9kSA==";
        public static ServerHandler handler;
        static void Main(string[] args)
        {
          handler = new ServerHandler();
          int server_id = handler.promptServer();
          Server m_Server = handler.servers[server_id-1];
          Console.Write("Enter username: ");
          String user = Console.ReadLine();
          SecureString pass = getPasswordFromConsole("Enter password: ");
          PR2Socket sock = new PR2Socket(m_Server.address, m_Server.port, user, ConvertToUnsecureString(pass), server_id);
          while(true)
            sock.chat(Console.ReadLine());
 //         HttpHandler.Send();
 //         sock.Send("get_customize_info`");
        }
                public static string ConvertToUnsecureString(this SecureString securePassword)
{
    if (securePassword == null)
        throw new ArgumentNullException("securePassword");

    IntPtr unmanagedString = IntPtr.Zero;
    try
    {
        unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
        return Marshal.PtrToStringUni(unmanagedString);
    }
    finally
    {
        Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
    }
}
public static SecureString getPasswordFromConsole(String displayMessage) {
    SecureString pass = new SecureString();
    Console.Write(displayMessage);
    ConsoleKeyInfo key;

    do {
        key = Console.ReadKey(true);

        // Backspace Should Not Work
        if (!char.IsControl(key.KeyChar)) {
            pass.AppendChar(key.KeyChar);
            Console.Write("*");
        } else {
            if (key.Key == ConsoleKey.Backspace && pass.Length > 0) {
                pass.RemoveAt(pass.Length - 1);
                Console.Write("\b \b");
            }
        }
    }
    // Stops Receving Keys Once Enter is Pressed
    while (key.Key != ConsoleKey.Enter);
    return pass;
}
    }
}
