/* 
    Main Developer: Juan Antonio De Rus Arance
    Supervisor: Mario Montagud (mario.montagud@i2cat)
    This is a work done at the University of Valencia (Spain), in collaboration with the i2CAT Foundation (Barcelona, Spain)
    
    
    This script provides performance metrics (CPU Usage(%), GPU Usage(%), Memory Usage (GBs)
    
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
    

    
    static void Main()
    {
        DirectoryInfo di_aux = Directory.CreateDirectory("logs");
        DirectoryInfo di = Directory.CreateDirectory("logs/" 
            + DateTime.Now.ToString("yyyy_MM_dd"));
        string path = di_aux.ToString() + '/' + di.ToString() + '/'
            + "PerformanceData " + DateTime.Now.ToString("yyyy_MM_dd-_-HH_mm_ss") + ".txt";
        StreamWriter output = new StreamWriter(path);


        string process_name = "Unity"; 
        
        // Initializing needed variables
        float f = 1024f;                                                            //variable to divide the memory counter (given in bytes)
        double cpuUsage = 0f, gpuUsage = 0f, memoryMBs = 0f;                        //Raw values about GPU, CPU, and RAM usage given by Diagnostic libraries 
        double incCPU = 0f, incGPU = 0f, incMemory = 0f;                            //Processed values about GPU, CPU, and RAM usage 
        double numOfProcessors = Convert.ToDouble(Environment.ProcessorCount);      // Number of processors of the system
        int cnt = 0;
        String gpu_str, cpu_str, ram_str, enginesGPU;

        //Strings to query the powershell
        String ps_script_gpu_first= "(Get-WmiObject Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine -Filter \"Name LIKE 'pid_";
        String ps_script_gpu_second= "%'\" | Sort-Object -Descending -Property UtilizationPercentage  | Select-Object -First 1  | select-Object UtilizationPercentage).UtilizationPercentage";

        String ps_script_cpu_first= "(Get-WmiObject -class Win32_PerfFormattedData_PerfProc_Process -filter IDProcess=";
        String ps_script_cpu_second= " | select-object PercentProcessorTime).PercentProcessorTime";

        String ps_script_ram_first = "(Get-WmiObject -class Win32_PerfFormattedData_PerfProc_Process -filter IDProcess=";
        String ps_script_ram_second = " | select-object WorkingSetPrivate).WorkingSetPrivate";

        String ps_script_nameEngines_first = "Get-WmiObject Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine -Filter \"Name LIKE '%pid_";
        String ps_script_nameEngines_second ="%'\" | select-object name";

        String ps_script_GPUuseEngines_first = "(Get-WmiObject Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine -Filter \"Name LIKE '%pid_";
        String ps_script_GPUuseEngines_second = "%'\" | select-object utilizationPercentage).UtilizationPercentage";

        /* In this block we'll try to use Powershell for debug purposes  */
        /* and to put examples for powershell use                        */
        {

            /*****************************************************************/
            /*****************************************************************/
            /*Console.WriteLine("Before starting the script we'll try to use powershell");*/

            /*
            Console.WriteLine("\r\n Task 1: Get process list");
            foreach (String str in
            PowerShell.Create()
                .AddScript("Get-Process")
                .AddCommand("Out-String")
                .Invoke<String>())
            {
                Console.WriteLine(str);
            };
            */

            /*
            Console.WriteLine("\r\n Task 2: Get specific process information");
            Console.WriteLine("Getting pid of Explorer");
            Process[] Explorer = Process.GetProcessesByName("Explorer");
            int pid = Explorer[0].Id;
            Console.WriteLine("Pid of Explorer is: " + pid);
            String ps_script = "Get-WmiObject -class Win32_PerfFormattedData_PerfProc_Process -filter IDProcess=" + pid;
            Console.WriteLine("Now we are gonna execute the next script in Powershell: >>>>  \r\n" + ps_script);

            foreach (String str in
            PowerShell.Create()
                .AddScript(ps_script)
                .AddCommand("Out-String")
                .Invoke<String>())
            {
                Console.WriteLine(str);
            };
            */

            /*
            Console.WriteLine("\r\n Task 3: Get specific process %CPU and Ram usage");
            Console.WriteLine("Getting pid of Explorer");
            Process[] Explorer = Process.GetProcessesByName("Explorer");
            int pid = Explorer[0].Id;
            Console.WriteLine("Pid of Explorer is: " + pid);
            String ps_script = "Get-WmiObject -class Win32_PerfFormattedData_PerfProc_Process -filter IDProcess=" + pid
                + " | select-object PercentProcessorTime , @{Name=\"RAM\"; Expression={$_.WorkingSetPrivate/1mb}}";
            Console.WriteLine("Now we are gonna execute the next script in Powershell: >>>>  \r\n" + ps_script);

            foreach (String str in
            PowerShell.Create()
                .AddScript(ps_script)
                .AddCommand("Out-String")
                .Invoke<String>())
            {
                Console.WriteLine(str);
            };*/

            /*
            Console.WriteLine("\r\n Task 4: Get specific process GPU use information");
            Console.WriteLine("Getting pid of Explorer");
            Process[] Explorer = Process.GetProcessesByName("Explorer");
            int pid = Explorer[0].Id;
            Console.WriteLine("Pid of Explorer is: " + pid);
            String ps_script = "Get-WmiObject Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine -Filter \"Name LIKE 'pid_" + pid
                + "%'\"";
            Console.WriteLine("Now we are gonna execute the next script in Powershell: >>>>  \r\n" + ps_script);

            foreach (String str in
            PowerShell.Create()
                .AddScript(ps_script)
                .AddCommand("Out-String")
                .Invoke<String>())
            {
                Console.WriteLine(str);
            };
            */


            /*
            Console.WriteLine("\r\n Task 5: Get from an specific process only the %GPU usage of the most working GPU engine");
            Console.WriteLine("Getting pid of Explorer");
            Process[] Explorer = Process.GetProcessesByName("Explorer");
            int pid = Explorer[0].Id;
            Console.WriteLine("Pid of Explorer is: " + pid);
            String ps_script = "Get-WmiObject Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine -Filter \"Name LIKE 'pid_" + pid+ "%'\""
                +" | Sort-Object -Descending -Property UtilizationPercentage "
                +" | Select-Object -First 1 "
                +" | select-Object UtilizationPercentage";
            Console.WriteLine("Now we are gonna execute the next script in Powershell: >>>>  \r\n" + ps_script);

            foreach (String str in
            PowerShell.Create()
                .AddScript(ps_script)
                .AddCommand("Out-String")
                .Invoke<String>())
            {
                Console.WriteLine(str);
            };
            */

            /*
            Console.WriteLine("\r\n Task 6: Get from an specific process only the %GPU usage of the most working GPU engine without headers");
            Console.WriteLine("Getting pid of Explorer");
            Process[] Explorer = Process.GetProcessesByName("Explorer");
            int pid = Explorer[0].Id;
            Console.WriteLine("Pid of Explorer is: " + pid);
            String ps_script = "Get-WmiObject Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine -Filter \"Name LIKE 'pid_" + pid + "%'\""
                + " | Sort-Object -Descending -Property UtilizationPercentage "
                + " | Select-Object -First 1 "
                + " | select-Object UtilizationPercentage";
            ps_script = "(" + ps_script + ").UtilizationPercentage";
            Console.WriteLine("Now we are gonna execute the next script in Powershell: >>>>  \r\n" + ps_script);

            foreach (String str in
            PowerShell.Create()
                .AddScript(ps_script)
                .AddCommand("Out-String")
                .Invoke<String>())
            {
                Console.WriteLine(str);
            };
            */

            /*
            Console.WriteLine("\r\n Task 7: Get from an specific process only the %CPU usage and Ram without headers");
            Console.WriteLine("Getting pid of Explorer");
            Process[] Explorer = Process.GetProcessesByName("Explorer");
            int pid = Explorer[0].Id;
            Console.WriteLine("Pid of Explorer is: " + pid);
            String ps_script = "Get-WmiObject -class Win32_PerfFormattedData_PerfProc_Process -filter IDProcess=" + pid
                + " | select-object PercentProcessorTime, {$_.WorkingSetPrivate/1mb}";
            ps_script =ps_script + " | ft -hidetableheaders";
            Console.WriteLine("Now we are gonna execute the next script in Powershell: >>>>  \r\n" + ps_script);


            foreach (String str in
            PowerShell.Create()
                .AddScript(ps_script)
                .AddCommand("Out-String")
                .Invoke<String>())
            {
                Console.WriteLine(str);
            };
            */

            /*
            Console.WriteLine("\r\n Task 8: Alternative commands");
            Console.WriteLine("We are gonna show the explorer %CPU use");
            String ps_script = "Get-Counter \"\\proceso(Explorer)\\% de tiempo de procesador\"";
            //if the system language (it can be checked runing <<get-UIculture>> used was English the script would be:
            //Get-Counter "\Process(Explorer)\% Processor Time"
            //in Spanish we check the posible properties running this:
            //>>$ListSet = Get-Counter -ListSet proceso
            //>> $ListSet.Counter
            Console.WriteLine("Now we are gonna execute the next script in Powershell: >>>>  \r\n" + ps_script);


            foreach (String str in
            PowerShell.Create()
                .AddScript(ps_script)
                .AddCommand("Out-String")
                .Invoke<String>())
            {
                Console.WriteLine(str);
            };
            */


            /***/
            /***/
            /*Console.WriteLine("End of trial of powershell");*/
            /*****************************************************************/
            /*****************************************************************/
        }
        /*****************************************************************/

        /* In this block we'll explain the ps queries used in this Script*/
        {
            /*GENERAL*/
            /*
             * We always use the sentence <<Get-WmiObject>> this returns the type of objects used
             * in powershell to do queries.
             * 
            */



            /* CPU and memory*/
            /*
             *We use the sentence <<-class Win32_PerfFormattedData_PerfProc_Process>> to obtain an object that
             * contains the formatted data of a process's performance data.
             * 
             * We use a filter to obtain the data of an specific process given the pid 
             * <<-filter IDProcess= $PID$>>
             * 
             * Then we select the desired property using a selector.
             * For the CPU usage, we use: <<select-object PercentProcessorTime>>
             * and for the memory used: <<select-object WorkingSetPrivate>> which is given in bytes
            */



            /* GPU */
            /*
             * To obtain an object with the performance data of the GPU engines for the processes running
             * we use the following sentence: <<Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine>>
             * 
             * This type of object doesn't contain a property for the PID of the process, but the names 
             * are like the following: "pid_856_luid_0x00000000_0x000098FE_phys_0_eng_9_engtype_Graphics_1"
             * We can see that they are composed by the PID of the process, an hexadecimal direction, and
             * a kind of description of the given Engine (the object contains information of all the engines), in this
             * king it seems to be the graphics engine.
             * To obtain the information for a given process we use the following filter: 
             * <<-Filter \"Name LIKE '%pid_$PID$%'>>
             * 
             * To know all the types of GPU engine running the following sentence can be used (using a valid pid):
             * <<Get-WmiObject Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine -Filter "Name LIKE '%pid_856%'" | select-object name>>
             * The former sentences returns the following results in the Computer used to test this:
             *      pid_856_luid_0x00000000_0x000098FE_phys_0_eng_0_engtype_3D
                    pid_856_luid_0x00000000_0x000098FE_phys_0_eng_1_engtype_Compute_0
                    pid_856_luid_0x00000000_0x000098FE_phys_0_eng_10_engtype_Compute_1
                    pid_856_luid_0x00000000_0x000098FE_phys_0_eng_11_engtype_VR
                    pid_856_luid_0x00000000_0x000098FE_phys_0_eng_2_engtype_LegacyOverlay
                    pid_856_luid_0x00000000_0x000098FE_phys_0_eng_3_engtype_VideoDecode
                    pid_856_luid_0x00000000_0x000098FE_phys_0_eng_4_engtype_Security
                    pid_856_luid_0x00000000_0x000098FE_phys_0_eng_5_engtype_Copy
                    pid_856_luid_0x00000000_0x000098FE_phys_0_eng_6_engtype_Copy
                    pid_856_luid_0x00000000_0x000098FE_phys_0_eng_7_engtype_VideoEncode
                    pid_856_luid_0x00000000_0x000098FE_phys_0_eng_8_engtype_VideoEncode
                    pid_856_luid_0x00000000_0x000098FE_phys_0_eng_9_engtype_Graphics_1
             *
             * If we want to sort the engines by GPU usage we use the following sentence:
             * << Sort-Object -Descending -Property UtilizationPercentage >>
             * 
             * If we want to select only the first one after sorting them we use:
             * <<Select-Object -First 1>>
             * 
             * Lastly to select only the usage, we use:
             * <<Select-Object -First 1  | select-Object UtilizationPercentage>>
            */
        }
        /*****************************************************************/

        Console.WriteLine("Profiling Script was successfully initialized.");


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


        // Waiting for user to open a Unity process
        while (Process.GetProcessesByName(process_name).Length == 0)
        {
            
            Console.WriteLine("Waiting for user to open a project named <<"+process_name+">>");
            Thread.Sleep(2000);

        }
        
        cnt = 0;

        Process[] activeProc = Process.GetProcessesByName(process_name);
        int unity_pid = activeProc[0].Id;
        output.Write("Captured information of Process Named: "+process_name+ " \r\n \r\n");

        /***********************/
        /*Engine Names        **/
        /***********************/
        Console.WriteLine("GPU engine names: ");
        output.Write("GPU Engine names: \r\n");                                                             // '/n' works on unix and mac. For windows it is '/r/n'
        foreach (String str in PowerShell.Create()
                    .AddScript(ps_script_nameEngines_first + unity_pid + ps_script_nameEngines_second)
                    .AddCommand("Out-String")
                    .Invoke<String>())
        {
            Console.WriteLine(str);
            output.Write(str + "\r\n");
        }
        Console.WriteLine("");
        output.Write("\r\n");
        /**                   **/
        /***********************/

        Console.WriteLine("Data recording started. Press Escape button to terminate.");
        output.Write("CPU Usage(%), GPU Usage(%), GPU Engines use, RAM Memory(GBs) \r\n");
        output.Write("------------------------------------------------------------\r\n");





        // Recording is terminated when Escape button is pressed
        try { 
            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)&& (activeProc.Length >= 1))
        { 
            // Ensuring Unity is still running
            
            if (activeProc.Length >= 1)
            {
                //Getting strings with values
                //GPU//
                gpu_str = PowerShell.Create()
                    .AddScript(ps_script_gpu_first+unity_pid+ps_script_gpu_second)
                    .AddCommand("Out-String")
                    .Invoke<String>()[0].ToString();
                //CPU//
                cpu_str = PowerShell.Create()
                    .AddScript(ps_script_cpu_first + unity_pid + ps_script_cpu_second)
                    .AddCommand("Out-String")
                    .Invoke<String>()[0].ToString();

                //RAM//
                ram_str = PowerShell.Create()
                    .AddScript(ps_script_ram_first + unity_pid + ps_script_ram_second)
                    .AddCommand("Out-String")
                    .Invoke<String>()[0].ToString();

                //GPU Engines//
                bool noFirst = false;
                enginesGPU = "[";
                foreach(PSObject str in PowerShell.Create()
                                        .AddScript(ps_script_GPUuseEngines_first + unity_pid + ps_script_GPUuseEngines_second)
                                        .Invoke())
                    {
                        if (noFirst)
                        {
                            enginesGPU += ",";
                        }
                        noFirst = true;
                        enginesGPU += str;
                        
                        
                    }
                    enginesGPU += "]";

                //Getting numeric values
                incGPU = double.Parse(gpu_str);
                incCPU = double.Parse(cpu_str) / numOfProcessors;
                incMemory = double.Parse(ram_str) / f / f ; ;
                
                
                
                // Printing and saving metrics 
                if (incCPU*incGPU != 0) //If we get a zero in any of the readings we discard this sample
                {

                    cpuUsage += incCPU;
                    gpuUsage += incGPU;
                    memoryMBs += incMemory;
                    cnt += 1;
                    Console.WriteLine("Sample " + cnt + " was received.");
                    Console.WriteLine("CPU %: " + incCPU);
                    Console.WriteLine("GPU %: " + incGPU);
                    Console.WriteLine("GPU Engines %: " + enginesGPU);
                    Console.WriteLine("RAM (MBs): " + incMemory);
                    output.Write(DateTime.UtcNow+", "+incCPU + ", " + incGPU + ", " + enginesGPU + ", " + incMemory + "\r\n");
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
            Console.WriteLine("An exception has happened. Has Unity been closed?");
            }
        // Printing and saving average values of metrics
        
        Console.WriteLine("The session was terminated by the user after the reception of " + cnt + " samples.");
        Console.WriteLine("Average CPU %: " + (cpuUsage / cnt));
        Console.WriteLine("Average GPU %: " + (gpuUsage / cnt));
        Console.WriteLine("Average RAM (MBs): " + (memoryMBs / cnt));
        output.Write("------------------------------------------------------------\r\n");
        output.Write("Average CPU %: " + (cpuUsage / cnt) + ", Average GPU %: " + (gpuUsage / cnt) + ", Average RAM (MBs): " + (memoryMBs / cnt) + "\r\n");

        output.Flush();
        output.Close();

        while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
        {
            Console.WriteLine("Press Escape to terminate the console application.");
            Thread.Sleep(2000);
        }
        

    }
}
