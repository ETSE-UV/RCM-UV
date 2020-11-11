/* 
    This script provides performance metrics (CPU Usage(%), GPU Usage(%), Memory Usage (GBs)
    
 */
/*
   POWERSHELL 3.0 MUST BE INSTALLED. And a refence to System.Management.Automation.dll must be included. (This DLL is normally on <<C:\Program Files (x86)\Reference Assemblies\Microsoft\WindowsPowerShell\3.0>>)
*/
using System.IO;
using System;
using System.Diagnostics;
using System.Threading;

//inports for POWERSHELL
using System.Management.Automation;

public class ProfilingScript
{
    // Defining the path and the file name under which the text file will be saved



    static void Main(string[] args)
    {
        bool write_output_to_concole = true;

        DirectoryInfo di_aux = Directory.CreateDirectory("logs");
        DirectoryInfo di = Directory.CreateDirectory("logs/"
            + DateTime.Now.ToString("yyyy_MM_dd"));
        

        string process_name = "Unity";

        // Initializing needed variables
        float f = 1024f;                                                            //variable to divide the memory counter (given in bytes)
        double cpuUsage = 0f, gpuUsage = 0f, memoryMBs = 0f;                        //Raw values about GPU, CPU, and RAM usage given by Diagnostic libraries 
        double incGPU = 0f;                            //Processed values about GPU, CPU, and RAM usage 
        double numOfProcessors = Convert.ToDouble(Environment.ProcessorCount);      // Number of processors of the system
        //Console.WriteLine(numOfProcessors);
        int cnt = 0;
        
        string whole_cpu_ram_str, whole_gpu_str, whole_network_str;
        double incWholeCPU = 0f, incWholeMemory = 0f;

        //Strings to query the powershell
        String ps_script_gpu_first = "Get-WmiObject Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine -Filter \"Name LIKE 'pid_";
        String ps_script_gpu_second = "%'\" | select-Object UtilizationPercentage | Measure-Object -Sum UtilizationPercentage | Select-Object sum | ft -HideTableHeaders";
        String ps_script_gpu_all_pids = "";

        String ps_script_wholecpuram_first = "Get-WmiObject -class Win32_PerfFormattedData_PerfProc_Process -filter \"Name LIKE'";
        String ps_script_wholecpuram_second = "%'\" | Select-Object PercentProcessorTime, workingSetPrivate | Measure-Object -Sum PercentProcessorTime, WorkingSetPrivate | Select-Object Sum | ft -HideTableHeaders";
        
        
        Console.WriteLine("Profiling Script was successfully initialized.");

        switch (args.Length)
        {
            case 0:
                //Confirm Name of the process to capture
                do
                {
                    Console.WriteLine("Name of the process is: <<" + process_name + ">>>? Y/N");
                    string input = Console.ReadLine();
                    if (input == "N" || input == "n")
                    {
                        Console.WriteLine("Write name of process to study:");
                        process_name = Console.ReadLine();
                    }
                    else
                        break;
                } while (true);
                break;
            case 2:
                process_name = args[0];
                write_output_to_concole = Boolean.Parse(args[1]);
                System.Console.WriteLine("Profiling starterd with following arguments:");
                System.Console.WriteLine("process_name = " + process_name);
                System.Console.WriteLine("write_output_to_concole = " + write_output_to_concole);
                break;
            default:
                System.Console.WriteLine("Expecting 0 or 2 arguments");
                return;
        }

        string path = di_aux.ToString() + '/' + di.ToString() + '/'
            + "PerformanceData_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_") + process_name + ".txt";
        StreamWriter output = new StreamWriter(path);



        // Waiting for user to open a Unity process
        while (Process.GetProcessesByName(process_name).Length == 0)
        {

            Console.WriteLine("Waiting for user to open a project named <<" + process_name + ">>");
            Thread.Sleep(2000);

        }

        cnt = 0;

        Process[] activeProc = Process.GetProcessesByName(process_name);
        int unity_pid = activeProc[0].Id;
        output.Write("Captured information of Process Named: " + process_name + Environment.NewLine);

        

        Console.WriteLine("Data recording started. Press Escape button to terminate.");
        output.Write("CPU Usage(%), GPU Usage(%), RAM Memory(GBs), Total Received (Bytes), Total Sent (Bytes)" + Environment.NewLine);
        output.Write("------------------------------------------------------------" + Environment.NewLine);


        //lets create the power shell comands fist to get lower CPU usage
        PowerShell ps_cpu_ram = PowerShell.Create();
        ps_cpu_ram
            .AddScript(ps_script_wholecpuram_first + process_name + ps_script_wholecpuram_second)
            .AddCommand("Out-String");

        ps_script_gpu_all_pids = "";
        foreach (Process proc in Process.GetProcessesByName(process_name))
        {
            int this_pid = proc.Id;
            if (this_pid != unity_pid)
                ps_script_gpu_all_pids += "%' OR Name LIKE 'pid_" + this_pid;
        }

        PowerShell ps_gpu = PowerShell.Create();
        ps_gpu
            .AddScript(ps_script_gpu_first + unity_pid + ps_script_gpu_all_pids + ps_script_gpu_second)
            .AddCommand("Out-String");

        PowerShell ps_network = PowerShell.Create();
        ps_network
            .AddScript("Get-NetAdapterStatistics")
            .AddCommand("Out-String");

        // Recording is terminated when Escape button is pressed
        try
        {
            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape) && (activeProc.Length >= 1))
            {
                // Ensuring Unity is still running

                if (activeProc.Length >= 1)
                {
                    //Getting strings with values
                    //CPU and RAM
                    whole_cpu_ram_str = ps_cpu_ram
                        .Invoke<String>()[0].ToString();
                    string [] splits = whole_cpu_ram_str.Split(new[] { Environment.NewLine },StringSplitOptions.None);
                    //foreach (string strs in splits) Console.WriteLine("Splits -> " + strs);
                            try {incWholeCPU = double.Parse(splits[1]); } //Starts at index 1 because the powershell starts the output with an empty line
                            catch {
                                Console.WriteLine("Exception parsing whole CPU usage value");
                                incWholeCPU = 0.0;
                            }
                            try { incWholeMemory = double.Parse(splits[2]); }
                            catch {
                                Console.WriteLine("Exception parsing whole Memory usage value");
                                incWholeMemory = 0.0;
                            }
                    
                    incWholeCPU = incWholeCPU / numOfProcessors;
                    incWholeMemory = incWholeMemory / f / f;


                    //GPU
                    whole_gpu_str = ps_gpu
                        .Invoke<String>()[0].ToString();
                    splits = whole_gpu_str.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    //foreach (string strs in splits) Console.WriteLine("Splits sum-> " + strs);
                    
                    try { incGPU = double.Parse(splits[1]); } //Starts at index 3 because the powershell starts the output with an empty line
                    catch
                    {
                        Console.WriteLine("Exception parsing whole GPU usage value");
                        incGPU = 0.0;
                    }


                    // collecting all netowork metrics
                    whole_network_str = ps_network
                        .Invoke<String>()[0].ToString();
                    string[] split_lines = whole_network_str.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    int index = 2;
                    long totalReceivedBytes = 0;
                    long totalSentBytes = 0;
                    while (index < split_lines.Length)
                    {
                        string[] split_values = split_lines[index].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        string receivedBytes_str = split_values[1];
                        string sentBytes_str = split_values[3];
                        totalReceivedBytes += long.Parse(receivedBytes_str);
                        totalSentBytes += long.Parse(sentBytes_str);
                        index++;
                    }


                    // Printing and saving metrics 
                    if (incWholeCPU!=0||incGPU!= 0||incWholeMemory!=0) //If we get a zero in any of the readings we discard this sample
                    {
                        cpuUsage += incWholeCPU;
                        gpuUsage += incGPU;
                        memoryMBs += incWholeMemory;
                        cnt += 1;

                        if (write_output_to_concole)
                        {
                            Console.WriteLine("Sample " + cnt + " was received.");
                            Console.WriteLine("CPU %: " + incWholeCPU);
                            Console.WriteLine("GPU %: " + incGPU);
                            Console.WriteLine("RAM (MBs): " + incWholeMemory);
                            Console.WriteLine("Network received (Bytes): " + totalReceivedBytes);
                            Console.WriteLine("Network sent (Bytes): " + totalSentBytes);
                        }

                        output.Write(DateTime.UtcNow + ", " + incWholeCPU + ", " + incGPU + ", " + incWholeMemory + ", " + totalReceivedBytes + ", " + totalSentBytes + Environment.NewLine);
                        
                    }
                    

                    activeProc = Process.GetProcessesByName(process_name);
                    
                }

                else
                {
                    // Unity terminated
                    Console.WriteLine("Unity process not found.");
                    
                }



            }
        }
        catch (Exception e)
        {
            Console.WriteLine("An exception has happened. Has "+process_name +" been closed?");
        }
        // Printing and saving average values of metrics

        Console.WriteLine("The session was terminated by the user after the reception of " + cnt + " samples.");
        Console.WriteLine("Average CPU %: " + (cpuUsage / cnt));
        Console.WriteLine("Average GPU %: " + (gpuUsage / cnt));
        Console.WriteLine("Average RAM (MBs): " + (memoryMBs / cnt));
        output.Write("------------------------------------------------------------" + Environment.NewLine);
        output.Write("Average CPU %: " + (cpuUsage / cnt) + ", Average GPU %: " + (gpuUsage / cnt) + ", Average RAM (MBs): " + (memoryMBs / cnt) + Environment.NewLine);

        output.Flush();
        output.Close();

        while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
        {
            Console.WriteLine("Press Escape to terminate the console application.");
            Thread.Sleep(2000);
        }


    }
}
