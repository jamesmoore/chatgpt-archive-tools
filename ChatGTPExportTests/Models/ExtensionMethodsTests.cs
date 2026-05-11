using ChatGPTExport.Models;

namespace ChatGTPExportTests.Models;

public class ExtensionMethodsTests
{
    [Fact]
    public void GetLatestConversations_EmptyInput_ReturnsEmpty()
    {
        // Arrange
        var conversations = new List<Conversations>();

        // Act
        var result = conversations.GetLatestConversations();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetLatestConversations_SingleConversation_ReturnsSingleConversation()
    {
        // Arrange
        var conversations = new List<Conversations>
        {
            new Conversations
            {
                new Conversation
                {
                    conversation_id = "conv1",
                    update_time = 100,
                    mapping = new Dictionary<string, MessageContainer>()
                }
            }
        };

        // Act
        var result = conversations.GetLatestConversations().ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("conv1", result[0].conversation_id);
    }

    [Fact]
    public void GetLatestConversations_MultipleVersionsOfSameConversation_ReturnsLatestByUpdateTime()
    {
        // Arrange
        var conversations = new List<Conversations>
        {
            new Conversations
            {
                new Conversation
                {
                    conversation_id = "conv1",
                    update_time = 100,
                    title = "Old Version",
                    mapping = new Dictionary<string, MessageContainer>()
                },
                new Conversation
                {
                    conversation_id = "conv1",
                    update_time = 200,
                    title = "Latest Version",
                    mapping = new Dictionary<string, MessageContainer>()
                }
            }
        };

        // Act
        var result = conversations.GetLatestConversations().ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("conv1", result[0].conversation_id);
        Assert.Equal("Latest Version", result[0].title);
        Assert.Equal(200, result[0].update_time);
    }

    [Fact]
    public void GetLatestConversations_FiltersOutConversationsWithNullMapping()
    {
        // Arrange
        var conversations = new List<Conversations>
        {
            new Conversations
            {
                new Conversation
                {
                    conversation_id = "conv1",
                    update_time = 100,
                    mapping = null
                },
                new Conversation
                {
                    conversation_id = "conv1",
                    update_time = 200,
                    title = "Valid Version",
                    mapping = new Dictionary<string, MessageContainer>()
                }
            }
        };

        // Act
        var result = conversations.GetLatestConversations().ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Valid Version", result[0].title);
        Assert.Equal(200, result[0].update_time);
    }

    [Fact]
    public void GetLatestConversations_MultipleConversations_ReturnsLatestOfEach()
    {
        // Arrange
        var conversations = new List<Conversations>
        {
            new Conversations
            {
                new Conversation
                {
                    conversation_id = "conv1",
                    update_time = 100,
                    title = "Conv1 Old",
                    mapping = new Dictionary<string, MessageContainer>()
                },
                new Conversation
                {
                    conversation_id = "conv1",
                    update_time = 150,
                    title = "Conv1 Latest",
                    mapping = new Dictionary<string, MessageContainer>()
                },
                new Conversation
                {
                    conversation_id = "conv2",
                    update_time = 120,
                    title = "Conv2 Old",
                    mapping = new Dictionary<string, MessageContainer>()
                },
                new Conversation
                {
                    conversation_id = "conv2",
                    update_time = 180,
                    title = "Conv2 Latest",
                    mapping = new Dictionary<string, MessageContainer>()
                }
            }
        };

        // Act
        var result = conversations.GetLatestConversations().OrderBy(c => c.conversation_id).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("conv1", result[0].conversation_id);
        Assert.Equal("Conv1 Latest", result[0].title);
        Assert.Equal("conv2", result[1].conversation_id);
        Assert.Equal("Conv2 Latest", result[1].title);
    }

    [Fact]
    public void GetLatestConversations_OrdersByConversationId()
    {
        // Arrange
        var conversations = new List<Conversations>
        {
            new Conversations
            {
                new Conversation
                {
                    conversation_id = "conv3",
                    update_time = 100,
                    mapping = new Dictionary<string, MessageContainer>()
                },
                new Conversation
                {
                    conversation_id = "conv1",
                    update_time = 200,
                    mapping = new Dictionary<string, MessageContainer>()
                },
                new Conversation
                {
                    conversation_id = "conv2",
                    update_time = 150,
                    mapping = new Dictionary<string, MessageContainer>()
                }
            }
        };

        // Act
        var result = conversations.GetLatestConversations().ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("conv1", result[0].conversation_id);
        Assert.Equal("conv2", result[1].conversation_id);
        Assert.Equal("conv3", result[2].conversation_id);
    }

    [Fact]
    public void GetLatestConversations_MultipleConversationsListsInInput_CombinesAndReturnsLatest()
    {
        // Arrange
        var conversations = new List<Conversations>
        {
            new Conversations
            {
                new Conversation
                {
                    conversation_id = "conv1",
                    update_time = 100,
                    title = "Conv1 Old",
                    mapping = new Dictionary<string, MessageContainer>()
                }
            },
            new Conversations
            {
                new Conversation
                {
                    conversation_id = "conv1",
                    update_time = 200,
                    title = "Conv1 Latest",
                    mapping = new Dictionary<string, MessageContainer>()
                },
                new Conversation
                {
                    conversation_id = "conv2",
                    update_time = 150,
                    title = "Conv2",
                    mapping = new Dictionary<string, MessageContainer>()
                }
            }
        };

        // Act
        var result = conversations.GetLatestConversations().OrderBy(c => c.conversation_id).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("conv1", result[0].conversation_id);
        Assert.Equal("Conv1 Latest", result[0].title);
        Assert.Equal("conv2", result[1].conversation_id);
    }

    [Fact]
    public void GetLastestConversation_MostRecentInternalNode_SelectsDescendantLeafWithMissingTimestamp()
    {
        // Arrange
        var conversation = new Conversation
        {
            mapping = new Dictionary<string, MessageContainer>
            {
                ["root"] = new MessageContainer
                {
                    id = "root",
                    children = new List<string> { "recent-internal", "older-leaf" }
                },
                ["recent-internal"] = new MessageContainer
                {
                    id = "recent-internal",
                    parent = "root",
                    children = new List<string> { "missing-ts-leaf" },
                    message = new Message
                    {
                        id = "recent-internal",
                        update_time = 200
                    }
                },
                ["missing-ts-leaf"] = new MessageContainer
                {
                    id = "missing-ts-leaf",
                    parent = "recent-internal",
                    message = new Message
                    {
                        id = "missing-ts-leaf"
                    }
                },
                ["older-leaf"] = new MessageContainer
                {
                    id = "older-leaf",
                    parent = "root",
                    message = new Message
                    {
                        id = "older-leaf",
                        update_time = 150
                    }
                }
            }
        };

        // Act
        var latestConversation = conversation.GetLastestConversation();

        // Assert
        Assert.NotNull(latestConversation.mapping);
        Assert.Contains("missing-ts-leaf", latestConversation.mapping!.Keys);
        Assert.DoesNotContain("older-leaf", latestConversation.mapping.Keys);
    }

    [Fact]
    public void GetLastestConversation_MappingWithNoMessages_FallsBackToLastLeaf()
    {
        // Arrange
        var conversation = new Conversation
        {
            mapping = new Dictionary<string, MessageContainer>
            {
                ["root"] = new MessageContainer
                {
                    id = "root",
                    children = new List<string> { "leaf-one", "leaf-two" }
                },
                ["leaf-one"] = new MessageContainer
                {
                    id = "leaf-one",
                    parent = "root"
                },
                ["leaf-two"] = new MessageContainer
                {
                    id = "leaf-two",
                    parent = "root"
                }
            }
        };

        // Act
        var latestConversation = conversation.GetLastestConversation();

        // Assert
        Assert.NotNull(latestConversation.mapping);
        Assert.Contains("leaf-two", latestConversation.mapping!.Keys);
        Assert.DoesNotContain("leaf-one", latestConversation.mapping.Keys);
    }

    [Fact]
    public void GetLastestConversation_MostRecentSubtreeWithMultipleLevels_SelectsDeepestLeaf()
    {
        // Arrange
        var conversation = new Conversation
        {
            mapping = new Dictionary<string, MessageContainer>
            {
                ["root"] = new MessageContainer
                {
                    id = "root",
                    children = new List<string> { "recent-internal", "older-leaf" }
                },
                ["recent-internal"] = new MessageContainer
                {
                    id = "recent-internal",
                    parent = "root",
                    children = new List<string> { "middle" },
                    message = new Message
                    {
                        id = "recent-internal",
                        update_time = 300
                    }
                },
                ["middle"] = new MessageContainer
                {
                    id = "middle",
                    parent = "recent-internal",
                    children = new List<string> { "deep-leaf" },
                    message = new Message
                    {
                        id = "middle"
                    }
                },
                ["deep-leaf"] = new MessageContainer
                {
                    id = "deep-leaf",
                    parent = "middle",
                    message = new Message
                    {
                        id = "deep-leaf"
                    }
                },
                ["older-leaf"] = new MessageContainer
                {
                    id = "older-leaf",
                    parent = "root",
                    message = new Message
                    {
                        id = "older-leaf",
                        update_time = 250
                    }
                }
            }
        };

        // Act
        var latestConversation = conversation.GetLastestConversation();

        // Assert
        Assert.NotNull(latestConversation.mapping);
        Assert.Contains("recent-internal", latestConversation.mapping!.Keys);
        Assert.Contains("middle", latestConversation.mapping.Keys);
        Assert.Contains("deep-leaf", latestConversation.mapping.Keys);
        Assert.DoesNotContain("older-leaf", latestConversation.mapping.Keys);
    }

    [Fact]
    public void GetLastestConversation_SubtreeCycleWithoutLeaves_ReturnsEmptyMapping()
    {
        // Arrange
        var conversation = new Conversation
        {
            mapping = new Dictionary<string, MessageContainer>
            {
                ["a"] = new MessageContainer
                {
                    id = "a",
                    children = new List<string> { "b" },
                    message = new Message
                    {
                        id = "a",
                        update_time = 2
                    }
                },
                ["b"] = new MessageContainer
                {
                    id = "b",
                    parent = "a",
                    children = new List<string> { "a" },
                    message = new Message
                    {
                        id = "b",
                        update_time = 1
                    }
                }
            }
        };

        // Act
        var latestConversation = conversation.GetLastestConversation();

        // Assert
        Assert.NotNull(latestConversation.mapping);
        Assert.Empty(latestConversation.mapping!);
    }

    [Fact]
    public void GetMessagesWithContent_ConversationWithNullMapping_ReturnsEmpty()
    {
        // Arrange
        var conversation = new Conversation
        {
            mapping = null
        };

        // Act
        var result = conversation.GetMessagesWithContent();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetMessagesWithContent_ConversationWithMessages_ReturnsMessagesWithContent()
    {
        // Arrange
        var conversation = new Conversation
        {
            mapping = new Dictionary<string, MessageContainer>
            {
                ["msg1"] = new MessageContainer
                {
                    message = new Message
                    {
                        id = "msg1",
                        content = new ContentText { parts = new List<string> { "Hello" } }
                    }
                },
                ["msg2"] = new MessageContainer
                {
                    message = new Message
                    {
                        id = "msg2",
                        content = new ContentText { parts = new List<string> { "World" } }
                    }
                }
            }
        };

        // Act
        var result = conversation.GetMessagesWithContent().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.id == "msg1");
        Assert.Contains(result, m => m.id == "msg2");
    }

    [Fact]
    public void GetMessagesWithContent_FiltersOutMessagesWithNullContent()
    {
        // Arrange
        var conversation = new Conversation
        {
            mapping = new Dictionary<string, MessageContainer>
            {
                ["msg1"] = new MessageContainer
                {
                    message = new Message
                    {
                        id = "msg1",
                        content = new ContentText { parts = new List<string> { "Hello" } }
                    }
                },
                ["msg2"] = new MessageContainer
                {
                    message = new Message
                    {
                        id = "msg2",
                        content = null
                    }
                }
            }
        };

        // Act
        var result = conversation.GetMessagesWithContent().ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("msg1", result[0].id);
    }

    [Fact]
    public void GetMessagesWithContent_FiltersOutNullMessages()
    {
        // Arrange
        var conversation = new Conversation
        {
            mapping = new Dictionary<string, MessageContainer>
            {
                ["msg1"] = new MessageContainer
                {
                    message = new Message
                    {
                        id = "msg1",
                        content = new ContentText { parts = new List<string> { "Hello" } }
                    }
                },
                ["msg2"] = new MessageContainer
                {
                    message = null
                }
            }
        };

        // Act
        var result = conversation.GetMessagesWithContent().ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("msg1", result[0].id);
    }

    [Fact]
    public void GetMessagesWithContent_EmptyMapping_ReturnsEmpty()
    {
        // Arrange
        var conversation = new Conversation
        {
            mapping = new Dictionary<string, MessageContainer>()
        };

        // Act
        var result = conversation.GetMessagesWithContent();

        // Assert
        Assert.Empty(result);
    }
}
