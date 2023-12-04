using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

class Program
{
    // Constants for error codes
    const int ERROR_INSUFFICIENT_BUFFER = 122;
    const int NO_ERROR = 0;

    // Structure representing a row in the IP forward table
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_IPFORWARDROW
    {
        public uint dwForwardDest, dwForwardMask, dwForwardPolicy, dwForwardNextHop, dwForwardIfIndex, dwForwardType, dwForwardProto, dwForwardAge, dwForwardNextHopAS, dwForwardMetric1, dwForwardMetric2, dwForwardMetric3, dwForwardMetric4, dwForwardMetric5;
    }

    // Importing necessary functions from iphlpapi.dll
    [DllImport("iphlpapi.dll", SetLastError = true)]
    public static extern int GetIpForwardTable(IntPtr pIpForwardTable, ref int pdwSize, bool bOrder);

    [DllImport("iphlpapi.dll", SetLastError = true)]
    public static extern int FreeMibTable(IntPtr pTable);

    // Main entry point of the program
    static async Task Main()
    {
        // Asynchronously search for network destination routes
        await SearchRoutesForNetworkDestinationAsync("127.0.0.1");
        
        // Display message and wait for user input to keep console window open
        Console.WriteLine("Press enter to exit...");
        Console.ReadLine();
    }

    // Asynchronously search for network destination routes across all network interfaces
    static async Task SearchRoutesForNetworkDestinationAsync(string networkDestination)
    {
        // Asynchronously process routes for all network interfaces
        await Task.WhenAll(NetworkInterface.GetAllNetworkInterfaces().Select(async nic =>
        {
            Console.WriteLine($"Searching for network destination {networkDestination} in routes for interface: {nic.Name}\n");
            
            // Retrieve matching routes asynchronously
            var routes = await GetMatchingRoutesAsync(nic, networkDestination);

            // Display information for each matching route
            routes.ForEach(route =>
            {
                Console.WriteLine($"  {networkDestination} found on interface {nic.GetIPProperties().UnicastAddresses[0].Address} ({route.dwForwardIfIndex})");
                //Console.WriteLine($"  Interface Type: (You may need to determine this from your specific context)");
            });

            // Add a newline for better output formatting
            Console.WriteLine();
        }));
    }

    // Asynchronously retrieve matching routes for a specific network interface
    static async Task<List<MIB_IPFORWARDROW>> GetMatchingRoutesAsync(NetworkInterface nic, string networkDestination)
    {
        IntPtr pIpForwardTable = IntPtr.Zero;
        int dwSize = 0;

        try
        {
            // Check and allocate memory for the IP forward table
            if (GetIpForwardTable(IntPtr.Zero, ref dwSize, false) == ERROR_INSUFFICIENT_BUFFER)
            {
                pIpForwardTable = Marshal.AllocCoTaskMem(dwSize);

                // Retrieve the IP forward table
                if (GetIpForwardTable(pIpForwardTable, ref dwSize, false) == NO_ERROR)
                {
                    // Asynchronously parse the route table
                    return await Task.Run(() => ParseRouteTable(pIpForwardTable, networkDestination, nic));
                }
                else
                {
                    // Throw an exception if there is an error retrieving the routing table
                    throw new InvalidOperationException($"Error retrieving routing table. Error code: {Marshal.GetLastWin32Error()}");
                }
            }
            else
            {
                // Throw an exception if there is an error retrieving the routing table size
                throw new InvalidOperationException($"Error retrieving routing table size. Error code: {Marshal.GetLastWin32Error()}");
            }
        }
        finally
        {
            // Free allocated memory for the IP forward table
            if (pIpForwardTable != IntPtr.Zero)
            {
                FreeMibTable(pIpForwardTable);
            }
        }

        // Return an empty list if no matching routes are found
        return new List<MIB_IPFORWARDROW>();
    }

    // Parse the IP forward table and filter matching routes
    static List<MIB_IPFORWARDROW> ParseRouteTable(IntPtr pIpForwardTable, string networkDestination, NetworkInterface nic)
    {
        // Read the size of the IP forward table
        int tableSize = Marshal.ReadInt32(pIpForwardTable);
        int structSize = Marshal.SizeOf<MIB_IPFORWARDROW>();

        // Use parallel processing for improved performance
        return Enumerable.Range(0, tableSize)
            .AsParallel()
            .Select(i => (MIB_IPFORWARDROW)Marshal.PtrToStructure(new IntPtr(pIpForwardTable.ToInt64() + Marshal.SizeOf(typeof(int)) + i * structSize), typeof(MIB_IPFORWARDROW)))
            .Where(route => new IPAddress(route.dwForwardDest).ToString() == networkDestination && route.dwForwardIfIndex == nic.GetIPProperties().GetIPv4Properties().Index)
            .ToList();
    }
}
