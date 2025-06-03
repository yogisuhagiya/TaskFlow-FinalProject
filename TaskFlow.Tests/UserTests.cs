
using TaskFlow.Library.Models; 
using Xunit;                   

public class UserTests
{
    [Fact] 
    public void User_CanHaveUsername()
    {
        var myUser = new User();
        myUser.Username = "Testy McTestface";
        Assert.Equal("Testy McTestface", myUser.Username); 
    }
}