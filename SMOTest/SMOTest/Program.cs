//using System;
//using Microsoft.SqlServer.Management.Common;
//using Microsoft.SqlServer.Management.Smo;

//public class A
//{
//    public static void Main()
//    {
//        // For remote connection, remote server name / ServerInstance needs to be specified
//        ServerConnection srvConn2 = new ServerConnection("localhost");
//        srvConn2.LoginSecure = false;
//        srvConn2.Login = "sa";
//        srvConn2.Password = "0";
//        Server srv3 = new Server(srvConn2);
//        Console.WriteLine(srv3.Information.Version); // connection is established
//    }
//}
