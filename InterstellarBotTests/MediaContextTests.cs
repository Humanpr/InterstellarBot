using System.Collections;
using System.Collections.Generic;
using interstarbot.MediaContexts;
using Moq;
using Tweetinvi.Models;
using Xunit;

namespace InterstellarBotTests;

public class MediaContextTests
{
    [Theory]
    [MemberData(nameof(TestDataForBotOrder))]
    public void MediaContextShouldParseRight(decimal expectedStart,decimal expectedEnd,string orderText)
    {
        // Arrange
        var replyTweetMock = new Mock<ITweet>();
        var tweetUser = new Mock<IUser>();
        tweetUser.Setup(user => user.ToString()).Returns("TEST");
        replyTweetMock.Setup(tweet => tweet.Text).Returns(orderText);
        replyTweetMock.Setup(tweet => tweet.CreatedBy).Returns(tweetUser.Object);
        // Act
        var mediaContext = new MediaContext(replyTweetMock.Object,replyTweetMock.Object);
        // Assertion
        Assert.Equal((expectedStart,expectedEnd),(mediaContext.StartTime,mediaContext.EndTime));
    }

    public static IEnumerable<object[]> TestDataForBotOrder()
    {
        yield return new object[]
        {
            23,30,"@dockingbot start 23 end 30"
        };
        yield return new object[]
        {
            -1,-1,"@dockingbot staart 23 end 30"
        };
        yield return new object[]
        {
            999,999,"@dockingbot start 999 end 999"
        };
        yield return new object[]
        {
            -1,-1,"@dockingbot go 23 end 30"
        };
        yield return new object[]
        {
            -1,-1,"@dockingbot start 909090 end 30"
        };
        yield return new object[]
        {
            -1,-1,"@dockingbot start 30 end 898989"
        };
    }


    
    
}