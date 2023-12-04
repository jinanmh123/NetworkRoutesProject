# NetworkRoutesProject
> [!NOTE]
> This code is especially written for a freelance project (contest) on freelancer.com.
> This script is written using C#, and is developed to output network destinations on the interfaces,
> Name and address of the interface is also displayed in the output

> [!IMPORTANT]
> You can run the script by going to the folder for the script using `cd NetworkRoutesProject` and then using `dotnet run`.
> To run the script, the project file is necessary, ***do not delete it***. It is the `NetworkRoutesProject.csproj` file that I have incuded here.

The following modifications and improvements have been done to the script-
1. ***Better Readability and precision***:

- ***Constants***: Added constants for error codes (`ERROR_INSUFFICIENT_BUFFER` and `NO_ERROR`) to replace magic numbers, enhancing code readability.

- ***LINQ***: Leveraged LINQ to enhance the clarity of code, making it more concise and readable.


2. ***Enhancing Performance and Speed***:

- ***Better responsiveness and scalability, as well as speed***: Modified the `Main` and related methods to be asynchronous, allowing for improved responsiveness and scalability.

- ***Parallel Processing***: Introduced parallel processing using `AsParallel()` in the `ParseRouteTable` method for improved performance when processing large datasets concurrently.

- ***Faster Execution***: By making use of asynchronous programming and parallel processing, the code achieves faster execution, ensuring optimized performance.


3. ***Better Error Handling and Testing***:

- ***Error Handling***: Implemented better error handling.

- ***Testing and Debugging***: The code has undergone thorough testing to ensure reliability and functionality. It has been debugged to address any potential issues and to guarantee smooth execution under various scenarios.
