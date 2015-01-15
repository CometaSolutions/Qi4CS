In the properties of the test project, go to Debug tab.
There, check "start external program", and put path to nunit-console-x86.exe
The command line arguments are these:

Qi4j.Tests.dll /wait /noshadow

For older NUnit versions only:
Finally, navigate to nunit-console-x86.exe.config in the NUnit installation directory.
Add these lines directly after <configuration>:
    
    <startup>
    <requiredRuntime version="v4.0.30319" />
    </startup>