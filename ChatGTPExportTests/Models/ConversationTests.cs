using ChatGPTExport.Models;

namespace ChatGTPExportTests.Models;

public class ConversationTests
{
    [Fact]
    public void GetLatestConversation_UsesCurrentNodeBranch()
    {
        var conversation = new Conversation
        {
            current_node = "branch-b",
            mapping = new Dictionary<string, MessageContainer>
            {
                ["root"] = CreateMessageContainer("root", null, 1, ["leaf-a", "branch-b"]),
                ["leaf-a"] = CreateMessageContainer("leaf-a", "root", 10),
                ["branch-b"] = CreateMessageContainer("branch-b", "root", 2)
            }
        };

        var result = conversation.GetLatestConversation();

        Assert.NotNull(result.mapping);
        Assert.Equal(["root", "branch-b"], result.mapping!.Keys.ToList());
    }

    [Fact]
    public void GetLatestConversation_UsesLeafBelowCurrentNode()
    {
        var conversation = new Conversation
        {
            current_node = "branch-b",
            mapping = new Dictionary<string, MessageContainer>
            {
                ["root"] = CreateMessageContainer("root", null, 1, ["leaf-a", "branch-b"]),
                ["leaf-a"] = CreateMessageContainer("leaf-a", "root", 10),
                ["branch-b"] = CreateMessageContainer("branch-b", "root", 2, ["leaf-b"]),
                ["leaf-b"] = CreateMessageContainer("leaf-b", "branch-b", 3)
            }
        };

        var result = conversation.GetLatestConversation();

        Assert.NotNull(result.mapping);
        Assert.Equal(["root", "branch-b", "leaf-b"], result.mapping!.Keys.ToList());
    }

    [Fact]
    public void GetLatestConversation_FallsBackToMostRecentMessageBranchWhenCurrentNodeIsMissing()
    {
        var conversation = new Conversation
        {
            current_node = "missing",
            mapping = new Dictionary<string, MessageContainer>
            {
                ["root"] = CreateMessageContainer("root", null, 1, ["older-leaf", "recent-internal"]),
                ["older-leaf"] = CreateMessageContainer("older-leaf", "root", 10),
                ["recent-internal"] = CreateMessageContainer("recent-internal", "root", 20, ["recent-leaf"]),
                ["recent-leaf"] = CreateMessageContainer("recent-leaf", "recent-internal", 5)
            }
        };

        var result = conversation.GetLatestConversation();

        Assert.NotNull(result.mapping);
        Assert.Equal(["root", "recent-internal", "recent-leaf"], result.mapping!.Keys.ToList());
    }

    [Fact]
    public void GetLatestConversation_MostRecentInternalNode_SelectsDescendantLeafWithMissingTimestamp()
    {
        var conversation = new Conversation
        {
            mapping = new Dictionary<string, MessageContainer>
            {
                ["root"] = new MessageContainer
                {
                    id = "root",
                    children = ["recent-internal", "older-leaf"]
                },
                ["recent-internal"] = new MessageContainer
                {
                    id = "recent-internal",
                    parent = "root",
                    children = ["missing-ts-leaf"],
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

        var result = conversation.GetLatestConversation();

        Assert.NotNull(result.mapping);
        Assert.Contains("missing-ts-leaf", result.mapping!.Keys);
        Assert.DoesNotContain("older-leaf", result.mapping.Keys);
    }

    [Fact]
    public void GetLatestConversation_MappingWithNoMessages_FallsBackToLastLeaf()
    {
        var conversation = new Conversation
        {
            mapping = new Dictionary<string, MessageContainer>
            {
                ["root"] = new MessageContainer
                {
                    id = "root",
                    children = ["leaf-one", "leaf-two"]
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

        var result = conversation.GetLatestConversation();

        Assert.NotNull(result.mapping);
        Assert.Contains("leaf-two", result.mapping!.Keys);
        Assert.DoesNotContain("leaf-one", result.mapping.Keys);
    }

    [Fact]
    public void GetLatestConversation_MostRecentSubtreeWithMultipleLevels_SelectsDeepestLeaf()
    {
        var conversation = new Conversation
        {
            mapping = new Dictionary<string, MessageContainer>
            {
                ["root"] = new MessageContainer
                {
                    id = "root",
                    children = ["recent-internal", "older-leaf"]
                },
                ["recent-internal"] = new MessageContainer
                {
                    id = "recent-internal",
                    parent = "root",
                    children = ["middle"],
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
                    children = ["deep-leaf"],
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

        var result = conversation.GetLatestConversation();

        Assert.NotNull(result.mapping);
        Assert.Contains("recent-internal", result.mapping!.Keys);
        Assert.Contains("middle", result.mapping.Keys);
        Assert.Contains("deep-leaf", result.mapping.Keys);
        Assert.DoesNotContain("older-leaf", result.mapping.Keys);
    }

    [Fact]
    public void GetLatestConversation_SubtreeCycleWithoutLeaves_ReturnsEmptyMapping()
    {
        var conversation = new Conversation
        {
            mapping = new Dictionary<string, MessageContainer>
            {
                ["a"] = new MessageContainer
                {
                    id = "a",
                    children = ["b"],
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
                    children = ["a"],
                    message = new Message
                    {
                        id = "b",
                        update_time = 1
                    }
                }
            }
        };

        var result = conversation.GetLatestConversation();

        Assert.NotNull(result.mapping);
        Assert.Empty(result.mapping!);
    }

    private static MessageContainer CreateMessageContainer(string id, string? parent, decimal? updateTime, List<string>? children = null)
    {
        return new MessageContainer
        {
            id = id,
            parent = parent,
            children = children,
            message = updateTime.HasValue
                ? new Message
                {
                    id = id,
                    update_time = updateTime.Value
                }
                : null
        };
    }
}
