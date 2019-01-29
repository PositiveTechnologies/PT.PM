class TestTuples
{
    static void Main()
    {
        var tuple = (Password: "hardcoded", 1, 1.5, "otherstring");
        (string password, int x) = ("hardcoded", 1);
        (string password, int x) tuple = ("hardcoded", 1);
    }
}