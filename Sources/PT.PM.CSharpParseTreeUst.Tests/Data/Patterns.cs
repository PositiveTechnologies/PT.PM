public class CSharpSamples 
{
    const string passwordInField = "asdf";
    
    public void StringLiteralSample()
    {
        // Outputs "Hello World!"
        Console.WriteLn("Hello World!");
    }

    public void HardcodedPasswordSample()
    {
        // Hardcoded Default password="value"
        var adminPassword = "value";
    }

    public void WeakCryptographicHashSample()
    {
    	System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
    }

    public void InsecureTransportSample()
    {
        string str1 = "http://ptsecurity.ru";
    }

    public void InsecureRandomnessSample()
    {
        Random r = new Random();
    }

    public void BadCatchBlocks()
    {
        try
        {
        }
        catch(System.NullReferenceException e)
        {
            throw e;
        }
        catch
        {
        }
    }
}